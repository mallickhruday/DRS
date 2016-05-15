using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Web.Models;

namespace web.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasAnnotation("Relational:Name", "RoleNameIndex");

                    b.HasAnnotation("Relational:TableName", "AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasAnnotation("Relational:TableName", "AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasAnnotation("Relational:TableName", "AspNetUserRoles");
                });

            modelBuilder.Entity("Web.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasAnnotation("Relational:Name", "EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .HasAnnotation("Relational:Name", "UserNameIndex");

                    b.HasAnnotation("Relational:TableName", "AspNetUsers");
                });

            modelBuilder.Entity("Web.Models.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Abstract")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<string>("CreatedByUserId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTimeOffset>("CreatedOn");

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<int>("Status");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 60);

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Documents");
                });

            modelBuilder.Entity("Web.Models.DocumentContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<int>("DocumentId");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "DocumentContents");
                });

            modelBuilder.Entity("Web.Models.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedByUserId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTimeOffset>("CreatedOn");

                    b.Property<int>("DocumentId");

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 16);

                    b.Property<string>("Key")
                        .HasAnnotation("MaxLength", 1024);

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<int>("PageCount");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<long>("Size");

                    b.Property<int>("Status");

                    b.Property<string>("ThumbnailPath")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int>("VersionNum");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Files");
                });

            modelBuilder.Entity("Web.Models.Library", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedByUserId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTimeOffset>("CreatedOn");

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 56);

                    b.Property<int>("Status");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Libraries");
                });

            modelBuilder.Entity("Web.Models.LibraryDocument", b =>
                {
                    b.Property<int>("LibraryId");

                    b.Property<int>("DocumentId");

                    b.HasKey("LibraryId", "DocumentId");

                    b.HasAnnotation("Relational:TableName", "LibraryDocuments");
                });

            modelBuilder.Entity("Web.Models.PermissionType", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "Lookup");

                    b.HasAnnotation("Relational:TableName", "PermissionTypes");
                });

            modelBuilder.Entity("Web.Models.StatusType", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "Lookup");

                    b.HasAnnotation("Relational:TableName", "StatusTypes");
                });

            modelBuilder.Entity("Web.Models.UserDocument", b =>
                {
                    b.Property<int>("DocumentId");

                    b.Property<string>("ApplicationUserId");

                    b.Property<int>("Permissions");

                    b.HasKey("DocumentId", "ApplicationUserId");

                    b.HasAnnotation("Relational:TableName", "UserDocuments");
                });

            modelBuilder.Entity("Web.Models.UserLibrary", b =>
                {
                    b.Property<int>("LibraryId");

                    b.Property<string>("ApplicationUserId");

                    b.Property<int>("Permissions");

                    b.HasKey("LibraryId", "ApplicationUserId");

                    b.HasAnnotation("Relational:TableName", "UserLibraries");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNet.Identity.EntityFramework.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Web.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Web.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNet.Identity.EntityFramework.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId");

                    b.HasOne("Web.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Web.Models.DocumentContent", b =>
                {
                    b.HasOne("Web.Models.Document")
                        .WithOne()
                        .HasForeignKey("Web.Models.DocumentContent", "DocumentId");
                });

            modelBuilder.Entity("Web.Models.File", b =>
                {
                    b.HasOne("Web.Models.Document")
                        .WithMany()
                        .HasForeignKey("DocumentId");
                });

            modelBuilder.Entity("Web.Models.LibraryDocument", b =>
                {
                    b.HasOne("Web.Models.Document")
                        .WithMany()
                        .HasForeignKey("DocumentId");

                    b.HasOne("Web.Models.Library")
                        .WithMany()
                        .HasForeignKey("LibraryId");
                });

            modelBuilder.Entity("Web.Models.UserDocument", b =>
                {
                    b.HasOne("Web.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");

                    b.HasOne("Web.Models.Document")
                        .WithMany()
                        .HasForeignKey("DocumentId");
                });

            modelBuilder.Entity("Web.Models.UserLibrary", b =>
                {
                    b.HasOne("Web.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");

                    b.HasOne("Web.Models.Library")
                        .WithMany()
                        .HasForeignKey("LibraryId");
                });
        }
    }
}