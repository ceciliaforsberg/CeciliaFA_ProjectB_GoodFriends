using AppMvc.Models;
using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;
using Models.Interfaces;
using Models.DTO;
using System.Runtime.InteropServices;

public class FriendsController : Controller
{
    private readonly IFriendsService _service;
    private readonly IAddressesService _addressService;
    private readonly IQuotesService _quotesService;
    private readonly IPetsService _petsService;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(IFriendsService service, IAddressesService addressService, IQuotesService quotesService, IPetsService petsService, ILogger<FriendsController> logger)
    {
        _service = service;
        _addressService = addressService;
        _quotesService = quotesService;
        _petsService = petsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ListOfFriends(int pagenr, string search)
    {
        var vm = new ListOfFriendsViewModel {ThisPageNr = pagenr,SearchFilter = search};

        await LoadFriends(vm);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ListOfFriendsAndPets(ListOfFriendsAndPetsViewModel vm)
    {
        await LoadFriendsAndPets(vm);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> FriendDetails(Guid id)
    {
        var friend = (await _service.ReadFriendAsync(id, false)).Item;

        if (friend == null)return NotFound();

        var vm = new FriendDetailsViewModel{Friend = friend};

        return View(vm);
    }

    public async Task<IActionResult> EditFriend(Guid id)
    {
        var friend = (await _service.ReadFriendAsync(id, false)).Item;
        if (friend == null) return NotFound();

        var vm = new EditFriendViewModel(){FriendInput = new EditFriendViewModel.FriendIM(friend)};

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Search(ListOfFriendsViewModel vm)
    {
        await LoadFriends(vm);
        return View("ListOfFriends",vm);
    }

    [HttpPost]
    public async Task<IActionResult> SearchCity(ListOfFriendsAndPetsViewModel vm)
    {
        await LoadFriendsAndPets(vm);
        return View("ListOfFriendsAndPets",vm);
    }

    [HttpPost]
    public async Task<IActionResult> SaveFriend(EditFriendViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.HasValidationErrors = true;
            return View(vm);
        }

        //Check for deleted pets
        var deletedPets = vm.FriendInput.Pets.Where(p => p.StatusIM == EditFriendViewModel.StatusIM.Deleted).ToList();
        foreach (var pet in deletedPets) await _petsService.DeletePetAsync(pet.PetId);

        //Check for deleted quotes
        var deletedQuotes = vm.FriendInput.Quotes.Where(q => q.StatusIM == EditFriendViewModel.StatusIM.Deleted).ToList();
        foreach (var quote in deletedQuotes) await _quotesService.DeleteQuoteAsync(quote.QuoteId);

        //Update address if modified
        var originalFriend = (await _service.ReadFriendAsync(vm.FriendInput.FriendId, false)).Item;
        if (originalFriend.Address == null || !IsSameAddress(vm.FriendInput.Address, originalFriend.Address))
        {
            var cuDto = vm.FriendInput.Address.CreateCUdto(); // Du kan återanvända din DTO-metod
            cuDto.FriendsId = new List<Guid> { vm.FriendInput.FriendId };
            await _addressService.CreateAddressAsync(cuDto);
        }

        //Update friend
        var friend = await SaveAddress(vm);
        friend = vm.FriendInput.UpdateModel(friend);
        await _service.UpdateFriendAsync(new FriendCuDto(friend));

        return Redirect($"~/Friends/FriendDetails?id={vm.FriendInput.FriendId}");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(Guid itemId, string type, EditFriendViewModel vm)
    {
        var friend = await _service.ReadFriendAsync(vm.FriendInput.FriendId, false);
        vm.FriendInput = new EditFriendViewModel.FriendIM(friend.Item);

        if (type == "Pet")
        {
            vm.FriendInput.Pets.First(p => p.PetId == itemId).StatusIM = EditFriendViewModel.StatusIM.Deleted;
            return View("EditFriend", vm);
        }

        vm.FriendInput.Quotes.First(q => q.QuoteId == itemId).StatusIM = EditFriendViewModel.StatusIM.Deleted;
        return View("EditFriend", vm);
    }

    private async Task LoadFriends(ListOfFriendsViewModel vm)
    {
        var resp = await _service.ReadFriendsAsync(vm.UseSeeds, false, vm.SearchFilter, vm.ThisPageNr, vm.PageSize);

        vm.Friends = resp.PageItems;
        vm.NrOfFriends = resp.DbItemsCount;

        UpdatePagination(vm, resp.DbItemsCount);
    }
    private async Task<IFriend> SaveAddress(EditFriendViewModel vm)
    {
        var originalFriend = (await _service.ReadFriendAsync(vm.FriendInput.FriendId, false)).Item;
        if(originalFriend.Address != null) //If friend has no address
        {
            //Need to see changes to not create new address every time.
            var originalAddress = new EditFriendViewModel.AddressIM(originalFriend.Address);
            if(originalAddress.IsSameAs(vm.FriendInput.Address)) return originalFriend;
        } 

        var cuDto = vm.FriendInput.Address.CreateCUdto();
        cuDto.FriendsId = new List<Guid> { vm.FriendInput.FriendId };
        await _addressService.CreateAddressAsync(cuDto);

        var friend = await _service.ReadFriendAsync(vm.FriendInput.FriendId, false); //For test
        return friend.Item;
    }

    private async Task LoadFriendsAndPets(ListOfFriendsAndPetsViewModel vm)
    {
        //Load all avalible countries
        vm.AvailableCountries = await _addressService.ReadAllCountriesAsync(vm.UseSeeds);

        //Get total items, this is to not miss any items located on different pages in service in next step
        var totalResp = await _service.ReadFriendsAsync(vm.UseSeeds, false, null, 0, 1);
        int totalItems = totalResp.DbItemsCount;

        //Filter friends by search and get all avalible items for further filtering in next step
        var resp = await _service.ReadFriendsAsync(vm.UseSeeds, false, vm.SearchFilter, 0, totalItems);

        //Now that i have all items i can filter by chosen country
        var filteredFriends = resp.PageItems
            .Where(f => f.Address?.Country == vm.SelectedCountry)
            .ToList();

        vm.NrOfFriends = filteredFriends.Count;
        vm.NrOfPets = filteredFriends.Sum(f => f.Pets?.Count ?? 0);

        //Needed for pagination to work on the filtered list of friends, since i filter the list outside of the service logic 
        vm.Friends = filteredFriends
            .Skip(vm.ThisPageNr * vm.PageSize)
            .Take(vm.PageSize)
            .ToList();

        UpdatePagination(vm, vm.NrOfFriends);
    }

    private bool IsSameAddress(EditFriendViewModel.AddressIM input, IAddress model)
    {
        if (input == null || model == null) return false;
        return input.StreetAddress == model.StreetAddress &&
               input.City == model.City &&
               input.Country == model.Country &&
               input.ZipCode == model.ZipCode;
    }
    
    private void UpdatePagination(ListOfFriendsViewModel vm, int nrOfItems)
    {
        vm.NrOfPages = (int)Math.Ceiling((double)nrOfItems / vm.PageSize);
        vm.PrevPageNr = Math.Max(0, vm.ThisPageNr - 1);
        vm.NextPageNr = Math.Min(vm.NrOfPages - 1, vm.ThisPageNr + 1);
        vm.NrVisiblePages = Math.Min(10, vm.NrOfPages);
    }
    
    // Reuses base pagination logic via casting to ListOfFriendsViewModel.
    private void UpdatePagination(ListOfFriendsAndPetsViewModel vm, int nrOfItems) => UpdatePagination((ListOfFriendsViewModel)vm, nrOfItems);
}