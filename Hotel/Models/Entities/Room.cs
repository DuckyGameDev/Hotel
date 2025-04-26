using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Room
{
    public int RoomId { get; set; }

    public int CategoryId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Roomcategory Category { get; set; } = null!;
}
