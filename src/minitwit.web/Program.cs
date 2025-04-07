using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using minitwit.core;
using minitwit.infrastructure;
using Microsoft.AspNetCore.Identity;
using minitwit.web;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Network;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddSingleton<MetricsService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Configuration.AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var outputTemplate = "{Timestamp:dd-MM-YYYY HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Information()
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    // Buffer and batch write to console
    .WriteTo.Async(a => a.Console(new RenderedCompactJsonFormatter()))
    // Buffer, batch write, and configure TCP with retry and circuit breaker
    .WriteTo.Async(a => a.TCPSink(
        "tcp://209.38.112.21:5012",
        new RenderedCompactJsonFormatter()
    ))
    // Filter out noise
    .Filter.ByExcluding(evt =>
        evt.Level == LogEventLevel.Information &&
        evt.Properties.ContainsKey("RequestPath") &&
        evt.Properties["RequestPath"].ToString().Contains("/metrics"))
);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MinitwitDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<MinitwitDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseMiddleware<LogEnrichmentMiddleware>();
app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

Metrics.SuppressDefaultMetrics();
app.UseHttpMetrics();
app.MapMetrics();



using (var scope = app.Services.CreateScope())
{
    var minitwitDbContext = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
    minitwitDbContext.Database.EnsureCreated();
    //DbInitializer.SeedDatabase(minitwitDbContext);
}

app.Run();

Log.CloseAndFlush(); //Clean and shutdown logs

public partial class Program {}