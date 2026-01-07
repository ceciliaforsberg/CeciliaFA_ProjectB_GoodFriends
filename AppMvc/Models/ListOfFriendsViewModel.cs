using Microsoft.AspNetCore.Mvc;
using Models.Interfaces;

public class ListOfFriendsViewModel
{
    [BindProperty]
    public bool UseSeeds { get; set; } = true;

    public List<IFriend> Friends { get; set; } = new();
    public int NrOfFriends { get; set; }

    // Pagination
    public int PageSize { get; } = 10;
    public int ThisPageNr { get; set; }
    public int PrevPageNr { get; set; }
    public int NextPageNr { get; set; }
    public int NrVisiblePages { get; set; }
    public int NrOfPages { get; set; }

    [BindProperty]
    public string? SearchFilter { get; set; }
}