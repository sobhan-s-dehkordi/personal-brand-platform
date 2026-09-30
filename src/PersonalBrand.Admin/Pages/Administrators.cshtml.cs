using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PersonalBrand.Admin.Pages;

public sealed class AdministratorsModel(UserManager<IdentityUser> users, RoleManager<IdentityRole> roles) : PageModel
{
    [BindProperty]
    public AdminInput Input { get; set; } = new();
    public IList<IdentityUser> Administrators { get; set; } = [];

    public async Task OnGetAsync() => Administrators = await users.GetUsersInRoleAsync("Administrator");
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        if (await users.FindByEmailAsync(Input.Email.Trim()) is not null)
        {
            ModelState.AddModelError("Input.Email", "This email is already registered.");
            await OnGetAsync();
            return Page();
        }

        if (!await roles.RoleExistsAsync("Administrator"))
        {
            var roleResult = await roles.CreateAsync(new IdentityRole("Administrator"));
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError("", "Unable to create the administrator role.");
                await OnGetAsync();
                return Page();
            }
        }

        var user = new IdentityUser
        {
            UserName = Input.Email.Trim(),
            Email = Input.Email.Trim(),
            EmailConfirmed = true
        };
        var result = await users.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            result = await users.AddToRoleAsync(user, "Administrator");
            if (!result.Succeeded)
                await users.DeleteAsync(user);
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            await OnGetAsync();
            return Page();
        }

        TempData["Message"] = "Administrator created. Share the login details securely.";
        return RedirectToPage();
    }
}

public sealed class AdminInput
{
    [Required(ErrorMessage = "Enter an email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 12), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password), ErrorMessage = "The passwords do not match."), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}
