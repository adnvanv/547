using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;

namespace FitnessStudio.Web.Pages.Instructors;

public class IndexModel(InstructorService instructors) : PageModelBase
{
    public List<Instructor> Instructors { get; private set; } = [];

    public async Task OnGetAsync() => Instructors = await instructors.GetAllAsync();
}
