using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters; 
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Models;

namespace SporSalonuProjesi.Controllers
{
    public class AdminsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }
        //  GÜVENLİK KİLİDİ (ÇOK ÖNEMLİ)    
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("AdminOturumu") == null)
            {
                // Admin değilse direkt Login sayfasına fırlat
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
            base.OnActionExecuting(context);
        }
        // GET: Admins
        public async Task<IActionResult> Index()
        {
            return View(await _context.Adminler.ToListAsync());
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var admin = await _context.Adminler
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null) return NotFound();

            return View(admin);
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Sifre")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var admin = await _context.Adminler.FindAsync(id);
            if (admin == null) return NotFound();

            return View(admin);
        }

        // POST: Admins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Sifre")] Admin admin)
        {
            if (id != admin.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var admin = await _context.Adminler
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null) return NotFound();

            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Adminler.FindAsync(id);
            if (admin != null)
            {
                _context.Adminler.Remove(admin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _context.Adminler.Any(e => e.Id == id);
        }
    }
}