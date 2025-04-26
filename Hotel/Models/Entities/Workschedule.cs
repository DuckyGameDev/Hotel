using System;
using System.Collections.Generic;

namespace Hotel.Models.Entities;

public partial class Workschedule
{
    public int ScheduleId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
