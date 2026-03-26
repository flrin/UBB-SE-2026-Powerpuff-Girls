using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Repositories
{
    public class QuestionRepository
    {
        private readonly string _connectionString;

        public QuestionRepository()
        {
            _connectionString = Env.CONNECTION_STRING;
        }

        public async Task<List<Question>> FindByTestIdAsync(int testId)
        {
            var questionDictionary = new Dictionary<int, Question>();

            string query = @"
                SELECT 
                    q.id AS q_id, q.position_id, q.test_id, q.question_text, 
                    q.question_type_string, q.question_score, q.question_answer, q.options_json,
                    a.id AS a_id, a.attempt_id, a.question_id, a.value
                FROM Questions q
                LEFT JOIN Answers a ON q.id = a.question_id
                WHERE q.test_id = @test_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@test_id", testId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int questionId = reader.GetInt32(reader.GetOrdinal("q_id"));

                        if (!questionDictionary.TryGetValue(questionId, out var question))
                        {
                            question = new Question
                            {
                                Id = questionId,
                                PositionId = reader.IsDBNull(reader.GetOrdinal("position_id")) ? null : reader.GetInt32(reader.GetOrdinal("position_id")),
                                TestId = reader.IsDBNull(reader.GetOrdinal("test_id")) ? null : reader.GetInt32(reader.GetOrdinal("test_id")),
                                QuestionText = reader.GetString(reader.GetOrdinal("question_text")),
                                QuestionTypeString = reader.GetString(reader.GetOrdinal("question_type_string")),
                                QuestionScore = reader.GetFloat(reader.GetOrdinal("question_score")),
                                QuestionAnswer = reader.IsDBNull(reader.GetOrdinal("question_answer")) ? null : reader.GetString(reader.GetOrdinal("question_answer")),
                                OptionsJson = reader.IsDBNull(reader.GetOrdinal("options_json")) ? null : reader.GetString(reader.GetOrdinal("options_json")),
                                Answers = new List<Answer>()
                            };
                            questionDictionary.Add(questionId, question);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("a_id")))
                        {
                            var answer = new Answer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("a_id")),
                                AttemptId = reader.GetInt32(reader.GetOrdinal("attempt_id")),
                                QuestionId = reader.GetInt32(reader.GetOrdinal("question_id")),
                                Value = reader.IsDBNull(reader.GetOrdinal("value")) ? null : reader.GetString(reader.GetOrdinal("value"))
                            };
                            question.Answers.Add(answer);
                        }
                    }
                }
            }

            return new List<Question>(questionDictionary.Values);
        }

        public async Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId)
        {
            var questions = new List<Question>();
            string query = @"
                SELECT id, position_id, test_id, question_text, question_type_string, question_score, question_answer, options_json 
                FROM Questions 
                WHERE question_type_string = @question_type AND position_id = @position_id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@question_type", QuestionType.INTERVIEW.ToString());
                command.Parameters.AddWithValue("@position_id", positionId);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        questions.Add(new Question
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            PositionId = reader.IsDBNull(reader.GetOrdinal("position_id")) ? null : reader.GetInt32(reader.GetOrdinal("position_id")),
                            TestId = reader.IsDBNull(reader.GetOrdinal("test_id")) ? null : reader.GetInt32(reader.GetOrdinal("test_id")),
                            QuestionText = reader.GetString(reader.GetOrdinal("question_text")),
                            QuestionTypeString = reader.GetString(reader.GetOrdinal("question_type_string")),
                            QuestionScore = reader.GetFloat(reader.GetOrdinal("question_score")), // Fixed the GetDouble cast here too
                            QuestionAnswer = reader.IsDBNull(reader.GetOrdinal("question_answer")) ? null : reader.GetString(reader.GetOrdinal("question_answer")),
                            OptionsJson = reader.IsDBNull(reader.GetOrdinal("options_json")) ? null : reader.GetString(reader.GetOrdinal("options_json"))
                        });
                    }
                }
            }
            return questions;
        }
    }
}