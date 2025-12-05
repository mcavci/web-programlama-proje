using Microsoft.AspNetCore.Mvc;

namespace SporSalonuProjesi.Controllers
{
    public class IletisimController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Gonder(string ad, string email, string konu, string mesaj)
        {
             
            if (!string.IsNullOrEmpty(ad) && !string.IsNullOrEmpty(email))
            {
                ViewBag.Mesaj = "Talebiniz başarıyla alındı! En kısa sürede dönüş yapacağız.";
                ViewBag.Durum = "success";
            }
            else
            {
                ViewBag.Mesaj = "Lütfen tüm alanları doldurunuz.";
                ViewBag.Durum = "danger";
            }

            return View("Index");
        }
    }
}
