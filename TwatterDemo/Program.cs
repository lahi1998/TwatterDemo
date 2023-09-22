using Microsoft.EntityFrameworkCore;
using TwatterDemo;
var builder = WebApplication.CreateBuilder(args);

// Configure your database connection here
string connectionString = "Server=localhost;Database=twatterdb;User=Remote;Password=Kode1234!;";
// Specify the server version
ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<DBConnector>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 26)))
);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Add the session middleware

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure your session settings if needed
app.UseSession(new SessionOptions
{
    // Set your desired session options here, such as IdleTimeout, Cookie, etc.
});

app.Run();
