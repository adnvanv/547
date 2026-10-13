using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Instructors;

/// <summary>Adds an instructor (no id) or updates an existing one.</summary>
public class EditModel(InstructorService instructors) : PageModelBase
{
    [BindProperty]
    public Instructor Instructor { get; set; } = new();

    public bool IsNew => Instructor.InstructorId == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            var existing = await instructors.GetAsync(id.Value);
            if (existing is null)
                return NotFound();
            Instructor = existing;
            Instructor.Classes = [];
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        return IsNew
            ? await SaveAndRedirectAsync(() => instructors.AddAsync(Instructor), $"Added instructor {Instructor.FullName}.")
            : await SaveAndRedirectAsync(() => instructors.UpdateAsync(Instructor), $"Updated instructor {Instructor.FullName}.");
    }
}
