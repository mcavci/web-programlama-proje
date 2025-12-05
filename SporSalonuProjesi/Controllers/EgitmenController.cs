using Microsoft.AspNetCore.Mvc;
using SporSalonuProjesi.Models; 
using System.Collections.Generic;
using SporSalonuProjesi.Data;
namespace SporSalonuProjesi.Controllers
{
    public class EgitmenController : Controller
    {
        private readonly AppDbContext _context;

        public EgitmenController(AppDbContext context)
        {
            _context = context;
        }
       
       public IActionResult Hoca()
       {
            // Veritabanındaki "Egitmenler" tablosundaki tüm kayıtları listeye çevirip getirir.
            var egitmenler = _context.Egitmenler.ToList();

            return View(egitmenler);
       }
    }
}