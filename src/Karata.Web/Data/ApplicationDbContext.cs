﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Karata.Cards;
using Karata.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Karata.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Room> Rooms { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Game)
                .WithOne()
                .HasForeignKey<Game>(g => g.RoomId);

            modelBuilder.Entity<ApplicationUser>()
                .Property(a => a.Hand)
                .HasConversion(
                    hand => JsonSerializer.Serialize(hand, options),
                    json => JsonSerializer.Deserialize<List<Card>>(json, options),
                    new ValueComparer<List<Card>>(
                        (s1, s2) => s1.SequenceEqual(s2),
                        s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        s => s.ToList()
                    ));

            modelBuilder.Entity<Game>()
                .Property(g => g.CurrentRequest)
                .HasConversion(
                    request => JsonSerializer.Serialize(request, options),
                    json => JsonSerializer.Deserialize<Card>(json, options));

            modelBuilder.Entity<Game>()
                .Property(g => g.Deck)
                .HasConversion(
                    deck => JsonSerializer.Serialize(deck.Reverse(), options),
                    json => JsonSerializer.Deserialize<Deck>(json, options),
                    new ValueComparer<Deck>(
                        (s1, s2) => s1.SequenceEqual(s2),
                        s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        s => new(s.Reverse())
                    ));

            modelBuilder.Entity<Game>()
                .Property(g => g.Pile)
                .HasConversion(
                    pile => JsonSerializer.Serialize(pile.Reverse(), options),
                    json => JsonSerializer.Deserialize<Pile>(json, options),
                    new ValueComparer<Pile>(
                        (s1, s2) => s1.SequenceEqual(s2),
                        s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        s => new(s.Reverse())
                    ));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.LogTo(Console.WriteLine);
            // optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
