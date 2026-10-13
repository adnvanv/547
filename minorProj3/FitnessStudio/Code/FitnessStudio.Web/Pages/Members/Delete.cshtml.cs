using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Members;

public class DeleteModel(MemberService members) : PageModelBase
{
    public Member Member { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var member = await members.GetAsync(id);
        if (member is null)
            return NotFound();
        Member = member;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await members.DeleteAsync(id);
            StatusMessage = $"Deleted member #{id}.";
        }
        catch (DataLayerException ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage("Index");
    }
}
