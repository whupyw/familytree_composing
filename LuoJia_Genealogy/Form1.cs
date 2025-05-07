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
                MessageBox.Show($"����������ʱ��������{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // ����DbContextʵ��
                using var context = new IndividualDataContext();
                // ��������ʵ��
                var service = new IndividualService(context);
                var individuals = service.GetOrderedGenealogyWithInfo(geneId);

                if (layout == "��ͳ��ʽ")
                {
                    var builder = new Sushihz_ctBuild();
                    pages = builder.GeneratePages(individuals, templatePath, geneId);
                }
                else if (layout == "�ִ���ʽ")
                {
                    var builder = new Sushihz_xdBuild();
                    pages = builder.GeneratePages(individuals, templatePath, geneId);
                }

                // ���� PDF ���ɺ���
                string mergedPdfPath = Path.Combine(outputPath, $"{geneId}.pdf");

                var exporter = new PdfExporter();
                List<string> pdfFiles = await exporter.ExportPagesToPdfAsync(pages, outputPath);

                PdfMerger.MergeFiles(pdfFiles, mergedPdfPath);

                MessageBox.Show("PDF ������ɣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetTemplatePath(string layout, string font, string color)
        {
            string layoutCode = layout == "��ͳ��ʽ" ? "ct" : "xd";
            string fontCode = font == "����" ? "song" : "kai";
            string colorCode = color == "��ɫ" ? "black" : "red";

            return $"Template/{colorCode}_{fontCode}_{layoutCode}.html";
        }
    }
}