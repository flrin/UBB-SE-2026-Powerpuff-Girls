using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Repositories
{
    public class AnswerRepository
    {
        private readonly string _connectionString;

        public AnswerRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task SaveAsync(Answer answer)
        {
            string query = @"
                INSERT INTO Answers (attempt_id, question_id, value)
                VALUES (@attempt_id, @question_id, @value);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@attempt_id", answer.AttemptId);
                command.Parameters.AddWithValue("@question_id", answer.QuestionId);
                command.Parameters.AddWithValue("@value", answer.Value ?? (object)DBNull.Value);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Answer>> FindByAttemptAsync(int attemptId)
        {
            var answers = new List<Answer>();

            // The JOIN replaces EF Core's .Include(a => a.Question)
            string query = @"
                SELECT 
                    a.id AS answer_id, a.attempt_id, a.question_id, a.value,
                    q.id AS q_id, q.position_id, q.test_id, q.question_text, 
                    q.question_type_string, q.question_score, q.question_answer, q.options_json
                FROM Answers a
                INNER JOIN Questions q ON a.question_id = q.id
                WHERE a.attempt_id = @attempt_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@attempt_id", attemptId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var answer = new Answer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("answer_id")),
                            AttemptId = reader.GetInt32(reader.GetOrdinal("attempt_id")),
                            QuestionId = reader.GetInt32(reader.GetOrdinal("question_id")),
                            Value = reader.IsDBNull(reader.GetOrdinal("value")) ? null : reader.GetString(reader.GetOrdinal("value")),

                            // Manually map the included Question object
                            Question = new Question
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("q_id")),
                                PositionId = reader.IsDBNull(reader.GetOrdinal("position_id")) ? null : reader.GetInt32(reader.GetOrdinal("position_id")),
                                TestId = reader.IsDBNull(reader.GetOrdinal("test_id")) ? null : reader.GetInt32(reader.GetOrdinal("test_id")),
                                QuestionText = reader.GetString(reader.GetOrdinal("question_text")),
                                QuestionTypeString = reader.GetString(reader.GetOrdinal("question_type_string")),
                                QuestionScore = reader.GetFloat(reader.GetOrdinal("question_score")),
                                QuestionAnswer = reader.IsDBNull(reader.GetOrdinal("question_answer")) ? null : reader.GetString(reader.GetOrdinal("question_answer")),
                                OptionsJson = reader.IsDBNull(reader.GetOrdinal("options_json")) ? null : reader.GetString(reader.GetOrdinal("options_json"))
                            }
                        };
                        answers.Add(answer);
                    }
                }
            }
            return answers;
        }
    }
}