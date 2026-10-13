using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages;

public class SettingsModel : PageModelBase
{
    [BindProperty]
    public UserSettings Input { get; set; } = new();

    public void OnGet() => Input = UserSettings.Load(Request);

    public IActionResult OnPost()
    {
        Input.Save(Response);
        StatusMessage = "Settings saved.";
        return RedirectToPage();
    }

    public IActionResult OnPostReset()
    {
        new UserSettings().Save(Response);
        StatusMessage = "Settings reset to defaults.";
        return RedirectToPage();
    }
}
