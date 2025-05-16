using NLog;
using NorthwindConsole.Model;
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
    Console.WriteLine("7) Display Products");
    Console.WriteLine("8) Display Specific Product Information");
    Console.WriteLine("9) Edit Product");
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
            if (db.Categories.Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
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
                Console.Clear();
                logger.Info($"Option {userCategoryPropertyChoice} selected");
                if (userCategoryPropertyChoice == "1")
                {
                    Console.WriteLine("Enter new unique category name");
                    string userCategoryName = Console.ReadLine()!;
                    if (db.Categories.Any(c => c.CategoryName.ToLower() == userCategoryName.ToLower()) || string.IsNullOrEmpty(userCategoryName))
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
                    if (db.Categories.Any(c => c.CategoryName.ToLower() == userCategoryName.ToLower()) || string.IsNullOrEmpty(userCategoryName))
                    {
                        logger.Error(string.IsNullOrEmpty(userCategoryName) ? "Category name cannot be blank" : $"Category '{userCategoryName}' already exists");
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
                    logger.Error($"Option '{userCategoryPropertyChoice}' is Invalid");
                }
            }
            else
            {
                logger.Error($"Category ID '{id}' does not exist");
            }
        }
        else
        {
            logger.Error($"Category ID '{userCategoryIdChoice}' is Invalid");
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
                if (db.Products.Any(p => p.ProductName.ToLower() == product.ProductName.ToLower()))
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
            logger.Error($"Category ID '{userProductIdChoice}' is Invalid");
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
        Console.Clear();
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
            logger.Error($"Option '{displayChoice}' is Invalid");
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
                logger.Error($"Product #'{productDisplayChoice}' not found");
            }
            else
            {
                Console.Clear();
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
                    else if (prop.Name == "OrderDetails")
                    {
                        // Cast value to ICollection<OrderDetail> to get object's properties
                        var orderDetails = (ICollection<OrderDetail>)value!;
                        if (orderDetails.Count == 0)
                        {
                            Console.WriteLine($"{prop.Name}: No orders found");
                        }
                        else
                        {
                            Console.WriteLine($"{prop.Name}: {orderDetails.Count} Orders");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            // I wasn't sure how to handle order details, so I printed all fields in each OrderDetail in OrderDetails ICollection
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
            logger.Error($"Option '{productDisplayChoice}' is Invalid");
        }
    }
    else if (choice == "9")
    {
        // Edit Product
        var db = new DataContext();
        var query = db.Products.OrderBy(p => p.ProductId);

        Console.WriteLine("Select ID of Product to Edit");
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
                .FirstOrDefault(p => p.ProductId == id)!;
            if (product == null)
            {
                logger.Error($"Product #'{productDisplayChoice}' not found");
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Select the property you want to edit");
                Console.WriteLine("1) Product Name");
                Console.WriteLine("2) Supplier");
                Console.WriteLine("3) Category");
                Console.WriteLine("4) Quantity");
                Console.WriteLine("5) Unit Price");
                Console.WriteLine("6) Units in Stock");
                Console.WriteLine("7) Units on Order");
                Console.WriteLine("8) Reorder Level");
                Console.WriteLine("9) Discontinued Status");

                string productPropertyChoice = Console.ReadLine()!;
                Console.Clear();
                logger.Info($"Option {productPropertyChoice} Selected");
                if (productPropertyChoice == "1")
                {
                    // Edit name
                    Console.WriteLine("Enter the new unique Product Name");
                    string userProductName = Console.ReadLine()!;
                    logger.Info($"Product Name '{userProductName}' Entered");
                    if (db.Products.Any(p => p.ProductName.ToLower() == userProductName.ToLower()) || string.IsNullOrEmpty(userProductName))
                    {
                        logger.Error(string.IsNullOrEmpty(userProductName) ? "Product name cannot be blank" : $"Category '{userProductName}' already exists");
                    }
                    else
                    {
                        product.ProductName = userProductName;
                        logger.Info($"Product Name successfully changed to {product.ProductName}");
                        db.SaveChanges();
                    }
                }
                else if (productPropertyChoice == "2")
                {
                    // Edit Supplier
                    Console.WriteLine("Select the Supplier's ID to add to Product");
                    var suppliers = db.Suppliers.OrderBy(s => s.SupplierId);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    foreach (var s in suppliers)
                    {
                        Console.WriteLine($"{s.SupplierId}) {s.CompanyName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    string userSupplierChoice = Console.ReadLine()!;
                    if (int.TryParse(userSupplierChoice, out int supplierId))
                    {
                        logger.Info($"SupplierID '{supplierId}' Selected");
                        Supplier supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == supplierId)!;
                        if (supplier != null)
                        {
                            product.Supplier?.Products.Remove(product);
                            product.SupplierId = supplierId;
                            product.Supplier = supplier;
                            supplier.Products.Add(product);
                            logger.Info($"{product.ProductName}'s Supplier was successfully changed to {product.SupplierId}) {product.Supplier.CompanyName}.");
                            db.SaveChanges();
                        }
                        else
                        {
                            logger.Error($"Supplier '{supplierId}' not found");
                        }
                    }
                    else
                    {
                        logger.Error($"Option '{userSupplierChoice}' is Invalid");
                    }
                }
                else if (productPropertyChoice == "3")
                {
                    // Edit Category
                    Console.WriteLine("Select the Category's ID to add to Product");
                    var categories = db.Categories.OrderBy(c => c.CategoryId);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    foreach (var c in categories)
                    {
                        Console.WriteLine($"{c.CategoryId}) {c.CategoryName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    string userCategoryChoice = Console.ReadLine()!;
                    if (int.TryParse(userCategoryChoice, out int categoryId))
                    {
                        logger.Info($"Category ID {categoryId} Selected");
                        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == categoryId)!;
                        if (category != null)
                        {
                            product.Category?.Products.Remove(product);
                            product.CategoryId = categoryId;
                            product.Category = category;
                            category.Products.Add(product);
                            logger.Info($"{product.ProductName}'s Category was successfully changed to {product.CategoryId}) {product.Category.CategoryName}.");
                            db.SaveChanges();
                        }
                        else
                        {
                            logger.Error($"Category '{categoryId}' not found");
                        }
                    }
                    else
                    {
                        logger.Error($"Option '{userCategoryChoice}' is Invalid");
                    }
                }
                else if (productPropertyChoice == "4")
                {
                    // Edit Quantity string
                    Console.WriteLine("Enter the new Quantity per Unit");
                    string userQuantity = Console.ReadLine()!;
                    logger.Info($"Unit Price {userQuantity} Entered");
                    if (string.IsNullOrEmpty(userQuantity))
                    {
                        logger.Error("Quantity per Unit cannot be empty");
                    }
                    else
                    {
                        product.QuantityPerUnit = userQuantity;
                        logger.Info($"{product.ProductName}'s Quantity per Unit successfully changed to {product.QuantityPerUnit}");
                        db.SaveChanges();
                    }
                }
                else if (productPropertyChoice == "5")
                {
                    // Edit Unit Price decimal
                    Console.WriteLine("Enter the new Unit Price as a number");
                    string userUnitPrice = Console.ReadLine()!;
                    logger.Info($"Unit Price of '{userUnitPrice}' Entered");
                    if (decimal.TryParse(userUnitPrice, out decimal unitPrice) && unitPrice >= 0)
                    {
                        product.UnitPrice = unitPrice;
                        logger.Info($"{product.ProductName}'s Unit Price successfully changed to {product.UnitPrice:C2}");
                        db.SaveChanges();
                    }
                    else
                    {
                        logger.Error($"Price '{userUnitPrice}' is Invalid");
                    }
                }
                else if (productPropertyChoice == "6")
                {
                    // Edit Units in Stock short
                    Console.WriteLine("Enter the new number of Unit's in Stock");
                    string userStock = Console.ReadLine()!;
                    logger.Info($"Unit Stock of '{userStock}' Entered");
                    if (short.TryParse(userStock, out short unitStock))
                    {
                        product.UnitsInStock = unitStock;
                        logger.Info($"{product.ProductName}'s Stock was updated to {product.UnitsInStock}");
                        db.SaveChanges();
                    }
                    else
                    {
                        logger.Error($"Unit Stock '{userStock}' is Invalid");
                    }
                }
                else if (productPropertyChoice == "7")
                {
                    // Edit Units on Order short
                    Console.WriteLine("Enter the new number of Unit's on Order");
                    string userUnitsOnOrder = Console.ReadLine()!;
                    logger.Info($"User enetered '{userUnitsOnOrder}' Units on Order");
                    if (short.TryParse(userUnitsOnOrder, out short onOrder))
                    {
                        product.UnitsOnOrder = onOrder;
                        logger.Info($"{product.ProductName}'s Unit's on Order was updated to {product.UnitsOnOrder}");
                        db.SaveChanges();
                    }
                    else
                    {
                        logger.Error($"Units on Order '{userUnitsOnOrder}' is Invalid");
                    }
                }
                else if (productPropertyChoice == "8")
                {
                    // Edit Reorder Level short
                    Console.WriteLine("Enter the new Reorder Level");
                    string userReorderLevel = Console.ReadLine()!;
                    logger.Info($"User enetered '{userReorderLevel}' for Reorder Level");
                    if (short.TryParse(userReorderLevel, out short reorderLevel))
                    {
                        product.ReorderLevel = reorderLevel;
                        logger.Info($"{product.ProductName}'s Reorder Level was updated to {product.ReorderLevel}");
                        db.SaveChanges();
                    }
                    else
                    {
                        logger.Error($"Reorder Level '{userReorderLevel}' is Invalid");
                    }
                }
                else if (productPropertyChoice == "9")
                {
                    // Edit Discontinued bool
                    Console.WriteLine("Select the new Discontinued status");
                    Console.WriteLine("1) Active");
                    Console.WriteLine("2) Discontinued");
                    string userDiscontinued = Console.ReadLine()!;
                    logger.Info($"User selected option '{userDiscontinued}' for Discontinued Status");
                    if (userDiscontinued == "1")
                    {
                        product.Discontinued = false;
                        logger.Info($"Discontinued Status successfully updated to Active");
                        db.SaveChanges();
                    }
                    else if (userDiscontinued == "2")
                    {
                        product.Discontinued = true;
                        logger.Info($"Discontinued Status successfully updated to Discontinued");
                        db.SaveChanges();
                    }
                    else
                    {
                        logger.Error($"Option {userDiscontinued} is Invalid");
                    }
                }
                else
                {
                    logger.Error($"Property choice of '{productPropertyChoice}' is invalid");
                }
            }
        }
        else
        {
            logger.Error($"Option '{productDisplayChoice}' is Invalid");
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
