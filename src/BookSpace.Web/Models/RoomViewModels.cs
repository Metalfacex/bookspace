using System.ComponentModel.DataAnnotations;
using BookSpace.Core.Domain;
using BookSpace.Core.Services;

namespace BookSpace.Web.Models;

public class RoomFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "დასახელება სავალდებულოა.")]
    [StringLength(100)]
    [Display(Name = "დასახელება")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "მდებარეობა სავალდებულოა.")]
    [StringLength(200)]
    [Display(Name = "მდებარეობა")]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "ტევადობა")]
    public int Capacity { get; set; } = 1;

    [Display(Name = "აქტიურია")]
    public bool IsActive { get; set; } = true;

    public static RoomFormViewModel From(Room room) => new()
    {
        Id = room.Id,
        Name = room.Name,
        Location = room.Location,
        Capacity = room.Capacity,
        IsActive = room.IsActive
    };
}

public class RoomDetailsViewModel
{
    public required Room Room { get; init; }
    public required DaySchedule Schedule { get; init; }
    public required DateOnly MinDate { get; init; }
    public required DateOnly MaxDate { get; init; }
}
