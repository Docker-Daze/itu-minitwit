using Chirp.Infrastructure;
using JetBrains.Annotations;
using Xunit;
using Chirp.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.AuthorRepository.Tests;

[TestSubject(typeof(Infrastructure.AuthorRepository))]
public class AuthorRepositoryTest
{
    private readonly ChirpDBContext _dbContext;

    public AuthorRepositoryTest()
    {
        SqliteConnection connection;
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);
        _dbContext = new ChirpDBContext(builder.Options);

        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateAuthor_Stores_New_Author_In_Database()
    {
        // Arrange
        var authorDto = new AuthorDTO { Name = "John Doe", Email = "john.doe@example.com" };

        ICheepRepository cheepRepository = new CheepRepository(_dbContext);
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);

        // Act
        var createdAuthor = await authorRepository.CreateAuthor(authorDto);
        var author = await cheepRepository.FindAuthorByName(createdAuthor.UserName!);

        // Assert
        Assert.NotNull(createdAuthor);
        Assert.Equal(authorDto.Name, createdAuthor.UserName);
        Assert.Equal(authorDto.Email, createdAuthor.Email);

        Assert.NotNull(author);
        Assert.Equal(authorDto.Name, author.UserName);
        Assert.Equal(authorDto.Email, author.Email);
    }

    [Fact]
    public async Task FindAuthor()
    {
        // Arrange
        var authorDto = new AuthorDTO { Name = "John Doe", Email = "name@example.com" };

        ICheepRepository cheepRepository = new CheepRepository(_dbContext);
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);

        // Act
        var createdAuthor = await authorRepository.CreateAuthor(authorDto);
        var author = await cheepRepository.FindAuthorByName(createdAuthor.UserName!);

        // Assert
        Assert.NotNull(author);
        Assert.Equal("John Doe", author.UserName);
        Assert.Equal("name@example.com", author.Email);
    }

    [Fact]
    public async Task IsFollowing_Returns_True_If_User_Is_Following_Author_And_False_if_Not()
    {
        // Arrange
        var authorDto = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorDto2 = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(authorDto);
        await authorRepository.CreateAuthor(authorDto2);
        await authorRepository.Follow(authorDto.Name, authorDto2.Name);
        
        // Assert
        Assert.True(await authorRepository.IsFollowing(authorDto.Name, authorDto2.Name));
        Assert.False(await authorRepository.IsFollowing(authorDto2.Name, authorDto.Name));
    }

    [Fact]
    public async Task User_can_follow_author()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);

        // Assert
        var isFollowing = await authorRepository.IsFollowing(userFollowing.Name, authorFollowed.Name);
        Assert.True(isFollowing);
    }

    [Fact]
    public async Task FollowingList_Stores_Followed_Authors()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);
        
        //Assert
        var following = await authorRepository.GetFollowing(userFollowing.Name);
        Assert.NotEmpty(following);
        Assert.Contains(following, author => author == authorFollowed.Name);
    }
    
    [Fact]
    public async Task FollowerList_Stores_Following_Users()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);
        
        //Assert
        var followers = await authorRepository.GetFollowers(authorFollowed.Name);
        Assert.NotEmpty(followers);
        Assert.Contains(followers, user => user == userFollowing.Name);
    }

    [Fact]
    public async Task Users_Cannot_Follow_Themselves()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        
        await authorRepository.Follow(userFollowing.Name, userFollowing.Name);
        
        // Assert
        var following = await authorRepository.GetFollowing(userFollowing.Name);
        Assert.Empty(following);
        Assert.DoesNotContain(following, author => author == userFollowing.Name);
    }
    
    [Fact]
    public async Task User_can_unfollow_author()
    {
        // Arrange
        var authorDto = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorDto2 = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(authorDto);
        await authorRepository.CreateAuthor(authorDto2);
        await authorRepository.Follow(authorDto.Name, authorDto2.Name);
        await authorRepository.Unfollow(authorDto.Name, authorDto2.Name);
        
        // Assert
        Assert.False(await authorRepository.IsFollowing(authorDto.Name, authorDto2.Name));
    }
    
    [Fact]
    public async Task FollowingList_Does_Not_Store_Unfollowed_Authors()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);
        
        // Assert
        var followingBefore = await authorRepository.GetFollowing(userFollowing.Name);
        Assert.NotEmpty(followingBefore);
        Assert.Contains(followingBefore, author => author == authorFollowed.Name);
        
        // Act
        await authorRepository.Unfollow(userFollowing.Name, authorFollowed.Name);
        
        // Assert
        var followingAfter = await authorRepository.GetFollowing(userFollowing.Name);
        Assert.Empty(followingAfter);
        Assert.DoesNotContain(followingAfter, author => author == authorFollowed.Name);
    }

    [Fact]
    public async Task FollowerList_Does_Not_Store_Unfollowing_Users()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);

        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);

        // Assert
        var followersBefore = await authorRepository.GetFollowers(authorFollowed.Name);
        Assert.NotEmpty(followersBefore);
        Assert.Contains(followersBefore, user => user == userFollowing.Name);

        // Act
        await authorRepository.Unfollow(userFollowing.Name, authorFollowed.Name);

        // Assert
        var followersAfter = await authorRepository.GetFollowers(authorFollowed.Name);

        Assert.Empty(followersAfter);
        Assert.DoesNotContain(followersAfter, user => user == userFollowing.Name);
    }

    [Fact]
    public async Task GetFollowing_Returns_Correct_Following()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);

        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);

        // Assert
        var following = await authorRepository.GetFollowing(userFollowing.Name);
        Assert.NotEmpty(following);
        Assert.Contains(following, author => author == authorFollowed.Name);
        Assert.Single(following);
    }
    
    [Fact]
    public async Task TestGetProfilePicture()
    {
        // Arrange
        var user = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
       
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);

        // Act
        await authorRepository.CreateAuthor(user);

        var expectedPicture = "https://cdn.pixabay.com/photo/2024/01/29/09/06/ai-generated-8539307_1280.png";
        var userPicture = await authorRepository.GetProfilePicture(user.Name);

        // Assert
        Assert.Equal(expectedPicture,userPicture);
    }

    [Fact]
    public async Task TestChangeProfilePicture()
    {
        // Arrange
        var user = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };

        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        var author = await authorRepository.CreateAuthor(user);
        
        // Act
        await authorRepository.ChangeProfilePicture(user.Name,
            "https://i2.wp.com/www.testpic.com/wp-content/uploads/2019/12/logo.png?w=1080&ssl=1");
        
        // Assert
        var defaultPicture = "https://cdn.pixabay.com/photo/2024/01/29/09/06/ai-generated-8539307_1280.png";
        var expectedPicture = "https://i2.wp.com/www.testpic.com/wp-content/uploads/2019/12/logo.png?w=1080&ssl=1";
        Assert.NotEqual(defaultPicture, author.ProfilePicture);
        Assert.Equal(expectedPicture, author.ProfilePicture);
    }

    [Fact]
    public async Task GetFollowers_Returns_Correct_Followers()
    {
        // Arrange
        var userFollowing = new AuthorDTO { Name = "John Doe", Email = "johndoe@example.com" };
        var authorFollowed = new AuthorDTO { Name = "Not John Doe", Email = "notjohndoe@example.com" };
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);

        // Act
        await authorRepository.CreateAuthor(userFollowing);
        await authorRepository.CreateAuthor(authorFollowed);
        await authorRepository.Follow(userFollowing.Name, authorFollowed.Name);

        // Assert
        var followers = await authorRepository.GetFollowers(authorFollowed.Name);
        Assert.NotEmpty(followers);
        Assert.Contains(followers, user => user == userFollowing.Name);
        Assert.Single(followers);
    }
    
    
    [Fact]
    public async Task DeleteAccount_Removes_Author_From_UserManager_And_Database()
    {   
        //Arrange
        var userManager = AuthorRepositoryUtil.GetUserManager(_dbContext);
        var author = new Author { UserName = "JohnDoe", Email = "johndoe@example.com" };
        var password = "SecurePassword123!";
        IAuthorRepository authorRepository = new Infrastructure.AuthorRepository(_dbContext);
        
        //Act
        await userManager.CreateAsync(author, password);
        var author2 = await authorRepository.FindAuthor("JohnDoe");
        
        //Assert
        Assert.Equal(author, author2);
        
        //Act
        await userManager.DeleteAsync(author!);
        
        //Assert
        try
        {
            await authorRepository.FindAuthor("JohnDoe");
        }
        catch (Exception e)
        {
            Assert.Equal("Author with name 'JohnDoe' was not found.", e.Message);
        }
    }
}