using FitnessStudio.Data;
using FitnessStudio.Web.Infrastructure;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

// Prices are in US dollars regardless of the machine's regional settings.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Data layer (FitnessStudio.Data project)
builder.Services.AddDbContext<FitnessStudioContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FitnessStudioDB")
                         ?? FitnessStudioContext.DefaultConnectionString));
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<InstructorService>();
builder.Services.AddScoped<FitnessClassService>();
builder.Services.AddScoped<MembershipPlanService>();

// User settings (theme, date format, admin mode...) read from a cookie
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext is { } context
        ? UserSettings.Load(context.Request)
        : new UserSettings());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
