using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.DAL.UnitOfWork;
using App.Domain;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CadastersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CadastersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Cadasters
        public async Task<IActionResult> Index()
        {
            // Get all cadasters - for eager loading we need the repository method
            // Using GetAllAsync for now, can add GetAllWithLandPropertyAsync if needed
            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
            return View(cadasters);
        }

        // GET: Cadasters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cadaster = await _unitOfWork.Cadasters.GetByIdAsync(id.Value);
            if (cadaster == null)
            {
                return NotFound();
            }

            return View(cadaster);
        }

        // GET: Cadasters/Create
        public async Task<IActionResult> Create()
        {
            var properties = await _unitOfWork.LandProperties.GetAllAsync();
            ViewData["PropertyId"] = new SelectList(properties, "Id", "Name");
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
                    var properties = await _unitOfWork.LandProperties.GetAllAsync();
                    ViewData["PropertyId"] = new SelectList(properties, "Id", "Name");
                    return View(model);
                }

                // Verify the property exists
                var propertyExists = await _unitOfWork.LandProperties.ExistsAsync(model.LandPropertyId);
                if (!propertyExists)
                {
                    ModelState.AddModelError("PropertyId", "Selected property does not exist.");
                    var properties = await _unitOfWork.LandProperties.GetAllAsync();
                    ViewData["PropertyId"] = new SelectList(properties, "Id", "Name");
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

                await _unitOfWork.Cadasters.AddAsync(cadaster);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var properties2 = await _unitOfWork.LandProperties.GetAllAsync();
            ViewData["PropertyId"] = new SelectList(properties2, "Id", "Name", model.LandPropertyId);
            return View(model);
        }

        // GET: Cadasters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cadaster = await _unitOfWork.Cadasters.GetByIdAsync(id.Value);
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

            var properties = await _unitOfWork.LandProperties.GetAllAsync();
            ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", cadaster.LandPropertyId);
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
                    var properties = await _unitOfWork.LandProperties.GetAllAsync();
                    ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", model.LandPropertyId);
                    return View(model);
                }

                // Verify the property exists
                var propertyExists = await _unitOfWork.LandProperties.ExistsAsync(model.LandPropertyId);
                if (!propertyExists)
                {
                    ModelState.AddModelError("PropertyId", "Selected property does not exist.");
                    var properties = await _unitOfWork.LandProperties.GetAllAsync();
                    ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", model.LandPropertyId);
                    return View(model);
                }

                try
                {
                    var cadaster = await _unitOfWork.Cadasters.GetByIdAsync(id);
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

                    await _unitOfWork.Cadasters.UpdateAsync(cadaster);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!await CadasterExists(model.Id))
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
            var properties2 = await _unitOfWork.LandProperties.GetAllAsync();
            ViewData["PropertyId"] = new SelectList(properties2, "Id", "Name", model.LandPropertyId);
            return View(model);
        }

        // GET: Cadasters/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cadaster = await _unitOfWork.Cadasters.GetByIdAsync(id.Value);
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
            await _unitOfWork.Cadasters.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CadasterExists(Guid id)
        {
            return await _unitOfWork.Cadasters.ExistsAsync(id);
        }
    }
}
