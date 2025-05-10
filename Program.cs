using NLog;
using System.Linq;
using NorthwindConsole.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";
// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
    Console.WriteLine("1) Display Categories");
    Console.WriteLine("2) Add Category");
    Console.WriteLine("3) Display Category and related active products");
    Console.WriteLine("4) Display all Categories and their related active products");
    Console.WriteLine("5) Edit Category");
    Console.WriteLine("6) Add Product");
    Console.WriteLine("7) Edit Product");
    Console.WriteLine("8) Display Products");
    Console.WriteLine("9) Display Specific Product Information");
    Console.WriteLine("Enter to quit");
    string? choice = Console.ReadLine();
    Console.Clear();
    logger.Info("Option {choice} selected", choice);

    if (choice == "1")
    {
        // display categories
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

        var config = configuration.Build();

        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryName);

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"{query.Count()} records returned");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName} - {item.Description}");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "2")
    {
        // Add category
        Category category = new();
        Console.WriteLine("Enter Category Name:");
        category.CategoryName = Console.ReadLine()!;
        Console.WriteLine("Enter the Category Description:");
        category.Description = Console.ReadLine();
        ValidationContext context = new ValidationContext(category, null, null);
        List<ValidationResult> results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(category, context, results, true);
        if (isValid)
        {
            var db = new DataContext();
            // check for unique name
            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
            {
                // generate validation error
                isValid = false;
                results.Add(new ValidationResult("Name exists", ["CategoryName"]));
            }
            else
            {
                logger.Info("Validation passed");
                db.AddCategory(category);
                logger.Info($"Category {category.CategoryName} successfully added");
            }
        }
        if (!isValid)
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
        }
    }
    else if (choice == "3")
    {
        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);

        Console.WriteLine("Select the ID of the category whose products you want to display:");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        string userCategoryIdChoice = Console.ReadLine()!;
        if (int.TryParse(userCategoryIdChoice, out int id))
        {
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"{category.Products.Count} products returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (Product p in category.Products)
            {
                if (!p.Discontinued)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else
        {
            logger.Error("Category ID {userCategoryIdChoice} is Invalid", userCategoryIdChoice);
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "4")
    {
        var db = new DataContext();
        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
        foreach (var item in query)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (Product p in item.Products)
            {
                if (!p.Discontinued)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "5")
    {
        // Edit Category
        // Display categories
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

        var config = configuration.Build();

        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryName);

        Console.WriteLine("Select the ID of the category to edit");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName} - {item.Description}");
        }
        Console.ForegroundColor = ConsoleColor.White;

        // TODO: Prompt user for ID, find Category, ask what to edit, then edit

    }
    else if (choice == "6")
    {
        // Add product
        // Create new product
        Product product = new();
        // Prompt for and store name
        Console.WriteLine("Enter Product Name:");
        product.ProductName = Console.ReadLine()!;

        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);
        Console.WriteLine($"Select the category you want to add {product.ProductName} to");

        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        string userProductIdChoice = Console.ReadLine()!;
        if (int.TryParse(userProductIdChoice, out int id))
        {
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            product.Category = db.Categories.FirstOrDefault(c => c.CategoryId == id)!;
            product.CategoryId = id;

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", ["ProductName"]));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddProduct(product);
                    logger.Info($"Product {product.ProductName} successfully added to {product.Category}");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else
        {
            logger.Error("Category ID {userProductIdChoice} is Invalid", userProductIdChoice);
        }
    }
    else if (choice == "7")
    {
        // Edit Product
    }
    else if (choice == "8")
    {
        // Display products
        Console.WriteLine("Select display option");
        Console.WriteLine("1) All Products");
        Console.WriteLine("2) All Active Products");
        Console.WriteLine("3) All Discontinued Products");
        string displayChoice = Console.ReadLine()!;
        logger.Info("Option {displayChoice} selected", displayChoice);

        var db = new DataContext();
        var query = db.Products.OrderBy(p => p.ProductName);

        if (displayChoice == "1")
        {
            // Display All
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"\t{item.ProductName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (displayChoice == "2")
        {
            // Display Active
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                if (!item.Discontinued)
                {
                    Console.WriteLine($"\t{item.ProductName}");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (displayChoice == "3")
        {
            // Display discontinued
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                if (item.Discontinued)
                {
                    Console.WriteLine($"\t{item.ProductName}");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            logger.Error("Option {displayChoice} is invalid", displayChoice);
        }
    }
    else if (choice == "9")
    {
        // Display specific product
        var db = new DataContext();
        var query = db.Products.OrderBy(p => p.ProductId);

        Console.WriteLine("Select ID of Product to Display");
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.ProductId}) {item.ProductName}");
        }
        Console.ForegroundColor = ConsoleColor.White;

        string productDisplayChoice = Console.ReadLine()!;
        if (int.TryParse(productDisplayChoice, out int id))
        {
            logger.Info("Product #{id} selected", id);
            Product? product = db.Products.FirstOrDefault(p => p.ProductId == id)!;
            if (product == null)
            {
                logger.Error("Product #{productDisplayChoice} not found", productDisplayChoice);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Product ID: {product.ProductId}");
                Console.WriteLine($"Product Name: {product.ProductName}");
                Console.WriteLine($"Supplier ID: {product.SupplierId}");
                Console.WriteLine($"Category ID: {product.CategoryId}");
                Console.WriteLine($"Quantity: {product.QuantityPerUnit}");
                Console.WriteLine($"Unit Price: {product.UnitPrice:C2}");
                Console.WriteLine($"Units in Stock: {product.UnitsInStock}");
                Console.WriteLine($"Units on Order: {product.UnitsOnOrder}");
                Console.WriteLine($"Reorder Level: {product.ReorderLevel}");
                Console.WriteLine(product.Discontinued ? "Active" : "Discontinued");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else
        {
            logger.Error("Option {productDisplayChoice} is invalid", productDisplayChoice);
        }
    }
    else if (string.IsNullOrEmpty(choice))
    {
        break;
    }
    Console.WriteLine();
} while (true);

Console.Clear();
logger.Info("Program ended");