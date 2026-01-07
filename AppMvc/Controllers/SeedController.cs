using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Services.Interfaces;
using AppMvc.Models;

namespace AppMvc.Controllers;

public class SeedController : Controller
{
    readonly IAdminService _adminService = null;
    readonly ILogger<SeedController> _logger;

    public SeedController(IAdminService adminService, ILogger<SeedController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Seed()
    {
        var vm = new SeedViewModel{NrOfFriends = await GetNrOfFriends()};

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Seed(SeedViewModel vm)
    {
        if (ModelState.IsValid)
        {
            if (vm.RemoveSeeds)
            {
                await _adminService.RemoveSeedAsync(true);
                await _adminService.RemoveSeedAsync(false);
            }

            await _adminService.SeedAsync(vm.NrOfItemsToSeed);
            var info = await GetNrOfFriends();
            vm.Message = $"Seeding completed successfully! Friends added:";
            vm.NrOfFriends = info;

            return View(vm);
        }

        return View(vm);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<int> GetNrOfFriends()
    {
        var info = await _adminService.GuestInfoAsync();
        return info.Item.Db.NrSeededFriends + info.Item.Db.NrUnseededFriends;
    }
}