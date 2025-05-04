using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Booking
{
    public int BookingId { get; set; }

    public int GuestId { get; set; }

    public int RoomId { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public virtual Guest Guest { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Mealorder> Mealorders { get; set; } = new List<Mealorder>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Serviceorder> Serviceorders { get; set; } = new List<Serviceorder>();
}
