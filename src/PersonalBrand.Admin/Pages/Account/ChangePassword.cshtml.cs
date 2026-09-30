using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PersonalBrand.Admin.Pages.Account;

public sealed class ChangePasswordModel(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn) : PageModel
{
    [BindProperty, Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [BindProperty, Required, StringLength(128, MinimumLength = 12), DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [BindProperty, Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Challenge();
        var result = await users.ChangePasswordAsync(user, CurrentPassword, NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return Page();
        }

        await signIn.RefreshSignInAsync(user);
        TempData["Message"] = "Your password was changed successfully.";
        return RedirectToPage();
    }
}
