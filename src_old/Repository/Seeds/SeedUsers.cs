using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Domain.Consts;

namespace Repository.Seeds
{
    public static class SeedUsers
    {
        public async static Task Seed(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var usersWithRoleUser = await userManager.GetUsersInRoleAsync(Roles.User);
            var usersWithRoleAdmin = await userManager.GetUsersInRoleAsync(Roles.Admin);

            if (!usersWithRoleUser.Any())
            {
                var user = new User
                {
                    Email = "uzytkownik@test1.com",
                    UserName = "uzytkownikTest1",
                    NormalizedEmail = "uzytkownik@test1.com".ToUpper(),
                    NormalizedUserName = "uzytkownikTest1".ToUpper(),
                    EmailConfirmed = true,
                    UserSettings = new UserSetting
                    {
                        IsDarkMode = false,
                        ReceiveNotifications = true
                    }
                };

                await userManager.CreateAsync(user, "Uzytkownik1");
                await userManager.AddToRoleAsync(user, Roles.User);
            }

            if (!usersWithRoleAdmin.Any())
            {
                var user = new User
                {
                    Email = "admin@test1.com",
                    UserName = "adminTest1",
                    NormalizedEmail = "admin@test1.com".ToUpper(),
                    NormalizedUserName = "adminTest1".ToUpper(),
                    EmailConfirmed = true,
                    UserSettings = new UserSetting
                    {
                        IsDarkMode = false,
                        ReceiveNotifications = true
                    }
                };

                await userManager.CreateAsync(user, "AdminTest1");
                await userManager.AddToRoleAsync(user, Roles.Admin);
            }
        }
    }
}
