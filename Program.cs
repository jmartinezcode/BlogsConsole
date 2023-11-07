using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq;
using System.Runtime.InteropServices;

string path = Directory.GetCurrentDirectory() + "\\nlog.config";
// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

while (true)
{
    var choice = DisplayMenu();
    logger.Info($"Option \"{choice}\" selected");

    if (choice == "q")
        break;
    
    switch (choice)
    {
        case "1":
            DisplayBlogs();
            break;
        case "2":
            AddBlog(logger);
            break;
        case "3":
            CreatePost(logger);
            break;
        case "4":
            DisplayPosts(logger);
            break;
        default:
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Invalid selection. Please try again.");
            Console.ResetColor();
            break;
    }
}

logger.Info("Program ended");

static string DisplayMenu()
{
    Console.WriteLine("\nEnter your selection:");
    Console.WriteLine("1) Display all blogs");
    Console.WriteLine("2) Add Blog");
    Console.WriteLine("3) Create Post");
    Console.WriteLine("4) Display Posts");
    Console.WriteLine("Enter q to quit");

    return Console.ReadLine();
}

static void DisplayBlogs()
{
    // Display all Blogs from the database
    var db = new BloggingContext();
    var blogs = db.Blogs.OrderBy(b => b.Name).ToList();
    Console.WriteLine($"\n{blogs.Count} Blogs returned");

    foreach (var blog in blogs)
        Console.WriteLine(blog.Name);
}

static void AddBlog(Logger logger)
{
    // Create and add Blog
    Console.Write("Enter a name for a new Blog: ");
    var name = Console.ReadLine();
    // Do not allow null Blog names
    if (string.IsNullOrEmpty(name))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        logger.Error("Blog name cannot be null");
        Console.ResetColor();
        return;
    }

    var blog = new Blog { Name = name };

    var db = new BloggingContext();
    db.AddBlog(blog);
    logger.Info("Blog added - {name}", name);
}

static void CreatePost(Logger logger)
{
    // Create a New Post
    var db = new BloggingContext();
    var blogs = db.Blogs.OrderBy(b => b.BlogId).ToList();
    // List all Options
    Console.WriteLine("Select the blog you'd like to post to: ");
    for (int i = 0; i < blogs.Count; i++)
    {
        Console.WriteLine($"{i + 1}) {blogs[i].Name}");
    }
    // Error handling of invalid inputs
    if (!int.TryParse(Console.ReadLine(), out int blogID) || blogID < 1 || blogID > blogs.Count)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        logger.Error("Invalid Blog ID");
        Console.ResetColor();
        return;
    }

    var selectedBlog = blogs[blogID - 1];
    // Add Post Title
    Console.Write("Enter the Post Title: ");
    var title = Console.ReadLine();

    // Check that title is not null
    if (string.IsNullOrEmpty(title))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        logger.Error("Post Title cannot be null.");
        Console.ResetColor();
        return;
    }
    // Add Post Content
    Console.Write("Enter the Post Content: ");
    var content = Console.ReadLine();
    // Create Post
    var post = new Post
    {
        Title = title,
        Content = content,
        BlogId = selectedBlog.BlogId
    };
    // Add post to dB
    db.AddPost(post);
    logger.Info($"Post added - {title}");
}
static void DisplayPosts(Logger logger)
{
    // Display Posts
    var db = new BloggingContext();
    var blogs = db.Blogs.OrderBy(b => b.BlogId).ToList();

    Console.WriteLine("Select the blog's posts to display:");
    Console.WriteLine("0) Posts from ALL blogs");

    for (int i = 0; i < blogs.Count; i++)
    {
        Console.WriteLine($"{i + 1}) Posts from {blogs[i].Name}");
    }
    // Error Handling Invalid Options
    if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > blogs.Count)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        logger.Error("Invalid Selection");
        Console.ResetColor();
        return;
    }
    if (choice == 0)
    {
        // Display ALL posts from ALL blogs
        var allPosts = db.Posts.Include(p => p.Blog).OrderBy(p => p.PostId).ToList();

        Console.WriteLine($"\n{allPosts.Count} post(s) returned");
        foreach (var post in allPosts)
        {
            Console.WriteLine($"Blog: {post.Blog.Name}");
            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine($"Content: {post.Content}\n");
        }
    }
    else
    {
        // Display posts from selected blog
        var selectedBlog = blogs[choice - 1];
        db.Entry(selectedBlog).Collection(b => b.Posts).Load(); //To poulate Posts and prevent null exception errors
        var blogPosts = selectedBlog.Posts.OrderBy(p => p.PostId).ToList();

        Console.WriteLine($"\n{blogPosts.Count} post(s) returned");
        foreach (var post in blogPosts)
        {
            Console.WriteLine($"Blog: {post.Blog.Name}");
            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine($"Content: {post.Content}\n");
        }
    }
}