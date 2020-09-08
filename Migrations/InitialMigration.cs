﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Library.API.Migrations
{
    // ReSharper disable once UnusedType.Global
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    DateOfBirth = table.Column<DateTimeOffset>(),
                    FirstName = table.Column<string>(maxLength: 50),
                    Genre = table.Column<string>(maxLength: 50),
                    LastName = table.Column<string>(maxLength: 50)
                },
                constraints: table => table.PrimaryKey("PK_Authors", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(),
                    AuthorId = table.Column<Guid>(),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Title = table.Column<string>(maxLength: 100)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorId",
                table: "Books",
                column: "AuthorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
