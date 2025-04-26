using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Spaservice
{
    public int SpaServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Duration { get; set; }

    public virtual ICollection<Spaserviceorder> Spaserviceorders { get; set; } = new List<Spaserviceorder>();
}
