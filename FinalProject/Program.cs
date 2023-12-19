using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using ConsoleTable;
using Newtonsoft.Json;
using System.Linq;
interface IAccount
{
    void Initializedb();
    void AddUser(string username, string password);
    bool CheckUser(string username, string password);
}
interface IProduct
{
    void AddProduct(string productName, double productStock, double pricePerKg);
    void ModifyProduct(int productId, string newProductName, double newProductStock, double newPricePerKg);
    void DeleteProduct(int productId);
    CombinedEntities ReadFromJsonFile();
    void WriteToJsonFile(string filePath, CombinedEntities tables);
}
interface IOrders
{
    void AddOrder(int userId, List<OrderedProduct> ordered, double totalOrderPrice, string? payMethod, string? deliveryAddress);
    void ChangeOrderOrder(int orderId, string status);
}
class filePath { public string path = "finalProjectDB.json"; }
class UserAccount : IAccount
{
    filePath _filePath = new filePath();
    public void Initializedb()
    {
        if (!File.Exists(_filePath.path))
        {
            var adminAccount = new UserAccounts { Id = 1, UserName = "Admin", Password = "123" };

            var tables = new CombinedEntities
            {
                Users = new List<UserAccounts> { adminAccount },
                Products = new List<Product> { },
                Orders = new List<Order> { }
            };
            WriteToJsonFile(_filePath.path, tables);
        }
    }
    public void AddUser(string username, string password)
    {
        CombinedEntities tables = ReadFromJsonFile();
        tables.Users.Add(new UserAccounts { Id = GetNextId(tables.Users), UserName = username, Password = password });
        WriteToJsonFile(_filePath.path, tables);
    }
    public bool CheckUser(string username, string password)
    {
        CombinedEntities tables = ReadFromJsonFile();
        UserAccounts checkUser = tables.Users.Find(u => u.UserName == username && u.Password == password);
        if (checkUser != null)
        {
            return true;
        }
        return false;

    }
    public CombinedEntities ReadFromJsonFile()
    {
        if (File.Exists(_filePath.path))
        {
            string json = File.ReadAllText(_filePath.path);
            return JsonConvert.DeserializeObject<CombinedEntities>(json);
        }
        return new CombinedEntities();
    }
    private void WriteToJsonFile(string filePath, CombinedEntities tables)
    {
        string json = JsonConvert.SerializeObject(tables, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    static int GetNextId(List<UserAccounts> users)
    {
        int maxId = users.Count > 0 ? users.Max(u => u.Id) : 0;
        return maxId + 1;
    }

}
class ProductManagement : OrderManagement, IProduct
{
    filePath _filePath = new filePath();
    public void WriteToJsonFile(string filePath, CombinedEntities tables)
    {
        string json = JsonConvert.SerializeObject(tables, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    public void AddProduct(string productName, double productStock, double pricePerKg)
    {
        try
        {
            CombinedEntities tables = ReadFromJsonFile();
            tables.Products.Add(new Product { ProductId = GetNextId(tables.Products), ProductName = productName, ProductStock = productStock, PricePerKg = pricePerKg });
            
            WriteToJsonFile(_filePath.path, tables);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding product: {ex.Message}");
        }
    }

    public void ModifyProduct(int productId, string newProductName, double newProductStock, double newPricePerKg)
    {
        try
        {
            CombinedEntities tables = ReadFromJsonFile();
            Product productToModify = tables.Products.Find(p => p.ProductId == productId);

            if (productToModify != null)
            {
                productToModify.ProductName = newProductName;
                productToModify.ProductStock = newProductStock;
                productToModify.PricePerKg = newPricePerKg;
            }
            WriteToJsonFile(_filePath.path, tables);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error modifying product: {ex.Message}");
        }
    }

    public void DeleteProduct(int productId)
    {
        try
        {
            CombinedEntities tables = ReadFromJsonFile();

            // Find the index of the product with the given ID
            int productIndex = tables.Products.FindIndex(p => p.ProductId == productId);

            if (productIndex != -1)
            {
                tables.Products.RemoveAt(productIndex);

                Console.WriteLine("Product deleted successfully.");
            }
            else
            {
                Console.WriteLine("Product not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting product: {ex.Message}");
        }
    }

    public CombinedEntities ReadFromJsonFile()
    {
        try
        {
            if (File.Exists(_filePath.path))
            {
                string json = File.ReadAllText(_filePath.path);
                return JsonConvert.DeserializeObject<CombinedEntities>(json);
            }
            return new CombinedEntities();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from JSON file: {ex.Message}");
            return new CombinedEntities();
        }
    }

    static int GetNextId(List<Product> users)
    {
        int maxId = users.Count > 0 ? users.Max(u => u.ProductId) : 0;
        return maxId + 1;
    }
}
class OrderManagement : IOrders
{
    filePath _filePath = new filePath();
    public void AddOrder(int userId, List<OrderedProduct> ordered, double totalOrderPrice, string? payMethod, string? deliveryAddress)
    {
        CombinedEntities tables = ReadFromJsonFile();
        tables.Orders.Add(new Order
        {
            OrderId = GetNextId(tables.Orders),
            UserId = userId,
            Ordered = ordered,
            TotalOrderPrice = totalOrderPrice,
            Status = "Pending",
            PaymentMethod = payMethod,
            DeliveryAddress = deliveryAddress
        });
        WriteToJsonFile(_filePath.path, tables);
    }
    public void ChangeOrderOrder(int orderId, string status)
    {
        CombinedEntities tables = ReadFromJsonFile();
        Order order = tables.Orders.Find(o=>o.OrderId == orderId);
        order.Status = status;
        WriteToJsonFile(_filePath.path, tables);
    }
    public CombinedEntities ReadFromJsonFile()
    {
        if (File.Exists(_filePath.path))
        {
            string json = File.ReadAllText(_filePath.path);
            return JsonConvert.DeserializeObject<CombinedEntities>(json);
        }
        return new CombinedEntities();
    }
    private void WriteToJsonFile(string filePath, CombinedEntities tables)
    {
        string json = JsonConvert.SerializeObject(tables, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    static int GetNextId(List<Order> orders)
    {
        int maxId = orders.Count > 0 ? orders.Max(u => u.OrderId) : 0;
        return maxId + 1;
    }
}

public class signup
{
    public string start()
    {
        IAccount account = new UserAccount();

        account.Initializedb();

        string message = "Type the number of your choice..";
        while (true)
        {
            Console.Clear();
            Console.WriteLine("┌───────────────────────────────────────┐");
            Console.WriteLine("│          FishMarket de Elena          │");
            Console.WriteLine("└───────────────────────────────────────┘");
            Console.WriteLine(">>" + message);
            Console.WriteLine();
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine();
            Console.Write("input >> ");
            string choice = Console.ReadLine();

            if (!int.TryParse(choice, out int parsedChoice))
            {
                Console.Beep();
                message = "Invalid input. Please enter a valid number.";
                continue;
            }

            if (parsedChoice == 1) //login
            {
                Console.Clear();
                Console.WriteLine("───────────────────────────────────────");
                Console.WriteLine("               User Login");
                Console.WriteLine("───────────────────────────────────────");
                Console.WriteLine();
                Console.Write("User Name: ");
                string username = Console.ReadLine();
                Console.Write("Password: ");
                string password = GetMaskedInput();

                if (username != null && password != null)
                {
                    if (account.CheckUser(username, password))
                    {
                        return username;
                    }
                    else
                    {
                        Console.Beep();
                        message = "Invalid Username and Password";
                    }
                }
            }
            else if (parsedChoice == 2) // registration
            {
                Console.Clear();
                Console.WriteLine("───────────────────────────────────────");
                Console.WriteLine("             App Registration");
                Console.WriteLine("───────────────────────────────────────");
                Console.WriteLine();
                Console.Write("User Name: ");
                string username = Console.ReadLine();
                Console.WriteLine();
                Console.Write("Password: ");
                string password = GetMaskedInput();
                Console.WriteLine();
                Console.Write("Confirm password: ");
                string ConfirmPassword = GetMaskedInput();

                if (password != ConfirmPassword)
                {
                    message = "Input Password does not match!";
                    continue;
                }

                if (username != "" && password != "")
                {

                    if (!account.CheckUser(username, password))
                    {
                        account.AddUser(username, password);
                        message = "User Registered.";
                    }
                    else
                    {
                        message = "Username already exist!";
                    }
                }
                else
                {
                    message = "Invalid username or password.";
                }
            }
            else
            {
                Console.Clear();
                message = "Invalid choice. Please enter a valid option.";
            }
        }
    }
    static string GetMaskedInput()
    {
        StringBuilder input = new StringBuilder();
        string result = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true); // Hide the key being pressed

            if (char.IsLetterOrDigit(key.KeyChar) || char.IsSymbol(key.KeyChar) || char.IsPunctuation(key.KeyChar))
            {
                input.Append('*');
                result += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input.Length--;
                result = result.Substring(0, result.Length - 1);
                Console.Write("\b \b"); // Move the cursor back, write a space, move back again
            }
        } while (key.Key != ConsoleKey.Enter);

        return result;
    }
}
public class FishMarket
{
    IProduct product = new ProductManagement();
    IOrders orderings = new OrderManagement();
    static List<Order> orders = new List<Order>();
    static List<OrderedProduct> ordered = new List<OrderedProduct>();
    //static bool success = false;
    public void Tasks(string username)
    {
        string message = "Type the number of your choice..";
        while (true)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("┌───────────────────────────────────────────────────┐");
                Console.WriteLine("  Hello \"" + username + "\"!, Welcome to FishMarket de Elena ");
                Console.WriteLine("└───────────────────────────────────────────────────┘");
                Console.WriteLine(">> " + message);
                Console.WriteLine();
                Console.WriteLine("1. Place Order");
                Console.WriteLine("2. My Purchases");
                Console.WriteLine("3. Log out");

                string choice = Console.ReadLine();

                if (!int.TryParse(choice, out int parsedChoice))
                {
                    message = "Invalid input. Please enter a valid number.";
                    continue;
                }

                switch (parsedChoice)
                {
                    case 1:
                        PlaceOrder(username); // Ensure this method also has proper exception handling
                        break;
                    case 2:
                        DisplayOrders(username); // Ensure this method also has proper exception handling
                        break;
                    case 3:
                        orders.Clear();
                        ordered.Clear();
                        return; // Log out
                    default:
                        message = "Invalid choice. Please enter a valid option.";
                        break;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O Error: {ex.Message}");
                // Consider logging the error details for troubleshooting
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Consider logging the error details for troubleshooting
            }
        }
    }

    public void DisplayOrderDetails(int orderNumber)
    {
        Console.Clear();
        CombinedEntities table = product.ReadFromJsonFile();
        var selectedOrder = table.Orders.Find(o => o.OrderId == orderNumber);
        if (selectedOrder != null)
        {
            var tableForm3 = new Table();
            tableForm3.SetHeaders("Fish", "Order Quantity", "Price Per Kg", "Total Price");
            foreach (var ordered in selectedOrder.Ordered)
            {
                tableForm3.AddRow(
                    table.Products.Find(p => p.ProductId == ordered.ProductId).ProductName,
                    (ordered.OrderedQty).ToString(),
                    (ordered.OrderedPricePerKg).ToString(),
                    (ordered.OrderedPriceTotal).ToString());
            }
            Console.WriteLine(tableForm3.ToString());
            Console.Read();
        }
    }
    public void DisplayOrders(string username)
    {
        CombinedEntities table = product.ReadFromJsonFile();
        int uid = table.Users.Find(u => u.UserName == username).Id;

        string message = "Input the Order Id to show the details.";
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            Console.WriteLine("input \"b\" to Return | \"e\" to exit app");
            Console.WriteLine("user: " + username);
            Console.WriteLine(">> " + message);
            Console.WriteLine();
            Console.WriteLine("ORDERS:");

            var tableForm = new Table();
            tableForm.SetHeaders("Order Id", "Total Price", "Status", "Payment", "Date Ordered");

            foreach (var ord in table.Orders)
            {
                if (ord.UserId == uid)
                {
                    tableForm.AddRow(
                    $"{ord.OrderId}",
                    (ord.TotalOrderPrice).ToString() + " php",
                    ord.Status,
                    ord.PaymentMethod,
                    ord.Date.ToShortDateString());
                }
            }
            Console.WriteLine(tableForm.ToString());

            //Order details
            Console.Write("input >> ");
            string choice = Console.ReadLine();
            if (!int.TryParse(choice, out int parsed))
            {
                if (choice == "b")
                {
                    Console.Clear();
                    return;
                }
                else if (choice == "e")
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.Clear();
                    message = "Invalid choice. Please enter a valid value.";
                }
            }
            DisplayOrderDetails(parsed);
        }


    }
    public void DisplayProducts(int uid)
    {
        try
        {
            CombinedEntities table = product.ReadFromJsonFile();

            Console.WriteLine("FISH PRODUCTS AVAILABLE:");

            var tableForm = new Table();
            tableForm.SetHeaders("Choice", "Fish Product", "Stocks", "Price/Kg");

            foreach (var prod in table.Products)
            {
                double sum = 0;
                foreach (var ord in ordered)
                {
                    if (prod.ProductId == ord.ProductId)
                    {
                        sum += ord.OrderedQty;
                    }
                }
                tableForm.AddRow($"{prod.ProductId}", prod.ProductName, $"{(prod.ProductStock - sum).ToString()} kg", $"{(prod.PricePerKg).ToString()} php");
            }
            Console.WriteLine(tableForm.ToString());
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: File not found - {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O Error: {ex.Message}");
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine($"Null Reference Error: {ex.Message}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Format Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }
    public void DisplayOrderCart(int uid)
    {
        CombinedEntities table = product.ReadFromJsonFile();

        Console.WriteLine("PLACED ORDERS: ");
        var tableForm = new Table();
        tableForm.SetHeaders("Fish Product", "Qty (kg)", "Total Price");

        double grandTotal = 0d;
        foreach (var ord in ordered)
        {
            grandTotal += ord.OrderedPriceTotal;
            tableForm.AddRow(
                table.Products.Find(p => p.ProductId == ord.ProductId).ProductName,
                (ord.OrderedQty).ToString(),
                (Math.Round(ord.OrderedPriceTotal, 2)).ToString() + " php");
        }
        grandTotal = Math.Round(grandTotal, 2);
        Console.WriteLine(tableForm.ToString());
        Console.WriteLine("Grand Total: " + grandTotal + " php");
    }
    public void CheckOutOrder(int uid)
    {
        CombinedEntities table = product.ReadFromJsonFile();
        string paymethod = "COD", deliveryAddress = "";
        double total = 0d, grandTotal = 0d, SalesTax = 0;
        foreach (var ord in ordered)
        {
            total += ord.OrderedPriceTotal;
        }
        total = Math.Round(total, 2);
        double taxRate = 0.0725d;
        SalesTax = Math.Round(total * taxRate, 2);
        grandTotal = total + SalesTax;
        string message = "";
        while (true)
        {
            Console.Clear();

            Console.WriteLine("Input \"b\" to Return | \"e\" to exit app");
            Console.WriteLine(">> " + message);
            Console.WriteLine();
            Console.WriteLine("PLACED ORDERS: ");
            var tableForm = new Table();
            tableForm.SetHeaders("Fish Product", "Qty (kg)", "Total Price");
            foreach (var ord in ordered)
            {
                tableForm.AddRow(
                    table.Products.Find(p => p.ProductId == ord.ProductId).ProductName,
                    (ord.OrderedQty).ToString(),
                    (Math.Round(ord.OrderedPriceTotal, 2)).ToString() + " php");
            }
            Console.WriteLine(tableForm.ToString());
            Console.WriteLine("Subtotal: " + total + " php");


            var tableForm2 = new Table();
            tableForm2.SetHeaders("", "Cost");
            tableForm2.AddRow("Subtotal", "₱" + total.ToString());
            tableForm2.AddRow("Tax", "₱" + SalesTax.ToString());
            tableForm2.AddRow("TOTAL", "₱" + grandTotal.ToString());
            Console.WriteLine(tableForm2.ToString());

            Console.WriteLine("Details");
            Console.WriteLine("─────────────────────────────");
            Console.Write("Delivery Option (Pick up: 1 | Delivery: 2)  >> ");
            string deliveryOption = Console.ReadLine();
            if (!int.TryParse(deliveryOption, out int doParsed))
            {
                if (deliveryOption == "b")
                {
                    return;
                }
                if (deliveryOption == "e")
                {
                    Environment.Exit(0);
                }
                message = "Invalid Input Delivery choice must be 1 or 2";
                continue;
            }
            if (doParsed != 1 && doParsed != 2)
            {
                message = "Invalid Input Delivery choice must be 1 or 2.";
                continue;
            }

            int parsed = 0;
            if (doParsed == 2)
            {
                Console.Write("Payment Method (Online:1 | COD:2)  >> ");
                string paymentMethod = Console.ReadLine();
                if (!int.TryParse(paymentMethod, out parsed))
                {
                    if (paymentMethod == "b")
                    {
                        return;
                    }
                    if (paymentMethod == "e")
                    {
                        Environment.Exit(0);
                    }
                    message = "Invalid Input payment choice must be 1 or 2";
                    continue;
                }
                if (parsed != 1 && parsed != 2)
                {
                    message = "Invalid Input payment choice must be 1 or 2";
                    continue;
                }
                Console.Write("Delivery Address  >> ");
                deliveryAddress = Console.ReadLine();
            }
            else if (doParsed == 1)
            {

            }

            Console.WriteLine();
            Console.WriteLine("Please confirm the details above----");
            Console.Write("Do you want to Check out the order? (y or n)  >> ");
            string choice = Console.ReadLine();

            if (choice == "y")
            {
                paymethod = "COD";
                if (parsed == 1)
                {
                    paymethod = "Online";
                }
                break;
            }
            else if (choice == "n")
            {
                continue;
            }
            else if (choice == "b")
            {
                return;
            }
            else
            {
                message = "Invalid Input.";
                continue;
            }
        }

        //if break will add the orders
        double sum = 0d;
        
        foreach (var ord in ordered)
        {
            sum += ord.OrderedPriceTotal;
            double newStockQty = table.Products.Find(p => p.ProductId == ord.ProductId).ProductStock -= ord.OrderedQty;
            product.ModifyProduct(
                ord.ProductId,
                table.Products.Find(p => p.ProductId == ord.ProductId).ProductName,
                newStockQty,
                table.Products.Find(p => p.ProductId == ord.ProductId).PricePerKg);
        }
        orderings.AddOrder(uid, ordered, Math.Round(grandTotal, 2), paymethod, deliveryAddress);


        var tableForm4 = new Table2();
        var tableForm5 = new Table2();
        var tableForm3 = new Table2();
        tableForm3.SetHeaders("Fish Market de Elena", "- Official Receipt");
        tableForm3.AddRow("--------------", "-------------");
        foreach (var ord in ordered)
        {
            tableForm4.AddRow(
                table.Products.Find(p => p.ProductId == ord.ProductId).ProductName,
                (ord.OrderedQty).ToString(),
                (Math.Round(ord.OrderedPriceTotal, 2)).ToString() + " php");
        }

        tableForm5.SetHeaders("", "Cost");
        tableForm5.AddRow("Subtotal", "₱" + total.ToString());
        tableForm5.AddRow("Tax", "₱" + SalesTax.ToString());
        tableForm5.AddRow("TOTAL", "₱" + grandTotal.ToString());

        string orderid = "";
        if (table.Orders.Count == 0)
        {
            orderid = "1";
        }
        else
        {
            orderid = (table.Orders.Last().OrderId + 1).ToString();
        }
        string filename = $"Order{orderid}-{(orders[0].Date).ToString("yyyyMMdd")}.txt";
        SaveDataToFile(filename, $"{tableForm3.ToString()}\n{tableForm4.ToString()}\n{tableForm5.ToString()}");


        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("┌──────────────────────────────────────────────┐");
        Console.WriteLine("│               ORDER SUCCESSFUL!              │");
        Console.WriteLine("│                                              │");
        Console.WriteLine("│              FishMarket de Elena             │");
        Console.WriteLine("│            Thank You For Ordering!           │");
        Console.WriteLine("│                                              │");
        Console.WriteLine("│         Your receipt has been printed!       │");
        Console.WriteLine("│              - - - - - - - - - -             │");
        Console.WriteLine("└──────────────────────────────────────────────┘");
        Console.WriteLine("  ORDER NUMBER: " + orderid + "\n");
        Console.WriteLine("\n Press enter to continue..");
        Console.Read();
        //success = true;
        orders.Clear();
        ordered.Clear();
    }
    static void SaveDataToFile(string filePath, string data)
    {
        try
        {
            File.WriteAllText(filePath, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    public void PlaceOrder(string username)
    {
        //get username Id

        string message = "";
        int x = 0;
        while (x == 0)
        {
            CombinedEntities table = product.ReadFromJsonFile();
            int currentUID = table.Users.Find(u => u.UserName == username).Id;

            Console.Clear();
            Console.Out.Flush();
            Console.WriteLine("| input \"ok\" to Confirm Order | \"m\" to Modify Order | \"d\" to Delete Product |");
            Console.WriteLine("| \"cancel\" to Cancel Order | \"b\" to Return| \"exit\" to exit app |");
            Console.WriteLine("user: " + username);
            Console.WriteLine(">> " + message);
            Console.WriteLine();
            DisplayOrderCart(currentUID);
            Console.WriteLine();
            DisplayProducts(currentUID);

            Console.Write("Input >> ");
            string choice = Console.ReadLine();

            if (!int.TryParse(choice, out int parsedChoice))
            {
                if (choice == "ok")
                {
                    if (orders.Count > 0)
                    {
                        foreach (var prod in table.Products) // check if exceeds the total stocks
                        {
                            double sum = 0d;
                            foreach (var ord in ordered)
                            {
                                if (ord.ProductId == prod.ProductId) { sum += ord.OrderedQty; }
                            }
                            if (sum > prod.ProductStock)
                            {
                                message = "Cannot proceed, Total Quantity Exceeds the Stock limit.";
                                continue;
                            }
                        }
                        CheckOutOrder(currentUID);
                        //if (success)
                        //{
                        //    message = "- - - - - - - - - - - - - - - - - - - - - - -\n" +
                        //              ">> - - - - - - - ORDER SUCCESSFUL! - - - - - - -\n" +
                        //              ">> - - - - - - - - - - - - - - - - - - - - - - -\n";
                        //}
                    }
                }
                else if (choice == "cancel")
                {
                    orders.Clear();
                    ordered.Clear();
                }
                else if (choice == "b")
                {
                    x = 1;
                }
                else if (choice == "m")
                {
                    EditOrder();
                }
                else if (choice == "d")
                {
                    DeleteProduct();
                }
                else if (choice == "e")
                {
                    Environment.Exit(0);
                }
                else
                {
                    message = "Invalid input. Please enter a valid number.";
                    continue;
                }
            }
            void EditOrder()
            {
                CombinedEntities table = product.ReadFromJsonFile();
                Console.WriteLine("Enter the product ID to edit the quantity:");
                Console.Write("Input >> ");
                string editChoice = Console.ReadLine();

                if (int.TryParse(editChoice, out int editProductId))
                {
                    var existingOrderedProduct = table.Products.Find(op => op.ProductId == editProductId);
                    if (existingOrderedProduct != null)
                    {
                        Console.Write("Enter the new Quantity of " + existingOrderedProduct.ProductName + ": ");
                        string newQty = Console.ReadLine();

                        if (double.TryParse(newQty, out double newQuantity) && newQuantity > 0)
                        {
                            // Update the quantity of the existing ordered product
                            //existingOrderedProduct.OrderedQty = newQuantity;
                            message = "Order quantity updated successfully.";
                        }
                        else
                        {
                            message = "Invalid input. Please enter a valid quantity.";
                        }
                    }
                    else
                    {
                        message = "Product not found in the order.";
                    }
                }
                else
                {
                    message = "Invalid input. Please enter a valid product ID.";
                }
            }

            void DeleteProduct()
            {
                Console.WriteLine("Enter the product ID to delete:");
                Console.Write("Input >> ");
                string deleteChoice = Console.ReadLine();

                if (int.TryParse(deleteChoice, out int deleteProductId))
                {
                    var existingOrderedProduct = ordered.Find(op => op.ProductId == deleteProductId);
                    if (existingOrderedProduct != null)
                    {
                        // Remove the product from the order
                        ordered.Remove(existingOrderedProduct);
                        message = "Product removed from the order.";
                    }
                    else
                    {
                        message = "Product not found in the order.";
                    }
                }
                else
                {
                    message = "Invalid input. Please enter a valid product ID.";
                }
            }

            var selectedProduct = table.Products.Find(p => p.ProductId == parsedChoice);
            if (selectedProduct != null)
            {
                Console.Write("Enter the Quantity of " + table.Products.Find(p => p.ProductId == selectedProduct.ProductId).ProductName + ": ");
                string qty = Console.ReadLine();
                if (!double.TryParse(qty, out double quantity))
                {
                    message = "Invalid input. Please enter a valid Value.";
                    continue;
                }
                if (quantity <= 0)
                {
                    message = "Invalid input. Please enter a valid quantity.";
                    continue;
                }
                //(sum all current orders + new order) - current stock if feasible.
                double sum = 0;
                foreach (var ord in ordered)
                {
                    if (ord.ProductId == selectedProduct.ProductId)
                    {
                        sum += ord.OrderedQty;
                    }
                }
                sum += quantity;
                if (sum > selectedProduct.ProductStock)
                {
                    message = "Cannot Place Order, Quantity Exceeds the Maximum stocks.";
                    continue;
                }
                else
                {
                    if (orders.Count == 0)
                    {
                        orders.Add(new Order
                        {
                            OrderId = 1,
                            UserId = currentUID,
                            Ordered = ordered,
                            TotalOrderPrice = Math.Round(double.Parse(qty) * selectedProduct.PricePerKg, 2),
                            Status = "Pending"
                        });
                    }
                    ordered.Add(new OrderedProduct
                    {
                        ProductId = selectedProduct.ProductId,
                        OrderedQty = quantity,
                        OrderedPricePerKg = table.Products.Find(p => p.ProductId == selectedProduct.ProductId).PricePerKg,
                        OrderedPriceTotal = quantity * table.Products.Find(p => p.ProductId == selectedProduct.ProductId).PricePerKg,
                    });
                }
            }
        }
    }
}
public class AdminController
{
    filePath _filePath = new filePath();
    IProduct product = new ProductManagement();
    IOrders orderings = new OrderManagement();

    public void run()
    {
        string message = "Type the number of your choice..";
        while (true)
        {
            Console.Clear();
            Console.WriteLine("┌──────────────────────────────────────────────┐");
            Console.WriteLine("│        FishMarket de Elena : ADMIN PAGE      │");
            Console.WriteLine("└──────────────────────────────────────────────┘");
            Console.WriteLine(">> " + message);
            Console.WriteLine();
            Console.WriteLine("1. Manage Orders");
            Console.WriteLine("2. Manage Products");
            Console.WriteLine("3. Manage Users");
            Console.WriteLine("4. Sales Report");
            Console.WriteLine("5. Log out");
            string choice = Console.ReadLine();

            if (!int.TryParse(choice, out int parsedChoice))
            {
                message = "Invalid input. Please enter a valid number.";
                continue;
            }
            switch (parsedChoice)
            {
                case 1:
                    ManageOrders();
                    break;
                case 2:
                    ManageProducts();
                    break;
                case 3:
                    ManageUsers();
                    break;
                case 4:
                    GenerateSalesReport();
                    break;
                case 5:
                    return;
                default:
                    message = "Invalid input. Please enter a valid number.";
                    break;
            }
        }
    }
    public void DisplayOrders()
    {
        try
        {
            CombinedEntities table = product.ReadFromJsonFile();
            Console.WriteLine("FISH PRODUCTS:");
            var tableForm = new Table();
            tableForm.SetHeaders("Order Id", "Username", "Qty", "Total Price", "Status", "Pay Method", "Delivery Address", "Date Ordered");
            foreach (var ord in table.Orders)
            {
                tableForm.AddRow(
                    $"{ord.OrderId}",
                    table.Users.Find(u => u.Id == ord.UserId).UserName,
                    (ord.Ordered.Count()).ToString(),
                    "₱ " + (ord.TotalOrderPrice).ToString(),
                    ord.Status,
                    ord.PaymentMethod,
                    ord.DeliveryAddress,
                    ord.Date.ToShortDateString());
            }
            Console.WriteLine(tableForm.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error displaying orders: {ex.Message}");
        }
    }
    public void OrderStatus(int ordernum)
    {
        while (true)
        {
            CombinedEntities table = product.ReadFromJsonFile();

            Console.Clear();
            Console.WriteLine("Order Details:");
            DisplayProducts(ordernum);
            Console.WriteLine();
            Console.WriteLine("Pick the new status for Order number: " + ordernum);
            Console.WriteLine("───────────────────────────");
            Console.WriteLine("Current Order status: " + table.Orders.Find(o=>o.OrderId==ordernum).Status + "\n");
            Console.WriteLine("a. Pending");
            Console.WriteLine("b. On hold");
            Console.WriteLine("c. Returned to Sender");
            Console.WriteLine("d. Out for Delivery");
            Console.WriteLine("e. Delivered/Received");
            Console.Write("\nInput  >> ");
            string choice = Console.ReadLine();

            if (choice == "a")
            {
                orderings.ChangeOrderOrder(ordernum, "Pending");
                return;
            }
            else if (choice == "b")
            {
                orderings.ChangeOrderOrder(ordernum, "On hold");
                return;
            }
            else if (choice == "c")
            {
                orderings.ChangeOrderOrder(ordernum, "Returned to Sender");
                return;
            }
            else if (choice == "d")
            {
                orderings.ChangeOrderOrder(ordernum, "Out for Delivery");
                return;
            }
            else if (choice == "e")
            {
                orderings.ChangeOrderOrder(ordernum, "Delivered/Received");
                return;
            }

        }
    }
    public void DisplayProducts([Optional]int? orderid)
    {
        CombinedEntities table = product.ReadFromJsonFile();

        if (orderid== null)
        {
            Console.WriteLine("FISH PRODUCTS:");
            var tableForm = new Table();
            tableForm.SetHeaders("Choice", "Fish Product", "Stocks", "Price/Kg");
            foreach (var prod in table.Products)
            {
                tableForm.AddRow($"{prod.ProductId}", prod.ProductName, $"{(prod.ProductStock).ToString()} kg", $"₱ {(prod.PricePerKg).ToString()}");
            }
            Console.WriteLine(tableForm.ToString());
        }
        else
        {
            var tableForm = new Table();
            tableForm.SetHeaders("Fish Product", "Order Qty", "Price/Kg", "Total Price");
            foreach (var prod in table.Orders.Find(o => o.OrderId == orderid).Ordered)
            {
                tableForm.AddRow(
                    table.Products.Find(p=>p.ProductId==prod.ProductId).ProductName, 
                    $"{prod.OrderedQty.ToString()} kg", $"₱ {prod.OrderedPricePerKg.ToString()}", 
                    $"₱ {prod.OrderedPriceTotal.ToString()}");
            }
            Console.WriteLine(tableForm.ToString());
        }
    }
    public void DisplayUsers()
    {
        try
        {
            CombinedEntities table = product.ReadFromJsonFile();
            Console.WriteLine("Users:");
            var tableForm = new Table();
            tableForm.SetHeaders("User Id", "Username", "Order Count", "Total Spent", "Last Purchase Date");

            // Check if table and Orders are not null
            if (table != null && table.Orders != null)
            {
                foreach (var user in table.Users)
                {
                    if (user.Id == 1) { continue; }
                    int count = 0;
                    double totalSpent = 0d;

                    // Check if Orders is not null
                    if (table.Orders != null)
                    {
                        foreach (var ord in table.Orders)
                        {
                            if (ord.UserId == user.Id)
                            {
                                count++;
                                totalSpent += ord.TotalOrderPrice;
                            }
                        }
                    }

                    // Check if there are any orders for the user before accessing the Last Purchase Date
                    string lastPurchaseDate = "N/A";
                    var userOrders = table.Orders.FindAll(o => o.UserId == user.Id);
                    if (userOrders.Count > 0)
                    {
                        lastPurchaseDate = userOrders.Max(o => o.Date).ToString();
                    }

                    tableForm.AddRow(
                        user.Id.ToString(),
                        user.UserName,
                        count.ToString(),
                        totalSpent.ToString(),
                        lastPurchaseDate);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error displaying users: {ex.Message}");
        }
    }
    public void ManageUsers()
    {
        try
        {
            CombinedEntities table = product.ReadFromJsonFile();

            Console.Clear();
            Console.WriteLine("\x1b[3J");

            Console.WriteLine("┌──────────────────────────────────────────────┐");
            Console.WriteLine("│             ADMIN/Manage Users               │");
            Console.WriteLine("└──────────────────────────────────────────────┘");
            Console.WriteLine();

            DisplayUsers(table);

            Console.WriteLine("\nPress enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error managing users: {ex.Message}");
        }
    }

 
    private void DisplayUsers(CombinedEntities table)
    {
        var userTable = new Table();
        userTable.SetHeaders("User ID", "Username", "Other Details");

        foreach (var user in table.Users)
        {
            userTable.AddRow(user.Id.ToString(), user.UserName, "Other user details");
        }

        Console.WriteLine(userTable.ToString());
    }
    public void ManageOrders()
    {
        try
        {
            CombinedEntities table = product.ReadFromJsonFile();

            string message = "Type 'b' to Return";
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\x1b[3J");

                Console.WriteLine("Input 'b' to Return");
                Console.WriteLine("");
                Console.WriteLine("┌──────────────────────────────────────────────┐");
                Console.WriteLine("│             ADMIN/Manage Orders              │");
                Console.WriteLine("└──────────────────────────────────────────────┘");
                Console.WriteLine(">> " + message);
                Console.WriteLine();

                Console.WriteLine("Enter the first letter of the status to search for (leave blank for all orders): ");
                string statusFirstLetter = Console.ReadLine();

                Console.WriteLine("Enter the first letter of the payment method to search for (leave blank for all orders): ");
                string paymentMethodFirstLetter = Console.ReadLine();

                DisplayOrdersByStatusAndPaymentFirstLetter(statusFirstLetter, paymentMethodFirstLetter);

                Console.Write("Input >> ");
                string choice = Console.ReadLine();

                if (choice == "b")
                {
                    return;
                }
                else if (int.TryParse(choice, out int parsedChoice))
                {
                    var order = table.Orders.Find(o => o.OrderId == parsedChoice);
                    if (order != null)
                    {
                        OrderStatus(order.OrderId);
                    }
                    else
                    {
                        message = "Invalid input. Order Id not found.";
                        continue;
                    }
                }
                else
                {
                    message = "Invalid input. Please enter a valid option.";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error managing orders: {ex.Message}");
        }
    }

    private void DisplayOrdersByStatusAndPaymentFirstLetter(string statusFirstLetter, string paymentMethodFirstLetter)
    {
        try
        {
            CombinedEntities table = product.ReadFromJsonFile();
            var filteredOrders = table.Orders;

            if (!string.IsNullOrWhiteSpace(statusFirstLetter))
            {
                filteredOrders = filteredOrders.Where(o => o.Status.StartsWith(statusFirstLetter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(paymentMethodFirstLetter))
            {
                filteredOrders = filteredOrders.Where(o => o.PaymentMethod.StartsWith(paymentMethodFirstLetter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (filteredOrders.Count == 0)
            {
                Console.WriteLine("No orders matching the specified criteria found.");
            }
            else
            {
                Console.WriteLine("FISH PRODUCTS:");
                var tableForm = new Table();
                tableForm.SetHeaders("Order Id", "Username", "Qty", "Total Price", "Status", "Pay Method", "Delivery Address", "Date Ordered");

                foreach (var ord in filteredOrders)
                {
                    tableForm.AddRow(
                        $"{ord.OrderId}",
                        table.Users.Find(u => u.Id == ord.UserId).UserName,
                        (ord.Ordered.Count()).ToString(),
                        "₱ " + (ord.TotalOrderPrice).ToString(),
                        ord.Status,
                        ord.PaymentMethod,
                        ord.DeliveryAddress,
                        ord.Date.ToShortDateString());
                }

                Console.WriteLine(tableForm.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error displaying orders: {ex.Message}");
        }
    }

    public void ManageProducts()
    {
        string message = "Type the number of your choice..";

        while (true)
        {
            CombinedEntities table = product.ReadFromJsonFile();

            Console.Clear();
            Console.WriteLine("input \"a\" to Add New Product | \"d\" to Delete a Product | \"m\" to Modify | \"b\" to Return");
            Console.WriteLine("");
            Console.WriteLine("┌──────────────────────────────────────────────┐");
            Console.WriteLine("│             ADMIN/Manage Products            │");
            Console.WriteLine("└──────────────────────────────────────────────┘");
            Console.WriteLine(">> " + message);
            Console.WriteLine();

            //display products table
            DisplayProducts();

            Console.Write("Input >> ");
            string choice = Console.ReadLine();

            if (choice == "a")
            {
                Console.WriteLine("─────────────────────────────────────────────");
                Console.WriteLine("Add New Product");
                Console.Write("Fish: ");
                string fish = Console.ReadLine();
                Console.Write("Stock/Limit(kg): ");
                string stock = Console.ReadLine();
                Console.Write("Price per Kilo: ");
                string price = Console.ReadLine();
                if (double.TryParse(stock, out double parsedStock) && double.TryParse(price, out double parsedPrice) && fish != "")
                {
                    if (table.Products.Find(p => p.ProductName == fish) == null)
                    {
                        product.AddProduct(fish, parsedStock, parsedPrice);
                        continue;
                    }
                    else { message = "Fish Product Already exist!"; }
                }
                else { message = "Invalid input. Please enter a valid value."; }
            }
            else if (choice == "d")
            {
                Console.Write("Enter the product ID to delete: ");
                string deleteProductId = Console.ReadLine();

                if (int.TryParse(deleteProductId, out int productId))
                {
                    var existingProduct = table.Products.Find(p => p.ProductId == productId);
                    if (existingProduct != null)
                    {
                        // Remove the product from the list
                        table.Products.Remove(existingProduct);
                        message = "Product removed successfully.";

                        // Save changes to the file after deletion
                        product.WriteToJsonFile(_filePath.path, table);
                    }
                    else
                    {
                        message = "Product not found.";
                    }
                }
                else
                {
                    message = "Invalid input. Please enter a valid product ID.";
                }
            }

            else if (choice == "m")
            {
                Console.Write("Enter the product ID to modify: ");
                string modifyProductId = Console.ReadLine();

                if (int.TryParse(modifyProductId, out int productId))
                {
                    var existingProduct = table.Products.Find(p => p.ProductId == productId);
                    if (existingProduct != null)
                    {
                        Console.WriteLine("Choose what to modify:");
                        Console.WriteLine("1. Name");
                        Console.WriteLine("2. Stock");
                        Console.WriteLine("3. Price");
                        Console.Write("Enter your choice (1, 2, or 3): ");
                        string propertyChoice = Console.ReadLine();

                        if (int.TryParse(propertyChoice, out int choices))
                        {
                            switch (choices)
                            {
                                case 1:
                                    Console.Write("Enter the new name for the product: ");
                                    string newName = Console.ReadLine();
                                    existingProduct.ProductName = newName;
                                    message = "Product name modified successfully.";
                                    break;

                                case 2:
                                    Console.Write("Enter the new stock for the product: ");
                                    string newStock = Console.ReadLine();
                                    if (double.TryParse(newStock, out double parsedStock))
                                    {
                                        existingProduct.ProductStock = parsedStock;
                                        message = "Product stock modified successfully.";
                                    }
                                    else
                                    {
                                        message = "Invalid input. Please enter a valid stock value.";
                                    }
                                    break;

                                case 3:
                                    Console.Write("Enter the new price for the product: ");
                                    string newPrice = Console.ReadLine();
                                    if (double.TryParse(newPrice, out double parsedPrice))
                                    {
                                        existingProduct.PricePerKg = parsedPrice;
                                        message = "Product price modified successfully.";
                                    }
                                    else
                                    {
                                        message = "Invalid input. Please enter a valid price value.";
                                    }
                                    break;

                                default:
                                    message = "Invalid choice. Please enter a valid option.";
                                    break;
                            }

                            // Save changes to the file after modification
                            product.WriteToJsonFile(_filePath.path, table);
                        }
                        else
                        {
                            message = "Invalid input. Please enter a valid option.";
                        }
                    }
                    else
                    {
                        message = "Product not found.";
                    }
                }
                else
                {
                    message = "Invalid input. Please enter a valid product ID.";
                }
            }

            else if (choice == "b")
            {
                break;
            }
            else
            {
            }
        }
    }
    public void GenerateSalesReport()
    {
        try
        {
            DateTime startDate, endDate;
            GetDateRange(out startDate, out endDate); // Get the date range based on user choice

            CombinedEntities table = product.ReadFromJsonFile();

            var filteredOrders = table.Orders
                .Where(order => order.Date >= startDate && order.Date <= endDate)
                .ToList();

            Console.Clear();
            Console.WriteLine($"Sales Report from {startDate.ToShortDateString()} to {endDate.ToShortDateString()}");
            Console.WriteLine("───────────────────────────────────────");

            var productSalesData = new Dictionary<int, (double QuantitySold, double TotalPrice)>();

            foreach (var order in filteredOrders)
            {
                foreach (var orderedProduct in order.Ordered)
                {
                    if (productSalesData.ContainsKey(orderedProduct.ProductId))
                    {
                        productSalesData[orderedProduct.ProductId] =
                            (productSalesData[orderedProduct.ProductId].QuantitySold + orderedProduct.OrderedQty,
                             productSalesData[orderedProduct.ProductId].TotalPrice + orderedProduct.OrderedPriceTotal);
                    }
                    else
                    {
                        productSalesData.Add(orderedProduct.ProductId,
                            (orderedProduct.OrderedQty, orderedProduct.OrderedPriceTotal));
                    }
                }
            }

            // Create and display a table for product sales data
            var tableForm = new Table();
            tableForm.SetHeaders("Product", "Quantity Sold", "Total Price");

            foreach (var productId in productSalesData.Keys)
            {
                string productName = table.Products.Find(p => p.ProductId == productId)?.ProductName ?? "Unknown Product";
                var salesData = productSalesData[productId];
                tableForm.AddRow(productName, salesData.QuantitySold.ToString(), $"₱{salesData.TotalPrice:F2}");
            }

            Console.WriteLine(tableForm.ToString());

            // Display total sales
            double totalSales = productSalesData.Sum(p => p.Value.TotalPrice);
            Console.WriteLine($"Total Sales: ₱{totalSales:F2}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating sales report: {ex.Message}");
        }
    }


    private void GetDateRange(out DateTime startDate, out DateTime endDate)
    {
        Console.WriteLine("Date Range Options:");
        Console.WriteLine("1. Daily");
        Console.WriteLine("2. Weekly");
        Console.WriteLine("3. Monthly");
        Console.WriteLine("4. Yearly");
        Console.WriteLine("5. Specific Date Range");
        Console.WriteLine("6. All Time");

        Console.Write("Select an option (1-6): ");
        string option = Console.ReadLine();

        switch (option)
        {
            case "1": // Daily
                startDate = DateTime.Now.Date;
                endDate = startDate;
                break;

            case "2": // Weekly
                startDate = DateTime.Now.Date.AddDays(-7);
                endDate = DateTime.Now.Date;
                break;

            case "3": // Monthly
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                break;

            case "4": // Yearly
                startDate = new DateTime(DateTime.Now.Year, 1, 1);
                endDate = new DateTime(DateTime.Now.Year, 12, 31);
                break;

            case "5": // Specific Date Range
                GetSpecificDateRange(out startDate, out endDate);
                break;

            case "6": // All Time (Default)
            default:
                startDate = DateTime.MinValue;
                endDate = DateTime.MaxValue;
                break;
        }
    }
    private void GetSpecificDateRange(out DateTime startDate, out DateTime endDate)
    {
        startDate = DateTime.Now.Date;
        endDate = DateTime.Now.Date; // Default to today's date, in case of invalid input

        Console.Write("Enter start date (yyyy-MM-dd): ");
        string startInput = Console.ReadLine();
        if (!DateTime.TryParse(startInput, out startDate))
        {
            Console.WriteLine("Invalid start date. Using today's date.");
            startDate = DateTime.Now.Date;
        }

        Console.Write("Enter end date (yyyy-MM-dd): ");
        string endInput = Console.ReadLine();
        if (!DateTime.TryParse(endInput, out endDate) || endDate < startDate)
        {
            Console.WriteLine("Invalid end date. Using start date.");
            endDate = startDate;
        }
    }
    private double CalculateTotalSales(List<Order> orders)
    {
        double totalSales = orders.Sum(order => order.TotalOrderPrice);
        return totalSales;
    }


}
public class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        while (true)
        {
            signup signup = new signup();
            string username = signup.start();

            FishMarket fishMarket = new FishMarket();
            AdminController adminController = new AdminController();

            if (username == "Admin")
            {
                adminController.run();
            }
            else
            {
                fishMarket.Tasks(username);
            }
        }
    }
}
class CombinedEntities
{
    public List<UserAccounts> Users { get; set; }
    public List<Product> Products { get; set; }
    public List<Order> Orders { get; set; }
}
class UserAccounts
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public double ProductStock { get; set; }
    public double PricePerKg { get; set; }
}
class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public List<OrderedProduct>? Ordered { get; set; }
    public double TotalOrderPrice { get; set; }
    public string Status { get; set; }
    public string PaymentMethod { get; set; } = "COD";
    public string? DeliveryAddress { get; set; }

    public DateTime Date { get; set; }

    public Order()
    {
        Date = DateTime.Now;
    }
}
class OrderedProduct
{
    public int ProductId { get; set; }
    public double OrderedQty { get; set; }
    public double OrderedPricePerKg { get; set; }
    public double OrderedPriceTotal { get; set; }
}