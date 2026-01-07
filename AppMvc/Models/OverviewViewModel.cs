using Microsoft.AspNetCore.Mvc;

public class OverviewViewModel
{
    public enum OverviewMode { FriendsOnly, FriendsAndPets }

    [BindProperty(SupportsGet = true)]
    public OverviewMode Mode { get; set; } = OverviewMode.FriendsOnly; //To choose what to show the user

    public bool UseSeeds { get; set; } = true;
    public int NrOfFriends { get; set; }

    public string? ExpandedCountry { get; set; }
    public string? PreviouslyExpandedCountry { get; set; }

    public List<string> AvalibleCountries { get; set; } = new();
    public List<string> ExpandedCities { get; set; } = new();

    public Dictionary<string, (int FriendsCount, int PetsCount)> CityData { get; set; } = new(); //For FriendsOnly overview
    public Dictionary<string, (int FriendsCount, int PetsCount)> CountryData { get; set; } = new(); //For Friends and pets overview
}