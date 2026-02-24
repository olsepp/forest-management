using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LandPropertiesController : Controller
    {
        private readonly AppDbContext _context;

        public LandPropertiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LandProperties
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LandProperties.Include(l => l.Company);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LandProperties/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landProperty = await _context.LandProperties
                .Include(l => l.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (landProperty == null)
            {
                return NotFound();
            }

            return View(landProperty);
        }

        // GET: LandProperties/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return View(new LandPropertyCreateEditViewModel());
        }

        // POST: LandProperties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LandPropertyCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var landProperty = new LandProperty
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    RegistrationNumber = model.RegistrationNumber,
                    County = model.County,
                    Parish = model.Parish,
                    Village = model.Village,
                    BoughtDate = model.BoughtDate,
                    SoldDate = model.SoldDate,
                    Status = model.Status,
                    CompanyId = model.CompanyId
                };

                _context.Add(landProperty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.CompanyId);
            return View(model);
        }

        // GET: LandProperties/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landProperty = await _context.LandProperties.FindAsync(id);
            if (landProperty == null)
            {
                return NotFound();
            }

            var model = new LandPropertyCreateEditViewModel
            {
                Id = landProperty.Id,
                Name = landProperty.Name,
                RegistrationNumber = landProperty.RegistrationNumber,
                County = landProperty.County,
                Parish = landProperty.Parish,
                Village = landProperty.Village,
                BoughtDate = landProperty.BoughtDate,
                SoldDate = landProperty.SoldDate,
                Status = landProperty.Status,
                CompanyId = landProperty.CompanyId
            };

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.CompanyId);
            return View(model);
        }

        // POST: LandProperties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, LandPropertyCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var landProperty = await _context.LandProperties.FindAsync(id);
                    if (landProperty == null)
                    {
                        return NotFound();
                    }

                    landProperty.Name = model.Name;
                    landProperty.RegistrationNumber = model.RegistrationNumber;
                    landProperty.County = model.County;
                    landProperty.Parish = model.Parish;
                    landProperty.Village = model.Village;
                    landProperty.BoughtDate = model.BoughtDate;
                    landProperty.SoldDate = model.SoldDate;
                    landProperty.Status = model.Status;
                    landProperty.CompanyId = model.CompanyId;

                    _context.Update(landProperty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LandPropertyExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.CompanyId);
            return View(model);
        }

        // GET: LandProperties/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landProperty = await _context.LandProperties
                .Include(l => l.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (landProperty == null)
            {
                return NotFound();
            }

            return View(landProperty);
        }

        // POST: LandProperties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var landProperty = await _context.LandProperties.FindAsync(id);
            if (landProperty != null)
            {
                _context.LandProperties.Remove(landProperty);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LandPropertyExists(Guid id)
        {
            return _context.LandProperties.Any(e => e.Id == id);
        }
    }
}
