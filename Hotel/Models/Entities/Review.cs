using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int BookingId { get; set; }

    public int Rating { get; set; }

    public string? ReviewText { get; set; }

    public DateOnly ReviewDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
