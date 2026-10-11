using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TradicioCarnica.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        public IndexModel(UserManager<IdentityUser> userManager) => _userManager = userManager;

        public IList<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public HashSet<string> Admins { get; set; } = new();

        [TempData]
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            Users = _userManager.Users.OrderBy(u => u.Email).ToList();
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            Admins = admins.Select(a => a.Id).ToHashSet();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (id == _userManager.GetUserId(User))
            {
                Error = "No puedes eliminar tu propio usuario.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                Error = string.Join(" ", result.Errors.Select(e => e.Description));

            return RedirectToPage();
        }
    }
}