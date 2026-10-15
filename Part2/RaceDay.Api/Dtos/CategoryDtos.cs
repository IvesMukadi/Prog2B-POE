using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

// Used to add or update a category
public class SaveCategoryDto
{
    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = "";

    [MaxLength(50)]
    public string? AgeGroup { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? AgeGroup { get; set; }

    public static CategoryDto FromCategory(Category category)
    {
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            EventId = category.EventId,
            CategoryName = category.CategoryName,
            AgeGroup = category.AgeGroup
        };
    }
}
