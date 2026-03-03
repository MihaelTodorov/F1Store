using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace F1Store.Infrastructure.Data.Infrastructure
{
    public static class ApplicationBuilderExtension
    {
        public static async Task<IApplicationBuilder> PrepareDatabase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var services = serviceScope.ServiceProvider;

            await RoleSeeder(services);
            await SeedAdministrator(services);

            var dataCategory = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SeedCategories(dataCategory);

            var dataTeam = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SeedTeams(dataTeam);

            return app;
        }

        private static async Task RoleSeeder(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Administrator", "Client" };

            foreach (var role in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);

                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdministrator(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByNameAsync("admin") == null)
            {
                ApplicationUser user = new ApplicationUser();
                user.FirstName = "admin";
                user.LastName = "admin";
                user.UserName = "admin";
                user.Email = "admin@admin.com";
                user.Address = "admin address";
                user.PhoneNumber = "0888888888";

                var result = await userManager.CreateAsync(user, "Admin123456");

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Administrator").Wait();
                }
            }
        }

        private static void SeedCategories(ApplicationDbContext dataCategory)
        {
            if (dataCategory.Categories.Any())
            {
                return;
            }

            dataCategory.Categories.AddRange(new[]
            {
                new Category { CategoryName = "Accessories" },   //must add alphabetical
                new Category { CategoryName = "Auto Accessories" },
                new Category { CategoryName = "Backpacks & Bags" },
                new Category { CategoryName = "Collectibles & Memorabilia" },
                new Category { CategoryName = "Face Coverings" },
                new Category { CategoryName = "Footwear" },
                new Category { CategoryName = "Headwear" },
                new Category { CategoryName = "Home & Office" },
                new Category { CategoryName = "Hoodies & Sweatshirts" },
                new Category { CategoryName = "Jackets" },
                new Category { CategoryName = "Jerseys" },
                new Category { CategoryName = "Pajamas & Underwear" },
                new Category { CategoryName = "Pants" },
                new Category { CategoryName = "Polos" },
                new Category { CategoryName = "Shirts & Sweaters" },
                new Category { CategoryName = "Shorts" },
                new Category { CategoryName = "Swim & Beach" },
                new Category { CategoryName = "T-Shirts" },
            });

            dataCategory.SaveChanges();
        }

        private static void SeedTeams(ApplicationDbContext dataTeam)
        {
            if (dataTeam.Teams.Any())
            {
                return;
            }

            dataTeam.Teams.AddRange(new[]
            {
                new Team { TeamName = "Alpine" },
                new Team { TeamName = "Aston Martin" },
                new Team { TeamName = "Audi" },
                new Team { TeamName = "Cadillac" },
                new Team { TeamName = "Ferrari" },
                new Team { TeamName = "Haas" },
                new Team { TeamName = "McLaren" },
                new Team { TeamName = "Mercedes" },
                new Team { TeamName = "Racing Bulls" },
                new Team { TeamName = "Red Bull" },
                new Team { TeamName = "Williams" },
            });

            dataTeam.SaveChanges();
        }
    }
}
