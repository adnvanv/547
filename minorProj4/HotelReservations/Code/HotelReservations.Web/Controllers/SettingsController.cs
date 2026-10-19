using HotelReservations.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Web.Controllers;

public class SettingsController : AppController
{
    public IActionResult Index() => View(Settings);

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index(UserSettings input)
    {
        input.Save(Response);
        StatusMessage = input.T("Settings saved.");
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        new UserSettings().Save(Response);
        StatusMessage = "Settings reset to defaults.";
        return RedirectToAction("Index");
    }
}
