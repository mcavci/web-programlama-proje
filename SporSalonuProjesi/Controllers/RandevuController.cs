using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;
using System.Text.Json;

[Authorize]
public class RandevuController : Controller
{
    private readonly AppDbContext _context;

    public RandevuController(AppDbContext context)
    {
        _context = context;
    }
   
   
  
    [HttpGet]
    public JsonResult GetMusaitSaatler(int egitmenId, DateTime tarih)
    {
        
        var kultur = new System.Globalization.CultureInfo("tr-TR");
        string secilenGunAdi = kultur.DateTimeFormat.GetDayName(tarih.DayOfWeek);
        secilenGunAdi = char.ToUpper(secilenGunAdi[0]) + secilenGunAdi.Substring(1);

        // 2. Hocanın o gündeki derslerini çek
        var gunlukDersler = _context.Dersler
                                .Where(x => x.EgitmenId == egitmenId && x.Gun == secilenGunAdi)
                                .ToList();

        var musaitSaatler = new List<string>();

        // 3. Tek tek dersleri kontrol et: Dolu mu?
        foreach (var ders in gunlukDersler)
        {
            string dersSaati = ders.BaslangicSaati.ToString(@"hh\:mm");

            // O saatte kaç kişi kayıtlı?
            int icerdekiKisiSayisi = _context.Randevular.Count(r =>
                r.EgitmenId == egitmenId &&
                r.Tarih.Date == tarih.Date &&
                r.Saat == dersSaati && 
                r.Durum != "İptal");

            //    KONTENJAN KONTROLÜ BURADA 
        
            if (icerdekiKisiSayisi < ders.Kontenjan)
            {
                musaitSaatler.Add(dersSaati);
            }
        }

        musaitSaatler.Sort();
        return Json(musaitSaatler);
    }
    //  RANDEVU ALMA SAYFASI
    [HttpGet]
    public IActionResult Al(string hocaAdi)
    {
        var sessionVerisi = HttpContext.Session.GetString("AktifKullanici");
        if (string.IsNullOrEmpty(sessionVerisi)) return RedirectToAction("Hesap", "Login");

        // --- 1. SEÇİLİ HOCAYI BULMA ---
        int? seciliHocaId = null;
        if (!string.IsNullOrEmpty(hocaAdi))
        {
            var hoca = _context.Egitmenler.FirstOrDefault(x => x.AdSoyad == hocaAdi);
            if (hoca != null) seciliHocaId = hoca.Id;
        }

        //      . HOCA BELLİYSE BUGÜNÜN BOŞ SAATLERİNİ GETİR    
        List<string> musaitSaatler = new List<string>();

        if (seciliHocaId != null)
        {
            DateTime bugun = DateTime.Today;
                  
            var kultur = new System.Globalization.CultureInfo("tr-TR");
            string turkceGun = kultur.DateTimeFormat.GetDayName(bugun.DayOfWeek);           
            turkceGun = char.ToUpper(turkceGun[0]) + turkceGun.Substring(1);
          
            // a) Ders Programını Çek
            var dersProgrami = _context.Dersler
                                    .Where(d => d.EgitmenId == seciliHocaId && d.Gun == turkceGun)
                                    .ToList();

            // b) Dolu Randevuları Çek
            var doluRandevular = _context.Randevular
                                     .Where(r => r.EgitmenId == seciliHocaId && r.Tarih.Date == bugun)
                                     .Select(r => r.Saat)
                                     .ToList();

            // c) Eşleştirme Yap
            foreach (var ders in dersProgrami)
            {
                string saatFormat = ders.BaslangicSaati.ToString(@"hh\:mm");

                // Eğer saat dolu DEĞİLSE listeye ekle
                if (!doluRandevular.Contains(saatFormat))
                {
                    musaitSaatler.Add(saatFormat + " - " + ders.DersAdi);
                }
            }
        }

        //        VERİLERİ VİEW'A GÖNDERME    

        ViewBag.MusaitSaatler = new SelectList(musaitSaatler);
        ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler.ToList(), "Id", "AdSoyad", seciliHocaId);

        Randevu model = new Randevu();
        model.Tarih = DateTime.Today;

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Al([Bind("Id,Durum,UyeAdSoyad,EgitmenAdi,Tarih,Saat,EgitmenId")] Randevu randevu)
    {
        string sessionUyeId = HttpContext.Session.GetString("UyeId");

        // 1. Giriş Kontrolü
        if (string.IsNullOrEmpty(sessionUyeId) || !int.TryParse(sessionUyeId, out int uyeIdInt))
        {
            return RedirectToAction("Hesap", "Login");
        }

       
        // Formdan gelmeyen ama Modelde zorunlu olan alanların hatasını siliyoruz.
        randevu.UyeId = sessionUyeId;

        ModelState.Remove("UyeId");      // Üye ID boş olamaz
        ModelState.Remove("UyeAdSoyad"); // Ad Soyad alanı zorunludur
        ModelState.Remove("EgitmenAdi"); // Eğitmen seçimi yapılmadı 
        ModelState.Remove("Egitmen");    // İlişkili tablo
        ModelState.Remove("Uye");        // İlişkili tablo
        ModelState.Remove("Durum");      
                                        

        var uye = await _context.Uyeler
            .Include(u => u.Paket)
            .FirstOrDefaultAsync(u => u.UyeId == uyeIdInt);

        // 2. Paket Kontrolü
        if (uye == null || uye.Paket == null)
        {
            ViewBag.Hata = "Paket bilgisi bulunamadı.";
            ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View("Al", randevu);
        }

        // 3. Haftalık Limit Kontrolü
        if (uye.Paket.SinirsizMi == false)
        {
            DateTime buHaftaBasi = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + 1);
            DateTime buHaftaSonu = buHaftaBasi.AddDays(7);

            int buHaftakiRandevuSayisi = await _context.Randevular
                .Where(r => r.UyeId == sessionUyeId &&
                             r.Tarih >= buHaftaBasi &&
                             r.Tarih < buHaftaSonu &&
                             r.Durum != "İptal")
                .CountAsync();

            if (buHaftakiRandevuSayisi >= uye.Paket.HaftalikRandevuLimiti)
            {
                ViewBag.Hata = $"🛑 LİMİT DOLDU: Haftada en fazla {uye.Paket.HaftalikRandevuLimiti} randevu alabilirsin.";
                ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
                return View("Al", randevu);
            }
        }

