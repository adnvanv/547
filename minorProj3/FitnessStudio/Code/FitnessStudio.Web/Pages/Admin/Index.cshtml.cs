using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;

namespace FitnessStudio.Web.Pages.Admin;

public class IndexModel(MembershipPlanService plans) : AdminPageModelBase
{
    public List<MembershipPlan> Plans { get; private set; } = [];

    public async Task OnGetAsync() => Plans = await plans.GetAllAsync();
}
