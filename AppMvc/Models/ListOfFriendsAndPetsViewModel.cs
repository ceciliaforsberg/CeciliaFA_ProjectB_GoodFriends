using Microsoft.AspNetCore.Mvc;

public class ListOfFriendsAndPetsViewModel : ListOfFriendsViewModel
{
    public int NrOfPets { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedCountry { get; set; } // Country filter
    public List<string> AvailableCountries { get; set; } = new(); //To save all unique countries from database

    public bool HasSelectedCountry =>
        !string.IsNullOrWhiteSpace(SelectedCountry); //To use in cshtml to choose what to show 
}