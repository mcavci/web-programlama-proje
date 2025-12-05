using Microsoft.AspNetCore.Mvc;
using SporSalonuProjesi.Models;
using SporSalonuProjesi.Data;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace SporSalonuProjesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sessionVerisi = HttpContext.Session.GetString("AktifKullanici");
            int? aktifPaketId = null;

            if (sessionVerisi != null)
            {
                var gelenUye = JsonSerializer.Deserialize<Uye>(sessionVerisi);
                if (gelenUye != null)
                {
                    ViewBag.KullaniciAd = gelenUye.Ad + " " + gelenUye.Soyad;

                   
                    var guncelUye = await _context.Uyeler.FirstOrDefaultAsync(u => u.UyeId == gelenUye.UyeId);

                    if (guncelUye != null)
                    {
                        aktifPaketId = guncelUye.PaketId;
                    }

                    // Vücut Kitle İndeksi (VKİ) Hesaplama
                    if (gelenUye.Boy > 0 && gelenUye.Kilo > 0)
                    {
                        double boyMetre = gelenUye.Boy / 100.0;
                        double vki = gelenUye.Kilo / (boyMetre * boyMetre);
                        ViewBag.VkiDegeri = Math.Round(vki, 2);

                        if (vki < 18.5) { ViewBag.VkiDurum = "Zayıf"; ViewBag.VkiRenk = "warning"; }
                        else if (vki < 24.9) { ViewBag.VkiDurum = "İdeal Kilo"; ViewBag.VkiRenk = "success"; }
                        else if (vki < 29.9) { ViewBag.VkiDurum = "Fazla Kilolu"; ViewBag.VkiRenk = "warning"; }
                        else { ViewBag.VkiDurum = "Obez"; ViewBag.VkiRenk = "danger"; }
                    }
                }
            }

            // View tarafında "Mevcut Paket" yazısı için bu ID lazım
            ViewBag.AktifPaketId = aktifPaketId;

            // 2. PAKETLERİ ÇEK (Fiyata göre ucuzdan pahalıya)
            var paketListesi = await _context.Paketler.OrderBy(p => p.Fiyat).ToListAsync();

            // 3. DERS PROGRAMINI ÇEK (Hocalarıyla beraber)
            ViewBag.DersProgrami = _context.Dersler.Include(x => x.Egitmen).ToList();

            return View(paketListesi);
        }
       

        
        // PAKET SEÇME VE GÜNCELLEME İŞLEMİ
      
        [HttpPost]
        public async Task<IActionResult> PaketSec(int paketId)
        {
            // 1. Session'dan STRING olarak çekiyoruz
            string uyeIdString = HttpContext.Session.GetString("UyeId");

            // 2. Güvenlik ve Çevirme İşlemi
           
            if (string.IsNullOrEmpty(uyeIdString) || !int.TryParse(uyeIdString, out int uyeId))
            {
                // Eğer sayıya çevrilemezse veya boşsa giriş sayfasına at
                return RedirectToAction("Login", "Hesap");
            }

           
            var uye = await _context.Uyeler.FindAsync(uyeId);

            var secilenPaket = await _context.Paketler.FindAsync(paketId);

            if (uye == null || secilenPaket == null) return NotFound();

           
            uye.PaketId = secilenPaket.PaketId;
            uye.PaketBitisTarihi = DateTime.Now.AddMonths(secilenPaket.SureAy);

            if (secilenPaket.SinirsizMi) uye.KalanAiHakki = 9999;
            else uye.KalanAiHakki = secilenPaket.ToplamAiHakki;

            _context.Update(uye);
            await _context.SaveChangesAsync();

           
            string guncelJson = JsonSerializer.Serialize(uye);
            HttpContext.Session.SetString("AktifKullanici", guncelJson);

            TempData["Mesaj"] = $"Tebrikler! {secilenPaket.PaketAdi} paketine geçiş yaptınız.";
            return RedirectToAction("Index");
        }        
        public IActionResult PaketleriDoldur()
        {
            if (_context.Paketler.Any())
            {
                return Content("Paketler zaten veritabanında mevcut.");
            }

            var paketler = new List<Paket>
            {
                new Paket { PaketAdi = "Başlangıç (Temel)", Fiyat = 750, SureAy = 1, HaftalikRandevuLimiti = 2, ToplamAiHakki = 5, SinirsizMi = false },
                new Paket { PaketAdi = "Gold Üyelik", Fiyat = 3500, SureAy = 6, HaftalikRandevuLimiti = 5, ToplamAiHakki = 20, SinirsizMi = false },
                new Paket { PaketAdi = "Platinum VIP", Fiyat = 6000, SureAy = 12, HaftalikRandevuLimiti = 999, ToplamAiHakki = 9999, SinirsizMi = true }
            };

            _context.Paketler.AddRange(paketler);
            _context.SaveChanges();

            return Content("✅ Paketler başarıyla oluşturuldu! Şimdi kayıt olup test edebilirsin.");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult GetDolulukDurumu(int year, int month)
        {
            // 1. Ayın başını ve sonunu belirle
            DateTime baslangic = new DateTime(year, month, 1);
            DateTime bitis = baslangic.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

            // 2. VERİTABANINDAN SADECE SAATLERİ ÇEK 
            // .ToList() diyerek veriyi önce hafızaya alıyoruz 
            var hamVeriler = _context.Randevular
                                     .Select(r => r.Saat)
                                     .ToList();

            // 3. HAFIZADA FİLTRELEME VE SAYIM YAP
            var gunlukSayimlar = hamVeriler
                .Select(tarihString => DateTime.Parse(tarihString)) 
                .Where(tarih => tarih >= baslangic && tarih <= bitis) 
                .GroupBy(x => x.Day) 
                .Select(g => new
                {
                    Gun = g.Key,
                    Sayi = g.Count()
                })
                .ToList();

           
            var sonucListesi = new List<object>();
            int oAydakiGunSayisi = DateTime.DaysInMonth(year, month);

            for (int i = 1; i <= oAydakiGunSayisi; i++)
            {
                var kayit = gunlukSayimlar.FirstOrDefault(x => x.Gun == i);
                int randevuSayisi = kayit != null ? kayit.Sayi : 0;

                string durum = "MÜSAİT";
                string renk = "bg-success";

               
                if (randevuSayisi >= 15)
                {
                    durum = "DOLU";
                    renk = "bg-danger";
                }
                else if (randevuSayisi >= 8)
                {
                    durum = "YOĞUN";
                    renk = "bg-warning text-dark";
                }

                sonucListesi.Add(new
                {
                    gun = i,
                    durum = durum,
                    renk = renk,
                    sayi = randevuSayisi
                });
            }

            return Json(sonucListesi);
        }

    }
}