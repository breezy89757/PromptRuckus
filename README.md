# 🎮 PromptRuckus

**PromptRuckus** 是一款基於 AI 的多人創意寫作派對遊戲 (Jackbox Style)。
玩家需要根據 AI 隨機生成的怪誕題目，寫出最有趣的 Prompt (提示詞)，並接受來自不同人格的 AI 毒舌評審的犀利講評！

![Home Screen](doc/home_screen.png)

## ✨ 特色功能 (Features)

### 1. 無限動態題目 (Dynamic AI Themes)
告別枯燥的固定題庫！每一回合的題目都由 AI 現場生成，從「直銷話術賣空氣」到「瓊瑤劇罵 Bug」，保證每次玩都有新驚喜。

![Prompting Phase](doc/prompting_screen.png)

### 2. 百變毒舌評審 (Variable Design Personas)
評審不再是冷冰冰的機器人。每一局 AI 都會隨機扮演不同角色：
*   🍳 **地獄廚神**：把你罵得狗血淋頭。
*   😒 **厭世店員**：對你的創意愛理不理。
*   🙀 **傲嬌貓咪**：可能只會喵喵叫。
*   ...還有更多隱藏角色！

![Generating & Judging](doc/generating_screen.png)

### 3. 智能反作弊系統 (Smart Anti-Cheat)
想偷懶直接複製題目？別想！
我們實作了 **Levenshtein Distance** 字串相似度偵測。如果你的 Prompt 和題目太像，評審會直接給出 **< 30分** 的不及格分數，並公開羞辱你的懶惰行為。

![Results](doc/results_screen.png)

### 4. 提示詞範本庫 (Prompt Template Library) 🆕
不知道怎麼寫 Prompt？沒問題！
遊戲內建 **12+ 種範本**，涵蓋搞笑、戲劇、正經、黑暗等多種風格：
*   🎤 **脫口秀演員**：幽默諷刺風格
*   👑 **古裝劇角色**：文言文武俠風
*   📺 **新聞主播**：正式客觀報導
*   🍳 **廚神評審**：毒舌專業點評
*   📖 **詩人**：優美押韻詩詞
*   😴 **厭世店員**：疲憊無奈風格
*   🔥 **熱血動漫角色**：中二激情滿滿
*   ...還有更多！

只需點選分類和範本，一鍵套用，讓創作更輕鬆！

### 5. 自訂評審人格 (Custom Judge Personas) 🆕
想要更個性化的遊戲體驗？房主可以：
*   ➕ 新增專屬的評審角色（例如：「愛說冷笑話的爸爸」、「嚴格的數學老師」）
*   🎲 自訂評審會與內建評審混合，隨機出現在遊戲中
*   🗑️ 隨時移除不需要的自訂角色

讓每一局遊戲都充滿驚喜和個性！

### 6. 觀眾模式 (Spectator Mode) 🆕
遊戲開始後朋友才到？沒關係！
*   👁️ 啟用觀眾模式後，玩家可以在遊戲進行中以**觀眾身份**加入
*   觀眾可以觀看所有遊戲內容，但不參與 Prompt 提交
*   房主可以在設定中開啟或關閉觀眾模式

讓更多人一起歡樂，不錯過精彩時刻！

### 7. 玩家成就系統 (Achievement System) 🆕
記錄你的遊戲旅程，解鎖專屬榮譽！
*   🏆 **8 種成就**：首次獲勝、完美表現、連勝王者、絕地反攻、天才創作者、派對常客、骨灰級玩家、創意無限
*   📊 **詳細統計**：遊戲場次、獲勝次數、最高分數、最佳連勝等
*   🏅 **排行榜系統**：與其他玩家比拼，爭奪榜首位置
*   💾 **自動追蹤**：每場遊戲結束後自動記錄成績並檢查新成就
*   🎉 **即時通知**：遊戲結束時立即顯示新解鎖的成就

訪問 **/stats** 頁面查看你的完整統計資料和成就收藏！

### 8. 遊戲歷史記錄 (Game History) 🆕
回顧過往的精彩對局！
*   📜 **完整記錄**：保存所有完成的遊戲資訊
*   🔍 **詳細回顧**：查看每一回合的題目、評審、玩家成績和評語
*   👤 **個人記錄**：篩選查看自己參與的遊戲
*   📈 **數據統計**：了解遊戲趨勢和玩家表現
*   🎯 **快速訪問**：從首頁直接進入歷史記錄頁面

訪問 **/history** 頁面重溫你的遊戲時光！

## 🚀 如何開始 (How to Run)

1. **先決條件**:
   - .NET 10 SDK
   - OpenAI / Azure OpenAI API Key

2. **設定 API Key**:
   在 `appsettings.json` 中填入你的 Key：
   ```json
   "AiModel": {
     "Provider": "OpenAI",
     "OpenAI": {
       "ApiKey": "sk-..."
     }
   }
   ```

3. **啟動遊戲**:
   ```bash
   dotnet run
   ```
   瀏覽器打開 `http://localhost:5000` 即可開始遊玩！

## 🛠️ 技術堆疊 (Tech Stack)
*   **Framework**: ASP.NET Core Blazor (Interactive Server)
*   **AI SDK**: Azure.AI.OpenAI / Microsoft Agent Framework (Conceptual)
*   **Real-time Communication**: SignalR (Built-in Blazor)
*   **Styles**: Pure CSS (Glassmorphism Design)

## 📜 License
本專案採用 [MIT License](LICENSE) 授權。
