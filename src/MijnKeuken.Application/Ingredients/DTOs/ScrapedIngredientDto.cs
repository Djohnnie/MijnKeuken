using System.ComponentModel;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Ingredients.DTOs;

public record ScrapedIngredientDto(
    [property: Description("The product title or name")]
    string Title,
    [property: Description("A short product description")]
    string Description,
    [property: Description("The total amount/quantity of the product (e.g. weight in grams or number of units)")]
    decimal Amount,
    [property: Description("The unit type: Grams for weight-based products, Units for countable products")]
    UnitType Unit);
