using GardPortal.Data;
using GardPortal.Models;
using GardPortal.Services;
using GardPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Controllers;

public class ClaimsController : Controller
{
    private readonly IClaimService _claimService;
    private readonly GardDbContext _db;

    public ClaimsController(IClaimService claimService, GardDbContext db)
    {
        _claimService = claimService;
        _db = db;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        ClaimStatus? status = null,
        ClaimCategory? category = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (items, total) = await _claimService.GetAllAsync(page, 20, status, category, from, to);
        ViewBag.Status     = status;
        ViewBag.Category   = category;
        ViewBag.From       = from;
        ViewBag.To         = to;
        ViewBag.TotalCount = total;
        ViewBag.Page       = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / 20);
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var claim = await _claimService.GetByIdAsync(id);
        if (claim is null) return NotFound();

        var nextStatuses = GetValidTransitions(claim.Status)
            .Select(s => new SelectListItem(s.ToString(), s.ToString()))
            .ToList();

        var vm = new ClaimDetailViewModel
        {
            Claim        = claim,
            NextStatuses = nextStatuses,
        };
        return View(vm);
    }

    public async Task<IActionResult> Create(int? policyId = null)
    {
        ViewBag.Policies = await GetActivePoliciesSelectList();
        ViewBag.SelectedPolicyId = policyId;
        return View(new Claim { PolicyId = policyId ?? 0, IncidentDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Claim claim)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Policies = await GetActivePoliciesSelectList();
            return View(claim);
        }

        try
        {
            await _claimService.CreateAsync(claim);
            TempData["Success"] = $"Claim '{claim.ClaimNumber}' filed.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Policies  = await GetActivePoliciesSelectList();
            return View(claim);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, ClaimStatus newStatus, string? notes)
    {
        try
        {
            if (newStatus == ClaimStatus.Settled)
            {
                TempData["Error"] = "Use the Settle action to settle a claim.";
                return RedirectToAction(nameof(Details), new { id });
            }
            await _claimService.TransitionStatusAsync(id, newStatus, notes);
            TempData["Success"] = $"Claim status updated to {newStatus}.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settle(int id, decimal settlementAmount, string? notes)
    {
        try
        {
            await _claimService.SettleAsync(id, settlementAmount, notes);
            TempData["Success"] = $"Claim settled for {settlementAmount:C2}.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private static IEnumerable<ClaimStatus> GetValidTransitions(ClaimStatus current) =>
        current switch
        {
            ClaimStatus.Reported    => new[] { ClaimStatus.UnderReview, ClaimStatus.Denied },
            ClaimStatus.UnderReview => new[] { ClaimStatus.Approved,    ClaimStatus.Denied },
            ClaimStatus.Approved    => new[] { ClaimStatus.Settled,     ClaimStatus.Denied },
            _ => Enumerable.Empty<ClaimStatus>(),
        };

    private async Task<List<SelectListItem>> GetActivePoliciesSelectList()
        => await _db.Policies
            .Include(p => p.Vessel)
            .Where(p => p.Status == PolicyStatus.Active)
            .OrderBy(p => p.PolicyNumber)
            .Select(p => new SelectListItem(p.PolicyNumber + " - " + p.Vessel.Name, p.Id.ToString()))
            .ToListAsync();
}
