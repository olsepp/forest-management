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
    public class CadastersController : Controller
    {
        private readonly AppDbContext _context;

        public CadastersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cadasters
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Cadasters.Include(c => c.LandProperty);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Cadasters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cadaster = await _context.Cadasters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cadaster == null)
            {
                return NotFound();
            }

            return View(cadaster);
        }

        // GET: Cadasters/Create
        public IActionResult Create()
        {
            ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name");
            return View(new CadasterCreateEditViewModel());
        }

        // POST: Cadasters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CadasterCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate PropertyId is not empty
                if (model.LandPropertyId == Guid.Empty)
                {
                    ModelState.AddModelError("PropertyId", "Please select a property.");
                    ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name");
                    return View(model);
                }

                // Verify the property exists
                var propertyExists = await _context.LandProperties.AnyAsync(p => p.Id == model.LandPropertyId);
                if (!propertyExists)
                {
                    ModelState.AddModelError("PropertyId", "Selected property does not exist.");
                    ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name");
                    return View(model);
                }

                var cadaster = new Cadaster
                {
                    Id = Guid.NewGuid(),
                    CadastralNumber = model.CadastralNumber,
                    ForestArea = model.ForestArea,
                    ArableArea = model.ArableArea,
                    GrasslandArea = model.GrasslandArea,
                    YardArea = model.YardArea,
                    BuildingFootprintArea = model.BuildingFootprintArea,
                    UnderwaterArea = model.UnderwaterArea,
                    OtherArea = model.OtherArea,
                    SoilQualityIndex = model.SoilQualityIndex,
                    CalculatedVolume = model.CalculatedVolume,
                    VolumeGrowth = model.VolumeGrowth,
                    LandPropertyId = model.LandPropertyId
                };

                _context.Add(cadaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name", model.LandPropertyId);
            return View(model);
        }

        // GET: Cadasters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cadaster = await _context.Cadasters.FindAsync(id);
            if (cadaster == null)
            {
                return NotFound();
            }

            var model = new CadasterCreateEditViewModel
            {
                Id = cadaster.Id,
                CadastralNumber = cadaster.CadastralNumber,
                ForestArea = cadaster.ForestArea,
                ArableArea = cadaster.ArableArea,
                GrasslandArea = cadaster.GrasslandArea,
                YardArea = cadaster.YardArea,
                BuildingFootprintArea = cadaster.BuildingFootprintArea,
                UnderwaterArea = cadaster.UnderwaterArea,
                OtherArea = cadaster.OtherArea,
                SoilQualityIndex = cadaster.SoilQualityIndex,
                CalculatedVolume = cadaster.CalculatedVolume,
                VolumeGrowth = cadaster.VolumeGrowth,
                LandPropertyId = cadaster.LandPropertyId
            };

            ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name", cadaster.LandPropertyId);
            return View(model);
        }

        // POST: Cadasters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CadasterCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validate PropertyId is not empty
                if (model.LandPropertyId == Guid.Empty)
                {
                    ModelState.AddModelError("PropertyId", "Please select a property.");
                    ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name", model.LandPropertyId);
                    return View(model);
                }

                // Verify the property exists
                var propertyExists = await _context.LandProperties.AnyAsync(p => p.Id == model.LandPropertyId);
                if (!propertyExists)
                {
                    ModelState.AddModelError("PropertyId", "Selected property does not exist.");
                    ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name", model.LandPropertyId);
                    return View(model);
                }

                try
                {
                    var cadaster = await _context.Cadasters.FindAsync(id);
                    if (cadaster == null)
                    {
                        return NotFound();
                    }

                    cadaster.CadastralNumber = model.CadastralNumber;
                    cadaster.ForestArea = model.ForestArea;
                    cadaster.ArableArea = model.ArableArea;
                    cadaster.GrasslandArea = model.GrasslandArea;
                    cadaster.YardArea = model.YardArea;
                    cadaster.BuildingFootprintArea = model.BuildingFootprintArea;
                    cadaster.UnderwaterArea = model.UnderwaterArea;
                    cadaster.OtherArea = model.OtherArea;
                    cadaster.SoilQualityIndex = model.SoilQualityIndex;
                    cadaster.CalculatedVolume = model.CalculatedVolume;
                    cadaster.VolumeGrowth = model.VolumeGrowth;
                    cadaster.LandPropertyId = model.LandPropertyId;

                    _context.Update(cadaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CadasterExists(model.Id))
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
            ViewData["PropertyId"] = new SelectList(_context.LandProperties, "Id", "Name", model.LandPropertyId);
            return View(model);
        }

        // GET: Cadasters/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cadaster = await _context.Cadasters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cadaster == null)
            {
                return NotFound();
            }

            return View(cadaster);
        }

        // POST: Cadasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cadaster = await _context.Cadasters.FindAsync(id);
            if (cadaster != null)
            {
                _context.Cadasters.Remove(cadaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CadasterExists(Guid id)
        {
            return _context.Cadasters.Any(e => e.Id == id);
        }
    }
}
