using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using minitwit.infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace minitwit.tests;

public class TestAPI : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;
    private MinitwitDbContext _dbContext;
    private SqliteConnection _connection;

    public TestAPI(WebApplicationFactory<Program> fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions
            { AllowAutoRedirect = true, HandleCookies = true });
    }

    private async Task InitializeDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MinitwitDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new MinitwitDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    private async Task DisposeDbContext()
    {
        await _dbContext.DisposeAsync();
        
        _connection.Close();
        _connection.Dispose();
    }


    private async Task<HttpResponseMessage> Register(string username, string? password, string? password2 = null,
        string? email = null)
    {
        if (password2 == null)
        {
            password2 = password;
        }

        if (email == null)
        {
            email = username + "@example.com";
        }

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string?>("Username", username),
            new KeyValuePair<string, string?>("Password", password),
            new KeyValuePair<string, string?>("Password2", password2),
            new KeyValuePair<string, string?>("Email", email),
        });

        return await _client.PostAsync("/register", formData);
    }

    private async Task<HttpResponseMessage> Login(string username, string? password)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string?>("Username", username),
            new KeyValuePair<string, string?>("Password", password),
        });
        return await _client.PostAsync("/login", formData);
    }

    private async Task<HttpResponseMessage> RegisterAndLogin(string username, string? password)
    {
        await Register(username, password);
        return await Login(username, password);
    }

    private async Task<HttpResponseMessage> Logout()
    {
        return await _client.GetAsync("/logout");
    }

    private async Task AddMessage(string text)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("text", text)
        });
        
        var response = await _client.PostAsync("/add_message", formData);
        
        if (!string.IsNullOrEmpty(text) && response.IsSuccessStatusCode)
        {
            Assert.Contains("Your message was recorded", response.Content.ReadAsStringAsync().Result);
        }
    }

    [Fact]
    public async Task TestRegister()
    {
        await InitializeDbContext();
        
        var response = await Register("user1", "default");
        var content = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(content);
        Assert.True(response.IsSuccessStatusCode);
        
        response = await Register("user1", "default");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The username is already taken", content);

        response = await Register("", "default");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a username", content);

        response = await Register("meh", "");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a password", content);

        response = await Register("meh", "x", "y");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The two passwords do not match", content);

        response = await Register("meh", "foo", null, "broken");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a valid email address", content);
        
        await DisposeDbContext();
    }

    [Fact]
    public async Task TestLoginLogout()
    {
        await InitializeDbContext();
        
        var response = await RegisterAndLogin("user1", "default");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You were logged in", content);

        response = await Logout();
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You were logged out", content);

        response = await Login("user1", "wrongpassword");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid password", content);

        response = await Login("user2", "wrongpassword");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username", content);
        
        await DisposeDbContext();
    }
    
        
    [Fact]
    public async Task TestMessageRecording() {
        await InitializeDbContext();
        
        await RegisterAndLogin("foo", "default");
        await AddMessage("test message 1");
        await AddMessage("<test message 2>");
        
        var response = await _client.GetAsync("/public");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("test message 1", content);
        Assert.Contains("&lt;test message 2&gt;", content);
        
        await DisposeDbContext();
    }

    [Fact]
    public async Task TestTimelines() 
    {
        await InitializeDbContext();
        
        await RegisterAndLogin("foo", "default");
        await AddMessage("the message by foo");
        await Logout();
        
        await RegisterAndLogin("bar", "default");
        await AddMessage("the message by bar");
        var response = await _client.GetAsync("/public");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);
        
        // bar's timeline should just show bar's message
        response = await _client.GetAsync("/");
        content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
        
        // now let's follow foo
        response = await _client.GetAsync("/foo/follow");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You are now following &quot;foo&quot;", content);
        
        // we should now see foo's message
        response = await _client.GetAsync("/");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);
        
        // but on the user's page we only want the user's message
        response = await _client.GetAsync("/bar");
        content = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine(content);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
        response = await _client.GetAsync("/foo");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("the message by foo", content);
        Assert.DoesNotContain("the message by bar", content);
        
        // now unfollow and check if that worked
        response = await _client.GetAsync("/foo/unfollow");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You are no longer following &quot;foo&quot;", content);
        response = await _client.GetAsync("/");
        content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
        
        await DisposeDbContext();
    }
}