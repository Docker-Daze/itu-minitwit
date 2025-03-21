using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace minitwit.tests;

public class playwright_test : PageTest
{
    public static async Task RegisterUser(IPage page, string password = "Password123!")
    {
        await page.GotoAsync("localhost:5114/").ConfigureAwait(false);
        await page.GetByRole(AriaRole.Link, new() { Name = "sign up" }).ClickAsync().ConfigureAwait(false);
        await page.Locator("#Username").ClickAsync().ConfigureAwait(false);
        await page.Locator("#Username").FillAsync("username").ConfigureAwait(false);
        await page.Locator("#Email").ClickAsync().ConfigureAwait(false);
        await page.Locator("#Email").FillAsync("name@example.com").ConfigureAwait(false);
        await page.Locator("#Password").ClickAsync().ConfigureAwait(false);
        await page.Locator("#Password").FillAsync("Password123!").ConfigureAwait(false);
        await page.Locator("#Password2").ClickAsync().ConfigureAwait(false);
        await page.Locator("#Password2").FillAsync("Password123!").ConfigureAwait(false);
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync().ConfigureAwait(false);
        await page.Locator("#Username").ClickAsync().ConfigureAwait(false);
        await page.Locator("#Username").FillAsync("username").ConfigureAwait(false);
        await page.Locator("#Password").ClickAsync().ConfigureAwait(false);
        await page.Locator("#Password").FillAsync("Password123!").ConfigureAwait(false);
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task _register_user_via_gui()
    {
        //Arrange
        await Page.GotoAsync("localhost:5114/").ConfigureAwait(false);

        //Act
        await RegisterUser(Page, password: "Password123!").ConfigureAwait(false);

        //Assert
        await Expect(Page.GetByText("You were logged in")).ToBeVisibleAsync().ConfigureAwait(false);
        Assert.IsNotEmpty(await Page.ContentAsync().ConfigureAwait(false));

    }
}