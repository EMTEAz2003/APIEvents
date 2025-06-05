using System;
using System.Collections.Generic;

namespace APIEvents.Models;

public partial class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxRegistrations { get; set; }

    public string Location { get; set; } = null!;

    public virtual ICollection<EventUser> EventUsers { get; set; } = new List<EventUser>();
}
