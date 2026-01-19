using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PromptRuckus.Models;

namespace PromptRuckus.Services
{
    public class GameHistoryService
    {
        // In-memory storage (in a real app, use database)
        private readonly ConcurrentDictionary<string, GameHistory> _gameHistories = new();
        private readonly ConcurrentDictionary<string, GameHistory> _activeGames = new(); // roomId -> GameHistory

        public void StartGame(string roomId, List<Player> players)
        {
            var history = new GameHistory
            {
                RoomId = roomId,
                StartedAt = DateTime.UtcNow,
                ParticipantIds = players.Select(p => p.Id).ToList(),
                ParticipantNames = players.Select(p => p.Name).ToList()
            };

            _activeGames.TryAdd(roomId, history);
        }

        public void RecordRound(string roomId, int roundNumber, string theme, string judgePersona, List<RoundResult> results)
        {
            if (!_activeGames.TryGetValue(roomId, out var history)) return;

            var roundHistory = new RoundHistory
            {
                RoundNumber = roundNumber,
                Theme = theme,
                JudgePersona = judgePersona,
                Results = new List<RoundResult>(results)
            };

            history.Rounds.Add(roundHistory);
        }

        public void CompleteGame(string roomId, string winnerId, string winnerName, int winnerScore, int totalRounds)
        {
            if (!_activeGames.TryRemove(roomId, out var history)) return;

            history.CompletedAt = DateTime.UtcNow;
            history.WinnerId = winnerId;
            history.WinnerName = winnerName;
            history.WinnerScore = winnerScore;
            history.TotalRounds = totalRounds;

            _gameHistories.TryAdd(history.Id, history);
        }

        public List<GameHistory> GetAllGames(int limit = 50)
        {
            return _gameHistories.Values
                .OrderByDescending(g => g.CompletedAt)
                .Take(limit)
                .ToList();
        }

        public List<GameHistory> GetPlayerGames(string playerId, int limit = 20)
        {
            return _gameHistories.Values
                .Where(g => g.ParticipantIds.Contains(playerId))
                .OrderByDescending(g => g.CompletedAt)
                .Take(limit)
                .ToList();
        }

        public GameHistory? GetGameById(string gameId)
        {
            _gameHistories.TryGetValue(gameId, out var history);
            return history;
        }

        public int GetTotalGamesCount()
        {
            return _gameHistories.Count;
        }
    }
}
