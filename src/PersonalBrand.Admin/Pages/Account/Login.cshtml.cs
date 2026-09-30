using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PersonalBrand.Admin.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(SignInManager<IdentityUser> signIn, UserManager<IdentityUser> users) : PageModel
{
    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty, Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        if (!ModelState.IsValid)
            return Page();
        var user = await users.FindByEmailAsync(Email);
        var result = user is null ? Microsoft.AspNetCore.Identity.SignInResult.Failed : await signIn.PasswordSignInAsync(user, Password, false, true);
        if (result.Succeeded)
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        ModelState.AddModelError("", "Sign-in failed. Check your email and password or try again later.");
        return Page();
    }
}
