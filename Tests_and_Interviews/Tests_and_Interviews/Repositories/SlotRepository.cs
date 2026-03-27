using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Repositories
{
    public class SlotRepository
    {
        private readonly string _connectionString;

        public SlotRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        // --- Asynchronous Methods ---

        public async Task<List<Slot>> GetSlotsAsync(int recruiterId, DateTime date)
        {
            var slots = new List<Slot>();
            string query = @"
                SELECT id, recruiter_id, start_time, end_time, duration, status, interview_type
                FROM Slots 
                WHERE recruiter_id = @recruiter_id AND CAST(start_time AS DATE) = CAST(@date AS DATE)
                ORDER BY start_time";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@recruiter_id", recruiterId);
                command.Parameters.AddWithValue("@date", date);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        slots.Add(MapSlot(reader));
                    }
                }
            }
            return slots;
        }

        public async Task<List<Slot>> GetAllSlotsAsync(int recruiterId)
        {
            var slots = new List<Slot>();
            string query = @"
                SELECT id, recruiter_id, start_time, end_time, duration, status, interview_type
                FROM Slots 
                WHERE recruiter_id = @recruiter_id
                ORDER BY start_time";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@recruiter_id", recruiterId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        slots.Add(MapSlot(reader));
                    }
                }
            }
            return slots;
        }

        public async Task<Slot?> GetByIdAsync(int id)
        {
            string query = "SELECT id, recruiter_id, start_time, end_time FROM Slots WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapSlot(reader);
                    }
                }
            }
            return null;
        }

        public async Task AddAsync(Slot slot)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkOverlapQuery = @"
                    SELECT COUNT(1) 
                    FROM Slots 
                    WHERE recruiter_id = @recruiter_id 
                      AND CAST(start_time AS DATE) = CAST(@start_time AS DATE)
                      AND @start_time < end_time 
                      AND @end_time > start_time";

                using (var checkCommand = new SqlCommand(checkOverlapQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@recruiter_id", slot.RecruiterId);
                    checkCommand.Parameters.AddWithValue("@start_time", slot.StartTime);
                    checkCommand.Parameters.AddWithValue("@end_time", slot.EndTime);

                    int overlapCount = (int)await checkCommand.ExecuteScalarAsync();
                    if (overlapCount > 0)
                    {
                        throw new Exception("Slot overlaps with an existing appointment!");
                    }
                }

                string insertQuery = @"
                    INSERT INTO Slots (recruiter_id, start_time, end_time) 
                    OUTPUT INSERTED.id 
                    VALUES (@recruiter_id, @start_time, @end_time)";

                using (var insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@recruiter_id", slot.RecruiterId);
                    insertCommand.Parameters.AddWithValue("@start_time", slot.StartTime);
                    insertCommand.Parameters.AddWithValue("@end_time", slot.EndTime);

                    slot.Id = (int)await insertCommand.ExecuteScalarAsync();
                }
            }
        }

        public async Task UpdateAsync(Slot slot)
        {
            string query = @"
                UPDATE Slots 
                SET start_time = @start_time, end_time = @end_time 
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", slot.Id);
                command.Parameters.AddWithValue("@start_time", slot.StartTime);
                command.Parameters.AddWithValue("@end_time", slot.EndTime);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new Exception("Slot not found");
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            string query = "DELETE FROM Slots WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        // --- Synchronous Methods ---

        public List<Slot> GetSlots(int recruiterId, DateTime date)
        {
            var slots = new List<Slot>();
            string query = @"
                SELECT id, recruiter_id, start_time, end_time 
                FROM Slots 
                WHERE recruiter_id = @recruiter_id AND CAST(start_time AS DATE) = CAST(@date AS DATE)
                ORDER BY start_time";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@recruiter_id", recruiterId);
                command.Parameters.AddWithValue("@date", date);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        slots.Add(MapSlot(reader));
                    }
                }
            }
            return slots;
        }

        public List<Slot> GetAllSlots(int recruiterId)
        {
            var slots = new List<Slot>();
            string query = @"
                SELECT id, recruiter_id, start_time, end_time, duration, status, interview_type
                FROM Slots 
                WHERE recruiter_id = @recruiter_id
                ORDER BY start_time";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@recruiter_id", recruiterId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        slots.Add(MapSlot(reader));
                    }
                }
            }
            return slots;
        }

        public Slot? GetById(int id)
        {
            string query = "SELECT id, recruiter_id, start_time, end_time FROM Slots WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapSlot(reader);
                    }
                }
            }
            return null;
        }

        public void Add(Slot slot)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string checkOverlapQuery = @"
                    SELECT COUNT(1) 
                    FROM Slots 
                    WHERE recruiter_id = @recruiter_id 
                      AND CAST(start_time AS DATE) = CAST(@start_time AS DATE)
                      AND @start_time < end_time 
                      AND @end_time > start_time";

                using (var checkCommand = new SqlCommand(checkOverlapQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@recruiter_id", slot.RecruiterId);
                    checkCommand.Parameters.AddWithValue("@start_time", slot.StartTime);
                    checkCommand.Parameters.AddWithValue("@end_time", slot.EndTime);

                    int overlapCount = (int)checkCommand.ExecuteScalar();
                    if (overlapCount > 0)
                    {
                        throw new Exception("Slot overlaps with an existing appointment!");
                    }
                }

                string insertQuery = @"
                    INSERT INTO Slots (recruiter_id, start_time, end_time, status, duration, interview_type) 
                    OUTPUT INSERTED.id 
                    VALUES (@recruiter_id, @start_time, @end_time, @status, @duration, @interview_type)";

                using (var insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@recruiter_id", slot.RecruiterId);
                    insertCommand.Parameters.AddWithValue("@start_time", slot.StartTime);
                    insertCommand.Parameters.AddWithValue("@end_time", slot.EndTime);
                    insertCommand.Parameters.AddWithValue("@status", slot.Status);
                    insertCommand.Parameters.AddWithValue("@duration", slot.Duration);
                    insertCommand.Parameters.AddWithValue("@interview_type", slot.InterviewType);

                    slot.Id = (int)insertCommand.ExecuteScalar();
                }
            }
        }

        public void Update(Slot slot)
        {
            string query = @"
                UPDATE Slots 
                SET start_time = @start_time, end_time = @end_time, recruiter_id = @recruiter_id, duration = @duration, status = @status, interview_type = @interview_type
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", slot.Id);
                command.Parameters.AddWithValue("@start_time", slot.StartTime);
                command.Parameters.AddWithValue("@end_time", slot.EndTime);
                command.Parameters.AddWithValue("@recruiter_id", slot.RecruiterId);
                command.Parameters.AddWithValue("@duration", slot.Duration);
                command.Parameters.AddWithValue("@status", slot.Status);
                command.Parameters.AddWithValue("@interview_type", slot.InterviewType);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Slot not found");
                }
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM Slots WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        private Slot MapSlot(SqlDataReader reader)
        {
            return new Slot
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                RecruiterId = reader.GetInt32(reader.GetOrdinal("recruiter_id")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("start_time")),
                EndTime = reader.GetDateTime(reader.GetOrdinal("end_time")),
                Duration = reader.GetInt32(reader.GetOrdinal("duration")),
                Status = (SlotStatus)reader.GetInt32(reader.GetOrdinal("status")),
                InterviewType = reader.GetString(reader.GetOrdinal("interview_type"))
            };
        }
    }
}