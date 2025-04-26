using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Serviceorder
{
    public int OrderId { get; set; }

    public int BookingId { get; set; }

    public int ServiceId { get; set; }

    public DateOnly ServiceDate { get; set; }

    public TimeOnly ServiceTime { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
