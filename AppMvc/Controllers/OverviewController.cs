using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

public class OverviewController : Controller
{
    private readonly IFriendsService _friendService;
    private readonly IAddressesService _addressService;
    private readonly ILogger<OverviewController> _logger;

    public OverviewController(IFriendsService friendService, IAddressesService addressService, ILogger<OverviewController> logger)
    {
        _friendService = friendService;
        _addressService = addressService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ShowOverview(OverviewViewModel.OverviewMode mode = OverviewViewModel.OverviewMode.FriendsOnly)
    {
        var vm = new OverviewViewModel();
        vm.Mode = mode;

        await LoadCountriesAsync(vm);

        return View("Overview", vm);
    }

    [HttpPost]
    public async Task<IActionResult> ExpandCountry(OverviewViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.ExpandedCountry))
            return View("Overview", vm);

        if (vm.ExpandedCountry == vm.PreviouslyExpandedCountry)
            vm.ExpandedCountry = null;

        await LoadCountriesAsync(vm);

        if (!string.IsNullOrEmpty(vm.ExpandedCountry))
            await LoadCitiesForCountry(vm);

        return View("Overview", vm);
    }

    private async Task LoadCountriesAsync(OverviewViewModel vm)
    {
        vm.AvalibleCountries = await _addressService.ReadAllCountriesAsync(vm.UseSeeds);

        var totalResp = await _friendService.ReadFriendsAsync(vm.UseSeeds, false, null, 0, 1);
        vm.NrOfFriends = totalResp.DbItemsCount;

        vm.CountryData.Clear();

        foreach (var country in vm.AvalibleCountries)
        {
            var resp = await _friendService.ReadFriendsAsync(vm.UseSeeds, false, country, 0, vm.NrOfFriends);

            int friendsCount = resp.PageItems.Count;
            int petsCount = resp.PageItems.Sum(f => f.Pets?.Count ?? 0);

            vm.CountryData.Add(country, (friendsCount, petsCount));
        }
    }

    private async Task LoadCitiesForCountry(OverviewViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.ExpandedCountry)) return;

        vm.ExpandedCities = await _addressService.ReadAllCitiesAsync(vm.UseSeeds, vm.ExpandedCountry);
        vm.CityData.Clear();

        foreach (var city in vm.ExpandedCities)
        {
            var resp = await _friendService.ReadFriendsAsync(vm.UseSeeds, false, city, 0, vm.NrOfFriends);

            int friendsCount = resp.PageItems.Count;
            int petsCount = resp.PageItems.Sum(f => f.Pets?.Count ?? 0);

            vm.CityData.Add(city, (friendsCount, petsCount));
        }
    }
}