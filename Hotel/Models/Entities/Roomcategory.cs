using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Roomcategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal PricePerNight { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
