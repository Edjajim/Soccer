using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Soccer.Api.Models;

namespace Soccer.Api.Data;

public partial class SoccerContext : DbContext
{
    public SoccerContext(DbContextOptions<SoccerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<League> Leagues { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<LeagueTeam> LeagueTeams { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<League>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__League__3214EC071F07B9CF");

            entity.ToTable("League");

            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Enabled)
                .HasDefaultValue(true);

            entity.Property(e => e.EndDate)
                .HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(CONVERT([date],getdate()))");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Team__3214EC0763C2154B");

            entity.ToTable("Team");

            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Enabled)
                .HasDefaultValue(true);

            entity.Property(e => e.PlayersQuantity);

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LeagueTeam>(entity =>
        {
            entity.HasKey(e => new { e.LeagueId, e.TeamId });

            entity.ToTable("LEAGUE_TEAM");

            entity.HasOne(e => e.League)
                .WithMany(e => e.LeagueTeams)
                .HasForeignKey(e => e.LeagueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LEAGUE_TEAM_League");

            entity.HasOne(e => e.Team)
                .WithMany(e => e.LeagueTeams)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LEAGUE_TEAM_Team");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
