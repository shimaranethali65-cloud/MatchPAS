using BlindMatchPAS.Extensions;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = RoleNames.Supervisor)]
public sealed class SupervisorController : Controller
{
    private readonly IMatchingService _matchingService;
    private readonly IResearchAreaService _researchAreaService;

    public SupervisorController(IMatchingService matchingService, IResearchAreaService researchAreaService)
    {
        _matchingService = matchingService;
        _researchAreaService = researchAreaService;
    }

    public async Task<IActionResult> Index(int? researchAreaId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var filterAreas = await _researchAreaService.GetForSupervisorAsync(userId, cancellationToken);
        var proposals = await _matchingService.ListAnonymousProposalsAsync(userId, researchAreaId, cancellationToken);
        ViewBag.ResearchAreaFilter = researchAreaId;
        ViewBag.SupervisorResearchAreas = filterAreas;
        return View(proposals);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var result = await _matchingService.GetAnonymousProposalForSupervisorAsync(userId, id, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var result = await _matchingService.ConfirmMatchAsync(userId, id, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Match confirmed. Student identity is now visible to you on this proposal.";
        return RedirectToAction(nameof(MatchedDetails), new { id });
    }

    public async Task<IActionResult> MatchedDetails(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var result = await _matchingService.GetMatchedProposalForSupervisorAsync(userId, id, cancellationToken);
        if (!result.Success)
            return NotFound();

        return View(result.Data);
    }
}
