using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TradicioCarnica.Pages.Admin.Users
{
    public class UpsertModel : PageModel
    {
        private const string AdminRole = "Admin";

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UpsertModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool IsEdit => !string.IsNullOrEmpty(Input.Id);

        public class InputModel
        {
            public string? Id { get; set; }

            [Required(ErrorMessage = "El email es obligatorio")]
            [EmailAddress]
            public string Email { get; set; } = "";

            [DataType(DataType.Password)]
            public string? Password { get; set; }

            public bool IsAdmin { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null) return Page();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            Input = new InputModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                IsAdmin = await _userManager.IsInRoleAsync(user, AdminRole)
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!IsEdit && string.IsNullOrWhiteSpace(Input.Password))
                ModelState.AddModelError("Input.Password", "La contraseña es obligatoria.");

            if (!ModelState.IsValid) return Page();

            IdentityUser user;
            IdentityResult result;

            if (!IsEdit)
            {
                user = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true   // RequireConfirmedAccount = true en Program.cs
                };
                result = await _userManager.CreateAsync(user, Input.Password!);
                if (!Check(result)) return Page();
            }
            else
            {
                user = (await _userManager.FindByIdAsync(Input.Id!))!;
                if (user == null) return NotFound();

                user.UserName = Input.Email;
                user.Email = Input.Email;
                result = await _userManager.UpdateAsync(user);
                if (!Check(result)) return Page();

                if (!string.IsNullOrWhiteSpace(Input.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
                    if (!Check(result)) return Page();
                }
            }

            if (!await _roleManager.RoleExistsAsync(AdminRole))
                await _roleManager.CreateAsync(new IdentityRole(AdminRole));

            var isAdmin = await _userManager.IsInRoleAsync(user, AdminRole);

            if (Input.IsAdmin && !isAdmin)
                await _userManager.AddToRoleAsync(user, AdminRole);
            else if (!Input.IsAdmin && isAdmin)
            {
                if (user.Id == _userManager.GetUserId(User))
                {
                    ModelState.AddModelError(string.Empty, "No puedes quitarte a ti mismo el rol de administrador.");
                    return Page();
                }
                await _userManager.RemoveFromRoleAsync(user, AdminRole);
            }

            return RedirectToPage("Index");
        }

        private bool Check(IdentityResult result)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return result.Succeeded;
        }
    }
}