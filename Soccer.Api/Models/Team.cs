using System;
using System.Collections.Generic;

namespace Soccer.Api.Models;

public partial class Team
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public int PlayersQuantity { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<League> Leagues { get; set; } = new List<League>();

    public virtual ICollection<LeagueTeam> LeagueTeams { get; set; }
    = new List<LeagueTeam>();
}
