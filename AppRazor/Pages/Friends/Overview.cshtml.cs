using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class OverviewModel : PageModel
    {
        readonly IFriendsService _friendService = null;
        readonly IAddressesService _addressService = null;
        readonly ILogger<SeedModel> _logger = null;

        public enum OverviewMode {FriendsOnly, FriendsAndPets};
        [BindProperty(SupportsGet = true)]
        public OverviewMode Mode {get; set;} = OverviewMode.FriendsOnly;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;
        [BindProperty]
        public int NrOfFriends {get; set;}
        [BindProperty]
        public string? ExpandedCountry { get; set; }
        [BindProperty]
        public string? PreviouslyExpandedCountry { get; set; }

        public List<string> AvalibleCountries { get; set; } = new();
        public List<string> ExpandedCities { get; set; } = new();
        public Dictionary<string,(int FriendsCount, int PetsCount)> CityData { get; set;} = new();
        public Dictionary<string,(int FriendsCount, int PetsCount)> CountryData { get; set; } = new();

        public async Task<IActionResult> OnGet(OverviewMode mode)
        {
            Mode = mode;

            await LoadCountriesAsync();

            return Page();
        }
        public async Task<IActionResult> OnPostExpandCountryAsync()
        {
            if (string.IsNullOrWhiteSpace(ExpandedCountry))
            return Page();

            if (ExpandedCountry == PreviouslyExpandedCountry) ExpandedCountry = null;

            await LoadCountriesAsync();

            await LoadCitiesForCountry(NrOfFriends);
            return Page();

        }
        private async Task LoadCountriesAsync()
        {
            AvalibleCountries = await _addressService.ReadAllCountriesAsync(UseSeeds);

            var totalResp = await _friendService.ReadFriendsAsync(UseSeeds, false, null, 0, 1);
            NrOfFriends = totalResp.DbItemsCount;

            foreach (var country in AvalibleCountries)
            {
                var resp = await _friendService.ReadFriendsAsync(UseSeeds, false, country, 0, NrOfFriends);
                
                int friendsCount = resp.PageItems.Count;
                int petsCount = resp.PageItems.Sum(f => f.Pets != null ? f.Pets.Count : 0);
                CountryData.Add(country, (friendsCount, petsCount));
            }
        }
        private async Task LoadCitiesForCountry(int totalItems)
        {

            ExpandedCities = await _addressService.ReadAllCitiesAsync(UseSeeds,ExpandedCountry);
            foreach(var city in ExpandedCities)
            {
                var resp = await _friendService.ReadFriendsAsync(UseSeeds, false, city, 0, totalItems);

                int friendsCount = resp.PageItems.Count;
                int petsCount = resp.PageItems.Sum(f => f.Pets != null ? f.Pets.Count : 0);
                CityData.Add(city, (friendsCount, petsCount));
            }
        }
        public OverviewModel(IFriendsService friendService, IAddressesService addressService ,ILogger<SeedModel> logger)
        {
            _friendService = friendService;
            _addressService = addressService;
            _logger = logger;
        }
    }
}