using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuoJia_Genealogy.PDFGenerator
{
    public class PdfExporter
    {
        public async Task<List<string>> ExportPagesToPdfAsync(
            List<string> htmlPages,
            string outputDirectory)
        {
            if (htmlPages == null)
                throw new ArgumentNullException(nameof(htmlPages));
            Directory.CreateDirectory(outputDirectory);

            // 下载 Chromium（二次调用会跳过已有下载）
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            // 启动无头浏览器
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

            var outputFiles = new List<string>();

            for (int i = 0; i < htmlPages.Count; i++)
            {
                using var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlPages[i]);

                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20mm",
                        Bottom = "20mm",
                        Left = "15mm",
                        Right = "15mm"
                    }
                };

                var filePath = Path.Combine(outputDirectory, $"page_{i + 1}.pdf");
                await page.PdfAsync(filePath, pdfOptions);

                outputFiles.Add(filePath);
            }

            return outputFiles;
        }
    }
}
