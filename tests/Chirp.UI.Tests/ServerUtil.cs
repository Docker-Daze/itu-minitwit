using System.Diagnostics;
using Microsoft.Playwright;

namespace Chirp.UI.Tests
{
    /// <summary>
    /// Provides utility methods used for UI testing.
    /// </summary>
    public static class ServerUtil 
    {
        /// <summary>
        /// Starts a web server used to run Playwright tests.
        /// </summary>
        /// <returns></returns>
        public static async Task<Process> StartServer()
        {
            var serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --project ../../../../../src/Chirp.Web",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            serverProcess.Start();
            await Task.Delay(10000);

            return serverProcess;

        }

        /// <summary>
        /// Helper method for logging in a user with the specified information.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public static async Task LoginUser(IPage page, string email = "name@example.com", string password = "Password123!")
        {

            var isUserLoggedIn = await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).IsVisibleAsync();
            if (isUserLoggedIn)
            {
                return;
            }
            
            //Arrange
            await page.GotoAsync("https://localhost:5273/Identity/Account/Login");

            await page.GetByPlaceholder("name@example.com").FillAsync(email);
            await page.GetByPlaceholder("password").FillAsync(password);
            
            await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        }
        
        /// <summary>
        /// Helper method for registering a user with the specified information.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public static async Task RegisterUser(IPage page, string username = "username", string email = "name@example.com", string password = "Password123!")
        {
            await page.GotoAsync("https://localhost:5273/");
            await page.GetByRole(AriaRole.Link, new() { Name = "register" }).ClickAsync();
            await page.GetByPlaceholder("user name").ClickAsync();
            
            await page.GetByPlaceholder("user name").FillAsync(username);
            await page.GetByPlaceholder("name@example.com").FillAsync(email);
            await page.GetByLabel("Password", new() { Exact = true }).FillAsync(password);
            await page.GetByLabel("Confirm Password").FillAsync(password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        }

        /// <summary>
        /// Helper method for logging in and deleting a users profile with the provided information.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public static async Task DeleteUser(IPage page, string email = "name@example.com", string password = "Password123!")
        {
            var isUserLoggedIn = await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).IsVisibleAsync();
            if (!isUserLoggedIn)
            {
                await LoginUser(page, email, password);

                var noUserFound = await page.GetByText("No user found").IsVisibleAsync();
                if (noUserFound)
                {
                    return;
                }
            }
            
            await page.GetByRole(AriaRole.Link, new() { Name = "manage account" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete" }).ClickAsync();
            await page.GetByPlaceholder("Please enter your password.").FillAsync(password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Delete data and close my account" }).ClickAsync();
        }
    }
}