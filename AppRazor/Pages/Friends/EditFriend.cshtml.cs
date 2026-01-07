using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using AppRazor.SeidoHelpers;
using Models.Interfaces;
using Services.Interfaces;
using Models.DTO;
using Models;

namespace AppRazor.Pages
{
    public class EditFriendModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly IAddressesService _addressService = null;
        readonly IPetsService _petSerivce = null;
        readonly IQuotesService _quotesService = null;
        readonly ILogger<FriendsByCountryModel> _logger = null;

        [BindProperty]
        public FriendIM FriendInput { get; set; }
        public bool HasValidationErrors { get; set; }
        public IEnumerable<string> ValidationErrorMsgs { get; set; }
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        public async Task<IActionResult> OnGet()
        {
            if (!Guid.TryParse(Request.Query["id"], out Guid friendId))return NotFound();

            var friend = (await _service.ReadFriendAsync(friendId, false));
            if (friend == null) return NotFound();

            FriendInput = new FriendIM(friend.Item);

            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        {
            string[] keys = { "FriendInput.FirstName",
                              "FriendInput.LastName",
                              "FriendInput.Birthday"};
            
            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Check for deleted pets
            var deletedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Deleted));
            foreach(var pet in deletedPets) await _petSerivce.DeletePetAsync(pet.PetId);

            //Check for deleted quotes
            var deletedQuotes = FriendInput.Quotes.FindAll(p => (p.StatusIM == StatusIM.Deleted));
            foreach(var quote in deletedQuotes) await _quotesService.DeleteQuoteAsync(quote.QuoteId);

            //Update friend and save to database
            var friend = await SaveAddress();
            friend = FriendInput.UpdateModel(friend);
            await _service.UpdateFriendAsync(new FriendCuDto(friend));

