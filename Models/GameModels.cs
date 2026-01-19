using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PromptRuckus.Models
{
    public enum GameState
    {
        Lobby,
        Prompting,
        Generating,
        Judging,
        Results
    }

    public class Player
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Anonymous";
        public int Score { get; set; }
        public bool IsReady { get; set; }
        public string ConnectionId { get; set; } // SignalR ConnectionId (optional if needed later)
    }

    public class RoundResult
    {
        public string PlayerId { get; set; }
        public string GeneratedContent { get; set; }
        public string PlayerPrompt { get; set; } // The prompt they used (New!)
        public int Score { get; set; }
        public string JudgeComment { get; set; }
        public string AssignedStyle { get; set; } // The style they interpreted
    }

    public class PromptTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Template { get; set; } = "";
        public string Category { get; set; } = "通用";
        public string Description { get; set; } = "";
    }

    public class Room
    {
        public string RoomId { get; set; } = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        public string HostPlayerId { get; set; }
        
        // Settings
        public string CurrentTheme { get; set; } = "Waiting for host...";
        public string CurrentJudgePersona { get; set; } = "AI Judge";
        public int MaxRounds { get; set; } = 3;
        public int CurrentRound { get; set; } = 1;
        public List<string> CustomJudgePersonas { get; set; } = new();
        public bool AllowSpectators { get; set; } = true;

        // State
        public GameState State { get; set; } = GameState.Lobby;
        public DateTime StateEndTime { get; set; } // For timers

        // Data
        public ConcurrentDictionary<string, Player> Players { get; set; } = new();
        public ConcurrentDictionary<string, Player> Spectators { get; set; } = new();
        
        // Round Data
        // PlayerId -> Prompt
        public ConcurrentDictionary<string, string> PlayerPrompts { get; set; } = new();
        
        // PlayerId -> Assigned Secret Style (New!)
        public ConcurrentDictionary<string, string> PlayerStyles { get; set; } = new();

        // PlayerId -> Result
        public ConcurrentDictionary<string, RoundResult> RoundResults { get; set; } = new();
        
        // Helper to get list
        public List<Player> PlayerList => Players.Values.ToList();
        public List<Player> SpectatorList => Spectators.Values.ToList();
    }
}
