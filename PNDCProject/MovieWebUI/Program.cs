var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(5);
});

// Register IHttpContextAccessor for _Layout injection
builder.Services.AddHttpContextAccessor();  // <-- ye line add karo

// HttpClients for backend
builder.Services.AddHttpClient("auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7016/api/");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddHttpClient("movies", client =>
{
    client.BaseAddress = new Uri("https://localhost:7288/api/");
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
