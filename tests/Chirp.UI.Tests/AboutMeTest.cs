using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.UI.Tests;

public class AboutMeTest : PageTest
{
    private Process _serverProcess;
    
    public override BrowserNewContextOptions ContextOptions()
    {
            
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        };
    }
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _serverProcess = await ServerUtil.StartServer();
    }
        
    [SetUp]
    public async Task Setup()
    {
        await ServerUtil.DeleteUser(Page);
        await ServerUtil.RegisterUser(Page);
        await ServerUtil.LoginUser(Page);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await ServerUtil.LoginUser(Page);
        await ServerUtil.DeleteUser(Page);
    }
        
    [OneTimeTearDown]
    public void OneTimeCleanup()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill();
            _serverProcess.Dispose();
        }
    }
    
    [Test]
    public async Task AboutMePageVisableWhenLoggedIn()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        
        // Assert
        await Expect(Page).ToHaveTitleAsync(new Regex("About me"));
    }
    
    [Test]
    public async Task AboutMePageNotVisableWhenNotLoggedIn()
    {
        // Arrange
        var isUserLoggedIn = await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).IsVisibleAsync();
        if (isUserLoggedIn)
        {
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [username]" }).ClickAsync();
        }
        
        await Page.GotoAsync("https://localhost:5273/about");
        
        // Assert
        await Expect(Page).ToHaveTitleAsync("Log in");
    }
    
    [Test]
    public async Task AboutMePageInNavigator()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5273");
        
        // Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "about me" }).ClickAsync();
            
        // Assert
        await Expect(Page).ToHaveTitleAsync(new Regex("About me"));
    }
    
    
    [Test]
    public async Task AboutMePageDisplayUsername()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        
        // Act
        var listItemText = Page.Locator("div.aboutContainer > ul > li");
            
        // Assert
        await Expect(listItemText.Nth(0)).ToHaveTextAsync("Username: username");
    }
        
    [Test]
    public async Task AboutMePageDisplayEmail()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        
        // Act
        var listItemText = Page.Locator("div.aboutContainer > ul > li");
            
        // Assert
        await Expect(listItemText.Nth(1)).ToHaveTextAsync("Email: name@example.com");
    }
    
    [Test]
    public async Task AboutMePageDisplayProfilePicture()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        
        // Act
        var listItemText = Page.Locator("div.aboutContainer > svg");
            
        // Assert
        await Expect(listItemText.Nth(0)).ToBeVisibleAsync();
    }
}