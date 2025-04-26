using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public decimal TotalRoomCost { get; set; }

    public decimal TotalServiceCost { get; set; }

    public decimal TotalMealCost { get; set; }

    public decimal TotalSpaCost { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
