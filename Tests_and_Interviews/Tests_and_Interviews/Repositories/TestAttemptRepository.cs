using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Repositories
{
    public class TestAttemptRepository
    {
        private readonly string _connectionString;

        public TestAttemptRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task<TestAttempt?> FindByUserAndTestAsync(int userId, int testId)
        {
            TestAttempt? testAttempt = null;

            string query = @"
                SELECT 
                    ta.id AS ta_id, ta.test_id, ta.external_user_id, ta.score, ta.status, 
                    ta.started_at, ta.completed_at, ta.answers_file_path, ta.is_validated, 
                    ta.percentage_score, ta.rejection_reason, ta.rejected_at,
                    a.id AS a_id, a.attempt_id, a.question_id, a.value
                FROM TestAttempts ta
                LEFT JOIN Answers a ON ta.id = a.attempt_id
                WHERE ta.external_user_id = @user_id AND ta.test_id = @test_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@test_id", testId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (testAttempt == null)
                        {
                            testAttempt = new TestAttempt
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ta_id")),
                                TestId = reader.GetInt32(reader.GetOrdinal("test_id")),
                                ExternalUserId = reader.IsDBNull(reader.GetOrdinal("external_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("external_user_id")),
                                Score = reader.IsDBNull(reader.GetOrdinal("score")) ? null : reader.GetDecimal(reader.GetOrdinal("score")),
                                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                                StartedAt = reader.IsDBNull(reader.GetOrdinal("started_at")) ? null : reader.GetDateTime(reader.GetOrdinal("started_at")),
                                CompletedAt = reader.IsDBNull(reader.GetOrdinal("completed_at")) ? null : reader.GetDateTime(reader.GetOrdinal("completed_at")),
                                AnswersFilePath = reader.IsDBNull(reader.GetOrdinal("answers_file_path")) ? null : reader.GetString(reader.GetOrdinal("answers_file_path")),
                                IsValidated = !reader.IsDBNull(reader.GetOrdinal("is_validated")) && reader.GetBoolean(reader.GetOrdinal("is_validated")),
                                PercentageScore = reader.IsDBNull(reader.GetOrdinal("percentage_score")) ? null : reader.GetDecimal(reader.GetOrdinal("percentage_score")),
                                RejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason")) ? null : reader.GetString(reader.GetOrdinal("rejection_reason")),
                                RejectedAt = reader.IsDBNull(reader.GetOrdinal("rejected_at")) ? null : reader.GetDateTime(reader.GetOrdinal("rejected_at")),
                                Answers = []
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("a_id")))
                        {
                            testAttempt.Answers.Add(new Answer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("a_id")),
                                AttemptId = reader.GetInt32(reader.GetOrdinal("attempt_id")),
                                QuestionId = reader.GetInt32(reader.GetOrdinal("question_id")),
                                Value = reader.IsDBNull(reader.GetOrdinal("value")) ? null : reader.GetString(reader.GetOrdinal("value"))
                            });
                        }
                    }
                }
            }
            return testAttempt;
        }

        public async Task SaveAsync(TestAttempt attempt)
        {
            string query = @"
                INSERT INTO TestAttempts 
                (test_id, external_user_id, score, status, started_at, completed_at, answers_file_path, is_validated, percentage_score, rejection_reason, rejected_at)
                OUTPUT INSERTED.id
                VALUES 
                (@test_id, @external_user_id, @score, @status, @started_at, @completed_at, @answers_file_path, @is_validated, @percentage_score, @rejection_reason, @rejected_at)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@test_id", attempt.TestId);
                command.Parameters.AddWithValue("@external_user_id", attempt.ExternalUserId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@score", attempt.Score ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@status", attempt.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@started_at", attempt.StartedAt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@completed_at", attempt.CompletedAt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@answers_file_path", attempt.AnswersFilePath ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_validated", attempt.IsValidated);
                command.Parameters.AddWithValue("@percentage_score", attempt.PercentageScore ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@rejection_reason", attempt.RejectionReason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@rejected_at", attempt.RejectedAt ?? (object)DBNull.Value);

                await connection.OpenAsync();
                attempt.Id = (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task<TestAttempt?> UpdateAsync(TestAttempt attempt)
        {
            string query = @"
                UPDATE TestAttempts
                SET score = @score, 
                    status = @status, 
                    completed_at = @completed_at, 
                    answers_file_path = @answers_file_path, 
                    is_validated = @is_validated, 
                    percentage_score = @percentage_score, 
                    rejection_reason = @rejection_reason, 
                    rejected_at = @rejected_at
                WHERE id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", attempt.Id);
                command.Parameters.AddWithValue("@score", attempt.Score ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@status", attempt.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@completed_at", attempt.CompletedAt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@answers_file_path", attempt.AnswersFilePath ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@is_validated", attempt.IsValidated);
                command.Parameters.AddWithValue("@percentage_score", attempt.PercentageScore ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@rejection_reason", attempt.RejectionReason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@rejected_at", attempt.RejectedAt ?? (object)DBNull.Value);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return null;
                }
            }

            return attempt;
        }

        public async Task<TestAttempt?> FindByIdAsync(int id)
        {
            TestAttempt? testAttempt = null;

            string query = @"
                SELECT 
                    ta.id AS ta_id, ta.test_id, ta.external_user_id, ta.score, ta.status, 
                    ta.started_at, ta.completed_at, ta.answers_file_path, ta.is_validated, 
                    ta.percentage_score, ta.rejection_reason, ta.rejected_at,
                    a.id AS a_id, a.attempt_id, a.question_id, a.value
                FROM TestAttempts ta
                LEFT JOIN Answers a ON ta.id = a.attempt_id
                WHERE ta.id = @id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (testAttempt == null)
                        {
                            testAttempt = new TestAttempt
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ta_id")),
                                TestId = reader.GetInt32(reader.GetOrdinal("test_id")),
                                ExternalUserId = reader.IsDBNull(reader.GetOrdinal("external_user_id")) ? null : reader.GetInt32(reader.GetOrdinal("external_user_id")),
                                Score = reader.IsDBNull(reader.GetOrdinal("score")) ? null : reader.GetDecimal(reader.GetOrdinal("score")),
                                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                                StartedAt = reader.IsDBNull(reader.GetOrdinal("started_at")) ? null : reader.GetDateTime(reader.GetOrdinal("started_at")),
                                CompletedAt = reader.IsDBNull(reader.GetOrdinal("completed_at")) ? null : reader.GetDateTime(reader.GetOrdinal("completed_at")),
                                AnswersFilePath = reader.IsDBNull(reader.GetOrdinal("answers_file_path")) ? null : reader.GetString(reader.GetOrdinal("answers_file_path")),
                                IsValidated = !reader.IsDBNull(reader.GetOrdinal("is_validated")) && reader.GetBoolean(reader.GetOrdinal("is_validated")),
                                PercentageScore = reader.IsDBNull(reader.GetOrdinal("percentage_score")) ? null : reader.GetDecimal(reader.GetOrdinal("percentage_score")),
                                RejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason")) ? null : reader.GetString(reader.GetOrdinal("rejection_reason")),
                                RejectedAt = reader.IsDBNull(reader.GetOrdinal("rejected_at")) ? null : reader.GetDateTime(reader.GetOrdinal("rejected_at")),
                                Answers = []
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("a_id")))
                        {
                            testAttempt.Answers.Add(new Answer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("a_id")),
                                AttemptId = reader.GetInt32(reader.GetOrdinal("attempt_id")),
                                QuestionId = reader.GetInt32(reader.GetOrdinal("question_id")),
                                Value = reader.IsDBNull(reader.GetOrdinal("value")) ? null : reader.GetString(reader.GetOrdinal("value"))
                            });
                        }
                    }
                }
            }
            return testAttempt;
        }

        public async Task<List<TestAttempt>> FindValidAttemptsByTestIdAsync(int testId)
        {
            var attempts = new List<TestAttempt>();

            string query = @"
                SELECT 
                    ta.id AS ta_id, ta.test_id, ta.external_user_id, ta.score, ta.status, 
                    ta.started_at, ta.completed_at, ta.answers_file_path, ta.is_validated, 
                    ta.percentage_score, ta.rejection_reason, ta.rejected_at,
                    u.id AS u_id, u.name, u.email
                FROM TestAttempts ta
                INNER JOIN Users u ON ta.external_user_id = u.id
                WHERE ta.test_id = @test_id 
                  AND ta.status = 'COMPLETED' 
                  AND ta.is_validated = 1 
                  AND ta.percentage_score IS NOT NULL 
                  AND ta.completed_at IS NOT NULL
                ORDER BY ta.percentage_score DESC, ta.completed_at ASC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@test_id", testId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var attempt = new TestAttempt
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ta_id")),
                            TestId = reader.GetInt32(reader.GetOrdinal("test_id")),
                            ExternalUserId = reader.GetInt32(reader.GetOrdinal("external_user_id")),
                            Score = reader.IsDBNull(reader.GetOrdinal("score")) ? null : reader.GetDecimal(reader.GetOrdinal("score")),
                            Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                            StartedAt = reader.IsDBNull(reader.GetOrdinal("started_at")) ? null : reader.GetDateTime(reader.GetOrdinal("started_at")),
                            CompletedAt = reader.IsDBNull(reader.GetOrdinal("completed_at")) ? null : reader.GetDateTime(reader.GetOrdinal("completed_at")),
                            AnswersFilePath = reader.IsDBNull(reader.GetOrdinal("answers_file_path")) ? null : reader.GetString(reader.GetOrdinal("answers_file_path")),
                            IsValidated = reader.GetBoolean(reader.GetOrdinal("is_validated")),
                            PercentageScore = reader.GetDecimal(reader.GetOrdinal("percentage_score")),
                            RejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason")) ? null : reader.GetString(reader.GetOrdinal("rejection_reason")),
                            RejectedAt = reader.IsDBNull(reader.GetOrdinal("rejected_at")) ? null : reader.GetDateTime(reader.GetOrdinal("rejected_at")),

                            User = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("u_id")),
                                Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email"))
                            }
                        };

                        attempts.Add(attempt);
                    }
                }
            }
            return attempts;
        }
    }
}