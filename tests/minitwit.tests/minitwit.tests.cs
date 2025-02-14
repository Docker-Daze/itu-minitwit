using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Razor.Language;
using Xunit;

namespace minitwit.tests;

public class TestAPI : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:5114";

    public TestAPI(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });
    }

    public async Task<HttpResponseMessage> Register(string username, string password, string password2 = null, string email = null)
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

    public async Task<HttpResponseMessage> Login(string username, string password)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", username),
            new KeyValuePair<string, string>("Password", password),
        });
        return await _client.PostAsync("/login", formData);
    }

    public async Task RegisterAndLogin(string username, string password)
    {
        Register(username, password);
        Login(username, password);
    }

    public async Task<HttpResponseMessage> Logout()
    {
        return await _client.GetAsync("/logout");
    }

    public async Task<HttpResponseMessage> AddMessage()
    {
        
    }

    [Fact]
    public async void TestRegister()
    {
        var response = await Register("user1", "Default123!");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You were successfully registered and can login now", content);
        
        response = await Register("user1", "Default123!");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The username is already taken", content);
        
        response = await Register("", "Default123!");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a username", content);
        
        response = await Register("meh", "");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a password", content);
        
        response = await Register("meh", "x123!", "y123!");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The two passwords do not match", content);
        
        response = await Register("meh", "foo", null, "broken");
        content = await response.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a valid email address", content);
    }
    
    
}