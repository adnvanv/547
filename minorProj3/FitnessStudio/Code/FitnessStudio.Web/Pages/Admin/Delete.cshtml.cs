using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Admin;

public class DeleteModel(MembershipPlanService plans) : AdminPageModelBase
{
    public MembershipPlan Plan { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var plan = await plans.GetAsync(id);
        if (plan is null)
            return NotFound();
        Plan = plan;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await plans.DeleteAsync(id);
            StatusMessage = $"Deleted plan #{id}.";
        }
        catch (DataLayerException ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage("Index");
    }
}
