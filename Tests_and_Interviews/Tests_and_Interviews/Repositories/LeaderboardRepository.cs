using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Repositories
{
    public class LeaderboardRepository
    {
        private readonly string _connectionString;

        public LeaderboardRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task<List<LeaderboardEntry>> FindByTestIdAsync(int testId)
        {
            var entries = new List<LeaderboardEntry>();
            string query = @"
                SELECT 
                    le.id AS le_id, le.test_id, le.user_id, le.rank_position, le.normalized_score,
                    u.id AS u_id, u.name, u.email,
                    t.id AS t_id, t.title, t.category, t.created_at
                FROM LeaderboardEntries le
                INNER JOIN Users u ON le.user_id = u.id
                INNER JOIN Tests t ON le.test_id = t.id
                WHERE le.test_id = @test_id
                ORDER BY le.rank_position";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@test_id", testId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(MapLeaderboardEntry(reader));
                    }
                }
            }
            return entries;
        }

        public async Task<List<LeaderboardEntry>> FindTopByTestIdAsync(int testId, int limit)
        {
            var entries = new List<LeaderboardEntry>();
            string query = @"
                SELECT TOP (@limit)
                    le.id AS le_id, le.test_id, le.user_id, le.rank_position, le.normalized_score,
                    u.id AS u_id, u.name, u.email,
                    t.id AS t_id, t.title, t.category, t.created_at
                FROM LeaderboardEntries le
                INNER JOIN Users u ON le.user_id = u.id
                INNER JOIN Tests t ON le.test_id = t.id
                WHERE le.test_id = @test_id
                ORDER BY le.rank_position";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@test_id", testId);
                command.Parameters.AddWithValue("@limit", limit);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(MapLeaderboardEntry(reader));
                    }
                }
            }
            return entries;
        }

        public async Task<LeaderboardEntry?> FindUserEntryAsync(int userId, int testId)
        {
            string query = @"
                SELECT 
                    le.id AS le_id, le.test_id, le.user_id, le.rank_position, le.normalized_score,
                    u.id AS u_id, u.name, u.email,
                    t.id AS t_id, t.title, t.category, t.created_at
                FROM LeaderboardEntries le
                INNER JOIN Users u ON le.user_id = u.id
                INNER JOIN Tests t ON le.test_id = t.id
                WHERE le.user_id = @user_id AND le.test_id = @test_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@test_id", testId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapLeaderboardEntry(reader);
                    }
                }
            }
            return null;
        }

        public async Task DeleteByTestIdAsync(int testId)
        {
            string query = "DELETE FROM LeaderboardEntries WHERE test_id = @test_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@test_id", testId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task SaveRangeAsync(List<LeaderboardEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            string query = @"
                INSERT INTO LeaderboardEntries (test_id, user_id, normalized_score, rank_position, tie_break_priority, last_recalculation_at) 
                VALUES (@test_id, @user_id, @normalized_score, @rank_position, @tie_break_priority, @last_recalculation_at)";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                using (var command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@test_id", SqlDbType.Int);
                    command.Parameters.Add("@user_id", SqlDbType.Int);
                    command.Parameters.Add("@normalized_score", SqlDbType.Decimal);
                    command.Parameters.Add("@rank_position", SqlDbType.Int);
                    command.Parameters.Add("@tie_break_priority", SqlDbType.Int);
                    command.Parameters.Add("@last_recalculation_at", SqlDbType.DateTime);

                    try
                    {
                        foreach (var entry in entries)
                        {
                            command.Parameters["@test_id"].Value = entry.TestId;
                            command.Parameters["@user_id"].Value = entry.UserId;
                            command.Parameters["@normalized_score"].Value = entry.NormalizedScore;
                            command.Parameters["@rank_position"].Value = entry.RankPosition;
                            command.Parameters["@tie_break_priority"].Value = entry.TieBreakPriority;
                            command.Parameters["@last_recalculation_at"].Value = entry.LastRecalculationAt;

                            await command.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }


        private LeaderboardEntry MapLeaderboardEntry(SqlDataReader reader)
        {
            return new LeaderboardEntry
            {
                Id = reader.GetInt32(reader.GetOrdinal("le_id")),
                TestId = reader.GetInt32(reader.GetOrdinal("test_id")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                RankPosition = reader.GetInt32(reader.GetOrdinal("rank_position")),
                NormalizedScore = reader.GetDecimal(reader.GetOrdinal("normalized_score")),

                User = new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("u_id")),
                    Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email"))
                },

                Test = new Test
                {
                    Id = reader.GetInt32(reader.GetOrdinal("t_id")),
                    Title = reader.IsDBNull(reader.GetOrdinal("title")) ? null : reader.GetString(reader.GetOrdinal("title")),
                    Category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString(reader.GetOrdinal("category")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                }
            };
        }
    }
}