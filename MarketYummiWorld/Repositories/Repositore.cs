using MarketYummiWorld.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketYummiWorld.Repositories
{
    public interface IDeliveryRepository : IDisposable
    {
        Task InitializeDatabaseAsync();

        // Customers
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<int> AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int id);

        // Products
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<int> AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);

        // Orders (с транзакцией для OrderItems)
        Task<Order?> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<int> CreateOrderWithItemsAsync(Order order, IEnumerable<OrderItem> items);
        Task UpdateOrderStatusAsync(int orderId, string status);
    }

    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly string _connectionString;

        public DeliveryRepository(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Foreign Keys=True;";
        }

        // --- Инициализация БД ---
        public async Task InitializeDatabaseAsync()
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                CREATE TABLE IF NOT EXISTS Categories (category_id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS Customers (customer_id INTEGER PRIMARY KEY AUTOINCREMENT, full_name TEXT NOT NULL, phone TEXT UNIQUE NOT NULL, email TEXT UNIQUE, registration_date TEXT DEFAULT (DATE('now')));
                CREATE TABLE IF NOT EXISTS Addresses (address_id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER, address_text TEXT NOT NULL, FOREIGN KEY (customer_id) REFERENCES Customers(customer_id));
                CREATE TABLE IF NOT EXISTS Couriers (courier_id INTEGER PRIMARY KEY AUTOINCREMENT, full_name TEXT NOT NULL, phone TEXT NOT NULL, transport_type TEXT, working_zone TEXT, status TEXT CHECK(status IN ('свободен', 'занят', 'в рейсе')) DEFAULT 'свободен');
                CREATE TABLE IF NOT EXISTS Products (product_id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, description TEXT, price REAL NOT NULL, category_id INTEGER, unit TEXT, stock_quantity INTEGER DEFAULT 0, production_date TEXT, expiry_date TEXT, FOREIGN KEY (category_id) REFERENCES Categories(category_id));
                CREATE TABLE IF NOT EXISTS Orders (order_id INTEGER PRIMARY KEY AUTOINCREMENT, customer_id INTEGER, order_date TEXT DEFAULT (DATETIME('now')), total_amount REAL DEFAULT 0, payment_method TEXT, payment_status TEXT DEFAULT 'не оплачен', order_status TEXT DEFAULT 'новый', FOREIGN KEY (customer_id) REFERENCES Customers(customer_id));
                CREATE TABLE IF NOT EXISTS OrderItems (item_id INTEGER PRIMARY KEY AUTOINCREMENT, order_id INTEGER, product_id INTEGER, quantity INTEGER NOT NULL, price_at_purchase REAL NOT NULL, FOREIGN KEY (order_id) REFERENCES Orders(order_id), FOREIGN KEY (product_id) REFERENCES Products(product_id));
                CREATE TABLE IF NOT EXISTS Deliveries (delivery_id INTEGER PRIMARY KEY AUTOINCREMENT, order_id INTEGER UNIQUE, courier_id INTEGER, address_id INTEGER, planned_time TEXT, actual_time TEXT, delivery_status TEXT DEFAULT 'ожидает', FOREIGN KEY (order_id) REFERENCES Orders(order_id), FOREIGN KEY (courier_id) REFERENCES Couriers(courier_id), FOREIGN KEY (address_id) REFERENCES Addresses(address_id));
            ";
            await using var cmd = new SqliteCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        // --- Customers ---
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT customer_id, full_name, phone, email, registration_date FROM Customers WHERE customer_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapCustomer(reader);
            return null;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            var list = new List<Customer>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT customer_id, full_name, phone, email, registration_date FROM Customers", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapCustomer(reader));
            return list;
        }

        public async Task<int> AddCustomerAsync(Customer customer)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("INSERT INTO Customers (full_name, phone, email, registration_date) VALUES (@name, @phone, @email, @date); SELECT last_insert_rowid();", conn);
            cmd.Parameters.AddWithValue("@name", customer.FullName);
            cmd.Parameters.AddWithValue("@phone", customer.Phone);
            cmd.Parameters.AddWithValue("@email", customer.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@date", customer.RegistrationDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("UPDATE Customers SET full_name=@name, phone=@phone, email=@email WHERE customer_id=@id", conn);
            cmd.Parameters.AddWithValue("@id", customer.CustomerId);
            cmd.Parameters.AddWithValue("@name", customer.FullName);
            cmd.Parameters.AddWithValue("@phone", customer.Phone);
            cmd.Parameters.AddWithValue("@email", customer.Email ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("DELETE FROM Customers WHERE customer_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // --- Products ---
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT product_id, name, description, price, category_id, unit, stock_quantity, production_date, expiry_date FROM Products WHERE product_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapProduct(reader);
            return null;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var list = new List<Product>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT product_id, name, description, price, category_id, unit, stock_quantity, production_date, expiry_date FROM Products", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapProduct(reader));
            return list;
        }

        public async Task<int> AddProductAsync(Product product)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("INSERT INTO Products (name, description, price, category_id, unit, stock_quantity, production_date, expiry_date) VALUES (@name, @desc, @price, @cat, @unit, @stock, @prod, @exp); SELECT last_insert_rowid();", conn);
            cmd.Parameters.AddWithValue("@name", product.Name);
            cmd.Parameters.AddWithValue("@desc", product.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@price", product.Price);
            cmd.Parameters.AddWithValue("@cat", product.CategoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@unit", product.Unit ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@stock", product.StockQuantity);
            cmd.Parameters.AddWithValue("@prod", product.ProductionDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@exp", product.ExpiryDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task UpdateProductAsync(Product product)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("UPDATE Products SET name=@name, description=@desc, price=@price, category_id=@cat, unit=@unit, stock_quantity=@stock WHERE product_id=@id", conn);
            cmd.Parameters.AddWithValue("@id", product.ProductId);
            cmd.Parameters.AddWithValue("@name", product.Name);
            cmd.Parameters.AddWithValue("@desc", product.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@price", product.Price);
            cmd.Parameters.AddWithValue("@cat", product.CategoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@unit", product.Unit ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@stock", product.StockQuantity);
            await cmd.ExecuteNonQueryAsync();
        }

        // --- Orders & OrderItems (с транзакцией) ---
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT order_id, customer_id, order_date, total_amount, payment_method, payment_status, order_status FROM Orders WHERE order_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapOrder(reader);
            return null;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var list = new List<Order>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT order_id, customer_id, order_date, total_amount, payment_method, payment_status, order_status FROM Orders", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapOrder(reader));
            return list;
        }

        public async Task<int> CreateOrderWithItemsAsync(Order order, IEnumerable<OrderItem> items)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                await using var cmdOrder = new SqliteCommand("INSERT INTO Orders (customer_id, total_amount, payment_method, payment_status, order_status) VALUES (@cust, @total, @payMethod, @payStatus, @status); SELECT last_insert_rowid();", conn, transaction);
                cmdOrder.Parameters.AddWithValue("@cust", order.CustomerId ?? (object)DBNull.Value);
                cmdOrder.Parameters.AddWithValue("@total", order.TotalAmount);
                cmdOrder.Parameters.AddWithValue("@payMethod", order.PaymentMethod ?? (object)DBNull.Value);
                cmdOrder.Parameters.AddWithValue("@payStatus", order.PaymentStatus);
                cmdOrder.Parameters.AddWithValue("@status", order.OrderStatus);
                var orderId = Convert.ToInt32(await cmdOrder.ExecuteScalarAsync());

                await using var cmdItem = new SqliteCommand("INSERT INTO OrderItems (order_id, product_id, quantity, price_at_purchase) VALUES (@oid, @pid, @qty, @price)", conn, transaction);
                cmdItem.Parameters.Add("@oid", Microsoft.Data.Sqlite.SqliteType.Integer);
                cmdItem.Parameters.Add("@pid", Microsoft.Data.Sqlite.SqliteType.Integer);
                cmdItem.Parameters.Add("@qty", Microsoft.Data.Sqlite.SqliteType.Integer);
                cmdItem.Parameters.Add("@price", Microsoft.Data.Sqlite.SqliteType.Real);

                foreach (var item in items)
                {
                    cmdItem.Parameters["@oid"].Value = orderId;
                    cmdItem.Parameters["@pid"].Value = item.ProductId;
                    cmdItem.Parameters["@qty"].Value = item.Quantity;
                    cmdItem.Parameters["@price"].Value = item.PriceAtPurchase;
                    await cmdItem.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return orderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("UPDATE Orders SET order_status = @status WHERE order_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", orderId);
            cmd.Parameters.AddWithValue("@status", status);
            await cmd.ExecuteNonQueryAsync();
        }

        public void Dispose() { /* SqliteConnection не требует явного Dispose, если используется await using */ }

        // --- Маппинг ---
        private Customer MapCustomer(SqliteDataReader r) => new()
        {
            CustomerId = r.GetInt32(0),
            FullName = r.GetString(1),
            Phone = r.GetString(2),
            Email = r.IsDBNull(3) ? null : r.GetString(3),
            RegistrationDate = r.IsDBNull(4) ? null : DateTime.Parse(r.GetString(4))
        };

        private Product MapProduct(SqliteDataReader r) => new()
        {
            ProductId = r.GetInt32(0),
            Name = r.GetString(1),
            Description = r.IsDBNull(2) ? null : r.GetString(2),
            Price = r.GetDecimal(3),
            CategoryId = r.IsDBNull(4) ? null : r.GetInt32(4),
            Unit = r.IsDBNull(5) ? null : r.GetString(5),
            StockQuantity = r.GetInt32(6),
            ProductionDate = r.IsDBNull(7) ? null : DateTime.Parse(r.GetString(7)),
            ExpiryDate = r.IsDBNull(8) ? null : DateTime.Parse(r.GetString(8))
        };

        private Order MapOrder(SqliteDataReader r) => new()
        {
            OrderId = r.GetInt32(0),
            CustomerId = r.IsDBNull(1) ? null : r.GetInt32(1),
            OrderDate = r.IsDBNull(2) ? null : DateTime.Parse(r.GetString(2)),
            TotalAmount = r.GetDecimal(3),
            PaymentMethod = r.IsDBNull(4) ? null : r.GetString(4),
            PaymentStatus = r.GetString(5),
            OrderStatus = r.GetString(6)
        };
    }
}
