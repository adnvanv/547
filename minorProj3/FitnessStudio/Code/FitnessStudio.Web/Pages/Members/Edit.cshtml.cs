using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessStudio.Web.Pages.Members;

/// <summary>Adds a member (no id) or updates an existing one.</summary>
public class EditModel(MemberService members, MembershipPlanService plans) : PageModelBase
{
    [BindProperty]
    public Member Member { get; set; } = new();

    public SelectList PlanOptions { get; private set; } = default!;

    public bool IsNew => Member.MemberId == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is not null)
        {
            var existing = await members.GetAsync(id.Value);
            if (existing is null)
                return NotFound();
            Member = existing;
        }
        await LoadPlansAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadPlansAsync();
            return Page();
        }

        Member.Plan = null;
        return IsNew
            ? await SaveAndRedirectAsync(() => members.AddAsync(Member), $"Added member {Member.FullName}.")
            : await SaveAndRedirectAsync(() => members.UpdateAsync(Member), $"Updated member {Member.FullName}.");
    }

    protected override Task OnSaveFailedAsync() => LoadPlansAsync();

    // New members can only pick active plans; an existing member keeps their current plan in the list.
    private async Task LoadPlansAsync()
    {
        var all = await plans.GetAllAsync();
        var choices = all.Where(p => p.IsActive || p.PlanId == Member.PlanId)
                         .Select(p => new { p.PlanId, Label = $"{p.Name} ({p.MonthlyPrice:C}/mo)" });
        PlanOptions = new SelectList(choices, "PlanId", "Label", Member.PlanId);
    }
}
