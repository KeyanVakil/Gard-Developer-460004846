using GardPortal.Data;
using GardPortal.Models;
using GardPortal.Services;
using GardPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Controllers;

public class PoliciesController : Controller
{
    private readonly IPolicyService _policyService;
    private readonly GardDbContext _db;

    public PoliciesController(IPolicyService policyService, GardDbContext db)
    {
        _policyService = policyService;
        _db = db;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        PolicyStatus? status = null,
        CoverageType? coverageType = null,
        string? vesselName = null)
    {
        var (items, total) = await _policyService.GetAllAsync(page, 20, status, coverageType, vesselName);
        ViewBag.Status       = status;
        ViewBag.CoverageType = coverageType;
        ViewBag.VesselName   = vesselName;
        ViewBag.TotalCount   = total;
        ViewBag.Page         = page;
        ViewBag.TotalPages   = (int)Math.Ceiling((double)total / 20);
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var policy = await _policyService.GetByIdAsync(id);
        if (policy is null) return NotFound();
        return View(policy);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new PolicyFormViewModel
        {
            Vessels = await GetVesselSelectList(),
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PolicyFormViewModel vm)
    {
        if (vm.EndDate <= vm.StartDate)
            ModelState.AddModelError("EndDate", "End date must be after start date.");

        if (!ModelState.IsValid)
        {
            vm.Vessels = await GetVesselSelectList();
            return View(vm);
        }

        var policy = new Policy
        {
            VesselId     = vm.VesselId,
            CoverageType = vm.CoverageType,
            StartDate    = vm.StartDate,
            EndDate      = vm.EndDate,
            InsuredValue = vm.InsuredValue,
            Notes        = vm.Notes,
        };

        await _policyService.CreateAsync(policy);
        TempData["Success"] = $"Policy '{policy.PolicyNumber}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(int id, PolicyStatus newStatus)
    {
        try
        {
            await _policyService.TransitionStatusAsync(id, newStatus);
            TempData["Success"] = $"Policy status updated to {newStatus}.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<List<SelectListItem>> GetVesselSelectList()
        => await _db.Vessels
            .OrderBy(v => v.Name)
            .Select(v => new SelectListItem(v.Name + " (IMO: " + v.ImoNumber + ")", v.Id.ToString()))
            .ToListAsync();
}
