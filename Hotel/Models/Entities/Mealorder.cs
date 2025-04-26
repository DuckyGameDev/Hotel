using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Mealorder
{
    public int MealOrderId { get; set; }

    public int BookingId { get; set; }

    public int MealId { get; set; }

    public DateOnly MealDate { get; set; }

    public TimeOnly MealTime { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Meal Meal { get; set; } = null!;
}