            return Redirect($"~/Friends/FriendDetails?id={FriendInput.FriendId}");

        }

        public async Task<IActionResult> OnPostDelete(Guid itemId, string type)
        {

            var friend = await _service.ReadFriendAsync(FriendInput.FriendId, false);
            FriendInput = new FriendIM(friend.Item);

            if (type == "Pet") //To use same method for pets and quotes
            {
                FriendInput.Pets.First(p => p.PetId == itemId).StatusIM = StatusIM.Deleted;
                return Page();
            }

            FriendInput.Quotes.First(q => q.QuoteId == itemId).StatusIM = StatusIM.Deleted;
            return Page();
        }

        public IActionResult OnPostSaveAddress() //For the address save button
        {
            string[] keys = { "FriendInput.Address.StreetAddress",
                              "FriendInput.Address.City",
                              "FriendInput.Address.Country"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            return Page();
        }

        private async Task<IFriend> SaveAddress()
        {
            var originalFriend = (await _service.ReadFriendAsync(FriendInput.FriendId, false)).Item;
            if(originalFriend.Address != null) //If friend has no address
            {
                //Need to see changes to not create new address every time.
                var originalAddress = new AddressIm(originalFriend.Address);
                if(originalAddress.IsSameAs(FriendInput.Address)) return originalFriend;
            } 

            var cuDto = FriendInput.Address.CreateCUdto();
            cuDto.FriendsId = new List<Guid> { FriendInput.FriendId };
            await _addressService.CreateAddressAsync(cuDto);

            var friend = await _service.ReadFriendAsync(FriendInput.FriendId, false); 
            return friend.Item;
        }

        public EditFriendModel (IFriendsService service, IAddressesService addressesService, IQuotesService quotesService,
         IPetsService petsService, ILogger<FriendsByCountryModel> logger)
        {
            _service = service;
            _addressService = addressesService;
            _petSerivce = petsService;
            _quotesService = quotesService;
            _logger = logger;
        }

        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }
        public class FriendIM //InputModel for validation
        {
            public StatusIM StatusIM { get; set; }
            public Guid FriendId { get; set; }

            [Required(ErrorMessage = "First name is required")] //Validation with error message
            [StringLength(50, ErrorMessage = "First name too long")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "You must provide a last name")]
            public string LastName { get; set; }

            public DateTime? Birthday { get; set; }
            public AddressIm Address { get; set; } = new AddressIm();
            public List<QuotesIm> Quotes { get; set; } = new List<QuotesIm>();
            public List<PetIm> Pets { get; set; } = new List<PetIm>();

            public FriendIM() { }

            public FriendIM(IFriend model)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;
                Birthday = model.Birthday;

                Quotes = model.Quotes?.Select(m => new QuotesIm(m)).ToList();
                Pets = model.Pets?.Select(m => new PetIm(m)).ToList();
                Address = new AddressIm(model.Address ?? new Address());
            }

            public IFriend UpdateModel(IFriend model) //to update the model in database
            {
                model.FriendId = this.FriendId;
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                model.Birthday = this.Birthday;
                return model;
            }
            public FriendCuDto CreateCUdto () => new FriendCuDto() //to create new friend in the database
            {
                FriendId = null,
                FirstName = this.FirstName,
                LastName = this.LastName
            };
        }

        public class AddressIm //Input model for validation, to update address
        {
            public StatusIM StatusIM { get; set; }
            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "Name of street is required")]
            public string StreetAddress { get; set; }

            [Required(ErrorMessage = "Zipcode is required")]
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "City is required")]
            public string City { get; set; }

            [Required(ErrorMessage = "Country is required")]
            public string Country { get; set; }

            public AddressIm(){}
            public AddressIm(AddressIm original)
            {
                StatusIM = original.StatusIM;
                AddressId = original.AddressId;
                StreetAddress = original.StreetAddress;
                ZipCode = original.ZipCode;
                City = original.City;
                Country = original.Country;
            }
            public AddressIm(IAddress model)
            {
                StatusIM = StatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = model.StreetAddress;
                ZipCode = model.ZipCode;
                City = model.City;
                Country = model.Country;
            }
            public IAddress UpdateModel(IAddress model)
            {
                model.AddressId = this.AddressId;
                model.StreetAddress = this.StreetAddress;
                model.ZipCode = this.ZipCode;
                model.City = this.City;
                model.Country = this.Country;
                return model;
            }

            public AddressCuDto CreateCUdto () => new AddressCuDto(){

                AddressId = null,
                StreetAddress = this.StreetAddress,
                ZipCode = this.ZipCode,
                City = this.City,
                Country = this.Country
            };
            public bool IsSameAs(AddressIm other)
            {
                if (other == null) return false;

                return string.Equals(StreetAddress?.Trim(), other.StreetAddress?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(City?.Trim(), other.City?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(Country?.Trim(), other.Country?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && ZipCode == other.ZipCode;
            }
        }
        public class QuotesIm //Input model for validation, to update address
        {
            public StatusIM StatusIM { get; set; }
            public Guid QuoteId { get; set; }

            [Required(ErrorMessage = "Qoute content is required")]
            public string Quote { get; set; }

            [Required(ErrorMessage = "Author is required")]
            public string Author { get; set; }
            public QuotesIm(){}
            public QuotesIm(QuotesIm original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                Quote = original.Quote;
                Author = original.Author;
            }
            public QuotesIm(IQuote model)
            {                
                StatusIM = StatusIM.Unchanged;
                QuoteId = model.QuoteId;
                Quote = model.QuoteText;
                Author = model.Author;
            }
            public IQuote UpdateModel(IQuote model)
            {
                model.QuoteId = this.QuoteId;
                model.QuoteText = this.Quote;
                model.Author = this.Author;
                return model;
            }
            public QuoteCuDto CreateCUdto () => new QuoteCuDto()
            {
                QuoteId = null,
                Quote = this.Quote,
                Author = this.Author
            };
        }
        public class PetIm //Input model for validation, to update address
        {
            public StatusIM StatusIM { get; set; }
            public Guid PetId { get; set; }

            [Required(ErrorMessage = "Must choose what kind of animal")]
            public AnimalKind Kind { get; set; }

            [Required(ErrorMessage = "Pet must have a name")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Must choose mood for animal")]
            public AnimalMood Mood { get; set; }
            public PetIm(){}
            public PetIm(PetIm original)
            {
                StatusIM = original.StatusIM;
                PetId = original.PetId;
                Kind = original.Kind;
                Name = original.Name;
                Mood = original.Mood;
            }
            public PetIm(IPet model)
            {                
                StatusIM = StatusIM.Unchanged;
                PetId = model.PetId;
                Kind = model.Kind;
                Name = model.Name;
                Mood = model.Mood;
            }
            public IPet UpdateModel(IPet model)
            {
                model.PetId = this.PetId;
                model.Kind = this.Kind;
                model.Name = this.Name;
                model.Mood = this.Mood;
                return model;
            }
            public PetCuDto CreateCUdto () => new PetCuDto()
            {
                PetId = null,
                Kind = this.Kind,
                Name = this.Name,
                Mood = this.Mood
            };
        }
    }
}