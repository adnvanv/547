// The client is plain HTML + JavaScript in wwwroot. This host only serves those files,
// on its own port, so every API call from the page is a cross-origin request.
var app = WebApplication.CreateBuilder(args).Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
