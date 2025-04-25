using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string ServiceType { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Serviceorder> Serviceorders { get; set; } = new List<Serviceorder>();
}
