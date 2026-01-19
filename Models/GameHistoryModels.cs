using System;
using System.Collections.Generic;

namespace PromptRuckus.Models
{
    public class GameHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RoomId { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalRounds { get; set; }
        public List<string> ParticipantIds { get; set; } = new();
        public List<string> ParticipantNames { get; set; } = new();
        public string WinnerId { get; set; } = "";
        public string WinnerName { get; set; } = "";
        public int WinnerScore { get; set; }
        public List<RoundHistory> Rounds { get; set; } = new();
    }

    public class RoundHistory
    {
        public int RoundNumber { get; set; }
        public string Theme { get; set; } = "";
        public string JudgePersona { get; set; } = "";
        public List<RoundResult> Results { get; set; } = new();
    }
}
