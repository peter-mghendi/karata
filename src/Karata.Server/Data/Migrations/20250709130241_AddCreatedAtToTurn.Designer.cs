﻿// <auto-generated />
using System;
using Karata.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Karata.Server.Data.Migrations
{
    [DbContext(typeof(KarataContext))]
    [Migration("20250709130241_AddCreatedAtToTurn")]
    partial class AddCreatedAtToTurn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Duende.IdentityServer.EntityFramework.Entities.DeviceFlowCodes", b =>
                {
                    b.Property<string>("UserCode")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(50000)
                        .HasColumnType("character varying(50000)");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("DeviceCode")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime>("Expiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("SubjectId")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("UserCode");

                    b.HasIndex("DeviceCode")
                        .IsUnique();

                    b.HasIndex("Expiration");

                    b.ToTable("DeviceCodes", (string)null);
                });

            modelBuilder.Entity("Duende.IdentityServer.EntityFramework.Entities.Key", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Algorithm")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("DataProtected")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsX509Certificate")
                        .HasColumnType("boolean");

                    b.Property<string>("Use")
                        .HasColumnType("text");

                    b.Property<int>("Version")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Use");

                    b.ToTable("Keys", (string)null);
                });

            modelBuilder.Entity("Duende.IdentityServer.EntityFramework.Entities.PersistedGrant", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("ConsumedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(50000)
                        .HasColumnType("character varying(50000)");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("SubjectId")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Key");

                    b.HasIndex("ConsumedTime");

                    b.HasIndex("Expiration");

                    b.HasIndex("SubjectId", "ClientId", "Type");

                    b.HasIndex("SubjectId", "SessionId", "Type");

                    b.ToTable("PersistedGrants", (string)null);
                });

            modelBuilder.Entity("Karata.Server.Models.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ActorId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ActorId");

                    b.ToTable("Activities");
                });

            modelBuilder.Entity("Karata.Server.Models.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid?>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<string>("SenderId")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("SentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.HasIndex("SenderId");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("Karata.Server.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CurrentTurn")
                        .HasColumnType("integer");

                    b.Property<string>("Deck")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Give")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsReversed")
                        .HasColumnType("boolean");

                    b.Property<long>("Pick")
                        .HasColumnType("bigint");

                    b.Property<string>("Pile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoomId")
                        .IsUnique();

                    b.ToTable("Games");
                });

            modelBuilder.Entity("Karata.Server.Models.GameResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ReasonType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ResultType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("WinnerId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GameId")
                        .IsUnique();

                    b.HasIndex("WinnerId");

                    b.ToTable("GameResult");
                });

            modelBuilder.Entity("Karata.Server.Models.Hand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsLastCard")
                        .HasColumnType("boolean");

                    b.Property<string>("PlayerId")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("PlayerId");

                    b.ToTable("Hands");
                });

            modelBuilder.Entity("Karata.Server.Models.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AdministratorId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("Hash")
                        .HasColumnType("bytea");

                    b.Property<byte[]>("Salt")
                        .HasColumnType("bytea");

                    b.HasKey("Id");

                    b.HasIndex("AdministratorId");

                    b.HasIndex("CreatorId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("Karata.Server.Models.Turn", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("HandId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsLastCard")
                        .HasColumnType("boolean");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("HandId");

                    b.ToTable("Turns");
                });

            modelBuilder.Entity("Karata.Server.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Karata.Server.Models.Activity", b =>
                {
                    b.HasOne("Karata.Server.Models.User", "Actor")
                        .WithMany()
                        .HasForeignKey("ActorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("System.Collections.Generic.Dictionary<string, object>", "Metadata", b1 =>
                        {
                            b1.Property<int>("ActivityId")
                                .HasColumnType("integer");

                            b1.HasKey("ActivityId");

                            b1.ToTable("Activities");

                            b1.ToJson("Metadata");

                            b1.WithOwner()
                                .HasForeignKey("ActivityId");
                        });

                    b.Navigation("Actor");

                    b.Navigation("Metadata")
                        .IsRequired();
                });

            modelBuilder.Entity("Karata.Server.Models.Chat", b =>
                {
                    b.HasOne("Karata.Server.Models.Room", null)
                        .WithMany("Chats")
                        .HasForeignKey("RoomId");

                    b.HasOne("Karata.Server.Models.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Karata.Server.Models.Game", b =>
                {
                    b.HasOne("Karata.Server.Models.Room", null)
                        .WithOne("Game")
                        .HasForeignKey("Karata.Server.Models.Game", "RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Karata.Cards.Card", "Request", b1 =>
                        {
                            b1.Property<int>("GameId")
                                .HasColumnType("integer");

                            b1.Property<int>("Face")
                                .HasColumnType("integer");

                            b1.Property<int>("Suit")
                                .HasColumnType("integer");

                            b1.HasKey("GameId");

                            b1.ToTable("Games");

                            b1.ToJson("Request");

                            b1.WithOwner()
                                .HasForeignKey("GameId");
                        });

                    b.Navigation("Request");
                });

            modelBuilder.Entity("Karata.Server.Models.GameResult", b =>
                {
                    b.HasOne("Karata.Server.Models.Game", null)
                        .WithOne("Result")
                        .HasForeignKey("Karata.Server.Models.GameResult", "GameId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Karata.Server.Models.User", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("Karata.Server.Models.Hand", b =>
                {
                    b.HasOne("Karata.Server.Models.Game", null)
                        .WithMany("Hands")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Karata.Server.Models.User", "Player")
                        .WithMany("Hands")
                        .HasForeignKey("PlayerId");

                    b.OwnsMany("Karata.Cards.Card", "Cards", b1 =>
                        {
                            b1.Property<int>("HandId")
                                .HasColumnType("integer");

                            b1.Property<int>("__synthesizedOrdinal")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<int>("Face")
                                .HasColumnType("integer");

                            b1.Property<int>("Suit")
                                .HasColumnType("integer");

                            b1.HasKey("HandId", "__synthesizedOrdinal");

                            b1.ToTable("Hands");

                            b1.ToJson("Cards");

                            b1.WithOwner()
                                .HasForeignKey("HandId");
                        });

                    b.Navigation("Cards");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Karata.Server.Models.Room", b =>
                {
                    b.HasOne("Karata.Server.Models.User", "Administrator")
                        .WithMany()
                        .HasForeignKey("AdministratorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Karata.Server.Models.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Administrator");

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("Karata.Server.Models.Turn", b =>
                {
                    b.HasOne("Karata.Server.Models.Hand", "Hand")
                        .WithMany("Turns")
                        .HasForeignKey("HandId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Karata.Server.Models.GameDelta", "Delta", b1 =>
                        {
                            b1.Property<int>("TurnId")
                                .HasColumnType("integer");

                            b1.Property<long>("Give")
                                .HasColumnType("bigint");

                            b1.Property<long>("Pick")
                                .HasColumnType("bigint");

                            b1.Property<long>("RemoveRequestLevels")
                                .HasColumnType("bigint");

                            b1.Property<int>("RequestLevel")
                                .HasColumnType("integer");

                            b1.Property<bool>("Reverse")
                                .HasColumnType("boolean");

                            b1.Property<long>("Skip")
                                .HasColumnType("bigint");

                            b1.HasKey("TurnId");

                            b1.ToTable("Turns");

                            b1.ToJson("Delta");

                            b1.WithOwner()
                                .HasForeignKey("TurnId");

                            b1.OwnsMany("Karata.Cards.Card", "Cards", b2 =>
                                {
                                    b2.Property<int>("GameDeltaTurnId")
                                        .HasColumnType("integer");

                                    b2.Property<int>("__synthesizedOrdinal")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("integer");

                                    b2.Property<int>("Face")
                                        .HasColumnType("integer");

                                    b2.Property<int>("Suit")
                                        .HasColumnType("integer");

                                    b2.HasKey("GameDeltaTurnId", "__synthesizedOrdinal");

                                    b2.ToTable("Turns");

                                    b2.WithOwner()
                                        .HasForeignKey("GameDeltaTurnId");
                                });

                            b1.Navigation("Cards");
                        });

                    b.OwnsMany("Karata.Cards.Card", "Cards", b1 =>
                        {
                            b1.Property<int>("TurnId")
                                .HasColumnType("integer");

                            b1.Property<int>("__synthesizedOrdinal")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<int>("Face")
                                .HasColumnType("integer");

                            b1.Property<int>("Suit")
                                .HasColumnType("integer");

                            b1.HasKey("TurnId", "__synthesizedOrdinal");

                            b1.ToTable("Turns");

                            b1.ToJson("Cards");

                            b1.WithOwner()
                                .HasForeignKey("TurnId");
                        });

                    b.OwnsMany("Karata.Cards.Card", "Picked", b1 =>
                        {
                            b1.Property<int>("TurnId")
                                .HasColumnType("integer");

                            b1.Property<int>("__synthesizedOrdinal")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<int>("Face")
                                .HasColumnType("integer");

                            b1.Property<int>("Suit")
                                .HasColumnType("integer");

                            b1.HasKey("TurnId", "__synthesizedOrdinal");

                            b1.ToTable("Turns");

                            b1.ToJson("Picked");

                            b1.WithOwner()
                                .HasForeignKey("TurnId");
                        });

                    b.OwnsOne("Karata.Cards.Card", "Request", b1 =>
                        {
                            b1.Property<int>("TurnId")
                                .HasColumnType("integer");

                            b1.Property<int>("Face")
                                .HasColumnType("integer");

                            b1.Property<int>("Suit")
                                .HasColumnType("integer");

                            b1.HasKey("TurnId");

                            b1.ToTable("Turns");

                            b1.ToJson("Request");

                            b1.WithOwner()
                                .HasForeignKey("TurnId");
                        });

                    b.Navigation("Cards");

                    b.Navigation("Delta");

                    b.Navigation("Hand");

                    b.Navigation("Picked");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Karata.Server.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Karata.Server.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Karata.Server.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Karata.Server.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Karata.Server.Models.Game", b =>
                {
                    b.Navigation("Hands");

                    b.Navigation("Result");
                });

            modelBuilder.Entity("Karata.Server.Models.Hand", b =>
                {
                    b.Navigation("Turns");
                });

            modelBuilder.Entity("Karata.Server.Models.Room", b =>
                {
                    b.Navigation("Chats");

                    b.Navigation("Game")
                        .IsRequired();
                });

            modelBuilder.Entity("Karata.Server.Models.User", b =>
                {
                    b.Navigation("Hands");
                });
#pragma warning restore 612, 618
        }
    }
}
