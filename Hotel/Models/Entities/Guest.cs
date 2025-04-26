using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Guest
{
    public int GuestId { get; set; }

    public string FullName { get; set; } = null!;

    public string PassportData { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual User? User { get; set; }
}
