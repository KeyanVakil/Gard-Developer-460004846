using GardPortal.Models;
using GardPortal.Services;
using GardPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GardPortal.Controllers;

public class VesselsController : Controller
{
    private readonly IVesselService _vesselService;

    public VesselsController(IVesselService vesselService) => _vesselService = vesselService;

    public async Task<IActionResult> Index(int page = 1, string? sortBy = null)
    {
        var (items, total) = await _vesselService.GetAllAsync(page, 20, sortBy);
        var vm = new VesselListViewModel
        {
            Vessels    = items,
            TotalCount = total,
            Page       = page,
            PageSize   = 20,
            SortBy     = sortBy,
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vessel = await _vesselService.GetByIdAsync(id);
        if (vessel is null) return NotFound();
        return View(vessel);
    }

    public IActionResult Create() => View(new Vessel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vessel vessel)
    {
        if (!ModelState.IsValid) return View(vessel);
        await _vesselService.CreateAsync(vessel);
        TempData["Success"] = $"Vessel '{vessel.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vessel = await _vesselService.GetByIdAsync(id);
        if (vessel is null) return NotFound();
        return View(vessel);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vessel vessel)
    {
        if (id != vessel.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vessel);
        try
        {
            await _vesselService.UpdateAsync(vessel);
            TempData["Success"] = $"Vessel '{vessel.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vessel = await _vesselService.GetByIdAsync(id);
        if (vessel is null) return NotFound();
        return View(vessel);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _vesselService.DeleteAsync(id);
            TempData["Success"] = "Vessel deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}
