using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using minitwit.infrastructure;
using Xunit;

namespace minitwit.tests;

public class TestAPI : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly HttpClient _client;
    private readonly MinitwitDbContext _dbContext;

    public TestAPI(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions
            { AllowAutoRedirect = true, HandleCookies = true });

        SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var builder = new DbContextOptionsBuilder<MinitwitDbContext>().UseSqlite(connection);
        _dbContext = new MinitwitDbContext(builder.Options);

        _dbContext.Database.EnsureCreated();
    }

    private async Task<HttpResponseMessage> Register(string username, string password, string password2 = null,
        string email = null)
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
            new KeyValuePair<string, string>("Username", username),
            new KeyValuePair<string, string>("Password", password),
            new KeyValuePair<string, string>("Password2", password2),
            new KeyValuePair<string, string>("Email", email),
        });

        return await _client.PostAsync("/register", formData);
    }

    private async Task<HttpResponseMessage> Login(string username, string password)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", username),
            new KeyValuePair<string, string>("Password", password),
        });
        return await _client.PostAsync("/login", formData);
    }

    private async Task<HttpResponseMessage> RegisterAndLogin(string username, string password)
    {
        await Register(username, password);
        return await Login(username, password);
    }

    private async Task<HttpResponseMessage> Logout()
    {
        return await _client.GetAsync("/logout");
    }

    private async Task<HttpResponseMessage> AddMessage(string text)
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
        return response;
    }

    [Fact]
    public async Task TestRegister()
    {
        var response = await Register("user1", "default");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You were successfully registered and can login now", content);

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
    }

    [Fact]
    public async Task TestLoginLogout()
    {
        await _dbContext.DisposeAsync();
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
    }
    
        
    [Fact]
    public async Task TestMessageRecording() {
        await RegisterAndLogin("foo", "default");
        await AddMessage("the message by foo");
        await AddMessage("<test message 2>");
        
        var response = await _client.GetAsync("/public");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("test message 1", content);
        Assert.Contains("&lt;test message 2&gt;", content);
    }

    [Fact]
    public async Task TestTimelines() {
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
        Assert.Contains("You are now following &#34;foo&#34;", content);
        
        // we should now see foo's message
        response = await _client.GetAsync("/");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);
        
        // but on the user's page we only want the user's message
        response = await _client.GetAsync("/bar");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("the message by foo", content);
        Assert.DoesNotContain("the message by bar", content);
        
        // now unfollow and check if that worked
        response = await _client.GetAsync("/foo/unfollow");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You are no longer following &#34;foo&#34;", content);
        response = await _client.GetAsync("/");
        content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
    }
}