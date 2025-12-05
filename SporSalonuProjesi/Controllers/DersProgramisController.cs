using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters; // Güvenlik Bekçisi İçin
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;

namespace SporSalonuProjesi.Controllers
{
    public class DersProgramisController : Controller
    {
        private readonly AppDbContext _context;

        public DersProgramisController(AppDbContext context)
        {
            _context = context;
        }
        //  GÜVENLİK KİLİDİ (SADECE ADMİN GİREBİLİR)
      
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
            base.OnActionExecuting(context);
        }
 
        // GET: DersProgramis
        public async Task<IActionResult> Index()
        {
            var dersler = _context.Dersler.Include(d => d.Egitmen);
            return View(await dersler.ToListAsync());
        }

        // GET: DersProgramis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dersProgrami = await _context.Dersler
                .Include(d => d.Egitmen)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dersProgrami == null) return NotFound();

            return View(dersProgrami);
        }

        // GET: DersProgramis/Create
        public IActionResult Create()
        {
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad");
            return View();
        }

        // POST: DersProgramis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Gun,BaslangicSaati,BitisSaati,DersAdi,EgitmenId,Kontenjan")] DersProgrami dersProgrami)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dersProgrami);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", dersProgrami.EgitmenId);
            return View(dersProgrami);
        }

        // GET: DersProgramis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dersProgrami = await _context.Dersler.FindAsync(id);
            if (dersProgrami == null) return NotFound();

            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", dersProgrami.EgitmenId);
            return View(dersProgrami);
        }

        // POST: DersProgramis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Gun,BaslangicSaati,BitisSaati,DersAdi,EgitmenId,Kontenjan")] DersProgrami dersProgrami)
        {
            if (id != dersProgrami.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dersProgrami);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                   
                    if (!DersProgramiExists(dersProgrami.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", dersProgrami.EgitmenId);
            return View(dersProgrami);
        }

        // GET: DersProgramis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dersProgrami = await _context.Dersler
                .Include(d => d.Egitmen)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dersProgrami == null) return NotFound();

            return View(dersProgrami);
        }

        // POST: DersProgramis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dersProgrami = await _context.Dersler.FindAsync(id);
            if (dersProgrami != null)
            {
                _context.Dersler.Remove(dersProgrami);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       
        private bool DersProgramiExists(int id)
        {
            return _context.Dersler.Any(e => e.Id == id);
        }
    }
}