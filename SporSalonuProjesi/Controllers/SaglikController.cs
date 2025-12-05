using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;
using System.Text;
using System.Text.Json;


namespace SporSalonuProjesi.Controllers
{
    [Authorize]
    public class SaglikController : Controller
    {
        private readonly AppDbContext _context;

        public SaglikController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Hesapla(double boy, double kilo, string cinsiyet, IFormFile vucutResmi)
        {
            
            // Session'dan STRING olarak çekiyoruz 
            string gelenUyeIdString = HttpContext.Session.GetString("UyeId");

            
            if (string.IsNullOrEmpty(gelenUyeIdString) || !int.TryParse(gelenUyeIdString, out int gercekUyeId))
            {
                return RedirectToAction("Login", "Uye");
            }

            // Veritabanından üyeyi ve paketini getir
            var uye = await _context.Uyeler
                .Include(u => u.Paket)
                .FirstOrDefaultAsync(u => u.UyeId == gercekUyeId);

            // KONTROL 1: Paket var mı?
            if (uye == null || uye.Paket == null) return RedirectToAction("Paketler", "Home");

            // KONTROL 2: VIP değilse ve Hakkı bitmişse 
            if (uye.Paket.SinirsizMi == false && uye.KalanAiHakki <= 0)
            {
                ViewBag.YapayZekaCevabi = $"⚠️ Üzgünüm, bu haftalık AI analiz hakkınız doldu. ({uye.Paket.PaketAdi} Paketi)";
                ViewBag.Goster = true;
                return View("Index");
            }
            // GEMINI API İŞLEMLERİ 
            string apiKey = "AIzaSyDxJvMEUidoegctw18tEvtiVuJxi8l5HEk";
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            string mesaj = $"Ben {boy} cm boyunda, {kilo} kg ağırlığında bir {cinsiyet} bireyim. Bana vücut kitle indeksimi söyle. ";

            if (vucutResmi != null)
            {
                mesaj += "Ayrıca yüklediğim fotoğrafa bakarak vücut tipimi ve tahmini yağ oranımı analiz et. ";
            }

            mesaj += $"Durumumu değerlendir ve bana uygun, maddeler halinde 1 günlük örnek antrenman ve beslenme programı yaz. Samimi bir spor hocası gibi konuş.";

           
            var partsList = new List<object>();
            partsList.Add(new { text = mesaj });

            if (vucutResmi != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await vucutResmi.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    partsList.Add(new
                    {
                        inline_data = new
                        {
                            mime_type = vucutResmi.ContentType,
                            data = base64String
                        }
                    });
                }
            }
            var istekVerisi = new { contents = new[] { new { parts = partsList } } };
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            string jsonBody = JsonSerializer.Serialize(istekVerisi);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(url, content);
                    var jsonSonuc = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var geminiCevap = JsonSerializer.Deserialize<GeminiResponse>(jsonSonuc, options);

                        if (geminiCevap != null && geminiCevap.candidates != null && geminiCevap.candidates.Count > 0)
                        {
                            string metin = geminiCevap.candidates[0].content.parts[0].text;
                            metin = metin.Replace("**", "");
                            ViewBag.YapayZekaCevabi = metin;
                            if (vucutResmi != null) ViewBag.ResimVarMi = true;
                                                 
                            if (uye.Paket.SinirsizMi == false)
                            {
                                uye.KalanAiHakki--; 
                                _context.Update(uye);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            ViewBag.YapayZekaCevabi = "Cevap geldi ama boştu.";
                        }
                    }
                    else
                    {
                        ViewBag.YapayZekaCevabi = $"🛑 HATA: {response.StatusCode}\nDetay: {jsonSonuc}";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.YapayZekaCevabi = $"Sistem Hatası: {ex.Message}";
                }
            }

            ViewBag.Goster = true;

            return View("Index");
        }
    }
}