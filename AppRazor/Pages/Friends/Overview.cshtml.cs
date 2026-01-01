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
        public int NrOfFriends {get; set;}
        public List<string> AvalibleCountries { get; set; } = new();
        public Dictionary<string,(int FriendsCount, int PetsCount)> CountryData { get; set; } = new();
        public async Task<IActionResult> OnGet(OverviewMode mode)
        {
            Mode = mode;
            AvalibleCountries = await _addressService.ReadAllCountriesAsync(UseSeeds);

            var totalResp = await _friendService.ReadFriendsAsync(UseSeeds, false, null, 0, 1);
            int totalItems = totalResp.DbItemsCount;

            foreach (var country in AvalibleCountries)
            {
                var resp = await _friendService.ReadFriendsAsync(UseSeeds, false, country, 0, totalItems);
                
                int friendsCount = resp.PageItems.Count;
                int petsCount = resp.PageItems.Sum(f => f.Pets != null ? f.Pets.Count : 0);
                CountryData.Add(country, (friendsCount, petsCount));
            }
            return Page();
        }
        public OverviewModel(IFriendsService friendService, IAddressesService addressService ,ILogger<SeedModel> logger)
        {
            _friendService = friendService;
            _addressService = addressService;
            _logger = logger;
        }
    }
}