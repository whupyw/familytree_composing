using LuoJia_Genealogy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuoJia_Genealogy.BuildPages
{
    public class Sushihz_xdBuild
    {
        private const int MaxRowsPerPage = 20;


        public List<string> GeneratePages(List<IndividualWithInfo> individuals, string templatePath, string title)
        {
            var pages = new List<string>();
            var currentPageRows = new List<string>();

            // 按世代排序
            var ordered = individuals
                .Select(i => new
                {
                    Data = i,
                    SortKey = TryParseGeneration(i.Person.Generation)
                })
                .OrderBy(x => x.SortKey)
                .Select(x => x.Data)
                .ToList();

            string currGen = null;
            int rowCount = 0;

            foreach (var ind in ordered)
            {
                // 世代切换
                if (ind.Person.Generation != currGen)
                {
                    if (rowCount + 2 > MaxRowsPerPage)
                        AddPage(pages, currentPageRows, title, rowCount, templatePath, ref rowCount);

                    currGen = ind.Person.Generation;
                    currentPageRows.Add(CreateGenerationRow(currGen));
                    rowCount++;
                }

                // 满行分页
                if (rowCount >= MaxRowsPerPage)
                    AddPage(pages, currentPageRows, title, rowCount, templatePath, ref rowCount);

                currentPageRows.Add(CreatePersonRow(ind));
                rowCount++;
            }

            // 最后一页
            if (currentPageRows.Any())
                AddPage(pages, currentPageRows, title, rowCount, templatePath, ref rowCount);

            return pages;
        }

        private void AddPage(
            List<string> pages,
            List<string> rows,
            string title,
            int rowCountBefore,
            string templatePath,
            ref int rowCount)
        {
            pages.Add(BuildPage(templatePath, title, rows, pages.Count + 1));
            rows.Clear();
            rowCount = 0;
        }

        private string BuildPage(
            string templatePath,
            string title,
            List<string> contentRows,
            int pageNumber)
        {
            var tpl = File.ReadAllText(templatePath);
            return tpl
                .Replace("{{TITLE}}", HtmlEncode(title))
                .Replace("{{CONTENT}}", string.Join("\n", contentRows))
                .Replace("{{PAGE}}", pageNumber.ToString());
        }

        private string CreateGenerationRow(string generation)
        {
            int genNum = int.TryParse(generation, out var n)
                         ? n
                         : TryParseGeneration(generation);
            var cn = NumberToChinese(genNum);
            return $@"<div class=""row generation-row"">
    <div class=""full-col"">第 {HtmlEncode(cn)} 世</div>
</div>";
        }

        private string CreatePersonRow(IndividualWithInfo individual)
        {
            var spouseMark = individual.IsSpouse ? "（配）" : "";
            return $@"<div class=""row"">
    <div class=""left-col"">{HtmlEncode(spouseMark)}{HtmlEncode(individual.Person.Name)}</div>
    <div class=""right-col"">{HtmlEncode(individual.Info)}</div>
</div>";
        }

        private int TryParseGeneration(string generation)
        {
            var dict = new Dictionary<string, int>
            {
                {"一",1}, {"二",2}, {"三",3}, {"四",4}, {"五",5},
                {"六",6}, {"七",7}, {"八",8}, {"九",9}, {"十",10},
                {"十一",11}, {"十二",12}, {"十三",13}, {"十四",14},
                {"十五",15}, {"十六",16}, {"十七",17}, {"十八",18},
                {"十九",19}, {"二十",20}, {"二十一",21}, {"二十二",22},
                {"二十三",23}, {"二十四",24}, {"二十五",25}, {"二十六",26},
                {"二十七",27}, {"二十八",28}, {"二十九",29}, {"三十",30}
            };
            return dict.TryGetValue(generation, out var v) ? v
                 : (int.TryParse(generation, out v) ? v : int.MaxValue);
        }

        private string NumberToChinese(int num)
        {
            if (num <= 0 || num > 30) return num.ToString();
            string[] d = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            if (num < 10) return d[num];
            if (num == 10) return "十";
            if (num < 20) return "十" + d[num % 10];
            if (num % 10 == 0) return d[num / 10] + "十";
            return d[num / 10] + "十" + d[num % 10];
        }

        private string HtmlEncode(string txt)
            => System.Web.HttpUtility.HtmlEncode(txt);
    }
}
