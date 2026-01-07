using Microsoft.AspNetCore.Mvc;

public class ListOfFriendsAndPetsViewModel : ListOfFriendsViewModel
{
    public int NrOfPets { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SelectedCountry { get; set; }// Country filter
    public List<string> AvailableCountries { get; set; } = new();

    public bool HasSelectedCountry =>
        !string.IsNullOrWhiteSpace(SelectedCountry);
}