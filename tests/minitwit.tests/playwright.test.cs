using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace minitwit.tests;

public class playwright_test : PageTest
{
    public static async Task RegisterUser(IPage page, string username = "username", string email = "name@example.com", string password = "Password123!")
    {
        await page.GotoAsync("localhost:5114/");
        await page.GetByRole(AriaRole.Link, new() { Name = "sign up" }).ClickAsync();
        await page.Locator("#Username").ClickAsync();
        await page.Locator("#Username").FillAsync("username");
        await page.Locator("#Email").ClickAsync();
        await page.Locator("#Email").FillAsync("name@example.com");
        await page.Locator("#Password").ClickAsync();
        await page.Locator("#Password").FillAsync("Password123!");
        await page.Locator("#Password2").ClickAsync();
        await page.Locator("#Password2").FillAsync("Password123!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" }).ClickAsync();
        await page.Locator("#Username").ClickAsync();
        await page.Locator("#Username").FillAsync("username");
        await page.Locator("#Password").ClickAsync();
        await page.Locator("#Password").FillAsync("Password123!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
    }

    [Test]
    public async Task _register_user_via_gui()
    {
        //Arrange
        await Page.GotoAsync("localhost:5114/");

        //Act
        await RegisterUser(Page, username: "username", email: "name@example.com", password: "Password123!");

        //Assert
        await Expect(Page.GetByText("You were logged in")).ToBeVisibleAsync();
        Assert.IsNotEmpty(await Page.ContentAsync());

    }
}