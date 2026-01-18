using System;
using System.Collections.Concurrent;
using System.Linq;
using PromptRuckus.Models;

namespace PromptRuckus.Services
{
    public class GameService
    {
        // Thread-safe dictionary to hold rooms
        private readonly ConcurrentDictionary<string, Room> _rooms = new();
        private readonly AiGenerationService _aiService;

        public event Action<string>? OnRoomStateChanged; // RoomId

        public GameService(AiGenerationService aiService)
        {
            _aiService = aiService;
        }

        public Room CreateRoom(string hostPlayerName, out Player hostPlayer)
        {
            var room = new Room();
            hostPlayer = new Player { Name = hostPlayerName, IsReady = true };
            
            room.HostPlayerId = hostPlayer.Id;
            room.Players.TryAdd(hostPlayer.Id, hostPlayer);
            
            _rooms.TryAdd(room.RoomId, room);
            return room;
        }
        
        // ... JoinRoom and GetRoom unchanged ...



        public Room? JoinRoom(string roomId, string playerName, out Player? player)
        {
            player = null;
            if (!_rooms.TryGetValue(roomId.ToUpper(), out var room))
            {
                return null;
            }

            // Check if game already started
            if (room.State != GameState.Lobby)
            {
                return null; // Or handle spectator
            }

            player = new Player { Name = playerName };
            room.Players.TryAdd(player.Id, player);
            NotifyStateChanged(roomId);
            
            return room;
        }

