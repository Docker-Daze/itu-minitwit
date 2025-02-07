using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(800));

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<Author>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ChirpDBContext>();

builder.Services.AddAuthentication()
    .AddCookie()
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["authentication_github_clientId"] ?? string.Empty;
        o.ClientSecret = builder.Configuration["authentication_github_clientSecret"] ?? string.Empty;
        o.CallbackPath ="/signin-github";
    }); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.MapRazorPages();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var chirpContext = scope.ServiceProvider.GetRequiredService<ChirpDBContext>();
    chirpContext.Database.EnsureCreated();
    DbInitializer.SeedDatabase(chirpContext);
}

app.Run();

public partial class Program {} // Necessary to make test successful
