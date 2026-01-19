using System;
using System.Collections.Generic;
using System.Linq;
using PromptRuckus.Models;

namespace PromptRuckus.Services
{
    public class PromptTemplateService
    {
        private readonly List<PromptTemplate> _templates;

        public PromptTemplateService()
        {
            _templates = new List<PromptTemplate>
            {
                new PromptTemplate
                {
                    Name = "脫口秀演員",
                    Template = "你是一個脫口秀演員，請用幽默諷刺的語氣來描述這個主題，要有梗、有笑點。",
                    Category = "搞笑",
                    Description = "用脫口秀的方式表達"
                },
                new PromptTemplate
                {
                    Name = "古裝劇角色",
                    Template = "請用古裝劇（武俠或宮廷劇）的語氣和用詞來描述，要有文言文的感覺。",
                    Category = "戲劇",
                    Description = "古色古香的表達方式"
                },
                new PromptTemplate
                {
                    Name = "新聞主播",
                    Template = "你是一位專業的新聞主播，請用正式、客觀的新聞報導口吻來描述這個主題。",
                    Category = "正經",
                    Description = "正式的新聞報導風格"
                },
                new PromptTemplate
                {
                    Name = "廚神評審",
                    Template = "你是一個米其林餐廳的主廚，請用專業但毒舌的方式來評論這個主題，就像 Gordon Ramsay。",
                    Category = "搞笑",
                    Description = "毒舌廚神風格"
                },
                new PromptTemplate
                {
                    Name = "詩人",
                    Template = "請用優美的詩詞或押韻的方式來描述這個主題，要有文學氣息。",
                    Category = "文藝",
                    Description = "詩意的表達"
                },
                new PromptTemplate
                {
                    Name = "科學教授",
                    Template = "你是一位嚴謹的科學教授，請用學術、專業的角度來分析這個主題，可以加入一些專業術語。",
                    Category = "正經",
                    Description = "學術專業風格"
                },
                new PromptTemplate
                {
                    Name = "厭世店員",
                    Template = "你是一個厭世的便利商店大夜班店員，請用疲憊、無奈、愛理不理的語氣來回應這個主題。",
                    Category = "搞笑",
                    Description = "厭世無奈風格"
                },
                new PromptTemplate
                {
                    Name = "熱血動漫角色",
                    Template = "你是一個熱血動漫的主角，請用充滿激情、永不放棄的態度來描述這個主題，要有中二感。",
                    Category = "戲劇",
                    Description = "熱血中二風格"
                },
                new PromptTemplate
                {
                    Name = "直銷業務員",
                    Template = "你是一個很會說話的直銷業務員，請用推銷話術來描述這個主題，要讓人感覺很想購買。",
                    Category = "搞笑",
                    Description = "推銷話術風格"
                },
                new PromptTemplate
                {
                    Name = "恐怖小說作家",
                    Template = "請用恐怖、詭異、令人毛骨悚然的方式來描述這個主題，營造陰森的氛圍。",
                    Category = "黑暗",
                    Description = "恐怖懸疑風格"
                },
                new PromptTemplate
                {
                    Name = "網紅Youtuber",
                    Template = "你是一個年輕的網紅Youtuber，請用流行語、很ㄎㄧㄤ的方式來介紹這個主題，要有網路感。",
                    Category = "搞笑",
                    Description = "網紅風格"
                },
                new PromptTemplate
                {
                    Name = "傲嬌角色",
                    Template = "請用傲嬌的語氣來描述這個主題，要有「才、才不是為了你」的感覺，要口是心非。",
                    Category = "戲劇",
                    Description = "傲嬌二次元風格"
                }
            };
        }

        public List<PromptTemplate> GetAllTemplates()
        {
            return _templates;
        }

        public List<PromptTemplate> GetTemplatesByCategory(string category)
        {
            return _templates.Where(t => t.Category == category).ToList();
        }

        public List<string> GetAllCategories()
        {
            return _templates.Select(t => t.Category).Distinct().OrderBy(c => c).ToList();
        }

        public PromptTemplate? GetTemplateById(string id)
        {
            return _templates.FirstOrDefault(t => t.Id == id);
        }
    }
}
