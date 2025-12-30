using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class SeedModel : PageModel
    {
        //Just like for WebApi
        readonly IAdminService _admin_service = null;
        readonly ILogger<SeedModel> _logger = null;

        public int NrOfFriends => nrOfFriends().Result;
        private async Task<int> nrOfFriends()
        {
            var info = await _admin_service.GuestInfoAsync();
            return info.Item.Db.NrSeededFriends + info.Item.Db.NrUnseededFriends;
        }

        [BindProperty]
        [Required (ErrorMessage = "You must enter nr of items to seed")]
        public int NrOfItemsToSeed { get; set; } = 100;

        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;

        [TempData]
        public string? Message { get; set; } = null;

        public IActionResult OnGet()
        {
            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (RemoveSeeds)
                {
                    await _admin_service.RemoveSeedAsync(true);
                    await _admin_service.RemoveSeedAsync(false);
                }
                await _admin_service.SeedAsync(NrOfItemsToSeed);
                var info = await _admin_service.GuestInfoAsync();
                var friendsSeeded = info.Item.Db.NrSeededFriends;

                Message = $"Seeding completed successfully! {friendsSeeded} friends added.";

                //return Redirect($"~/ListOfGroups"); Tillfälligt, finns inte just nu.
            }
            return Page();
        }

        public SeedModel(IAdminService admin_service ,ILogger<SeedModel> logger)
        {
            _admin_service = admin_service;
            _logger = logger;
        }
    }
}