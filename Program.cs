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
    // Done
    Console.WriteLine("1) Display Categories");
    // Done
    Console.WriteLine("2) Add Category");
    // Done
    Console.WriteLine("3) Display Category and related active products");
    // Done
    Console.WriteLine("4) Display all Categories and their related active products");
    // TODO
    Console.WriteLine("5) Edit Category");
    // Done
    Console.WriteLine("6) Add Product");
    // Done
    Console.WriteLine("7) Display Products");
    // Done
    Console.WriteLine("8) Display Specific Product Information");
    // TODO
    Console.WriteLine("9) Edit Product");
    // Done
    Console.WriteLine("Enter to quit");

    string? choice = Console.ReadLine();
    Console.Clear();
    logger.Info($"Option {choice} selected");

    if (choice == "1")
    {
        // display categories
        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);

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
        // Display 1 Category and related active products
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
            logger.Error($"Category ID {userCategoryIdChoice} is Invalid");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "4")
    {
        // Display all Categories and their related active products
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
        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);

        Console.WriteLine("Select the ID of the category to edit");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName} - {item.Description}");
        }
        Console.ForegroundColor = ConsoleColor.White;

        // TODO: store user input, find Category, ask what to edit, then edit
        string userCategoryIdChoice = Console.ReadLine()!;
        if (int.TryParse(userCategoryIdChoice, out int id))
        {
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            if (query.Any(c => c.CategoryId == id))
            {
                Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id)!;
                Console.WriteLine("Select the Property to edit");
                Console.WriteLine("1) Category Name");
                Console.WriteLine("2) Category Description");
                Console.WriteLine("3) Category Name and Category Description");
                string userCategoryPropertyChoice = Console.ReadLine()!;
                if (userCategoryPropertyChoice == "1")
                {
                    Console.WriteLine("Enter new unique category name");
                    string userCategoryName = Console.ReadLine()!;
                    if (db.Categories.Any(c => c.CategoryName == userCategoryName) || string.IsNullOrEmpty(userCategoryName))
                    {
                        logger.Error(string.IsNullOrEmpty(userCategoryName) ? "Category name cannot be blank" : $"Category {userCategoryName} already exists");
                    }
                    else
                    {
                        category.CategoryName = userCategoryName;
                        db.SaveChanges();
                        logger.Info($"Category {category.CategoryName} successfully changed");
                    }
                }
                else if (userCategoryPropertyChoice == "2")
                {
                    Console.WriteLine("Enter new category description");
                    string userCategoryDescription = Console.ReadLine()!;
                    if (string.IsNullOrEmpty(userCategoryDescription))
                    {
                        logger.Error("Description cannot be blank");
                    }
                    else
                    {
                        category.Description = userCategoryDescription;
                        db.SaveChanges();
                        logger.Info($"Category description successfully changed to {category.Description}");
                    }
                }
                else if (userCategoryPropertyChoice == "3")
                {
                    Console.WriteLine("Enter new unique category name");
                    string userCategoryName = Console.ReadLine()!;
                    if (db.Categories.Any(c => c.CategoryName == userCategoryName) || string.IsNullOrEmpty(userCategoryName))
                    {
                        logger.Error(string.IsNullOrEmpty(userCategoryName) ? "Category name cannot be blank" : $"Category {userCategoryName} already exists");
                    }
                    else
                    {
                        logger.Info($"Category name {userCategoryName} is valid");
                        Console.WriteLine("Enter new category description");
                        string userCategoryDescription = Console.ReadLine()!;
                        if (string.IsNullOrEmpty(userCategoryDescription))
                        {
                            logger.Error("Description cannot be blank");
                        }
                        else
                        {
                            logger.Info($"Category Description {category.Description} is valid");
                            category.CategoryName = userCategoryName;
                            category.Description = userCategoryDescription;
                            db.SaveChanges();
                            logger.Info($"Category successfully changed to {category.CategoryName} - {category.Description}");
                        }
                    }
                }
                else
                {
                    logger.Error($"Option {userCategoryPropertyChoice} is Invalid");
                }
            }
            else
            {
                logger.Error($"Category ID {id} does not exist");
            }
        }
        else
        {
            logger.Error($"Category ID {userCategoryIdChoice} is Invalid");
        }
        Console.ForegroundColor = ConsoleColor.White;
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
            logger.Error($"Category ID {userProductIdChoice} is Invalid");
        }
    }
    else if (choice == "7")
    {
        // Display products
        Console.WriteLine("Select display option");
        Console.WriteLine("1) All Products");
        Console.WriteLine("2) All Active Products");
        Console.WriteLine("3) All Discontinued Products");
        string displayChoice = Console.ReadLine()!;
        logger.Info($"Option {displayChoice} selected");

        var db = new DataContext();
        var query = db.Products.OrderBy(p => p.ProductId);

        if (displayChoice == "1")
        {
            // Display All
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var item in query)
            {
                Console.WriteLine($"\t{item.ProductName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (displayChoice == "2")
        {
            // Display Active
            Console.ForegroundColor = ConsoleColor.Green;
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
            Console.ForegroundColor = ConsoleColor.Green;
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
            logger.Error($"Option {displayChoice} is invalid");
        }
    }
    else if (choice == "8")
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
            logger.Info($"Product #{id} selected");
            Product? product = db.Products
            // include fields that contain another object
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.OrderDetails)
                .FirstOrDefault(p => p.ProductId == id)!;
            if (product == null)
            {
                logger.Error($"Product #{productDisplayChoice} not found");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                // Got typeof(Product).GetProperties() and prop.GetValue(product) from:
                // https://www.codeproject.com/Articles/667438/How-to-iterate-through-all-properties-of-a-class
                var properties = typeof(Product).GetProperties();
                foreach (var prop in properties)
                {
                    // Store as variable to not have to constantly get value
                    var value = prop.GetValue(product);
                    if (prop.Name == "UnitPrice" && value != null)
                    {
                        Console.WriteLine($"{prop.Name}: {value:C2}");
                    }
                    else if (prop.Name == "Category" && product.Category != null)
                    {
                        // Cast value to Category to get object's properties
                        Category category = (Category)value!;
                        Console.WriteLine($"{prop.Name}: {category.CategoryName}");
                    }
                    else if (prop.Name == "Supplier" && product.SupplierId != null)
                    {
                        // Cast value to Supplier to get object's properties
                        Supplier supplier = (Supplier)value!;
                        Console.WriteLine($"{prop.Name}: {supplier.CompanyName}");
                    }
                    // I wasn't sure how to handle order details, so I printed all fields in order details ICollection
                    else if (prop.Name == "OrderDetails")
                    {
                        // Vast value to ICollection<OrderDetail> to get object's properties
                        var orderDetails = (ICollection<OrderDetail>)value!;
                        if (orderDetails.Count == 0)
                        {
                            Console.WriteLine($"{prop.Name}: No orders found");
                        }
                        else
                        {
                            Console.WriteLine($"{prop.Name}: {orderDetails.Count} Orders");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (var detail in orderDetails)
                            {
                                Console.WriteLine($"\tOrder #{detail.OrderId}:");
                                Console.WriteLine($"\t\tQuantity: {detail.Quantity}");
                                Console.WriteLine($"\t\tUnit Price: {detail.UnitPrice:C2}");
                                Console.WriteLine($"\t\tDiscount: {detail.Discount:P0}");
                            }
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{prop.Name}: {value}");
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else
        {
            logger.Error($"Option {productDisplayChoice} is Invalid");
        }
    }
    else if (choice == "9")
    {
        // Edit Product
    }
    else if (string.IsNullOrEmpty(choice))
    {
        break;
    }
    Console.WriteLine();
} while (true);

Console.Clear();
logger.Info("Program ended");