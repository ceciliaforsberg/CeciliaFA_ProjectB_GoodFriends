using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

using Models.Interfaces;
using AppMvc.SeidoHelpers;
using Models.DTO;
using Models;

namespace AppMvc.Models
{
    public class EditFriendViewModel
    {
        [BindProperty]
        public FriendIM FriendInput { get; set; } = new FriendIM();
        public bool HasValidationErrors { get; set; }
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);
        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

        public class FriendIM //Input model for validation
        {
            public Guid FriendId { get; set; }
            public StatusIM StatusIM { get; set; }

            [Required(ErrorMessage = "First name is required")] //Validation message to show user
            [StringLength(50, ErrorMessage = "Name is too long")] //Validation for string length
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")] //Validation message to show user
            public string LastName { get; set; }

            public DateTime? Birthday { get; set; }
            public AddressIM Address { get; set; } = new AddressIM();
            public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();
            public List<PetIM> Pets { get; set; } = new List<PetIM>();

            public FriendIM() { }
            public FriendIM(IFriend model)
            {
                StatusIM = StatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;
                Birthday = model.Birthday;

                Quotes = model.Quotes?.Select(m => new QuoteIM(m)).ToList();
                Pets = model.Pets?.Select(m => new PetIM(m)).ToList();
                Address = new AddressIM(model.Address ?? new Address());
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

        public class AddressIM //Input model for validation
        {
            public Guid AddressId { get; set; }
            public StatusIM StatusIM { get; set; }

            [Required(ErrorMessage = "Name of street is required")] 
            public string StreetAddress { get; set; }

            [Required(ErrorMessage = "Zipcode is required")] 
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "City is required")] 
            public string City { get; set; }

            [Required(ErrorMessage = "Country is required")] 
            public string Country { get; set; }

            public AddressIM(){}
            public AddressIM(AddressIM original)
            {
                StatusIM = original.StatusIM;
                AddressId = original.AddressId;
                StreetAddress = original.StreetAddress;
                ZipCode = original.ZipCode;
                City = original.City;
                Country = original.Country;
            }
            public AddressIM(IAddress model)
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
            public bool IsSameAs(AddressIM other)
            {
                if (other == null) return false;

                return string.Equals(StreetAddress?.Trim(), other.StreetAddress?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(City?.Trim(), other.City?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(Country?.Trim(), other.Country?.Trim(), StringComparison.OrdinalIgnoreCase)
                    && ZipCode == other.ZipCode;
            }
        }

        public class QuoteIM //Input model for validation
        {
            public Guid QuoteId { get; set; }
            public StatusIM StatusIM { get; set; }

            [Required(ErrorMessage = "Qoute content is required")] 
            public string Quote { get; set; }

            [Required(ErrorMessage = "Author is required")] 
            public string Author { get; set; }

            public QuoteIM(){}
            public QuoteIM(QuoteIM original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                Quote = original.Quote;
                Author = original.Author;
            }
            public QuoteIM(IQuote model)
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

        public class PetIM //Input model for validation
        {
            public Guid PetId { get; set; }
            public StatusIM StatusIM { get; set; }

            [Required(ErrorMessage = "Pet must have a name")]
             public string Name { get; set; }

            [Required(ErrorMessage = "Must choose what kind of animal")] 
            public AnimalKind Kind { get; set; }

            [Required(ErrorMessage = "Must choose mood for animal")] 
            public AnimalMood Mood { get; set; }

            public PetIM(){}
            public PetIM(PetIM original)
            {
                StatusIM = original.StatusIM;
                PetId = original.PetId;
                Kind = original.Kind;
                Name = original.Name;
                Mood = original.Mood;
            }
            public PetIM(IPet model)
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