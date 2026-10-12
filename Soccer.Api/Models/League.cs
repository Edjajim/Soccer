using System;
using System.Collections.Generic;

namespace Soccer.Api.Models;

public partial class League
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual ICollection<LeagueTeam> LeagueTeams { get; set; }
    = new List<LeagueTeam>();
}
