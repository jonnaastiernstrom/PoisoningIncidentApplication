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

    }
}