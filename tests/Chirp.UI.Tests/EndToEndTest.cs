using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Chirp.UI.Tests
{
    public class EndToEndTest : PageTest
    {
        private Process _serverProcess;

        public override BrowserNewContextOptions ContextOptions()
        {

            return new BrowserNewContextOptions             
            {
                IgnoreHTTPSErrors = true
            };
        }

        /// <summary>
        /// Start a server process.
        /// </summary>
        [OneTimeSetUp]
        public async Task Setup()
        {
            _serverProcess = await ServerUtil.StartServer();
        }

        /// <summary>
        /// Close the server process.
        /// </summary>
        [OneTimeTearDown]
        public void Cleanup()
        {
            _serverProcess.Kill();
            _serverProcess.Dispose();
        }

        /// <summary>
        /// Deletes all created test users in the database.
        /// </summary>
        [TearDown]
        public async Task CleanupAccount()
        {
            await ServerUtil.DeleteUser(Page, "test@mail.com", "Testpassword123!");
            await ServerUtil.DeleteUser(Page, "testmail@mail.com", "Testpassword123!");
            await ServerUtil.DeleteUser(Page, "atest@mail.com", "Testpassword123!");
            await ServerUtil.DeleteUser(Page, "hello@mail.com", "Testpassword123!");
            await ServerUtil.DeleteUser(Page, "hello2@mail.com", "Testpassword123!");
        }

        /// <summary>
        /// User creates a new account. User logs in and out of account. User deletes account.
        /// </summary>
        [Test]
        public async Task UserRegistersANewAccountIsDirectlySignedInLogsOutAndLogsInWithNewAccountLogsOutLogsInDeletesAccount()
        {
            //Arrange
            await Page.GotoAsync("https://localhost:5273/");
         
            //Act
            await ServerUtil.RegisterUser(Page, "username1");
           
            //Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [username1]" }).ClickAsync();
            await ServerUtil.LoginUser(Page);

            //Assert
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "logout [username1]" })).ToBeVisibleAsync();

            //Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [username1]" }).ClickAsync();

            //Assert
            await Expect(Page.GetByText("login")).ToBeVisibleAsync();

            //Act
            await ServerUtil.LoginUser(Page);
            await ServerUtil.DeleteUser(Page);
            await ServerUtil.LoginUser(Page);

            //Assert
            await Expect(Page.GetByText("No user found")).ToBeVisibleAsync();
        }

        /// <summary>
        /// User creates a new account. User writes cheeps. User deletes account.
        /// </summary>
        [Test]
        public async Task UserRegistersANewAccountAndLogsInWithNewAccountWritesCheepDeletesAccount()
        {
            //Arrange
            await Page.GotoAsync("https://localhost:5273/");
         
            //Act
            await ServerUtil.RegisterUser(Page, "username5");
           
            //Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [username5]" }).ClickAsync();
            await ServerUtil.LoginUser(Page);

            //Assert
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "logout [username5]" })).ToBeVisibleAsync();

            //Act
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("This is a test cheep");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

            //Assert
            await Expect(Page.GetByText("This is a test cheep")).ToBeVisibleAsync();

            //Act
            await ServerUtil.DeleteUser(Page);
            await ServerUtil.LoginUser(Page);

            //Assert
            await Expect(Page.GetByText("No user found")).ToBeVisibleAsync();
        }
        
        /// <summary>
        /// User creates a new account. User follows another account. The newly follow account is displayed correctly in about me page.
        /// </summary>
        [Test]
        public async Task UserFollowsAccountsAndWritesCheepsCorrectAmountOfAccountsAndCheepsInAboutMePage()
        {
            //Arrange
            await ServerUtil.RegisterUser(Page, "ATestUser1", "atest@mail.com", "Testpassword123!");

            //Act
            await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst" }).Locator("#follow").ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "But what was" }).Locator("#follow").ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "At present I" }).Locator("#follow").ClickAsync();
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Hello World1");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Hello World2");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Hello World3");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();

            //Assert
            await Expect(Page.GetByText("Hello World1")).ToBeVisibleAsync();
            await Expect(Page.GetByText("Hello World2")).ToBeVisibleAsync();
            await Expect(Page.GetByText("Hello World3")).ToBeVisibleAsync();

            var follower1 = Page.GetByRole(AriaRole.Link, new() { Name = "Jacqualine Gilcoine" });
            var follower2 = Page.GetByRole(AriaRole.Link, new() { Name = "Malcolm Janski" });
            var follower3 = Page.GetByRole(AriaRole.Link, new() { Name = "Mellie Yost" });

            Assert.IsTrue(await follower1.IsVisibleAsync());
            Assert.IsTrue(await follower2.IsVisibleAsync());
            Assert.IsTrue(await follower3.IsVisibleAsync());

            //Act 
            await Page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "Hello World3" }).GetByRole(AriaRole.Button).ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "Hello World2" }).GetByRole(AriaRole.Button).ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "Hello World1" }).GetByRole(AriaRole.Button).ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst." }).Locator("#unfollow").ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "But what was behind the barricade." }).Locator("#unfollow").ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "At present I cannot spare energy and determination such as I did look up I saw a gigantic Sperm Whale is toothless." }).Locator("#unfollow").ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();

            //Assert
            await Expect(Page.GetByText("Hello World1")).ToBeHiddenAsync();
            await Expect(Page.GetByText("Hello World2")).ToBeHiddenAsync();
            await Expect(Page.GetByText("Hello World3")).ToBeHiddenAsync();

            Assert.IsFalse(await follower1.IsVisibleAsync());
            Assert.IsFalse(await follower2.IsVisibleAsync());
            Assert.IsFalse(await follower3.IsVisibleAsync());
        }

        /// <summary>
        /// User creates a new account. User follows another account and likes cheeps. The followed account has a follower and a like on their cheep.
        /// </summary>
        [Test]
        public async Task UserLikesCheepAndFollowsAccount()
        {
            //Arrange
            await ServerUtil.RegisterUser(Page, "ATestUser", "hello@mail.com");
            await Page.Locator("#Message").ClickAsync();
            await Page.Locator("#Message").FillAsync("Hello world");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [Atestuser]" }).ClickAsync();

            //Act
            await ServerUtil.RegisterUser(Page, "ATestUser2", "hello2@mail.com");
            await Page.Locator("li").Filter(new() { HasText = "Atestuser " }).Locator(".fa-heart-o.outlined-heart").ClickAsync();
            await Page.Locator("li").Filter(new() { HasText = "Atestuser " }).Locator("#follow").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [Atestuser2]" }).ClickAsync();

            //Act
            await ServerUtil.LoginUser(Page, "hello@mail.com");

            //Assert
            var like = Page.Locator("li").Filter(new() { HasText = "Atestuser " }).GetByText("1", new() { Exact = true });
            Assert.IsTrue(await like.IsVisibleAsync());

            //Act
            await Page.GetByRole(AriaRole.Link, new() { Name = "about me" }).ClickAsync();

            //Assert
            var follower1 = Page.GetByRole(AriaRole.Link, new() { Name = "Atestuser2" });
            Assert.IsTrue(await follower1.IsVisibleAsync());

            //Act
            await Page.GetByRole(AriaRole.Button, new() { Name = "logout [Atestuser]" }).ClickAsync();
            await ServerUtil.LoginUser(Page, "hello2@mail.com");
            await ServerUtil.DeleteUser(Page, "hello2@mail.com");
            
            await ServerUtil.LoginUser(Page, "hello@mail.com");

            //Assert
            Assert.IsFalse(await like.IsVisibleAsync());

            //Act
            await Page.GetByRole(AriaRole.Link, new() { Name = "about me" }).ClickAsync();

            //Assert
            Assert.IsFalse(await follower1.IsVisibleAsync());

            //Act
            await ServerUtil.DeleteUser(Page, "hello@mail.com");
        }
    }
}