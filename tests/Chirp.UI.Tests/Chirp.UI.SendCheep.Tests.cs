using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.UI.Tests
{
    public class Chirp_UI_SendCheep_Tests : PageTest
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
        public async Task CheepBoxIsVisibleWhenLoggedIn()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            // Act
            var shareButton = Page.GetByRole(AriaRole.Button, new() { Name = "Share" });
            
            // Assert
            await Expect(shareButton).ToBeVisibleAsync();
        }
        
        [Test]
        public async Task CheepBoxIsNotVisibleWhenNotLoggedIn()
        {
            // Arrange
            var isUserLoggedIn = await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).IsVisibleAsync();
            if (isUserLoggedIn)
            {
                await Page.GetByRole(AriaRole.Button, new() { Name = "logout [username]" }).ClickAsync();
            }
            
            // Act
            await Page.GotoAsync("https://localhost:5273");
            var shareButton = Page.GetByRole(AriaRole.Button, new() { Name = "Share" });
            
            // Assert
            await Expect(shareButton).ToBeHiddenAsync();
        }
        
        [Test]
        public async Task SendingCheepShowsNewCheepOnPublicTimeline()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            int randCheepId = new Random().Next();
            
            // Act
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Cheeping cheeps on Chirp!" + randCheepId);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
            var newCheep = Page.Locator("li").Filter(new() { HasText = "Cheeping cheeps on Chirp!" + randCheepId }).First;
            
            // Assert
            await Expect(newCheep).ToBeVisibleAsync();
            await Expect(newCheep).ToContainTextAsync("Cheeping cheeps on Chirp!" + randCheepId);
        }

        [Test]
        public async Task CheepboxDoesNotAllowUserToSendCheepLongerThan160Characters()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            int randCheepId = new Random().Next();
            string longCheepMessage = new string('a', 160) + randCheepId;
            
            // Act
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync(longCheepMessage);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
            await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            
            var newCheep =  Page.Locator("li").Filter(new() { HasText = longCheepMessage.Substring(0, 160) }).First;

            // Assert
            await Expect(newCheep).ToBeVisibleAsync();
            await Expect(newCheep).ToContainTextAsync(longCheepMessage.Substring(0, 160));
            string cheepText = await newCheep.InnerTextAsync();
            Assert.That(cheepText, Does.Not.Contain(randCheepId.ToString()));

        }
        
        [Test]
        public async Task CheepboxDoesNotAllowUserToSendEmptyCheeps()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            // Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            var cheepbox = Page.GetByText("Cheep cannot be empty Share");

            // Assert
            await Expect(cheepbox).ToBeVisibleAsync();
        }
        
        [Test]
        public async Task SendingCheepShowCheepOnPrivateTimelineForRespectiveAuthor()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            int randCheepId = new Random().Next();
            
            // Act
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Cheeping cheeps on Chirp!" + randCheepId);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
            await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            var newCheep = Page.Locator("li").Filter(new() { HasText = "Cheeping cheeps on Chirp!" + randCheepId }).First;
            
            // Assert
            await Expect(newCheep).ToBeVisibleAsync();
            await Expect(newCheep).ToContainTextAsync("Cheeping cheeps on Chirp!" + randCheepId);
        }

        [Test]
        public async Task CheepboxIsNotVulnerableForXSSAttacks()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            // Act
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Hello, I am feeling good!<script>alert('If you see this in a popup, you are in trouble!');</script>");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
            var newCheep = Page.Locator("li").Filter(new() { HasText = "<script>alert('If you see this in a popup, you are in trouble!');</script>"}).First;
            
            // Assert
            await Expect(newCheep).ToBeVisibleAsync();
            await Expect(newCheep)
                .ToContainTextAsync("<script>alert('If you see this in a popup, you are in trouble!');</script>");
        }

        [Test]
        public async Task TestSQLInjection()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            // Act
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("This is a cheep'); DROP TABLE Cheeps;--");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            
            var newCheep = Page.Locator("li").Filter(new() { HasText = "This is a cheep'); DROP TABLE Cheeps;--"}).First;
            
            // Assert
            await Expect(newCheep).ToBeVisibleAsync();
            await Expect(newCheep).ToContainTextAsync("This is a cheep'); DROP TABLE Cheeps;--");
        }
        
        [Test]
        public async Task CheepboxDoesNotAllowUserToSendEmptyCheepsAndCheepsStillAppearPublicTimeline()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            
            // Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            var cheepbox = Page.GetByText("Cheep cannot be empty Share");
            
            // Assert cheepbox is still there, and timeline still displays cheeps
            await Expect(Page.GetByText("Starbuck now is what we hear the worst")).ToBeVisibleAsync();
            await Expect(cheepbox).ToBeVisibleAsync();
        }
        
        [Test]
        public async Task CheepboxDoesNotAllowUserToSendEmptyCheepsAndCheepsStillAppearPrivateTimeline()
        {
            // Arrange
            await Page.GotoAsync("https://localhost:5273");
            await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst" }).Locator("#follow").ClickAsync();
            await Page.GotoAsync("https://localhost:5273/username");
            // Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            var cheepbox = Page.GetByText("Cheep cannot be empty Share");
            
            // Assert cheepbox is still there, and timeline still displays cheeps
            await Expect(Page.GetByText("Starbuck now is what we hear the worst")).ToBeVisibleAsync();
            await Expect(cheepbox).ToBeVisibleAsync();
        }
    }
}