using System;
using System.Collections.Generic;

namespace Soccer.Api.Models
{
    public partial class LeagueTeam
    {
        public int LeagueId { get; set; }

        public int TeamId { get; set; }

        public virtual League League { get; set; } = null!;

        public virtual Team Team { get; set; } = null!;
    }
}

