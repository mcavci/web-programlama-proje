using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies; 
using Microsoft.AspNetCore.Mvc;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;
using System.Security.Claims; 
using System.Text.Json;

namespace SporSalonuProjesi.Controllers
{
    public class HesapController : Controller
    {
        private readonly AppDbContext _context;

        public HesapController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // --- GİRİŞ YAPMA KISMI ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }      
        [HttpPost]
        public async Task<IActionResult> Login(string email, string sifre)
        {
            var uye = _context.Uyeler.FirstOrDefault(x => x.Email == email && x.Sifre == sifre);

            if (uye != null)
            {                
                //  RESMİ KİMLİK OLUŞTURMA COOKIE AUTHENTICATION               
                // Sisteme Bu adam kim bilgisini hazırlıyoruz
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, uye.Ad + " " + uye.Soyad), 
                    new Claim("UyeId", uye.UyeId.ToString()),             
                    new Claim(ClaimTypes.Email, uye.Email)              
                };

                var userIdentity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(userIdentity);

               
                await HttpContext.SignInAsync("Cookies", principal);  
                
                // SESSION (OTURUM) OLUŞTURMA               
                HttpContext.Session.SetString("UyeId", uye.UyeId.ToString());

                string jsonUye = JsonSerializer.Serialize(uye);
                HttpContext.Session.SetString("AktifKullanici", jsonUye);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Hata = "Email veya şifre hatalı!";
                return View();
            }
        }

        //  ÇIKIŞ YAPMA HEM COKIE HEM SESSION TEMİZLENİR
        public async Task<IActionResult> Logout()
        {
            //  Resmi çıkış yap (Authorize kilidi devreye girsin)
            await HttpContext.SignOutAsync("Cookies");

            //  Session'ı temizle (Hafıza boşalsın)
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("AktifKullanici");

           
            return RedirectToAction("Login", "Hesap");
        }

        // KAYIT OLMA (REGISTER) 
        [HttpPost]
        public IActionResult Register(Uye model)
        {
            if (ModelState.IsValid)
            {
                bool varMi = _context.Uyeler.Any(x => x.Email == model.Email);

                if (varMi)
                {
                    ViewBag.Hata = "Bu email adresi zaten sistemde kayıtlı!";
                    return View(model);
                }

                model.KayitTarihi = DateTime.Now;

                if (model.PaketId == 0)
                {
                    var varsayilanPaket = _context.Paketler.OrderBy(p => p.Fiyat).FirstOrDefault();

                    if (varsayilanPaket != null)
                    {
                        model.PaketId = varsayilanPaket.PaketId;
                        model.KalanAiHakki = varsayilanPaket.ToplamAiHakki;
                        model.PaketBitisTarihi = DateTime.Now.AddMonths(varsayilanPaket.SureAy);
                    }
                    else
                    {
                        ViewBag.Hata = "Sistemde hiç paket bulunamadı! Lütfen önce paketleri yükleyin (/Home/PaketleriDoldur).";
                        return View(model);
                    }
                }

                _context.Uyeler.Add(model);
                _context.SaveChanges();

                TempData["Mesaj"] = "Kayıt başarılı! Şimdi giriş yapabilirsin.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        //PROFİL SAYFASI
        [HttpGet]
        public IActionResult Profil()
        {
            var uyeJson = HttpContext.Session.GetString("AktifKullanici");
            if (string.IsNullOrEmpty(uyeJson)) return RedirectToAction("Login"); 

            var sessionUye = JsonSerializer.Deserialize<Uye>(uyeJson);

            string idYazi = sessionUye.UyeId.ToString();

            var randevular = _context.Randevular
                                     .Where(x => x.UyeId == idYazi)
                                     .OrderByDescending(x => x.Tarih)
                                     .ToList();

            ViewBag.Randevular = randevular;
            return View(sessionUye);
        }
    }
}