using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace minitwit.tests;

public class playwright_test : PageTest
{
    [Test]
    public async Task _register_user_via_gui()
    {
        //Arrange
        await Page.GotoAsync("https://localhost:????/");
         
        //Act
        await Page.Locator("#Message").ClickAsync();

        //Assert
        await Expect(Page.GetByText("No user found")).ToBeVisibleAsync();
    }

}