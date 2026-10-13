using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Classes;

public class DeleteModel(FitnessClassService classes) : PageModelBase
{
    public FitnessClass FitnessClass { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var fitnessClass = await classes.GetAsync(id);
        if (fitnessClass is null)
            return NotFound();
        FitnessClass = fitnessClass;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await classes.DeleteAsync(id);
            StatusMessage = $"Deleted class #{id}.";
        }
        catch (DataLayerException ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage("Index");
    }
}
