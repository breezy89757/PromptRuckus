using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Azure.Identity;
using Azure.AI.OpenAI;
using System.ClientModel;

namespace PromptRuckus.Services
{
    public class AiGenerationService
    {
        private readonly ChatClient _chatClient;

        public AiGenerationService(IConfiguration config)
        {
            var provider = config["AiModel:Provider"];
            var endpoint = config["AiModel:OpenAI:Endpoint"];
            var apiKey = config["AiModel:OpenAI:ApiKey"];
            var deployment = config["AiModel:OpenAI:DeploymentName"]; // For Azure
            var model = config["AiModel:OpenAI:Model"] ?? "gpt-4o-mini"; // For OpenAI

            // Check if we have Azure Config
            if (provider == "AzureOpenAI" 
                && !string.IsNullOrEmpty(endpoint) 
                && !string.IsNullOrEmpty(apiKey))
            {
                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
                _chatClient = azureClient.GetChatClient(deployment ?? "gpt-4o-mini");
            }
            else if (!string.IsNullOrEmpty(apiKey))
            {
                 // Default to OpenAI direct
                _chatClient = new ChatClient(model, apiKey);
            }
            else
            {
                // Fallback / Throw clear error
                throw new InvalidOperationException("AI Model Configuration Missing! Please check appsettings.json for 'AiModel:OpenAI:ApiKey'.");
            }
        }

        public async Task<string> GenerateContentAsync(string systemPrompt, string theme)
        {
            try
            {
                var prompt = $"Theme: {theme}\n\nTask: Generate content based on the theme, strictly following the style/instructions provided in the system prompt below.\n\nSystem Prompt: {systemPrompt}";
                
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a creative AI assistant for a party game. Follow the user's styling instructions precisely to generate short, funny, or interesting content."),
                    new UserChatMessage(prompt)
                };

                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                return $"Error generating content: {ex.Message}";
            }
        }

        public async Task<string> GenerateThemeAsync()
        {
            try 
            {
                 var messages = new List<ChatMessage>
                 {
                     new SystemChatMessage(@"You are a creative game host. 
Generate a short, funny, and bizarre theme for a writing prompt game.
The theme should be in Traditional Chinese (Taiwanese Mandarin).
Examples: '用『瓊瑤劇』的口吻大罵程式碼有Bug', '假裝你是『掃地機器人』正在抱怨這個家很髒', '用『直銷話術』推銷『空氣』'.
Output ONLY the theme text, nothing else."),
                     new UserChatMessage("Give me a new crazy theme.")
                 };

                 ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
                 return completion.Content[0].Text.Trim().Replace("\"", "");
            }
            catch
            {
                return "用『原始人』的方式解釋什麼是區塊鏈"; // Fallback
            }
        }

        private readonly string[] _judgePersonas = new[] 
        {
            "嚴格的米其林地獄廚神 (就像 Gordon Ramsay)",
            "只會講晶晶體的竹科工程師 (Tech Bro)",
            "極度厭世的超商大夜班店員",
            "講話一直押韻的饒舌歌手",
            "穿越時空的古代皇帝",
            "愛管閒事的隔壁大嬸 (Auntie)",
            "什麼都不滿意的貓咪 (Cat)"
        };

        public string GetRandomJudgePersona()
        {
            return _judgePersonas[new Random().Next(_judgePersonas.Length)];
        }

        public async Task<(int Score, string Comment)> JudgeContentAsync(string content, string theme, string userPrompt, string judgePersona)
        {
             try 
             {
                 var prompt = $@"
You are acting as: {judgePersona}.
Score the following content from 0-100 and provide a comment in character.

Game Context:
- Theme: {theme}
- Player's Prompt: {userPrompt}
- AI Output: {content}

Judging Criteria:
1. **Adherence**: Did they follow the theme?
2. **Creativity**: Is it funny?
3. **Effort**: Penalize lazy prompts!

Task:
Score (0-100) and Comment (1 sentence, witty, in character).
IMPORTANT: Comment MUST be in Traditional Chinese (Taiwan).

Output Format:
Score: [number]
Comment: [text]
";
                 // Auto-detect laziness/cheating (Similarity Check)
                 // Normalize strings (remove whitespace, lower case) to catch "adding spaces"
                 var normalizedPrompt = userPrompt.Replace(" ", "").Replace("\t", "").Replace("\n", "").ToLower();
                 var normalizedTheme = theme.Replace(" ", "").Replace("\t", "").Replace("\n", "").ToLower();
                 
                 var similarity = CalculateSimilarity(normalizedPrompt, normalizedTheme);

                 if (similarity > 0.6 || normalizedPrompt.Length < 5)
                 {
                     // Force strict mode in prompt
                     prompt += $"\n\nCRITICAL WARNING: The player's prompt is {similarity:P0} similar to the theme. This suggests they just COPIED the theme. This is CHEATING/LAZY. You MUST give a score below 30 and mock them for not being creative.";
                 }

                 var messages = new List<ChatMessage>
                 {
                     new SystemChatMessage($"You are a funny AI judge playing the role of: {judgePersona}. You act 100% in character. You speak Traditional Chinese (Taiwan)."),
                     new UserChatMessage(prompt)
                 };

                 ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
                 var text = completion.Content[0].Text;
                 
                 // Simple Parsing
                 var score = 50;
                 var comment = "不錯喔！";

                 foreach(var line in text.Split('\n'))
                 {
                     if (line.StartsWith("Score:", StringComparison.OrdinalIgnoreCase))
                     {
                         int.TryParse(line.Replace("Score:", "").Trim(), out score);
                     }
                     if (line.StartsWith("Comment:", StringComparison.OrdinalIgnoreCase))
                     {
                         comment = line.Replace("Comment:", "").Trim();
                     }
                 }

                 return (score, comment);
             }
             catch(Exception ex)
             {
                 return (50, "AI 評審累了，隨便給分。");
             }
        }

        private static double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;

            int n = source.Length;
            int m = target.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            int distance = d[n, m];
            int maxLen = Math.Max(n, m);
            if (maxLen == 0) return 1.0;

            return 1.0 - ((double)distance / maxLen);
        }
    }
}
