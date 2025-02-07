using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.UI.Tests;

public class IntegrationTest : PageTest
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
    public async Task Setup()
    {
        _serverProcess = await ServerUtil.StartServer();
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        _serverProcess.Kill();
        _serverProcess.Dispose();
    }

    [TearDown]
    public async Task CleanupAccount()
    {
        await ServerUtil.DeleteUser(Page, "test@mail.com", "Testpassword123!");
        await ServerUtil.DeleteUser(Page, "testmail@mail.com", "Testpassword123!");
        await ServerUtil.DeleteUser(Page, "atest@mail.com", "Testpassword123!");
        await ServerUtil.DeleteUser(Page, "hello@mail.com", "Testpassword123!");
        await ServerUtil.DeleteUser(Page, "hello2@mail.com", "Testpassword123!");
    }
    
    [Test]
    public async Task UserRegistersAndTestsPagination()
    {
        //Arrange
        await ServerUtil.RegisterUser(Page, "ATestUser", "test@mail.com", "Testpassword123!");
            
        //Assert
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Previous Page" })).Not.ToBeVisibleAsync(); //On page 1, so should not be visible
            
        //Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next Page" }).ClickAsync();
            
        //Assert
        Assert.That(Page.Url, Is.EqualTo("https://localhost:5273/?page=2"));
            
        //Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Previous Page" })).Not.ToBeVisibleAsync(); //On page 1, so should not be visible
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Next Page" })).Not.ToBeVisibleAsync(); //Haven't posted or followed, so button should not be visible
            
        //Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
        await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst." }).Locator("#follow").ClickAsync(); //Follow Jacqualine because she has > 32 cheeps
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next Page" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Previous Page" })).ToBeVisibleAsync(); 
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Next Page" })).ToBeVisibleAsync(); 
    }
    
    [Test]
    public async Task UserIsAbleToMakeAndDeleteCheeps()
    {
        //Arrange
        await ServerUtil.RegisterUser(Page, "ATestUser", "test@mail.com", "Testpassword123!");
            
        //Act
        await Page.Locator("#Message").ClickAsync();
        await Page.Locator("#Message").FillAsync("This is a test cheep");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
        await Page.GetByRole(AriaRole.Button, new() { Name = "DELETE" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("This is a test cheep")).Not.ToBeVisibleAsync();
            
        //Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("This is a test cheep")).Not.ToBeVisibleAsync();
            
        //Act
        await Page.Locator("#Message").ClickAsync();
        await Page.Locator("#Message").FillAsync("This is a test cheep on my timeline");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("This is a test cheep on my timeline")).ToBeVisibleAsync();
            
        //Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "DELETE" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("This is a test cheep on my timeline")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    public async Task UserChecksMyTimelineForOwnCheeps()
    {
        //Arrange
        await ServerUtil.RegisterUser(Page, "ATestUser", "test@mail.com", "Testpassword123!");
            
        //Act
        await Page.Locator("#Message").ClickAsync();
        await Page.Locator("#Message").FillAsync("This is a test cheep");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        //Assert
        await Expect(Page.GetByText("This is a test cheep")).ToBeVisibleAsync();
            
        //Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("This is a test cheep")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task CheckingMyTimelineForOtherCheeps()
    {
        //Arrange
        await ServerUtil.RegisterUser(Page, "ATestUser", "test@mail.com", "Testpassword123!");
            
        //Act
        await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the" }).Locator("#follow").ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("Starbuck now is what we hear the worst.")).ToBeVisibleAsync();
            
        //Act
        await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the" }).Locator("#unfollow").ClickAsync();
            
        //Assert
        await Expect(Page.GetByText("Starbuck now is what we hear the worst.")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    public async Task UserFollowsAnotherUserAndUnfollows()
    {
        //Act
        await ServerUtil.RegisterUser(Page, "ATestUser", "test@mail.com", "Testpassword123!");

        var buttonLocator = Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the" })
            .Locator("#follow");
        string buttonText = await buttonLocator.InnerTextAsync();

        //Assert
        Assert.That(buttonText, Is.EqualTo("Follow"));

        //Act
        await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst" })
            .Locator("#follow").ClickAsync();

        var buttonLocator2 = Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the" })
            .Locator("#unfollow");
        string buttonText2 = await buttonLocator2.InnerTextAsync();

        //Assert
        Assert.That(buttonText2, Is.EqualTo("Unfollow"));

        //Act
        await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst" })
            .Locator("#unfollow").ClickAsync();

        var buttonLocator3 = Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the" })
            .Locator("#follow");
        string buttonText3 = await buttonLocator3.InnerTextAsync();

        //Assert
        Assert.That(buttonText3, Is.EqualTo("Follow"));
    }
    
    [Test]
    public async Task ChangeUsernameFromManageAccount()
    {
        await ServerUtil.DeleteUser(Page);
        await ServerUtil.RegisterUser(Page);
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        var listItemText1 = Page.Locator("div.aboutContainer > ul > li");
        
        // Assert username is username
        await Expect(listItemText1.Nth(0)).ToHaveTextAsync("Username: username");
        
        // Act
        await Page.GotoAsync("https://localhost:5273/Identity/Account/Manage");
        var locator = Page.Locator("#profile-form input[placeholder='Please choose your username.']");
        await locator.ClickAsync();
        await locator.FillAsync("NewName");
        await Page.Locator("#update-profile-button").ClickAsync();
        var username = await locator.InputValueAsync();
        
        // Assert
        Assert.That(username, Is.Not.EqualTo("username"));
        
        await Page.GotoAsync("https://localhost:5273/about");
        var listItemText2 = Page.Locator("div.aboutContainer > ul > li");
        
        // Assert username is changed
        await Expect(listItemText2.Nth(0)).ToHaveTextAsync("Username: NewName");
        await ServerUtil.DeleteUser(Page);
    }
    
    [Test]
    public async Task ChangeEmailFromManageAccount()
    {
        await ServerUtil.DeleteUser(Page);
        await ServerUtil.RegisterUser(Page);
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        var listItemText1 = Page.Locator("div.aboutContainer > ul > li");
        
        // Assert Email is name@example.com
        await Expect(listItemText1.Nth(1)).ToHaveTextAsync("Email: name@example.com");
        
        // Act
        await Page.GotoAsync("https://localhost:5273/Identity/Account/Manage/Email");
        var locator = Page.Locator(".form-control");
        await locator.ClickAsync();
        await locator.FillAsync("NewName@example.com");
        await Page.Locator("#change-email-button").ClickAsync();
        var email = await locator.InputValueAsync();
        
        // Assert
        Assert.That(email, Is.Not.EqualTo("name@example.com"));
        
        await Page.GotoAsync("https://localhost:5273/about");
        var listItemText2 = Page.Locator("div.aboutContainer > ul > li");
        
        // Assert Email is changed
        await Expect(listItemText2.Nth(1)).ToHaveTextAsync("Email: NewName@example.com");
        await ServerUtil.DeleteUser(Page);
    }
    
    [Test]
    public async Task ChangeProfilePictureFromManageAccount()
    {
        await ServerUtil.DeleteUser(Page);
        await ServerUtil.RegisterUser(Page);
        // Arrange
        await Page.GotoAsync("https://localhost:5273/about");
        var listItemText1 = Page.Locator("div.aboutContainer > svg");
        var listItemText2 = Page.Locator("div.aboutContainer > img");
        
        // Assert ProfilePicture is visible, but not an img
        await Expect(listItemText1.Nth(0)).ToBeVisibleAsync();
        await Expect(listItemText2.Nth(0)).Not.ToBeVisibleAsync();
        
        // Act
        var NewProfilePicture = 
            "https://as1.ftcdn.net/v2/jpg/03/07/33/84/1000_F_307338469_HJlhqHUhowtwOVdjDUm1lwxaSvxDxXV4.jpg";
        
        await Page.GotoAsync("https://localhost:5273/Identity/Account/Manage");
        var locator = Page.Locator("#profile-form input[placeholder='Please enter your image link.']");
        await locator.ClickAsync();
        await locator.FillAsync(NewProfilePicture);
        await Page.Locator("#update-profile-button").ClickAsync();
        
        await Page.GotoAsync("https://localhost:5273/about");
        var listItemText3 = Page.Locator("div.aboutContainer > img");
        var imageSrc = await Page.Locator("img.claim-img-large").GetAttributeAsync("src");

        // Assert Profile picture is changed
        await Expect(listItemText3.Nth(0)).ToBeVisibleAsync();
        Assert.That(NewProfilePicture, Is.EqualTo(imageSrc));
        
        await ServerUtil.DeleteUser(Page);
    }
    [Test]
    public async Task UserTriesToChangeMailToAlreadyTakenMail()
    {
        //Arrange
        await ServerUtil.RegisterUser(Page, "ATestUser", "hello@mail.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "logout [Atestuser]" }).ClickAsync();

        //Act
        await ServerUtil.RegisterUser(Page, "ATestUser2", "hello2@mail.com");
        await Page.GetByRole(AriaRole.Link, new() { Name = "manage account" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Email" }).ClickAsync();
        await Page.GetByPlaceholder("Please enter new email.").ClickAsync();
        await Page.GetByPlaceholder("Please enter new email.").FillAsync("hello@mail.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Change email" }).ClickAsync();

        //Assert
        await Expect(Page.GetByText("The email address is already in use. Please choose a different one.")).ToBeVisibleAsync();

        //Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "logout [Atestuser2]" }).ClickAsync();
        await ServerUtil.LoginUser(Page, "hello@mail.com");
        await ServerUtil.DeleteUser(Page, "hello@mail.com");
        await ServerUtil.LoginUser(Page, "hello2@mail.com");
        await Page.GetByRole(AriaRole.Link, new() { Name = "manage account" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Email" }).ClickAsync();
        await Page.GetByPlaceholder("Please enter new email.").ClickAsync();
        await Page.GetByPlaceholder("Please enter new email.").FillAsync("hello@mail.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Change email" }).ClickAsync();

        //Assert
        await Expect(Page.GetByText("Your email has been changed.")).ToBeVisibleAsync();
        await ServerUtil.DeleteUser(Page, "hello@mail.com");
    }
}