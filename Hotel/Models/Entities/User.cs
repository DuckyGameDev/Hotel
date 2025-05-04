using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int GuestId { get; set; }

    public int? RoleId { get; set; }

    public virtual Guest Guest { get; set; } = null!;

    public virtual Role? Role { get; set; }

    public virtual ICollection<Spaserviceorder> Spaserviceorders { get; set; } = new List<Spaserviceorder>();
}
