using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Repositories
{
    public class InterviewSessionRepository
    {
        private readonly string _connectionString;

        public InterviewSessionRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task<InterviewSession> GetInterviewSessionByIdAsync(int id)
        {
            string query = @"
                SELECT id, position_id, external_user_id, interviewer_id, date_start, video, status, score 
                FROM InterviewSessions 
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapInterviewSession(reader);
                    }
                }
            }
            throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");
        }

        public InterviewSession GetInterviewSessionById(int id)
        {
            string query = @"
                SELECT id, position_id, external_user_id, interviewer_id, date_start, video, status, score 
                FROM InterviewSessions 
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapInterviewSession(reader);
                    }
                }
            }
            throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");
        }

        public async Task UpdateInterviewSessionAsync(InterviewSession updated)
        {
            string query = @"
                UPDATE InterviewSessions 
                SET interviewer_id = @interviewer_id, 
                    position_id = @position_id, 
                    external_user_id = @external_user_id, 
                    status = @status, 
                    date_start = @date_start, 
                    video = @video, 
                    score = @score 
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", updated.Id);
                command.Parameters.AddWithValue("@interviewer_id", updated.InterviewerId);
                command.Parameters.AddWithValue("@position_id", updated.PositionId);
                command.Parameters.AddWithValue("@external_user_id", (object)updated.ExternalUserId ?? DBNull.Value);
                command.Parameters.AddWithValue("@status", updated.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@date_start", updated.DateStart);
                command.Parameters.AddWithValue("@video", updated.Video ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@score", updated.Score);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public void Add(InterviewSession session)
        {
            string query = @"
                INSERT INTO InterviewSessions (position_id, external_user_id, interviewer_id, date_start, video, status, score)
                VALUES (@position_id, @external_user_id, @interviewer_id, @date_start, @video, @status, @score)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@position_id", session.PositionId);
                command.Parameters.AddWithValue("@external_user_id", (object)session.ExternalUserId ?? DBNull.Value);
                command.Parameters.AddWithValue("@interviewer_id", session.InterviewerId);
                command.Parameters.AddWithValue("@date_start", session.DateStart);
                command.Parameters.AddWithValue("@video", session.Video ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@status", session.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@score", session.Score);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(InterviewSession session)
        {
            string query = @"
                DELETE FROM InterviewSessions
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", session.Id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private InterviewSession MapInterviewSession(SqlDataReader reader)
        {
            return new InterviewSession
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                PositionId = reader.GetInt32(reader.GetOrdinal("position_id")),
                ExternalUserId = reader.IsDBNull(reader.GetOrdinal("external_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("external_user_id")),
                InterviewerId = reader.GetInt32(reader.GetOrdinal("interviewer_id")),
                DateStart = reader.GetDateTime(reader.GetOrdinal("date_start")),
                Video = reader.IsDBNull(reader.GetOrdinal("video")) ? null : reader.GetString(reader.GetOrdinal("video")),
                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                Score = reader.GetDecimal(reader.GetOrdinal("score"))
            };
        }

        public async Task<List<InterviewSession>> GetScheduledSessionsAsync()
        {
            var sessions = new List<InterviewSession>();
            string query = @"
                SELECT id, position_id, external_user_id, interviewer_id, date_start, video, status, score 
                FROM InterviewSessions 
                WHERE status = @status";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@status", InterviewStatus.Scheduled.ToString());

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sessions.Add(MapInterviewSession(reader));
                    }
                }
            }
            return sessions;
        }

        public async Task<List<InterviewSession>> GetSessionsByStatusAsync(string status)
        {
            var sessions = new List<InterviewSession>();
            string query = @"
                SELECT id, position_id, external_user_id, interviewer_id, date_start, video, status, score 
                FROM InterviewSessions 
                WHERE status = @status
                ORDER BY date_start DESC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@status", status);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        sessions.Add(MapInterviewSession(reader));
                    }
                }
            }
            return sessions;
        }
    }
}