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

    [Fact]
    public async void TestRegister()
    {
        var r = await Register("user1", "Default123!");
        var content = await r.Content.ReadAsStringAsync();
        Assert.Contains("You were successfully registered and can login now", content);
        
        r = await Register("user1", "Default123!");
        content = await r.Content.ReadAsStringAsync();
        Assert.Contains("The username is already taken", content);
        
        r = await Register("", "Default123!");
        content = await r.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a username", content);
        
        r = await Register("meh", "");
        content = await r.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a password", content);
        
        r = await Register("meh", "x123!", "y123!");
        content = await r.Content.ReadAsStringAsync();
        Assert.Contains("The two passwords do not match", content);
        
        r = await Register("meh", "foo", null, "broken");
        content = await r.Content.ReadAsStringAsync();
        Assert.Contains("You have to enter a valid email address", content);
    } 
    

    [Fact]
    public async void CanSeePublicTimeline()
    {
        var response = await _client.GetAsync("/public");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("Public Timeline", content);
    }
    
    [Fact]
    public async void CanSeeRegisterPage()
    {
        var response = await _client.GetAsync("/Identity/Account/Register");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains("Register", content);
    }
    
    [Fact]
    public async void CanSeeLogoutPage()
    {
        var response = await _client.GetAsync("/Identity/Account/Logout");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains("Log out", content);
    }

    [Fact]
    public async void CanSeeLoginPage()
    {
        var response = await _client.GetAsync("/Identity/Account/Login");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains("Log in", content);
    }
    
    [Theory]
    [InlineData("Jacqualine Gilcoine")]
    [InlineData("Johnnie Calixto")]
    public async void CanSeePrivateTimeline(string author)
    {
        var response = await _client.GetAsync($"/{author}");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error: {response.StatusCode}, {errorContent}");
        }
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
        Assert.Contains($"{author}'s Timeline", content);
    }
    
    [Fact]
    public async void IsPage1SameAsDefaultTimeline()
    {
        var responseHomePage = await _client.GetAsync("/");
        var content1 = await responseHomePage.Content.ReadAsStringAsync();
        responseHomePage.EnsureSuccessStatusCode();
        var responseFirstPage = await _client.GetAsync("/?page=1");
        var content2 = await responseFirstPage.Content.ReadAsStringAsync();
        responseFirstPage.EnsureSuccessStatusCode();
        
        Assert.Contains("Chirp!", content1);
        Assert.Contains("Public Timeline", content1);
        Assert.Contains("Chirp!", content2);
        Assert.Contains("Public Timeline", content2);
        Assert.Equal(content1, content2);
    }
    
    [Fact]
    public async void SiteHasMorePagesAndNotSameAsSameAsDefaultTimeline()
    {
        var responseHomePage = await _client.GetAsync("/");
        var content1 = await responseHomePage.Content.ReadAsStringAsync();
        responseHomePage.EnsureSuccessStatusCode();
        
        var responsePage4 = await _client.GetAsync("/?page=4");
        responsePage4.EnsureSuccessStatusCode();
        var content2 = await responsePage4.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content1);
        Assert.Contains("Public Timeline", content1);
        Assert.Contains("Chirp!", content2);
        Assert.Contains("Public Timeline", content2);
        Assert.NotEqual(content1, content2);
    }
    
    [Theory]
    [InlineData("Jacqualine Gilcoine")]
    [InlineData("Johnnie Calixto")]
    public async void PrivateTimelinePage1SameAsPrivateDefault(string author)
    {
        var responseHomePage = await _client.GetAsync($"/{author}");
        var content1 = await responseHomePage.Content.ReadAsStringAsync();
        responseHomePage.EnsureSuccessStatusCode();
        
        var responsePage4 = await _client.GetAsync($"/{author}?page=1");
        responsePage4.EnsureSuccessStatusCode();
        var content2 = await responsePage4.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content1);
        Assert.Contains($"{author}'s Timeline", content1);
        Assert.Contains("Chirp!", content2);
        Assert.Contains($"{author}'s Timeline", content2);
        Assert.Equal(content1, content2);
    }
    
    [Theory]
    [InlineData("Jacqualine Gilcoine")]
    [InlineData("Johnnie Calixto")]
    public async void PrivateTimelinePage4NotSameAsPrivateDefault(string author)
    {
        var responseHomePage = await _client.GetAsync($"/{author}");
        var content1 = await responseHomePage.Content.ReadAsStringAsync();
        responseHomePage.EnsureSuccessStatusCode();
        
        var responsePage4 = await _client.GetAsync($"/{author}?page=4");
        responsePage4.EnsureSuccessStatusCode();
        var content2 = await responsePage4.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content1);
        Assert.Contains($"{author}'s Timeline", content1);
        Assert.Contains("Chirp!", content2);
        Assert.Contains($"{author}'s Timeline", content2);
        Assert.NotEqual(content1, content2);
    }
}
