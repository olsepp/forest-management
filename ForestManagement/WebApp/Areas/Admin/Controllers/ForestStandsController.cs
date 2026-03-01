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
    public class ForestStandsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ForestStandsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: ForestStands
        public async Task<IActionResult> Index()
        {
            var forestStands = await _unitOfWork.ForestStands.GetAllAsync();
            return View(forestStands);
        }

        // GET: ForestStands/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forestStand = await _unitOfWork.ForestStands.GetByIdAsync(id.Value);
            if (forestStand == null)
            {
                return NotFound();
            }

            return View(forestStand);
        }

        // GET: ForestStands/Create
        public async Task<IActionResult> Create()
        {
            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber");
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
                    var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
                    ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber");
                    return View(model);
                }

                // Verify the cadaster exists
                var cadasterExists = await _unitOfWork.Cadasters.ExistsAsync(model.CadasterId);
                if (!cadasterExists)
                {
                    ModelState.AddModelError("CadasterId", "Selected cadaster does not exist.");
                    var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
                    ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber");
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

                await _unitOfWork.ForestStands.AddAsync(forestStand);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var cadasters2 = await _unitOfWork.Cadasters.GetAllAsync();
            ViewData["CadasterId"] = new SelectList(cadasters2, "Id", "CadastralNumber", model.CadasterId);
            return View(model);
        }

        // GET: ForestStands/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forestStand = await _unitOfWork.ForestStands.GetByIdAsync(id.Value);
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

            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber", forestStand.CadasterId);
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
                    var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
                    ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber", model.CadasterId);
                    return View(model);
                }

                // Verify the cadaster exists
                var cadasterExists = await _unitOfWork.Cadasters.ExistsAsync(model.CadasterId);
                if (!cadasterExists)
                {
                    ModelState.AddModelError("CadasterId", "Selected cadaster does not exist.");
                    var cadasters = await _unitOfWork.Cadasters.GetAllAsync();
                    ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber", model.CadasterId);
                    return View(model);
                }

                try
                {
                    var forestStand = await _unitOfWork.ForestStands.GetByIdAsync(id);
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

                    await _unitOfWork.ForestStands.UpdateAsync(forestStand);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!await ForestStandExists(model.Id))
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
            var cadasters2 = await _unitOfWork.Cadasters.GetAllAsync();
            ViewData["CadasterId"] = new SelectList(cadasters2, "Id", "CadastralNumber", model.CadasterId);
            return View(model);
        }

        // GET: ForestStands/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forestStand = await _unitOfWork.ForestStands.GetByIdAsync(id.Value);
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
            await _unitOfWork.ForestStands.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ForestStandExists(Guid id)
        {
            return await _unitOfWork.ForestStands.ExistsAsync(id);
        }
    }
}
