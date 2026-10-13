using FitnessStudio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessStudio.Web.Infrastructure;

/// <summary>Common helpers for the CRUD pages: status messages and data-layer error handling.</summary>
public abstract class PageModelBase : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Runs a database write. On success, sets the status message and redirects to the list page;
    /// on a <see cref="DataLayerException"/>, shows the error on the current page instead.
    /// </summary>
    protected async Task<IActionResult> SaveAndRedirectAsync(Func<Task> action, string successMessage,
                                                             string listPage = "./Index")
    {
        try
        {
            await action();
            StatusMessage = successMessage;
            return RedirectToPage(listPage);
        }
        catch (DataLayerException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await OnSaveFailedAsync();
            return Page();
        }
    }

    /// <summary>Reloads lookup lists (dropdowns etc.) before redisplaying a page after an error.</summary>
    protected virtual Task OnSaveFailedAsync() => Task.CompletedTask;
}

/// <summary>Base for the Administration pages: redirects to the "locked" page unless admin mode is on.</summary>
public abstract class AdminPageModelBase : PageModelBase
{
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!UserSettings.Load(Request).AdminMode)
            context.Result = RedirectToPage("/Admin/Locked");
    }
}
