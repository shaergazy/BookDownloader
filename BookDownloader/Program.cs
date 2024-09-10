using HtmlAgilityPack;
using QuickEPUB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var doc = new Epub("Book Title", "Author Name");

        //doc.AddCssFile("styles.css", @"
        //    .take_h1 { font-size: 24px; font-weight: bold; margin-top: 20px; }
        //    .MsoNormal { margin: 15px; text-align: left; width: 800px; color: #333333; }
        //    .em { font-style: italic; }
        //");

        string content = "";
        string currentTitle = "Introduction";
        StringBuilder sectionContent = new StringBuilder();

        for (int i = 1; i < 32; i++)
        {
            string url = $"http://loveread.ec/read_book.php?id=37343&p={i}";

            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(url);

            // Извлечение стилей и изображений
            var styleNodes = htmlDoc.DocumentNode.SelectNodes("//style");
            if (styleNodes != null)
            {
                foreach (var styleNode in styleNodes)
                {
                    using (var fs = new MemoryStream())
                    {
                        doc.Export(fs);
                        var cssContent = styleNode.InnerText;
                        doc.AddResource("C:\\Users\\malikov\\Pictures\\Saved Pictures\\Posters\\styles.css", EpubResourceType.CSS, fs);
                    }
                }
            }

            // Обработка текста
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='MsoNormal']|//div[@class='take_h1']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasClass("take_h1"))
                    {
                        // Сохраняем текущий раздел
                        if (sectionContent.Length > 0)
                        {
                            doc.AddSection(currentTitle, sectionContent.ToString(), "styles.css");
                            sectionContent.Clear();
                        }

                        // Устанавливаем новый заголовок
                        currentTitle = node.InnerText.Trim();
                    }
                    else
                    {
                        // Собираем контент
                        sectionContent.Append(node.OuterHtml);
                    }
                }
            }
            else
            {
                Console.WriteLine("Элементы <div> не найдены.");
            }
        }

        // Добавляем последний раздел
        if (sectionContent.Length > 0)
        {
            doc.AddSection(currentTitle, sectionContent.ToString(), "styles.css");
        }

        // Экспортируем в EPUB файл
        string outputFilePath = "C:\\Users\\malikov\\Pictures\\Saved Pictures\\Posters\\outputSample.epub";

        using (var fs = new FileStream(outputFilePath, FileMode.Create))
        {
            doc.Export(fs);
        }
    }
}
