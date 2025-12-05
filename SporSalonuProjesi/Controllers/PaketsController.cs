using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters; 
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;

namespace SporSalonuProjesi.Controllers
{
    public class PaketsController : Controller
    {
        private readonly AppDbContext _context;

        public PaketsController(AppDbContext context)
        {
            _context = context;
        }      
        // Bu blok, alttaki Index, Create, Edit  çalışmadan ÖNCE devreye girer.
        // Admin oturumu yoksa kapıdan içeri sokmaz.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
            base.OnActionExecuting(context);
        }     
        // GET: Pakets
        public async Task<IActionResult> Index()
        {
           
            return View(await _context.Paketler.ToListAsync());
        }

        // GET: Pakets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var paket = await _context.Paketler.FirstOrDefaultAsync(m => m.PaketId == id);
            if (paket == null) return NotFound();

            return View(paket);
        }

        // GET: Pakets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pakets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaketId,PaketAdi,SureAy,Fiyat,HaftalikRandevuLimiti,ToplamAiHakki,SinirsizMi")] Paket paket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paket);
        }

        // GET: Pakets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var paket = await _context.Paketler.FindAsync(id);
            if (paket == null) return NotFound();

            return View(paket);
        }

        // POST: Pakets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaketId,PaketAdi,SureAy,Fiyat,HaftalikRandevuLimiti,ToplamAiHakki,SinirsizMi")] Paket paket)
        {
            if (id != paket.PaketId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaketExists(paket.PaketId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(paket);
        }

        // GET: Pakets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var paket = await _context.Paketler.FirstOrDefaultAsync(m => m.PaketId == id);
            if (paket == null) return NotFound();

            return View(paket);
        }

        // POST: Pakets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paket = await _context.Paketler.FindAsync(id);
            if (paket != null)
            {
                _context.Paketler.Remove(paket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaketExists(int id)
        {
            return _context.Paketler.Any(e => e.PaketId == id);
        }
    }
}