using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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

            string query = @"
                SELECT 
                    r.company_id, 
                    r.name,
                    s.id AS slot_id, 
                    s.start_time
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
                        if (recruiter == null)
                        {
                            recruiter = new Recruiter
                            {
                                CompanyId = reader.GetInt32(reader.GetOrdinal("company_id")),
                                Slots = []
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("slot_id")))
                        {
                            recruiter.Slots.Add(new Slot
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("slot_id")),
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
                        });
                    }
                }
            }
            return slots;
        }

        public async Task SaveAsync(Recruiter recruiter)
        {
            string query = @"
                IF EXISTS (SELECT 1 FROM Recruiters WHERE company_id = @company_id)
                BEGIN
                    UPDATE Recruiters 
                    SET name = @name
                    WHERE company_id = @company_id
                END
                ELSE
                BEGIN
                    INSERT INTO Recruiters (company_id, name)
                    VALUES (@company_id, @name)
                END";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@company_id", recruiter.CompanyId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}