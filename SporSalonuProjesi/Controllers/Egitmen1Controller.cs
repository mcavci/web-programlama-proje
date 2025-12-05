using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;


namespace SporSalonuProjesi.Controllers
{
    public class Egitmen1Controller : Controller
    {
        private readonly AppDbContext _context;

        public Egitmen1Controller(AppDbContext context)
        {
            _context = context;
        }

        // GET: Egitmen1
        // Herkes görebilir
        public async Task<IActionResult> Index()
        {
            return View(await _context.Egitmenler.ToListAsync());
        }

        // GET: Egitmen1/Details/5
        // Herkes detaylara bakabilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler.FirstOrDefaultAsync(m => m.Id == id);
            if (egitmen == null) return NotFound();

            return View(egitmen);
        }

        // GET: Egitmen1/Create
        public IActionResult Create()
        {
            
            // Eğer Admin girişi yoksa Login'e git
            if (HttpContext.Session.GetString("AdminOturumu") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            return View();
        }

        // POST: Egitmen1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AdSoyad,Uzmanlik,Aciklama,FotoUrl,Instagram")] Egitmen egitmen)
        {
            // OTURUM KONTROLÜ
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login", "Admin");

            if (ModelState.IsValid)
            {
                _context.Add(egitmen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(egitmen);
        }

        // GET: Egitmen1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // OTURUM KONTROLÜ
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login", "Admin");

            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler.FindAsync(id);
            if (egitmen == null) return NotFound();

            return View(egitmen);
        }

        // POST: Egitmen1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdSoyad,Uzmanlik,Aciklama,FotoUrl,Instagram")] Egitmen egitmen)
        {
            // OTURUM KONTROLÜ
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login", "Admin");

            if (id != egitmen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(egitmen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EgitmenExists(egitmen.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(egitmen);
        }

        // GET: Egitmen1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // OTURUM KONTROLÜ
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login", "Admin");

            if (id == null) return NotFound();

            var egitmen = await _context.Egitmenler.FirstOrDefaultAsync(m => m.Id == id);
            if (egitmen == null) return NotFound();

            return View(egitmen);
        }

        // POST: Egitmen1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // OTURUM KONTROLÜ (Silme işlemi için de şart)
            if (HttpContext.Session.GetString("AdminOturumu") == null) return RedirectToAction("Login", "Admin");

            var egitmen = await _context.Egitmenler.FindAsync(id);
            if (egitmen != null)
            {
                _context.Egitmenler.Remove(egitmen);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EgitmenExists(int id)
        {
            return _context.Egitmenler.Any(e => e.Id == id);
        }
    }
}