        public Room? GetRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId.ToUpper(), out var room))
            {
                return room;
            }
            return null;
        }

        public void NotifyStateChanged(string roomId)
        {
            OnRoomStateChanged?.Invoke(roomId.ToUpper());
        }

        public void SubmitPrompt(string roomId, string playerId, string prompt)
        {
             if (_rooms.TryGetValue(roomId.ToUpper(), out var room))
            {
                room.PlayerPrompts.AddOrUpdate(playerId, prompt, (k, v) => prompt);

                // Early Finish Check
                // Note: In real app, we need to handle if players leave during round
                if (room.PlayerPrompts.Count >= room.Players.Count)
                {
                    // Everyone submitted!
                    EndPromptingPhase(roomId); // Fire and forget (it is async void compatible context or handled)
                }
            }
        }

        // Helper: Logic to start game
        public void StartGame(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                if (room.State == GameState.Lobby)
                {
                    StartRound(room);
                }
            }
        }

        private readonly string[] _themes = new[]
        {
            "讓 AI 用最像『慣老闆』的語氣解釋為什麼要加班",
            "用『武俠小說』的風格描寫寫程式的一天",
            "寫一封『給外星人』的地球推銷信，但要聽起來很像詐騙集團",
            "用『周星馳電影』的台詞風格來解釋量子力學",
            "寫一段關於『珍珠奶茶』的恐怖故事",
            "用『PTT鄉民』的語氣評論今天的生肖運勢",
            "假裝你是『古代皇帝』，正在煩惱今天要吃什麼外送"
        };
        
        // Keeps track of active firing timer to prevent double firing (Early finish + Timer)
        private ConcurrentDictionary<string, System.Timers.Timer> _roomTimers = new();

        private async void StartRound(Room room)
        {
            // Reset previous round data
            room.PlayerPrompts.Clear();
            room.RoundResults.Clear();
            
            room.State = GameState.Prompting;
            room.StateEndTime = DateTime.UtcNow.AddSeconds(60); // More time for prompting?
            
            // AI Generate Theme
            // Note: In async void, exceptions crash process. In real app use Task-safe wrapper.
            try 
            {
                room.CurrentTheme = "正在生成題目..."; // Placeholder
                NotifyStateChanged(room.RoomId);
                
                room.CurrentTheme = await _aiService.GenerateThemeAsync();
                
                // Random Judge Persona
                room.CurrentJudgePersona = _aiService.GetRandomJudgePersona();
            }
            catch(Exception ex)
            {
                room.CurrentTheme = "題目生成失敗，請自由發揮！";
                room.CurrentJudgePersona = "慈祥的奶奶"; // Fallback
                Console.WriteLine(ex);
            }
            
            NotifyStateChanged(room.RoomId);

            // Start Timer for this phase
            StartTimer(room.RoomId, 60000, async () => await EndPromptingPhase(room.RoomId));
        }

        private void StartTimer(string roomId, double interval, Action callback)
        {
            // Stop existing
            if (_roomTimers.TryRemove(roomId, out var oldTimer))
            {
                oldTimer.Stop();
                oldTimer.Dispose();
            }

            var timer = new System.Timers.Timer(interval);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => 
            {
                _roomTimers.TryRemove(roomId, out _);
                timer.Dispose();
                callback();
            };
            timer.Start();
            _roomTimers.TryAdd(roomId, timer);
        }

        private async Task EndPromptingPhase(string roomId)
        {
             if (!_rooms.TryGetValue(roomId, out var room)) return;
             
             // Prevent double entry if timer works + manual finish same time
             if (room.State != GameState.Prompting) return;
             
             // Stop timer if we finished early
             if (_roomTimers.TryRemove(roomId, out var timer))
             {
                 timer.Stop();
                 timer.Dispose();
             }

             room.State = GameState.Generating;
             room.StateEndTime = DateTime.UtcNow.AddSeconds(20); // Give AI time to Generate AND Judge
             NotifyStateChanged(roomId);

                 try 
                 {
                     // Add suspense delay (at least 3 seconds)
                     var suspenseTask = Task.Delay(3000);

                     var tasks = new List<Task>();
                     foreach(var playerPrompt in room.PlayerPrompts)
                     {
                         tasks.Add(Task.Run(async () => 
                         {
                             // 1. Generate Content
                             var content = await _aiService.GenerateContentAsync(playerPrompt.Value, room.CurrentTheme);
                             
                             // 2. Judge Content (With Persona!)
                             var (score, comment) = await _aiService.JudgeContentAsync(content, room.CurrentTheme, playerPrompt.Value, room.CurrentJudgePersona);
                             
                             var result = new RoundResult 
                             { 
                                PlayerId = playerPrompt.Key,
                                GeneratedContent = content,
                                PlayerPrompt = playerPrompt.Value, // Store the prompt
                                Score = score,
                                JudgeComment = comment
                             };

                             // TODO: Call _aiService.JudgeContentAsync(content, room.CurrentTheme);
                             
                             room.RoundResults.TryAdd(playerPrompt.Key, result);
                         }));
                     }

                     await Task.WhenAll(tasks);
                     await suspenseTask; // Ensure at least 3s passed
                 }
             catch(Exception ex)
             {
                 Console.WriteLine($"Generation Error: {ex}");
             }
             
             // Skip Voting (removed), go straight to Results (which acts as Showcase)
             // Or maybe we can have a "Judging" phase just for suspence?
             // Project plan says: "Implement Judging phase where AI scores results" 
             // But we do judging *during* generation for efficiency. 
             // Let's show "Judging" state briefly?
             
             // Let's go straight to Results for faster flow as user requested optimization
             ShowResults(room);
        }

        // Removed StartVotingPhase
        
        private void ShowResults(Room room)
        {
            room.State = GameState.Results;
            room.StateEndTime = DateTime.UtcNow.AddSeconds(15); // Show results longer
            NotifyStateChanged(room.RoomId);

            // Auto next round
            StartTimer(room.RoomId, 15000, () => 
            {
                if (room.CurrentRound < room.MaxRounds)
                {
                    room.CurrentRound++;
                    StartRound(room);
                }
                else
                {
                    // Game Over -> Back to Lobby
                    room.State = GameState.Lobby;
                    room.CurrentRound = 1;
                    NotifyStateChanged(room.RoomId);
                }
            });
        }


    }
}
