using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters; // Güvenlik bekçisi için şart
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;

namespace SporSalonuProjesi.Controllers
{
    public class RandevusController : Controller
    {
        private readonly AppDbContext _context;

        public RandevusController(AppDbContext context)
        {
            _context = context;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null)
            {
                // Admin değilse direkt Admin Login
               
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }

            base.OnActionExecuting(context);
        }
  

        // GET: Randevus (LİSTELEME)
        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .OrderByDescending(x => x.Tarih)
                .ToListAsync();

            return View(randevular);
        }

        // GET: Randevus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // GET: Randevus/Create (ADMIN İÇİN EKLEME)
        public IActionResult Create()
        {
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad");
            ViewData["UyeId"] = new SelectList(_context.Uyeler, "UyeId", "Ad"); 

            return View();
        }

        // POST: Randevus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UyeId,Durum,UyeAdSoyad,EgitmenAdi,Tarih,Saat,EgitmenId")] Randevu randevu)
        {
            
            if (string.IsNullOrEmpty(randevu.Durum)) randevu.Durum = "Onaylandı";

            if (ModelState.IsValid)
            {
               
                var egitmen = await _context.Egitmenler.FindAsync(randevu.EgitmenId);
                if (egitmen != null) randevu.EgitmenAdi = egitmen.AdSoyad;

                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View(randevu);
        }

        // GET: Randevus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View(randevu);
        }

        // POST: Randevus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UyeId,Durum,UyeAdSoyad,EgitmenAdi,Tarih,Saat,EgitmenId")] Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", randevu.EgitmenId);
            return View(randevu);
        }

        // GET: Randevus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FirstOrDefaultAsync(m => m.Id == id);
            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // POST: Randevus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }
    }
}