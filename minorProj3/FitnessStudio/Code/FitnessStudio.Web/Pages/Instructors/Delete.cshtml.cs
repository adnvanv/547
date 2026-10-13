using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Instructors;

public class DeleteModel(InstructorService instructors) : PageModelBase
{
    public Instructor Instructor { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var instructor = await instructors.GetAsync(id);
        if (instructor is null)
            return NotFound();
        Instructor = instructor;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await instructors.DeleteAsync(id);
            StatusMessage = $"Deleted instructor #{id}.";
        }
        catch (DataLayerException ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage("Index");
    }
}
