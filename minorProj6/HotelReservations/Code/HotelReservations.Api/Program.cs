using HotelReservations.Api;
using HotelReservations.Business;
using HotelReservations.Data;

var builder = WebApplication.CreateBuilder(args);

// Data layer (HotelReservations.Data project, ADO.NET)
builder.Services.AddSingleton(new SqlDatabase(
    builder.Configuration.GetConnectionString("HotelReservationsDB") ?? SqlDatabase.DefaultConnectionString));
builder.Services.AddScoped<RoomTypeRepository>();
builder.Services.AddScoped<RoomRepository>();
builder.Services.AddScoped<GuestRepository>();
builder.Services.AddScoped<ReservationRepository>();

// Business layer (HotelReservations.Business project)
builder.Services.AddScoped<RoomTypeManager>();
builder.Services.AddScoped<RoomManager>();
builder.Services.AddScoped<GuestManager>();
builder.Services.AddScoped<ReservationManager>();

// Azure Functions (HotelReservations.Functions project). The business layer's AzureFunctionsClient gets an
// HttpClient pointed at the function app; set AzureFunctions:BaseUrl in appsettings.json.
var functionsUrl = builder.Configuration["AzureFunctions:BaseUrl"]
    ?? throw new InvalidOperationException("AzureFunctions:BaseUrl is missing from appsettings.json.");
builder.Services.AddHttpClient<AzureFunctionsClient>(http =>
{
    http.BaseAddress = new Uri(functionsUrl.TrimEnd('/') + "/");
    // A function app on the Consumption plan can take several seconds to wake up from idle.
    http.Timeout = TimeSpan.FromSeconds(30);
});

// Service layer
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();

// CORS: browsers block fetch() calls from the client's origin (a different port) unless the API
// answers with Access-Control-Allow-Origin. Allowed origins are listed in appsettings.json.
const string ClientPolicy = "HotelClient";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(ClientPolicy, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .WithMethods("GET", "POST", "DELETE")));

var app = builder.Build();

app.UseExceptionHandler();
// UseCors must run before the endpoints so preflight (OPTIONS) requests get answered,
// and error responses (404/400/500) still carry the CORS header the browser needs to read them.
app.UseCors(ClientPolicy);

app.MapGet("/", () => Results.Json(new
{
    service = "Hotel Reservations API",
    endpoints = new[] { "/api/roomtypes", "/api/rooms", "/api/guests", "/api/reservations",
                        "/api/reservations/{id}/quote", "/api/guests/{id}/loyalty" },
}));
app.MapControllers();

app.Run();
