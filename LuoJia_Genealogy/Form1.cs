using LuoJia_Genealogy.Model;
using LuoJia_Genealogy.BuildPages;
using LuoJia_Genealogy.Data;
using LuoJia_Genealogy.Service;
using LuoJia_Genealogy.PDFGenerator;

namespace LuoJia_Genealogy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (var context = new IndividualDataContext())
                {
                    var geneIds = context.Individuals
                        .Select(i => i.GeneId)
                        .Distinct()
                        .OrderBy(g => g)
                        .ToList();

                    comboBox1.DataSource = geneIds;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载族谱名时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string layout { get; private set; }
        public string font { get; private set; }
        public string color { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    label3.Text = fbd.SelectedPath;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            using (Form2 settingsForm = new Form2())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    layout = settingsForm.SelectedLayout;
                    font = settingsForm.SelectedFont;
                    color = settingsForm.SelectedColor;
                }
            }
        }
        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string geneId = comboBox1.SelectedItem.ToString();
                string outputPath = label3.Text;
                string templatePath = GetTemplatePath(layout, font, color);
                List<string> pages = new List<string>();
                // 创建DbContext实例
                using var context = new IndividualDataContext();
                // 创建服务实例
                var service = new IndividualService(context);
                var individuals = service.GetOrderedGenealogyWithInfo(geneId);

                if (layout == "传统苏式")
                {
                    var builder = new Sushihz_ctBuild();
                    pages = builder.GeneratePages(individuals, templatePath, geneId);
                }
                else if (layout == "现代苏式")
                {
                    var builder = new Sushihz_xdBuild();
                    pages = builder.GeneratePages(individuals, templatePath, geneId);
                }

                // 调用 PDF 生成函数
                string mergedPdfPath = Path.Combine(outputPath, $"{geneId}.pdf");

                var exporter = new PdfExporter();
                List<string> pdfFiles = await exporter.ExportPagesToPdfAsync(pages, outputPath);

                PdfMerger.MergeFiles(pdfFiles, mergedPdfPath);

                MessageBox.Show("PDF 生成完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetTemplatePath(string layout, string font, string color)
        {
            string layoutCode = layout == "传统苏式" ? "ct" : "xd";
            string fontCode = font == "宋体" ? "song" : "kai";
            string colorCode = color == "黑色" ? "black" : "red";

            return $"Template/{colorCode}_{fontCode}_{layoutCode}.html";
        }
    }
}