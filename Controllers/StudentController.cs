using BlindMatchPAS.Extensions;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using BlindMatchPAS.Services.Requests;
using BlindMatchPAS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = RoleNames.Student)]
public sealed class StudentController : Controller
{
    private readonly IProposalService _proposalService;
    private readonly IResearchAreaService _researchAreaService;

    public StudentController(IProposalService proposalService, IResearchAreaService researchAreaService)
    {
        _proposalService = proposalService;
        _researchAreaService = researchAreaService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var proposals = await _proposalService.ListForStudentAsync(userId, cancellationToken);
        return View(proposals);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var proposal = await _proposalService.GetOwnedProposalAsync(id, userId, cancellationToken);
        if (proposal is null)
            return NotFound();

        return View(proposal);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var areas = await _researchAreaService.GetAllAsync(cancellationToken);
        var model = new StudentProposalFormViewModel { ResearchAreas = areas.ToList() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentProposalFormViewModel model, CancellationToken cancellationToken)
    {
        var areas = await _researchAreaService.GetAllAsync(cancellationToken);
        model.ResearchAreas = areas.ToList();

        if (!ModelState.IsValid)
            return View(model);

        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var request = new CreateProposalRequest
        {
            ResearchAreaId = model.ResearchAreaId,
            Title = model.Title,
            Abstract = model.Abstract,
            TechStack = model.TechStack
        };

        var result = await _proposalService.CreateAsync(userId, request, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create proposal.");
            return View(model);
        }

        TempData["Success"] = "Proposal created as draft.";
        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var proposal = await _proposalService.GetOwnedProposalAsync(id, userId, cancellationToken);
        if (proposal is null)
            return NotFound();

        if (proposal.Status is ProposalStatus.Matched or ProposalStatus.Withdrawn)
        {
            TempData["Error"] = "This proposal can no longer be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var areas = await _researchAreaService.GetAllAsync(cancellationToken);
        var model = new StudentProposalFormViewModel
        {
            Id = proposal.Id,
            ResearchAreaId = proposal.ResearchAreaId,
            Title = proposal.Title,
            Abstract = proposal.Abstract,
            TechStack = proposal.TechStack,
            ResearchAreas = areas.ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentProposalFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
            return BadRequest();

        var areas = await _researchAreaService.GetAllAsync(cancellationToken);
        model.ResearchAreas = areas.ToList();

        if (!ModelState.IsValid)
            return View(model);

        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var request = new UpdateProposalRequest
        {
            ResearchAreaId = model.ResearchAreaId,
            Title = model.Title,
            Abstract = model.Abstract,
            TechStack = model.TechStack
        };

        var result = await _proposalService.UpdateAsync(id, userId, request, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not update proposal.");
            return View(model);
        }

        TempData["Success"] = "Proposal updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var result = await _proposalService.WithdrawAsync(id, userId, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Proposal withdrawn.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Challenge();

        var result = await _proposalService.SubmitAsync(id, userId, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Proposal submitted for supervisor review.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
