using Microsoft.EntityFrameworkCore;
using minitwit.core;
using minitwit.infrastructure;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MinitwitDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<MinitwitDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var minitwitDbContext = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
    minitwitDbContext.Database.EnsureCreated();
    DbInitializer.SeedDatabase(minitwitDbContext);
}

app.Run();
