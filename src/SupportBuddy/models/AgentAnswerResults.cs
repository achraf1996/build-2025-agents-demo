using System.Collections.Generic;
using System.Text.Json.Serialization;

public class AgentAnswerResults
{
    [JsonPropertyName("answered_questions")]
    public List<AgentAnswer> AnsweredQuestions { get; set; } = [];

    [JsonPropertyName("unanswered_questions")]
    public List<string> UnansweredIssues { get; set; } = [];
}

public class AgentAnswer
{
    [JsonPropertyName("question_id")]
    public string QuestionId { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;
}