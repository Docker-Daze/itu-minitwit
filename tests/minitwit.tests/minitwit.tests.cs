using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using minitwit.infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace minitwit.tests;

public class TestAPI : IClassFixture<WebApplicationFactory<Program>>, IDisposable
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
        await _dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    private async Task DisposeDbContext()
    {
        await _dbContext.DisposeAsync().ConfigureAwait(false);

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

        return await _client.PostAsync("/register", formData).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> Login(string username, string? password)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string?>("Username", username),
            new KeyValuePair<string, string?>("Password", password),
        });
        return await _client.PostAsync("/login", formData).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> RegisterAndLogin(string username, string? password)
    {
        await Register(username, password).ConfigureAwait(false);
        return await Login(username, password).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> Logout()
    {
        return await _client.GetAsync("/logout").ConfigureAwait(false);
    }

    private async Task AddMessage(string text)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("text", text)
        });

        var response = await _client.PostAsync("/add_message", formData).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(text) && response.IsSuccessStatusCode)
        {
            Assert.Contains("Your message was recorded", response.Content.ReadAsStringAsync().Result);
        }
    }

    [Fact]
    public async Task TestRegister()
    {
        await InitializeDbContext().ConfigureAwait(false);

        var response = await Register("user1", "default").ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _testOutputHelper.WriteLine(content);
        Assert.True(response.IsSuccessStatusCode);

        response = await Register("user1", "default").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("The username is already taken", content);

        response = await Register("", "default").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You have to enter a username", content);

        response = await Register("meh", "").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You have to enter a password", content);

        response = await Register("meh", "x", "y").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("The two passwords do not match", content);

        response = await Register("meh", "foo", null, "broken").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You have to enter a valid email address", content);

        await DisposeDbContext().ConfigureAwait(false);
    }

    [Fact]
    public async Task TestLoginLogout()
    {
        await InitializeDbContext().ConfigureAwait(false);

        var response = await RegisterAndLogin("user1", "default").ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You were logged in", content);

        response = await Logout().ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You were logged out", content);

        response = await Login("user1", "wrongpassword").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("Invalid password", content);

        response = await Login("user2", "wrongpassword").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("Invalid username", content);

        await DisposeDbContext().ConfigureAwait(false);
    }


    [Fact]
    public async Task TestMessageRecording()
    {
        await InitializeDbContext().ConfigureAwait(false);

        await RegisterAndLogin("foo", "default").ConfigureAwait(false);
        await AddMessage("test message 1").ConfigureAwait(false);
        await AddMessage("<test message 2>").ConfigureAwait(false);

        var response = await _client.GetAsync("/public").ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("test message 1", content);
        Assert.Contains("&lt;test message 2&gt;", content);

        await DisposeDbContext().ConfigureAwait(false);
    }

    [Fact]
    public async Task TestTimelines()
    {
        await InitializeDbContext().ConfigureAwait(false);

        await RegisterAndLogin("foo", "default").ConfigureAwait(false);
        await AddMessage("the message by foo").ConfigureAwait(false);
        await Logout().ConfigureAwait(false);

        await RegisterAndLogin("bar", "default").ConfigureAwait(false);
        await AddMessage("the message by bar").ConfigureAwait(false);
        var response = await _client.GetAsync("/public").ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);

        // bar's timeline should just show bar's message
        response = await _client.GetAsync("/").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);

        // now let's follow foo
        response = await _client.GetAsync("/foo/follow").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You are now following &quot;foo&quot;", content);

        // we should now see foo's message
        response = await _client.GetAsync("/").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("the message by foo", content);
        Assert.Contains("the message by bar", content);

        // but on the user's page we only want the user's message
        response = await _client.GetAsync("/bar").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _testOutputHelper.WriteLine(content);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);
        response = await _client.GetAsync("/foo").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("the message by foo", content);
        Assert.DoesNotContain("the message by bar", content);

        // now unfollow and check if that worked
        response = await _client.GetAsync("/foo/unfollow").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.Contains("You are no longer following &quot;foo&quot;", content);
        response = await _client.GetAsync("/").ConfigureAwait(false);
        content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.DoesNotContain("the message by foo", content);
        Assert.Contains("the message by bar", content);

        await DisposeDbContext().ConfigureAwait(false);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}