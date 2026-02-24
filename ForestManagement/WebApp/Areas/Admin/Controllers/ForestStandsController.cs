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
    public class ForestStandsController : Controller
    {
        private readonly AppDbContext _context;

        public ForestStandsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ForestStands
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ForestStands.Include(f => f.Cadaster);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ForestStands/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forestStand = await _context.ForestStands
                .Include(f => f.Cadaster)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (forestStand == null)
            {
                return NotFound();
            }

            return View(forestStand);
        }

        // GET: ForestStands/Create
        public IActionResult Create()
        {
            ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber");
            return View(new ForestStandCreateEditViewModel());
        }

        // POST: ForestStands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForestStandCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate CadasterId is not empty
                if (model.CadasterId == Guid.Empty)
                {
                    ModelState.AddModelError("CadasterId", "Please select a cadaster.");
                    ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber");
                    return View(model);
                }

                // Verify the cadaster exists
                var cadasterExists = await _context.Cadasters.AnyAsync(c => c.Id == model.CadasterId);
                if (!cadasterExists)
                {
                    ModelState.AddModelError("CadasterId", "Selected cadaster does not exist.");
                    ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber");
                    return View(model);
                }

                var forestStand = new ForestStand
                {
                    Id = Guid.NewGuid(),
                    Number = model.Number,
                    Area = model.Area,
                    TotalVolume = model.TotalVolume,
                    IsActive = model.IsActive,
                    ValidFrom = model.ValidFrom,
                    ValidTo = model.ValidTo,
                    CadasterId = model.CadasterId
                };

                _context.Add(forestStand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber", model.CadasterId);
            return View(model);
        }

        // GET: ForestStands/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forestStand = await _context.ForestStands.FindAsync(id);
            if (forestStand == null)
            {
                return NotFound();
            }

            var model = new ForestStandCreateEditViewModel
            {
                Id = forestStand.Id,
                Number = forestStand.Number,
                Area = forestStand.Area,
                TotalVolume = forestStand.TotalVolume,
                IsActive = forestStand.IsActive,
                ValidFrom = forestStand.ValidFrom,
                ValidTo = forestStand.ValidTo,
                CadasterId = forestStand.CadasterId
            };

            ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber", forestStand.CadasterId);
            return View(model);
        }

        // POST: ForestStands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ForestStandCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validate CadasterId is not empty
                if (model.CadasterId == Guid.Empty)
                {
                    ModelState.AddModelError("CadasterId", "Please select a cadaster.");
                    ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber", model.CadasterId);
                    return View(model);
                }

                // Verify the cadaster exists
                var cadasterExists = await _context.Cadasters.AnyAsync(c => c.Id == model.CadasterId);
                if (!cadasterExists)
                {
                    ModelState.AddModelError("CadasterId", "Selected cadaster does not exist.");
                    ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber", model.CadasterId);
                    return View(model);
                }

                try
                {
                    var forestStand = await _context.ForestStands.FindAsync(id);
                    if (forestStand == null)
                    {
                        return NotFound();
                    }

                    forestStand.Number = model.Number;
                    forestStand.Area = model.Area;
                    forestStand.TotalVolume = model.TotalVolume;
                    forestStand.IsActive = model.IsActive;
                    forestStand.ValidFrom = model.ValidFrom;
                    forestStand.ValidTo = model.ValidTo;
                    forestStand.CadasterId = model.CadasterId;

                    _context.Update(forestStand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ForestStandExists(model.Id))
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
            ViewData["CadasterId"] = new SelectList(_context.Cadasters, "Id", "CadastralNumber", model.CadasterId);
            return View(model);
        }

        // GET: ForestStands/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forestStand = await _context.ForestStands
                .Include(f => f.Cadaster)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (forestStand == null)
            {
                return NotFound();
            }

            return View(forestStand);
        }

        // POST: ForestStands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forestStand = await _context.ForestStands.FindAsync(id);
            if (forestStand != null)
            {
                _context.ForestStands.Remove(forestStand);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ForestStandExists(Guid id)
        {
            return _context.ForestStands.Any(e => e.Id == id);
        }
    }
}
