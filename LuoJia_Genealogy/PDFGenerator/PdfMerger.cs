using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace LuoJia_Genealogy.PDFGenerator
{
    public static class PdfMerger
    {
        public static void MergeFiles(IEnumerable<string> sourceFiles, string destinationFile)
        {
            if (sourceFiles == null)
                throw new ArgumentNullException(nameof(sourceFiles));

            using var outputDoc = new PdfDocument();

            foreach (var file in sourceFiles)
            {
                if (!File.Exists(file))
                    throw new FileNotFoundException($"找不到文件: {file}");
                using var inputDoc = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                for (int idx = 0; idx < inputDoc.PageCount; idx++)
                {
                    outputDoc.AddPage(inputDoc.Pages[idx]);
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            outputDoc.Save(destinationFile);
            foreach (var file in sourceFiles)
            {
                if (file != destinationFile)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        // 处理删除失败的情况
                        Console.WriteLine($"删除文件 {file} 时出错: {ex.Message}");
                    }
                }
            }
        }
    }
}
