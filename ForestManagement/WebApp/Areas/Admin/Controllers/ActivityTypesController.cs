using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.DAL.UnitOfWork;
using App.Domain;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActivityTypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivityTypesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: ActivityTypes
        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.ActivityTypes.GetAllAsync());
        }

        // GET: ActivityTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityType = await _unitOfWork.ActivityTypes.GetByIdAsync(id.Value);
            if (activityType == null)
            {
                return NotFound();
            }

            return View(activityType);
        }

        // GET: ActivityTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ActivityTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ActivityTypeName,Id")] ActivityType activityType)
        {
            if (ModelState.IsValid)
            {
                activityType.Id = Guid.NewGuid();
                await _unitOfWork.ActivityTypes.AddAsync(activityType);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(activityType);
        }

        // GET: ActivityTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityType = await _unitOfWork.ActivityTypes.GetByIdAsync(id.Value);
            if (activityType == null)
            {
                return NotFound();
            }
            return View(activityType);
        }

        // POST: ActivityTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ActivityTypeName,Id")] ActivityType activityType)
        {
            if (id != activityType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.ActivityTypes.UpdateAsync(activityType);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!await ActivityTypeExists(activityType.Id))
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
            return View(activityType);
        }

        // GET: ActivityTypes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityType = await _unitOfWork.ActivityTypes.GetByIdAsync(id.Value);
            if (activityType == null)
            {
                return NotFound();
            }

            return View(activityType);
        }

        // POST: ActivityTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _unitOfWork.ActivityTypes.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ActivityTypeExists(Guid id)
        {
            return await _unitOfWork.ActivityTypes.ExistsAsync(id);
        }
    }
}
