using LuoJia_Genealogy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuoJia_Genealogy.BuildPages
{
    public class Sushihz_ctBuild
    {
        private const int COLUMNS_PER_PAGE = 16;

        public Sushihz_ctBuild()
        {
        }

        public List<string> GeneratePages(
            List<IndividualWithInfo> members,
            string templatePath,
            string title)
        {
            var pages = new List<string>();
            var cols = new List<string>();
            int currentGen = -1;
            int pageNo = 1;

            foreach (var m in members)
            {
                // 世代切换：数字字符串解析，若不相同则新增世代列
                if (int.TryParse(m.Person.Generation, out int gen) && gen != currentGen)
                {
                    currentGen = gen;
                    AddGenerationColumn(cols, gen);
                }
                AddMemberColumns(cols, m);

                // 达到每页列数上限则分页
                if (cols.Count >= COLUMNS_PER_PAGE)
                {
                    pages.Add(CreatePage(cols, pageNo++, templatePath, title));
                    cols.Clear();
                }
            }

            // 处理最后一页剩余列
            if (cols.Any())
            {
                pages.Add(CreatePage(cols, pageNo, templatePath, title));
            }

            return pages;
        }

        private void AddGenerationColumn(List<string> cols, int gen)
        {
            string text = $"第{ConvertToChinese(gen)}世";
            cols.Add($@"
<div class='column generation-column'>
  <div class='column-inner'>{HtmlEncode(text)}</div>
</div>");
        }

        private void AddMemberColumns(List<string> cols, IndividualWithInfo m)
        {
            string rel = m.IsSpouse ? "<span class='relation'>配</span>" : "";
            cols.Add($@"
<div class='column'>
  <div class='column-inner'>{rel}<span class='member-name'>{HtmlEncode(m.Person.Name)}</span></div>
</div>");
            cols.Add($@"
<div class='column'>
  <div class='column-inner'>{HtmlEncode(m.Info)}</div>
</div>");
        }

        private string CreatePage(
            List<string> cols,
            int pageNo,
            string templatePath,
            string title)
        {
            string tpl = File.ReadAllText(templatePath);

            string content = string.Join("\n", cols.AsEnumerable().Reverse());

            return tpl
                .Replace("{{GENE_NAME}}", HtmlEncode(title))
                .Replace("{{COLUMNS_CONTENT}}", content)
                .Replace("{{PAGE_NUMBER}}", pageNo.ToString());
        }

        private string ConvertToChinese(int num)
        {
            if (num <= 0 || num > 30)
                return num.ToString();

            string[] digs = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            if (num < 10) return digs[num];
            if (num == 10) return "十";
            if (num < 20) return "十" + digs[num % 10];
            if (num % 10 == 0) return digs[num / 10] + "十";
            return digs[num / 10] + "十" + digs[num % 10];
        }

        private string HtmlEncode(string s) =>
            System.Web.HttpUtility.HtmlEncode(s);
    }
}
