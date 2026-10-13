using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Admin;

/// <summary>Adds a membership plan (no id) or updates an existing one. Admin mode only.</summary>
public class EditModel(MembershipPlanService plans) : AdminPageModelBase
{
    [BindProperty]
    public MembershipPlan Plan { get; set; } = new();

    public bool IsNew => Plan.PlanId == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            var existing = await plans.GetAsync(id.Value);
            if (existing is null)
                return NotFound();
            Plan = existing;
            Plan.Members = [];
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        return IsNew
            ? await SaveAndRedirectAsync(() => plans.AddAsync(Plan), $"Added plan \"{Plan.Name}\".")
            : await SaveAndRedirectAsync(() => plans.UpdateAsync(Plan), $"Updated plan \"{Plan.Name}\".");
    }
}
