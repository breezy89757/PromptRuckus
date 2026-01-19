using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PromptRuckus.Models;

namespace PromptRuckus.Services
{
    public class AchievementService
    {
        // In-memory storage (in a real app, use database)
        private readonly ConcurrentDictionary<string, PlayerStats> _playerStats = new();
        private readonly List<Achievement> _availableAchievements;

        public AchievementService()
        {
            _availableAchievements = InitializeAchievements();
        }

        private List<Achievement> InitializeAchievements()
        {
            return new List<Achievement>
            {
                new Achievement
                {
                    Type = AchievementType.FirstWin,
                    Name = "初試啼聲",
                    Description = "獲得你的第一次勝利",
                    Icon = "🥇",
                    RequiredCount = 1
                },
                new Achievement
                {
                    Type = AchievementType.PerfectScore,
                    Name = "完美表現",
                    Description = "獲得 100 分的完美評分",
                    Icon = "💯",
                    RequiredCount = 1
                },
                new Achievement
                {
                    Type = AchievementType.Streak,
                    Name = "連勝王者",
                    Description = "連續贏得 3 場比賽",
                    Icon = "🔥",
                    RequiredCount = 3
                },
                new Achievement
                {
                    Type = AchievementType.Comeback,
                    Name = "絕地反攻",
                    Description = "從最後一名逆轉獲勝",
                    Icon = "🚀",
                    RequiredCount = 1
                },
                new Achievement
                {
                    Type = AchievementType.HighScore,
                    Name = "天才創作者",
                    Description = "單回合獲得 90 分以上",
                    Icon = "⭐",
                    RequiredCount = 1
                },
                new Achievement
                {
                    Type = AchievementType.Participation,
                    Name = "派對常客",
                    Description = "參與 10 場遊戲",
                    Icon = "🎉",
                    RequiredCount = 10
                },
                new Achievement
                {
                    Type = AchievementType.Veteran,
                    Name = "骨灰級玩家",
                    Description = "參與 50 場遊戲",
                    Icon = "👑",
                    RequiredCount = 50
                },
                new Achievement
                {
                    Type = AchievementType.Creative,
                    Name = "創意無限",
                    Description = "連續 5 場沒有被判定作弊",
                    Icon = "🎨",
                    RequiredCount = 5
                }
            };
        }

        public PlayerStats GetOrCreatePlayerStats(string playerId, string playerName)
        {
            return _playerStats.GetOrAdd(playerId, _ => new PlayerStats
            {
                PlayerId = playerId,
                PlayerName = playerName,
                CreatedAt = DateTime.UtcNow
            });
        }

        public void RecordGameResult(string playerId, string playerName, int score, bool isWinner, bool wasLastPlace, bool noCheating)
        {
            var stats = GetOrCreatePlayerStats(playerId, playerName);
            
            stats.TotalGamesPlayed++;
            stats.LastPlayedAt = DateTime.UtcNow;
            
            if (score > stats.HighestScore)
            {
                stats.HighestScore = score;
            }

            if (isWinner)
            {
                stats.TotalWins++;
                stats.CurrentStreak++;
                
                if (stats.CurrentStreak > stats.BestStreak)
                {
                    stats.BestStreak = stats.CurrentStreak;
                }

                // Check for comeback achievement
                if (wasLastPlace)
                {
                    stats.Comebacks++;
                    CheckAndUnlockAchievement(stats, AchievementType.Comeback, stats.Comebacks);
                }

                // Check for first win
                CheckAndUnlockAchievement(stats, AchievementType.FirstWin, stats.TotalWins);
                
                // Check for streak
                CheckAndUnlockAchievement(stats, AchievementType.Streak, stats.CurrentStreak);
            }
            else
            {
                stats.CurrentStreak = 0;
            }

            // Check for perfect score
            if (score == 100)
            {
                stats.PerfectScores++;
                CheckAndUnlockAchievement(stats, AchievementType.PerfectScore, stats.PerfectScores);
            }

            // Check for high score
            if (score >= 90)
            {
                CheckAndUnlockAchievement(stats, AchievementType.HighScore, 1);
            }

            // Check for participation
            CheckAndUnlockAchievement(stats, AchievementType.Participation, stats.TotalGamesPlayed);
            CheckAndUnlockAchievement(stats, AchievementType.Veteran, stats.TotalGamesPlayed);
        }

        private void CheckAndUnlockAchievement(PlayerStats stats, AchievementType type, int currentCount)
        {
            var achievement = _availableAchievements.FirstOrDefault(a => a.Type == type);
            if (achievement == null) return;

            // Check if already unlocked
            if (stats.UnlockedAchievements.Any(a => a.Type == type)) return;

            // Check if requirement met
            if (currentCount >= achievement.RequiredCount)
            {
                var unlockedAchievement = new Achievement
                {
                    Id = achievement.Id,
                    Type = achievement.Type,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    Icon = achievement.Icon,
                    RequiredCount = achievement.RequiredCount,
                    UnlockedAt = DateTime.UtcNow
                };
                
                stats.UnlockedAchievements.Add(unlockedAchievement);
            }
        }

        public List<Achievement> GetAllAchievements()
        {
            return _availableAchievements;
        }

        public PlayerStats? GetPlayerStats(string playerId)
        {
            _playerStats.TryGetValue(playerId, out var stats);
            return stats;
        }

        public List<PlayerStats> GetTopPlayers(int count = 10)
        {
            return _playerStats.Values
                .OrderByDescending(s => s.TotalWins)
                .ThenByDescending(s => s.HighestScore)
                .Take(count)
                .ToList();
        }

        public List<Achievement> GetNewlyUnlockedAchievements(string playerId)
        {
            var stats = GetPlayerStats(playerId);
            if (stats == null) return new List<Achievement>();

            // Return achievements unlocked in the last minute (for display purposes)
            return stats.UnlockedAchievements
                .Where(a => (DateTime.UtcNow - a.UnlockedAt).TotalSeconds < 60)
                .ToList();
        }
    }
}
