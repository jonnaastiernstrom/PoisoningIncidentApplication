using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PoisoningIncidentApplication
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        //public DatabaseService()
        //{
        //    // Use a secure method to store and retrieve your connection string.
        
        //    _connectionString = "Host=localhost;Port:5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";
        //    _connectionString = "Host=10.0.2.2:5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";

        //}
        public DatabaseService()
        {
            // Default connection string for Windows and iOS
            _connectionString = "Host=localhost;Port=5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";

            #if ANDROID
            // Connection string for Android emulator
            _connectionString = "Host=10.0.2.2;Port=5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";
            #endif
        }


        public async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

       public async Task<string> GetProductByNameAsync(string productName)
       {
            string product = null;
            
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new NpgsqlCommand("SELECT product_name FROM products WHERE product_name = @productName", connection))
                {
                    command.Parameters.AddWithValue("@productName", productName);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = reader.GetString(0);
                        }
                    }
                }
            }
            
            return product;
       }
       public async Task<string> GetProductDescriptionByNameAsync(string productName)
        {
            string description = null;

            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // The SQL query joins the products table with the danger_levels table to get the description
                await using (var command = new NpgsqlCommand("SELECT dl.description FROM products p INNER JOIN danger_levels dl ON p.danger_level = dl.level WHERE p.product_name = @productName", connection))
                {
                    command.Parameters.AddWithValue("@productName", productName);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Assuming description is the first column due to the SELECT statement only requesting the description
                            description = reader.GetString(0);
                            description = description.Replace("\\n", "\n");
                        }
                    }
                }
            }

            return description;
        }
        public async Task<List<string>> GetProductSuggestionsAsync(string searchTerm)
        {
            var products = new List<string>();

            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // Use ILIKE for case-insensitive matching and a wildcard to match any sequence of characters after the input
                await using (var command = new NpgsqlCommand("SELECT product_name FROM products WHERE product_name ILIKE @searchTerm", connection))
                {
                    command.Parameters.AddWithValue("@searchTerm", $"{searchTerm}%");  // Append a wildcard to the search term

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(reader.GetString(0));  // Add each matching product name to the list
                        }
                    }
                }
            }

            return products;
        }


    }
}