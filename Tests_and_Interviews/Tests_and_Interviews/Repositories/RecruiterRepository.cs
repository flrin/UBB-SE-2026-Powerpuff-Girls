using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Repositories
{
    public class RecruiterRepository
    {
        private readonly string _connectionString;

        public RecruiterRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task<Recruiter?> FindByIdAsync(int id)
        {
            Recruiter? recruiter = null;

            // We use a LEFT JOIN to fetch the recruiter and their calendar slots in one trip
            string query = @"
                SELECT 
                    r.company_id, 
                    r.name, -- Example placeholder: adjust to your actual model
                    s.id AS slot_id, 
                    s.start_time -- Example placeholder: adjust to your actual model
                FROM Recruiters r
                LEFT JOIN Slots s ON r.company_id = s.recruiter_id
                WHERE r.company_id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Initialize the Recruiter object on the first row
                        if (recruiter == null)
                        {
                            recruiter = new Recruiter
                            {
                                CompanyId = reader.GetInt32(reader.GetOrdinal("company_id")),
                                // Name = reader.GetString(reader.GetOrdinal("name")), // Adjust to actual properties
                                Slots = new List<Slot>()
                            };
                        }

                        // If the LEFT JOIN found an associated slot, map it and add it
                        if (!reader.IsDBNull(reader.GetOrdinal("slot_id")))
                        {
                            recruiter.Slots.Add(new Slot
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("slot_id")),
                                // StartTime = reader.GetDateTime(reader.GetOrdinal("start_time")) // Adjust to actual properties
                            });
                        }
                    }
                }
            }
            return recruiter;
        }

        public async Task<List<Slot>> GetCalendarAsync(int recruiterId)
        {
            var slots = new List<Slot>();
            string query = "SELECT * FROM Slots WHERE recruiter_id = @recruiter_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@recruiter_id", recruiterId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        slots.Add(new Slot
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            // Map your other Slot properties here
                        });
                    }
                }
            }
            return slots;
        }

        public async Task SaveAsync(Recruiter recruiter)
        {
            // In a real database, replacing an item in a list translates to an "Upsert" (Update or Insert)
            string query = @"
                IF EXISTS (SELECT 1 FROM Recruiters WHERE company_id = @company_id)
                BEGIN
                    UPDATE Recruiters 
                    SET name = @name -- Adjust to your actual properties
                    WHERE company_id = @company_id
                END
                ELSE
                BEGIN
                    INSERT INTO Recruiters (company_id, name) -- Adjust to your actual properties
                    VALUES (@company_id, @name)
                END";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@company_id", recruiter.CompanyId);

                // Add parameters for your other Recruiter properties here
                // command.Parameters.AddWithValue("@name", recruiter.Name ?? (object)DBNull.Value);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }

            // Note: If saving a recruiter also means overriding their entire calendar of slots, 
            // you will need to add a secondary SQL command here to DELETE existing slots and INSERT the new ones from recruiter.Slots.
        }
    }
}