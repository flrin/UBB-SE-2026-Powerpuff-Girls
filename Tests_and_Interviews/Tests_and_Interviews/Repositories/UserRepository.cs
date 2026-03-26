using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            string query = "SELECT id, name, email FROM Users WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapUser(reader);
                    }
                }
            }
            return null;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            string query = "SELECT id, name, email FROM Users";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }
            return users;
        }

        public async Task AddAsync(User user)
        {
            string query = @"
                INSERT INTO Users (name, email) 
                OUTPUT INSERTED.id 
                VALUES (@name, @email)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", user.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@email", user.Email ?? (object)DBNull.Value);

                await connection.OpenAsync();
                // Capture the newly generated Identity ID and assign it back to the model
                user.Id = (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task UpdateAsync(User user)
        {
            string query = @"
                UPDATE Users 
                SET name = @name, email = @email 
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@name", user.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@email", user.Email ?? (object)DBNull.Value);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            string query = "DELETE FROM Users WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        // --- Helper Mapping Method ---

        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email"))
            };
        }
    }
}