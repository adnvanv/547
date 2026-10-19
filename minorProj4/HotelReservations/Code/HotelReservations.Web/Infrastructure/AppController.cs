using HotelReservations.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelReservations.Web.Infrastructure;

/// <summary>Common helpers for the CRUD controllers: status messages and data-layer error handling.</summary>
public abstract class AppController : Controller
{
    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    protected UserSettings Settings => UserSettings.Load(Request);

    /// <summary>Translates interface text (including data-layer error messages) into the chosen language.</summary>
    protected string T(string english) => Settings.T(english);

    /// <summary>
    /// Runs a database write. On success, sets the status message and redirects to Index;
    /// on a <see cref="DataLayerException"/>, adds the error to ModelState and redisplays the form.
    /// </summary>
    protected async Task<IActionResult> SaveAndRedirectAsync(Func<Task> action, string successMessage,
                                                             Func<Task<IActionResult>> redisplay)
    {
        if (!ModelState.IsValid)
            return await redisplay();
        try
        {
            await action();
            StatusMessage = successMessage;
            return RedirectToAction("Index");
        }
        catch (DataLayerException ex)
        {
            ModelState.AddModelError(string.Empty, T(ex.Message));
            return await redisplay();
        }
    }

    /// <summary>Runs a delete; any error (e.g. record still in use) is shown on the list page.</summary>
    protected async Task<IActionResult> DeleteAndRedirectAsync(Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            StatusMessage = successMessage;
        }
        catch (DataLayerException ex)
        {
            ErrorMessage = T(ex.Message);
        }
        return RedirectToAction("Index");
    }
}

/// <summary>Redirects to Admin/Locked unless Administrator mode is on in Settings.</summary>
public class AdminOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var exempt = context.ActionDescriptor.EndpointMetadata.OfType<AllowWithoutAdminModeAttribute>().Any();
        if (!exempt && !UserSettings.Load(context.HttpContext.Request).AdminMode)
            context.Result = new RedirectToActionResult("Locked", "Admin", null);
    }
}

/// <summary>Marks an admin action (the "locked" page) that stays reachable when Administrator mode is off.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AllowWithoutAdminModeAttribute : Attribute;
