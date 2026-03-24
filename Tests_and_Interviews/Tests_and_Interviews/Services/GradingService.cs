using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Services
{
    public class GradingService
    {
        public void GradeSingleChoice(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }
            if (answer.Value.Trim() == question.QuestionAnswer.Trim())
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";

            }
        }

        public void GradeMultipleChoice(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }

            var correctAnswers = new List<int>();
            var selectedAnswers = new List<int>();

            foreach (var part in question.QuestionAnswer.Trim().TrimStart('[').TrimEnd(']').Split(','))
            {
                if (int.TryParse(part.Trim(), out int idx))
                {
                    correctAnswers.Add(idx);
                }
            }

            foreach (var part in answer.Value.Trim().TrimStart('[').TrimEnd(']').Split(','))
            {
                if (int.TryParse(part.Trim(), out int idx))
                {
                    selectedAnswers.Add(idx);
                }
            }

            bool isCorrect = correctAnswers.Count == selectedAnswers.Count && correctAnswers.All(i => selectedAnswers.Contains(i));
            if (isCorrect)
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";
            }
        }

        public void GradeText(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }

            bool isCorrect = string.Equals(
                answer.Value.Trim(),
                question.QuestionAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";
            }


        }

        public void GradeTrueFalse(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }
            bool isCorrect = string.Equals(
                answer.Value.Trim(),
                question.QuestionAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";
            }
        }

        public float CalculateFinalScore(TestAttempt attempt)
        {
            float totalScore = 0f;

            foreach(var answer in attempt.Answers)
            {
                if (!answer.Value.StartsWith("CORRECT:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string scorePart = answer.Value.Substring("CORRECT:".Length);

                if(float.TryParse(scorePart, NumberStyles.Float, CultureInfo.InvariantCulture, out float points))
                {
                    totalScore += points;
                }


            }                
            
            attempt.Score = (decimal)totalScore;

            return totalScore;

        }
    }
}