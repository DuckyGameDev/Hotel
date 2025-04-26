using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Spaserviceorder
{
    public int SpaOrderId { get; set; }

    public int BookingId { get; set; }

    public int SpaServiceId { get; set; }

    public DateOnly ServiceDate { get; set; }

    public TimeOnly ServiceTime { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Spaservice SpaService { get; set; } = null!;
}
