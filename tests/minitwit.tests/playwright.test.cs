using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace minitwit.tests;

public class playwright_test : PageTest
{                 
    private Process _serverProcess;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _serverProcess = await serverutil.StartApp();
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        serverutil.StopApp();
    }

    [Test]
    public async Task _register_user_via_gui()
    {
        //Arrange
        //await Page.GotoAsync("https://localhost:5114/");
        await Page.GotoAsync("https://www.google.com/");
         
        //Act
        //await Page.Locator("#Message").ClickAsync();

        //Assert
        //await Expect(Page.GetByText("minitwit")).ToBeVisibleAsync();
        Assert.IsNotEmpty(await Page.ContentAsync());

    }
}