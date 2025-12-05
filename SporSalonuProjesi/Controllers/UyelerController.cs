using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters; // Bekçi için şart
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;

namespace SporSalonuProjesi.Controllers
{
    public class UyelerController : Controller
    {
        private readonly AppDbContext _context;

        public UyelerController(AppDbContext context)
        {
            _context = context;
        }  
        // Bu kod sayesinde Admin olmayan kimse Create, Edit, Index hiçbirine giremez.
        // Tek tek if yazmaktan kurtulduk.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
            base.OnActionExecuting(context);
        }
    


        // GET: Uyeler
        public async Task<IActionResult> Index()
        {
          
            var appDbContext = _context.Uyeler
                                       .Include(u => u.Egitmen)
                                       .Include(u => u.Paket);

            return View(await appDbContext.ToListAsync());
        }

        // GET: Uyeler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var uye = await _context.Uyeler
                .Include(u => u.Egitmen)
                .Include(u => u.Paket) 
                .FirstOrDefaultAsync(m => m.UyeId == id);

            if (uye == null) return NotFound();

            return View(uye);
        }

        // GET: Uyeler/Create
        public IActionResult Create()
        {
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad");
            ViewData["PaketId"] = new SelectList(_context.Paketler, "PaketId", "PaketAdi");
            return View();
        }

        // POST: Uyeler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Uye uye)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uye);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", uye.EgitmenId);
            ViewData["PaketId"] = new SelectList(_context.Paketler, "PaketId", "PaketAdi", uye.PaketId);
            return View(uye);
        }

        // GET: Uyeler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();

            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", uye.EgitmenId);
            ViewData["PaketId"] = new SelectList(_context.Paketler, "PaketId", "PaketAdi", uye.PaketId);
            return View(uye);
        }

        // POST: Uyeler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UyeId,Ad,Soyad,Email,Sifre,Telefon,DogumTarihi,Boy,Kilo,KayitTarihi,PaketId,EgitmenId,KalanAiHakki,PaketBitisTarihi")] Uye uye)
        {
            if (id != uye.UyeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uye);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UyeExists(uye.UyeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EgitmenId"] = new SelectList(_context.Egitmenler, "Id", "AdSoyad", uye.EgitmenId);
            ViewData["PaketId"] = new SelectList(_context.Paketler, "PaketId", "PaketAdi", uye.PaketId);
            return View(uye);
        }

        // GET: Uyeler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var uye = await _context.Uyeler
                .Include(u => u.Egitmen)
                .Include(u => u.Paket)
                .FirstOrDefaultAsync(m => m.UyeId == id);

            if (uye == null) return NotFound();

            return View(uye);
        }

        // POST: Uyeler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye != null)
            {
                _context.Uyeler.Remove(uye);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UyeExists(int id)
        {
            return _context.Uyeler.Any(e => e.UyeId == id);
        }
    }
}