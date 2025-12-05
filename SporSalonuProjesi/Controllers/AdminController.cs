using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;
using System.Collections.Generic; 
using System.Linq; 

namespace SporSalonuProjesi.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. ADMIN GİRİŞ EKRANI (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string kadi, string sifre)
        {
            // Veritabanında bu email ve bu şifreye sahip biri var mı
            var admin = _context.Adminler.FirstOrDefault(x => x.Email == kadi && x.Sifre == sifre);

            if (admin != null)
            {
                // Giriş Başarılı
                HttpContext.Session.SetString("AdminOturumu", kadi); // Emailini session'a atalım
                return RedirectToAction("Index");
            }

            // Giriş Başarısız
            ViewBag.Hata = "Hatalı kullanıcı adı veya şifre!";
            return View();
        }

        //  (YÖNETİM PANELİ - ANA SAYFA)
        public IActionResult Index()
        {
            // Oturum Kontrolü (Giriş yapmamışsa Login'e at)
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login");

            // --- İSTATİSTİKLER (KARTLAR İÇİN) ---
            ViewBag.ToplamRandevu = _context.Randevular.Count();
            ViewBag.BekleyenRandevu = _context.Randevular.Where(x => x.Durum == "Onay Bekliyor").Count();
            ViewBag.ToplamEgitmen = _context.Egitmenler.Count();
            ViewBag.ToplamPaket = _context.Paketler.Count();
            ViewBag.ToplamUye = _context.Uyeler.Count();

            // 1. Listeleri tanımla
            var aylarListesi = new List<string>();
            var kazancListesi = new List<decimal>(); 
            // 2. Bugünün tarihini al
            var bugun = DateTime.Now;

          
            
            var hamVeri = _context.Uyeler
                .Include(u => u.Paket) 
                .Where(u => u.KayitTarihi >= bugun.AddMonths(-6)) 
                .ToList();

           
            for (int i = 5; i >= 0; i--)
            {
                var islemTarihi = bugun.AddMonths(-i);

                string ayAdi = islemTarihi.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));
                aylarListesi.Add(ayAdi);

               
                var oAyinKazanci = hamVeri
                    .Where(x => x.KayitTarihi.Month == islemTarihi.Month &&
                                x.KayitTarihi.Year == islemTarihi.Year)
                    .Sum(x => x.Paket != null ? x.Paket.Fiyat : 0); 

                kazancListesi.Add(oAyinKazanci);
            }

            
          
            // Randevularda hangi hoca kaç kez seçilmiş, onu gruplayıp sayıyoruz.
            var hocaAnalizi = _context.Randevular
                .GroupBy(r => r.EgitmenAdi)
                .Select(g => new { HocaAdi = g.Key, Sayi = g.Count() })
                .ToList();

            // İsimleri ve Sayıları ayrı listeler olarak View'a gönderiyoruz
            ViewBag.HocaIsimleri = hocaAnalizi.Select(x => x.HocaAdi).ToList();
            ViewBag.HocaSayilari = hocaAnalizi.Select(x => x.Sayi).ToList();


         
            // Sadece "Onay Bekliyor" olanları en yeniden eskiye doğru getir
            var bekleyenRandevular = _context.Randevular
                                    .Where(x => x.Durum == "Onay Bekliyor")
                                    .OrderByDescending(x => x.Tarih) // En yakın tarih en üstte
                                    .ToList();
            ViewBag.Aylar = aylarListesi;
            ViewBag.KazancVerileri = kazancListesi;
            return View(bekleyenRandevular);
        }

        // 4. RANDEVU ONAYLA
        public async Task<IActionResult> RandevuOnayla(int id)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login");

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = "Onaylandı";

            _context.Update(randevu);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Randevu başarıyla onaylandı!";
            return RedirectToAction(nameof(Index));
        }

        // 5. RANDEVU SİL / REDDET
        public async Task<IActionResult> RandevuSil(int id)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login");

            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
                TempData["Hata"] = "Randevu iptal edildi ve silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 6. TÜM RANDEVULARI GÖR
        public IActionResult TumRandevular()
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login");

            var liste = _context.Randevular.OrderByDescending(x => x.Tarih).ToList();
            return View(liste);
        }
        // 7. ÇIKIŞ YAP (GÜNCELLENMİŞ HALİ)
        public IActionResult Logout()
        {
            // tüm oturumu temizle
            HttpContext.Session.Clear();       
            if (Request.Cookies["AdminOturumu"] != null)
            {
                Response.Cookies.Delete("AdminOturumu");
            }

            
            return RedirectToAction("Login", "Admin");
        }
    }
}