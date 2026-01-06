using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class FriendDetailsModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<FriendsByCountryModel> _logger = null;

        public IFriend Friend { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Guid _firendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _service.ReadFriendAsync(_firendId, false)).Item;

            return Page();
        }

        public FriendDetailsModel(IFriendsService service, ILogger<FriendsByCountryModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}