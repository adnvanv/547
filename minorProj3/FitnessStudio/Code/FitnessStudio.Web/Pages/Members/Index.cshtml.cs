using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudio.Web.Pages.Members;

public class IndexModel(MemberService members) : PageModelBase
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public MemberSearchField Field { get; set; } = MemberSearchField.Any;

    [BindProperty(SupportsGet = true)]
    public MatchMode Match { get; set; } = MatchMode.Contains;

    public List<Member> Results { get; private set; } = [];

    public bool IsSearching => !string.IsNullOrWhiteSpace(Search);

    public async Task OnGetAsync() => Results = await members.SearchAsync(Search, Field, Match);
}
