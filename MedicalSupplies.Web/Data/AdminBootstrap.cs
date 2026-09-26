using MedicalSupplies.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace MedicalSupplies.Web.Data;

public static class AdminBootstrap
{
    public static async Task CreateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var email = ReadRequired("SuperAdmin email: ");

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            if (await userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
            {
                Console.WriteLine("This user is already a SuperAdmin.");
                return;
            }

            Console.WriteLine(
                "A user with that email already exists but is not a SuperAdmin. No changes were made.");
            return;
        }

        var fullName = ReadRequired("Full name: ");
        var password = ReadPassword("Password: ");
        var confirmPassword = ReadPassword("Confirm password: ");

        if (password != confirmPassword)
        {
            Console.WriteLine("Passwords do not match. No user was created.");
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            Console.WriteLine("Could not create the SuperAdmin:");

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }

            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, "SuperAdmin");

        if (!roleResult.Succeeded)
        {
            Console.WriteLine("The user was created, but assigning SuperAdmin failed:");

            foreach (var error in roleResult.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }

            return;
        }

        Console.WriteLine($"SuperAdmin created successfully for {email}.");
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("This value is required.");
        }
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);

        var password = string.Empty;
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();

        return password;
    }
}
