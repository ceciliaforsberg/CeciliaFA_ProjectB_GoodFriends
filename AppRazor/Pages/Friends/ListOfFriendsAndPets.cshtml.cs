using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class FriendsAndPetsByCityModel : PageModel
    {
        readonly IFriendsService _friendService = null;
        readonly IAddressesService _addressService = null;
        readonly ILogger<FriendsAndPetsByCityModel> _logger = null;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;

        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        public int NrOfFriends { get; set; }
        public int NrOfPets { get; set; }

        //Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 10;

        [BindProperty (SupportsGet = true)]
        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        [BindProperty]
        public string? SearchFilter { get; set; }

        //For country choice 
        [BindProperty(SupportsGet = true)]
        public string? SelectedCountry { get; set; }
        public List<string> AvailableCountries { get; set; } = new();
        public bool HasSelectedCountry =>
            !string.IsNullOrWhiteSpace(SelectedCountry);

        public async Task OnGetAsync(int pageNr, string? search)
        {
            ThisPageNr = pageNr;
            SearchFilter = search;
            await LoadFriendsAsync();
        }
        public async Task LoadFriendsAsync()
        {
            AvailableCountries = await _addressService.ReadAllCountriesAsync(UseSeeds);

            //To get actual amount of friends in database for use in next step
            var totalResp = await _friendService.ReadFriendsAsync(UseSeeds, false, null, 0, 1);
            int totalItems = totalResp.DbItemsCount;

            //I am doing this to prevent changing service too much, this is to filter the friends in selectedCountry.
            //I load all friends and filter after, could also be done directly in service. 
            var resp = await _friendService.ReadFriendsAsync(UseSeeds, false, SearchFilter, 0, totalItems);
            var filteredFriends = resp.PageItems
                .Where(f => f.Address?.Country == SelectedCountry)
                .ToList();

            NrOfFriends = filteredFriends.Count;
            NrOfPets = filteredFriends.Sum(f => f.Pets?.Count ?? 0);

            //Needed for pagination to work on the filtered list of friends, since i filter the list outside of the service logic 
            Friends = filteredFriends
                .Skip(ThisPageNr * PageSize)
                .Take(PageSize)
                .ToList();

            UpdatePagination(NrOfFriends);
        }
        private void UpdatePagination(int nrOfItems)
        {
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }
        public FriendsAndPetsByCityModel(IFriendsService service, IAddressesService aService, ILogger<FriendsAndPetsByCityModel> logger)
        {
            _friendService = service;
            _addressService = aService;
            _logger = logger;
        }
    }
}