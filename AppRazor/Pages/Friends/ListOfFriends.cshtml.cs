using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class FriendsByCountryModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<FriendsByCountryModel> _logger = null;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;

        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        public int NrOfFriends { get; set; }

        //Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 10;

        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        [BindProperty]
        public string? SearchFilter { get; set; }

        public async Task<IActionResult> OnGet()
        {   
            if (int.TryParse(Request.Query["pagenr"], out int pagenr))
            {
                ThisPageNr = pagenr;
            }

            SearchFilter = Request.Query["search"];

            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friends = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            UpdatePagination(resp.DbItemsCount);

            return Page();
        }
        private void UpdatePagination(int nrOfItems)
        {
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }
        public async Task<IActionResult> OnPostSearch()
        {
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friends = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            UpdatePagination(resp.DbItemsCount);

            return Page();
        }
        public async Task<IActionResult> OnPostDeleteFriend(Guid friendId)
        {
            await _service.DeleteFriendAsync(friendId);

            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            Friends = resp.PageItems;
            NrOfFriends = resp.DbItemsCount;

            UpdatePagination(resp.DbItemsCount);

            return Page();
        }
        public FriendsByCountryModel(IFriendsService service, ILogger<FriendsByCountryModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}