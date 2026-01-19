using System;
using System.Collections.Generic;

namespace PromptRuckus.Models
{
    public enum AchievementType
    {
        FirstWin,           // 首次獲勝
        PerfectScore,       // 獲得 100 分
        Comeback,           // 從最後名逆轉勝
        Streak,             // 連續獲勝
        Participation,      // 參與遊戲次數
        HighScore,          // 單回合高分
        Creative,           // 創意獎（避免作弊）
        Veteran             // 老玩家
    }

    public class Achievement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public AchievementType Type { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "🏆";
        public int RequiredCount { get; set; } = 1;
        public DateTime UnlockedAt { get; set; }

        public Achievement Clone(DateTime unlockedAt)
        {
            return new Achievement
            {
                Id = this.Id,
                Type = this.Type,
                Name = this.Name,
                Description = this.Description,
                Icon = this.Icon,
                RequiredCount = this.RequiredCount,
                UnlockedAt = unlockedAt
            };
        }
    }

    public class PlayerStats
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public int TotalGamesPlayed { get; set; }
        public int TotalWins { get; set; }
        public int HighestScore { get; set; }
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public int PerfectScores { get; set; }
        public int Comebacks { get; set; }
        public List<Achievement> UnlockedAchievements { get; set; } = new();
        public DateTime LastPlayedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
