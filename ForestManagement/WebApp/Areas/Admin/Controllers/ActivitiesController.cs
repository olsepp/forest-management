using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using App.DAL.UnitOfWork;
using App.Domain;
using App.Domain.Identity;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActivitiesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public ActivitiesController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: Activities
        public async Task<IActionResult> Index()
        {
            var activities = await _unitOfWork.Activities.GetAllAsync();
            return View(activities);
        }

        // GET: Activities/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _unitOfWork.Activities.GetByIdAsync(id.Value);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // GET: Activities/Create
        public async Task<IActionResult> Create()
        {
            // Get all activity types
            var activityTypes = await _unitOfWork.ActivityTypes.GetAllAsync();
            
            // Get all forest stands
            var forestStands = await _unitOfWork.ForestStands.GetAllAsync();
            
            // Get all cadasters
            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();

            ViewData["ActivityTypeId"] = new SelectList(activityTypes, "Id", "ActivityTypeName");
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "Number");
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber");
            
            return View(new ActivityCreateEditViewModel { Date = DateTime.UtcNow });
        }

        // POST: Activities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityCreateEditViewModel model)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                ModelState.AddModelError("", "User not found. Please log in.");
                return View(model);
            }
            
            model.UserId = currentUser.Id;

            // Validate mutual exclusivity
            if (model.ForestStandId.HasValue && model.CadasterId.HasValue)
            {
                ModelState.AddModelError("", "Please select either a Forest Stand or a Cadaster, not both.");
            }
            else if (!model.ForestStandId.HasValue && !model.CadasterId.HasValue)
            {
                ModelState.AddModelError("", "Please select either a Forest Stand or a Cadaster.");
            }

            if (ModelState.IsValid)
            {
                var activity = new Activity
                {
                    Id = Guid.NewGuid(),
                    Description = model.Description,
                    Quantity = model.Quantity,
                    Unit = model.Unit,
                    Notes = model.Notes,
                    Date = model.Date,
                    UserId = model.UserId,
                    ActivityTypeId = model.ActivityTypeId,
                    ForestStandId = model.ForestStandId,
                    CadasterId = model.CadasterId,
                    ApplicationStatus = model.ApplicationStatus
                };

                await _unitOfWork.Activities.AddAsync(activity);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns on validation failure
            var activityTypes = await _unitOfWork.ActivityTypes.GetAllAsync();
            var forestStands = await _unitOfWork.ForestStands.GetAllAsync();
            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();

            ViewData["ActivityTypeId"] = new SelectList(activityTypes, "Id", "ActivityTypeName", model.ActivityTypeId);
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "Number", model.ForestStandId);
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber", model.CadasterId);
            
            return View(model);
        }

        // GET: Activities/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _unitOfWork.Activities.GetByIdAsync(id.Value);
            if (activity == null)
            {
                return NotFound();
            }

            var model = new ActivityCreateEditViewModel
            {
                Id = activity.Id,
                Description = activity.Description,
                Quantity = activity.Quantity,
                Unit = activity.Unit,
                Notes = activity.Notes,
                Date = activity.Date,
                UserId = activity.UserId,
                ActivityTypeId = activity.ActivityTypeId,
                ForestStandId = activity.ForestStandId,
                CadasterId = activity.CadasterId,
                ApplicationStatus = activity.ApplicationStatus
            };

            // Repopulate dropdowns
            var activityTypes = await _unitOfWork.ActivityTypes.GetAllAsync();
            var forestStands = await _unitOfWork.ForestStands.GetAllAsync();
            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();

            ViewData["ActivityTypeId"] = new SelectList(activityTypes, "Id", "ActivityTypeName", activity.ActivityTypeId);
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "Number", activity.ForestStandId);
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber", activity.CadasterId);
            
            return View(model);
        }

        // POST: Activities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ActivityCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Validate mutual exclusivity
            if (model.ForestStandId.HasValue && model.CadasterId.HasValue)
            {
                ModelState.AddModelError("", "Please select either a Forest Stand or a Cadaster, not both.");
            }
            else if (!model.ForestStandId.HasValue && !model.CadasterId.HasValue)
            {
                ModelState.AddModelError("", "Please select either a Forest Stand or a Cadaster.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var activity = await _unitOfWork.Activities.GetByIdAsync(id);
                    if (activity == null)
                    {
                        return NotFound();
                    }

                    activity.Description = model.Description;
                    activity.Quantity = model.Quantity;
                    activity.Unit = model.Unit;
                    activity.Notes = model.Notes;
                    activity.Date = model.Date;
                    activity.ActivityTypeId = model.ActivityTypeId;
                    activity.ForestStandId = model.ForestStandId;
                    activity.CadasterId = model.CadasterId;
                    activity.ApplicationStatus = model.ApplicationStatus;

                    await _unitOfWork.Activities.UpdateAsync(activity);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!await ActivityExists(model.Id))
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

            // Repopulate dropdowns on validation failure
            var activityTypes = await _unitOfWork.ActivityTypes.GetAllAsync();
            var forestStands = await _unitOfWork.ForestStands.GetAllAsync();
            var cadasters = await _unitOfWork.Cadasters.GetAllAsync();

            ViewData["ActivityTypeId"] = new SelectList(activityTypes, "Id", "ActivityTypeName", model.ActivityTypeId);
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "Number", model.ForestStandId);
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "CadastralNumber", model.CadasterId);
            
            return View(model);
        }

        // GET: Activities/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _unitOfWork.Activities.GetByIdAsync(id.Value);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _unitOfWork.Activities.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ActivityExists(Guid id)
        {
            return await _unitOfWork.Activities.ExistsAsync(id);
        }
    }
}
