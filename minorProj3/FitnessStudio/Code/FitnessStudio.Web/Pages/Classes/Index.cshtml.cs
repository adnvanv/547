using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;

namespace FitnessStudio.Web.Pages.Classes;

public class IndexModel(FitnessClassService classes) : PageModelBase
{
    public List<FitnessClass> Classes { get; private set; } = [];

    public async Task OnGetAsync() => Classes = await classes.GetAllAsync();
}
