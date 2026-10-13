using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessStudio.Web.Pages.Classes;

/// <summary>Adds a class (no id) or updates an existing one.</summary>
public class EditModel(FitnessClassService classes, InstructorService instructors) : PageModelBase
{
    [BindProperty]
    public FitnessClass FitnessClass { get; set; } = new();

    public SelectList InstructorOptions { get; private set; } = default!;

    public bool IsNew => FitnessClass.ClassId == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            var existing = await classes.GetAsync(id.Value);
            if (existing is null)
                return NotFound();
            FitnessClass = existing;
        }
        await LoadInstructorsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!FitnessClass.Days.Contains(FitnessClass.DayOfWeek))
            ModelState.AddModelError("FitnessClass.DayOfWeek", "Pick a day of the week.");

        if (!ModelState.IsValid)
        {
            await LoadInstructorsAsync();
            return Page();
        }

        FitnessClass.Instructor = null;
        return IsNew
            ? await SaveAndRedirectAsync(() => classes.AddAsync(FitnessClass), $"Added class \"{FitnessClass.Title}\".")
            : await SaveAndRedirectAsync(() => classes.UpdateAsync(FitnessClass), $"Updated class \"{FitnessClass.Title}\".");
    }

    protected override Task OnSaveFailedAsync() => LoadInstructorsAsync();

    private async Task LoadInstructorsAsync()
    {
        var all = await instructors.GetAllAsync();
        InstructorOptions = new SelectList(
            all.Select(i => new { i.InstructorId, Label = $"{i.FullName} ({i.Specialty})" }),
            "InstructorId", "Label", FitnessClass.InstructorId);
    }
}
