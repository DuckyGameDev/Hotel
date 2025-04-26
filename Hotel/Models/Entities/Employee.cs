using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public DateOnly HireDate { get; set; }

    public virtual ICollection<Workschedule> Workschedules { get; set; } = new List<Workschedule>();
}
