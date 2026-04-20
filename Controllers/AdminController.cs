using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using BlindMatchPAS.Services.Requests;
using BlindMatchPAS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminController : Controller
{
    private readonly IResearchAreaService _researchAreaService;
    private readonly IMatchingService _matchingService;
    private readonly IAdminSupervisorAreaService _adminSupervisorAreaService;
    private readonly IAdminMatchOverrideService _adminMatchOverrideService;
    private readonly IAdminUserManagementService _adminUserManagementService;

    public AdminController(
        IResearchAreaService researchAreaService,
        IMatchingService matchingService,
        IAdminSupervisorAreaService adminSupervisorAreaService,
        IAdminMatchOverrideService adminMatchOverrideService,
        IAdminUserManagementService adminUserManagementService)
    {
        _researchAreaService = researchAreaService;
        _matchingService = matchingService;
        _adminSupervisorAreaService = adminSupervisorAreaService;
        _adminMatchOverrideService = adminMatchOverrideService;
        _adminUserManagementService = adminUserManagementService;
    }

    public async Task<IActionResult> ResearchAreas(CancellationToken cancellationToken)
    {
        var list = await _researchAreaService.GetAllAsync(cancellationToken);
        return View(list);
    }

    [HttpGet]
    public IActionResult CreateResearchArea()
    {
        return View(new ResearchAreaFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResearchArea(ResearchAreaFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _researchAreaService.CreateAsync(model.Name, model.Description, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create research area.");
            return View(model);
        }

        TempData["Success"] = "Research area created.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpGet]
    public async Task<IActionResult> EditResearchArea(int id, CancellationToken cancellationToken)
    {
        var area = await _researchAreaService.GetByIdAsync(id, cancellationToken);
        if (area is null)
            return NotFound();

        var model = new ResearchAreaFormViewModel
        {
            Id = area.Id,
            Name = area.Name,
            Description = area.Description
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResearchArea(ResearchAreaFormViewModel model, CancellationToken cancellationToken)
    {
        if (!model.Id.HasValue)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var result = await _researchAreaService.UpdateAsync(model.Id.Value, model.Name, model.Description, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not update research area.");
            return View(model);
        }

        TempData["Success"] = "Research area updated.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResearchArea(int id, CancellationToken cancellationToken)
    {
        var result = await _researchAreaService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error;
        else
            TempData["Success"] = "Research area deleted.";

        return RedirectToAction(nameof(ResearchAreas));
    }

    public async Task<IActionResult> Matches(CancellationToken cancellationToken)
    {
        var dashboard = await _matchingService.GetAdminMatchesDashboardAsync(cancellationToken);
        return View(dashboard);
    }

    public async Task<IActionResult> SupervisorAreas(CancellationToken cancellationToken)
    {
        var list = await _adminSupervisorAreaService.GetSupervisorsWithAreaCountsAsync(cancellationToken);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> EditSupervisorAreas(string id, CancellationToken cancellationToken)
    {
        var model = await _adminSupervisorAreaService.GetEditModelAsync(id, cancellationToken);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSupervisorAreas(string id, int[]? selectedResearchAreaIds, CancellationToken cancellationToken)
    {
        var ids = selectedResearchAreaIds?.ToList() ?? new List<int>();
        var result = await _adminSupervisorAreaService.SetSupervisorResearchAreasAsync(id, ids, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(EditSupervisorAreas), new { id });
        }

        TempData["Success"] = "Research areas updated for this supervisor.";
        return RedirectToAction(nameof(SupervisorAreas));
    }

    public async Task<IActionResult> MatchIntervention(int id, CancellationToken cancellationToken)
    {
        var result = await _adminMatchOverrideService.GetInterventionModelAsync(id, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Matches));
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearMatch(int id, CancellationToken cancellationToken)
    {
        var result = await _adminMatchOverrideService.ClearMatchAsync(id, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(MatchIntervention), new { id });
        }

        TempData["Success"] = "Match cleared. The proposal is submitted again for blind review.";
        return RedirectToAction(nameof(MatchIntervention), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReassignMatch(int id, string? selectedSupervisorId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchOverrideService.ReassignSupervisorAsync(id, selectedSupervisorId ?? "", cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(MatchIntervention), new { id });
        }

        TempData["Success"] = "Supervisor reassigned.";
        return RedirectToAction(nameof(MatchIntervention), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceMatch(int id, string? selectedSupervisorId, CancellationToken cancellationToken)
    {
        var result = await _adminMatchOverrideService.ForceMatchAsync(id, selectedSupervisorId ?? "", cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(MatchIntervention), new { id });
        }

        TempData["Success"] = "Match created by administrator.";
        return RedirectToAction(nameof(MatchIntervention), new { id });
    }

    public async Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        var list = await _adminUserManagementService.ListUsersAsync(cancellationToken);
        return View(list);
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateAdminUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateAdminUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var req = new CreateAdminUserRequest
        {
            Email = model.Email,
            Password = model.Password,
            DisplayName = model.DisplayName,
            IsStudent = model.IsStudent,
            IsSupervisor = model.IsSupervisor,
            IsAdmin = model.IsAdmin
        };

        var result = await _adminUserManagementService.CreateUserAsync(req, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create user.");
            return View(model);
        }

        TempData["Success"] = "User created.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> EditUserRoles(string id, CancellationToken cancellationToken)
    {
        var result = await _adminUserManagementService.GetEditRolesModelAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
            return NotFound();

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUserRoles(EditUserRolesViewModel model, CancellationToken cancellationToken)
    {
        var desired = new HashSet<string>(StringComparer.Ordinal);
        if (model.IsStudent)
            desired.Add(RoleNames.Student);
        if (model.IsSupervisor)
            desired.Add(RoleNames.Supervisor);
        if (model.IsAdmin)
            desired.Add(RoleNames.Admin);

        var result = await _adminUserManagementService.SetUserRolesAsync(model.UserId, desired, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not update roles.");
            return View(model);
        }

        TempData["Success"] = "Roles updated.";
        return RedirectToAction(nameof(Users));
    }
}
