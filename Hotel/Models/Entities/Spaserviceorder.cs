using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Spaserviceorder
{
    public int SpaOrderId { get; set; }

    public int SpaServiceId { get; set; }

    public DateOnly ServiceDate { get; set; }

    public TimeOnly ServiceTime { get; set; }

    public int UserId { get; set; }

    public virtual Spaservice SpaService { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
