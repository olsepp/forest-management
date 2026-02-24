using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActivitiesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ActivitiesController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Activities
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Activities
                .Include(a => a.ActivityType)
                .Include(a => a.Cadaster)
                .Include(a => a.ForestStand)
                .Include(a => a.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Activities/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _context.Activities
                .Include(a => a.ActivityType)
                .Include(a => a.Cadaster)
                .Include(a => a.ForestStand)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // GET: Activities/Create
        public IActionResult Create()
        {
            // Get ForestStands filtered by Active status properties through Cadaster -> LandProperty
            var forestStands = _context.ForestStands
                .Include(f => f.Cadaster)
                    .ThenInclude(c => c!.LandProperty)
                .Where(f => f.Cadaster != null && 
                           f.Cadaster.LandProperty != null && 
                           f.Cadaster.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(f => new 
                {
                    Id = f.Id,
                    DisplayName = $"#{f.Number} - {f.Cadaster!.CadastralNumber} - {f.Cadaster.LandProperty!.Name}"
                })
                .OrderBy(f => f.DisplayName)
                .ToList();

            // Get Cadasters filtered by Active status properties
            var cadasters = _context.Cadasters
                .Include(c => c.LandProperty)
                .Where(c => c.LandProperty != null && c.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(c => new
                {
                    Id = c.Id,
                    DisplayName = $"{c.CadastralNumber} - {c.LandProperty!.Name}"
                })
                .OrderBy(c => c.DisplayName)
                .ToList();

            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityTypes, "Id", "ActivityTypeName");
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "DisplayName");
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "DisplayName");
            
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

                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns on validation failure
            var forestStands = _context.ForestStands
                .Include(f => f.Cadaster)
                    .ThenInclude(c => c!.LandProperty)
                .Where(f => f.Cadaster != null && 
                           f.Cadaster.LandProperty != null && 
                           f.Cadaster.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(f => new 
                {
                    Id = f.Id,
                    DisplayName = $"#{f.Number} - {f.Cadaster!.CadastralNumber} - {f.Cadaster.LandProperty!.Name}"
                })
                .OrderBy(f => f.DisplayName)
                .ToList();

            var cadasters = _context.Cadasters
                .Include(c => c.LandProperty)
                .Where(c => c.LandProperty != null && c.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(c => new
                {
                    Id = c.Id,
                    DisplayName = $"{c.CadastralNumber} - {c.LandProperty!.Name}"
                })
                .OrderBy(c => c.DisplayName)
                .ToList();

            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityTypes, "Id", "ActivityTypeName", model.ActivityTypeId);
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "DisplayName", model.ForestStandId);
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "DisplayName", model.CadasterId);
            
            return View(model);
        }

        // GET: Activities/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _context.Activities.FindAsync(id);
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
            var forestStands = _context.ForestStands
                .Include(f => f.Cadaster)
                    .ThenInclude(c => c!.LandProperty)
                .Where(f => f.Cadaster != null && 
                           f.Cadaster.LandProperty != null && 
                           f.Cadaster.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(f => new 
                {
                    Id = f.Id,
                    DisplayName = $"#{f.Number} - {f.Cadaster!.CadastralNumber} - {f.Cadaster.LandProperty!.Name}"
                })
                .OrderBy(f => f.DisplayName)
                .ToList();

            var cadasters = _context.Cadasters
                .Include(c => c.LandProperty)
                .Where(c => c.LandProperty != null && c.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(c => new
                {
                    Id = c.Id,
                    DisplayName = $"{c.CadastralNumber} - {c.LandProperty!.Name}"
                })
                .OrderBy(c => c.DisplayName)
                .ToList();

            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityTypes, "Id", "ActivityTypeName", activity.ActivityTypeId);
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "DisplayName", activity.ForestStandId);
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "DisplayName", activity.CadasterId);
            
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
                    var activity = await _context.Activities.FindAsync(id);
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

                    _context.Update(activity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityExists(model.Id))
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
            var forestStands = _context.ForestStands
                .Include(f => f.Cadaster)
                    .ThenInclude(c => c!.LandProperty)
                .Where(f => f.Cadaster != null && 
                           f.Cadaster.LandProperty != null && 
                           f.Cadaster.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(f => new 
                {
                    Id = f.Id,
                    DisplayName = $"#{f.Number} - {f.Cadaster!.CadastralNumber} - {f.Cadaster.LandProperty!.Name}"
                })
                .OrderBy(f => f.DisplayName)
                .ToList();

            var cadasters = _context.Cadasters
                .Include(c => c.LandProperty)
                .Where(c => c.LandProperty != null && c.LandProperty.Status == EPropertyStatus.Active)
                .ToList() // Force client evaluation to avoid EF Core translation issues
                .Select(c => new
                {
                    Id = c.Id,
                    DisplayName = $"{c.CadastralNumber} - {c.LandProperty!.Name}"
                })
                .OrderBy(c => c.DisplayName)
                .ToList();

            ViewData["ActivityTypeId"] = new SelectList(_context.ActivityTypes, "Id", "ActivityTypeName", model.ActivityTypeId);
            ViewData["ForestStandId"] = new SelectList(forestStands, "Id", "DisplayName", model.ForestStandId);
            ViewData["CadasterId"] = new SelectList(cadasters, "Id", "DisplayName", model.CadasterId);
            
            return View(model);
        }

        // GET: Activities/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activity = await _context.Activities
                .Include(a => a.ActivityType)
                .Include(a => a.Cadaster)
                .Include(a => a.ForestStand)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivityExists(Guid id)
        {
            return _context.Activities.Any(e => e.Id == id);
        }
    }
}
