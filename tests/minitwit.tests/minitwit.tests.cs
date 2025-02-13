using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

using System.Text;
using System.Text.Json;  // or Newtonsoft.Json if you prefer


namespace minitwit.tests;

public class TestAPI : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly HttpClient _client;
    private string BaseUrl = "http://localhost:5114";

    public TestAPI(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });
    }

    private async Task<HttpResponseMessage> Register(string username, string password, string password2 = null, string email = null)
    {
        // If no confirmation password is provided, use the same password.
        if (password2 == null)
        {
            password2 = password;
        }

        // If no email is provided, use a default email based on the username.
        if (email == null)
        {
            email = $"{username}@example.com";
        }

        // Build the form data (using keys that match your form fields).
        var formData = new Dictionary<string, string>
        {
            { "Input.UserName", username },
            { "Input.Email", email },
            { "Input.Password", password },
            { "Input.ConfirmPassword", password2 }
        };

        var content = new FormUrlEncodedContent(formData);

        // Post the form data to the registration endpoint.
        return await _client.PostAsync("/register", content);
    }

    public async Task<(HttpResponseMessage response, HttpClient httpClient)> Login(string username, string password)
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = true };
        var httpClient = new HttpClient(handler);
        var url = $"{BaseUrl}/login";

        // Build the form data using keys expected by your login form.
        var formData = new Dictionary<string, string>
        {
            { "Input.UserName", username },
            { "Input.Password", password }
        };

        var content = new FormUrlEncodedContent(formData);

        HttpResponseMessage response = await httpClient.PostAsync(url, content);

        return (response, httpClient);
    }


    [Fact]
    public async Task test_login_logout()
    {
        await Register("user1", "Default123!");
        var response = await _client.GetAsync($"{BaseUrl}");
        var htmlContent = await response.Content.ReadAsStringAsync();

        // Assert that the logout button text is present in the HTML.
        Assert.Contains("logout [user1]", htmlContent, StringComparison.OrdinalIgnoreCase);
        
        await Login("user1", "Default123!");
        
        response = await _client.GetAsync($"{BaseUrl}/user1");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        // Verify that the "my timeline" option is present in the HTML.
        Assert.Contains("my timeline", content, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async void CanSeePublicTimeline()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Chirp!", content);
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
