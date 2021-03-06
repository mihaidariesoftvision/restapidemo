﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Library.API.Entities;

namespace Library.API.Migrations
{
    [DbContext(typeof(LibraryContext))]
    internal partial class LibraryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Library.API.Entities.Author", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<DateTimeOffset>("DateOfBirth");

                b.Property<DateTimeOffset?>("DateOfDeath");

                b.Property<string>("FirstName")
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property<string>("Genre")
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property<string>("LastName")
                    .IsRequired()
                    .HasMaxLength(50);

                b.HasKey("Id");

                b.ToTable("Authors");
            });

            modelBuilder.Entity("Library.API.Entities.Book", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<Guid>("AuthorId");

                b.Property<string>("Description")
                    .HasMaxLength(500);

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(100);

                b.HasKey("Id");

                b.HasIndex("AuthorId");

                b.ToTable("Books");
            });

            modelBuilder.Entity("Library.API.Entities.Book", b => b.HasOne("Library.API.Entities.Author", "Author")
                .WithMany("Books")
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.Cascade));
        }
    }
}
