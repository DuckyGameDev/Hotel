using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Meal
{
    public int MealId { get; set; }

    public string MealName { get; set; } = null!;

    public string MealType { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Mealorder> Mealorders { get; set; } = new List<Mealorder>();
}