        // 4. Çakışma Kontrolü
        bool zatenRandevusuVarMi = await _context.Randevular.AnyAsync(r =>
            r.UyeId == sessionUyeId &&
            r.Tarih.Date == randevu.Tarih.Date &&
            r.Saat == randevu.Saat &&
            r.Durum != "İptal");

        if (zatenRandevusuVarMi)
        {
            ViewBag.Hata = "⚠️ Bu tarih ve saatte zaten randevun var.";
            ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View("Al", randevu);
        }

        // 5. Ders Programı Kontrolü
        var kultur = new System.Globalization.CultureInfo("tr-TR");
        string secilenGunAdi = kultur.DateTimeFormat.GetDayName(randevu.Tarih.DayOfWeek);
        secilenGunAdi = char.ToUpper(secilenGunAdi[0]) + secilenGunAdi.Substring(1);
        TimeSpan secilenSaat = TimeSpan.Parse(randevu.Saat);

        var dersProgrami = await _context.Dersler.FirstOrDefaultAsync(x =>
            x.EgitmenId == randevu.EgitmenId &&
            x.Gun == secilenGunAdi &&
            x.BaslangicSaati == secilenSaat
        );

        if (dersProgrami == null)
        {
            ViewBag.Hata = "Bu saatte hocanın dersi yok.";
            ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View("Al", randevu);
        }

        // 6. Kontenjan Kontrolü
        int siniftakiOgrenciSayisi = await _context.Randevular.CountAsync(x =>
            x.EgitmenId == randevu.EgitmenId &&
            x.Tarih.Date == randevu.Tarih.Date &&
            x.Saat == randevu.Saat &&
            x.Durum != "İptal"
        );

        if (siniftakiOgrenciSayisi >= dersProgrami.Kontenjan)
        {
            ViewBag.Hata = $"DOLU! Kontenjan ({dersProgrami.Kontenjan}) dolmuş.";
            ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View("Al", randevu);
        }

        // 7. KAYIT (Final Vuruşu)
        if (ModelState.IsValid)
        {
            randevu.Durum = "Onay Bekliyor";
            randevu.UyeAdSoyad = uye.Ad + " " + uye.Soyad;

            var egitmen = await _context.Egitmenler.FindAsync(randevu.EgitmenId);
            if (egitmen != null) randevu.EgitmenAdi = egitmen.AdSoyad;

            _context.Add(randevu);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = "Randevunuz başarıyla alındı.";

            return RedirectToAction("Index");
        }

        // Eğer hata varsa görelim
        var hatalar = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        if (hatalar.Count > 0)
        {
            ViewBag.Hata = "Kayıt Hatası: " + string.Join(" | ", hatalar);
        }

        ViewBag.EgitmenListesi = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
        return View("Al", randevu);
    }
    // RANDEVULARIM LİSTESİ 
    [HttpGet]
    public IActionResult Index()
    {
       
        var sessionVerisi = HttpContext.Session.GetString("AktifKullanici");
        if (string.IsNullOrEmpty(sessionVerisi)) return RedirectToAction("Hesap", "Login");

        var aktifUye = JsonSerializer.Deserialize<Uye>(sessionVerisi);
        string stringId = aktifUye.UyeId.ToString();

      
        var kullaniciRandevulari = _context.Randevular
                                        .Where(x => x.UyeId == stringId)
                                        .OrderByDescending(x => x.Tarih)
                                        .ToList();

       
        foreach (var randevu in kullaniciRandevulari)
        {
           
            if (randevu.Tarih.Date == DateTime.Today &&
                randevu.Durum != "İptal" &&
                randevu.Durum != "Tamamlandı")
            {
               
                if (TimeSpan.TryParse(randevu.Saat, out TimeSpan dersSaati))
                {
                    DateTime dersZamani = DateTime.Today.Add(dersSaati);
                    TimeSpan kalanSure = dersZamani - DateTime.Now;

                    
                    if (kalanSure.TotalMinutes > 0 && kalanSure.TotalHours <= 2)
                    {
                        ViewBag.Uyari = $"🔔 HATIRLATMA: {randevu.EgitmenAdi} ile dersiniz yaklaşık {kalanSure.Hours} saat {kalanSure.Minutes} dakika sonra başlayacak!";
                    }
                }
            }
        }
        return View(kullaniciRandevulari);
    }
}