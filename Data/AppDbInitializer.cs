using Microsoft.AspNetCore.Identity;
using PharmacyChain.Models;

namespace PharmacyChain.Data
{
    public static class AppDbInitializer
    {
        public static async Task SeedRolesAndUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Roles
            string[] roles = { "Admin", "Pharmacist", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Admin
            var adminEmail = "admin@pharmacy.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var res = await userManager.CreateAsync(admin, "Admin123!");
                if (res.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Pharmacist
            var pharmEmail = "pharmacist@pharmacy.com";
            var pharm = await userManager.FindByEmailAsync(pharmEmail);

            if (pharm == null)
            {
                pharm = new ApplicationUser
                {
                    UserName = "pharmacist",
                    Email = pharmEmail,
                    FullName = "Default Pharmacist",
                    EmailConfirmed = true
                };

                var res = await userManager.CreateAsync(pharm, "Pharm123!");
                if (res.Succeeded)
                    await userManager.AddToRoleAsync(pharm, "Pharmacist");
            }

            // User
            var userEmail = "user@pharmacy.com";
            var basicUser = await userManager.FindByEmailAsync(userEmail);

            if (basicUser == null)
            {
                basicUser = new ApplicationUser
                {
                    UserName = "user",
                    Email = userEmail,
                    FullName = "Basic User",
                    EmailConfirmed = true
                };

                var res = await userManager.CreateAsync(basicUser, "User123!");
                if (res.Succeeded)
                    await userManager.AddToRoleAsync(basicUser, "User");
            }
        }
    }
}
