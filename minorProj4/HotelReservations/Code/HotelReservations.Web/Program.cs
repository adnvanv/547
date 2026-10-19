using System.Globalization;
using HotelReservations.Data;
using HotelReservations.Web.Infrastructure;

// Prices are in US dollars regardless of the machine's regional settings.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Data layer (HotelReservations.Data project, ADO.NET)
builder.Services.AddSingleton(new SqlDatabase(
    builder.Configuration.GetConnectionString("HotelReservationsDB") ?? SqlDatabase.DefaultConnectionString));
builder.Services.AddScoped<GuestRepository>();
builder.Services.AddScoped<RoomRepository>();
builder.Services.AddScoped<ReservationRepository>();
builder.Services.AddScoped<RoomTypeRepository>();

// User settings (theme, language, date format, admin mode...) read from a cookie
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext is { } context
        ? UserSettings.Load(context.Request)
        : new UserSettings());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
