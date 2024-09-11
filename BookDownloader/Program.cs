using HtmlAgilityPack;
using QuickEPUB;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Регистрация кодировок
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Создаем EPUB книгу
        var doc = new Epub("Book Title", "Author Name");

        string currentTitle = "Introduction"; // Начальный заголовок
        StringBuilder sectionContent = new StringBuilder(); // Хранение контента секции
        doc.AddSection("Hello", "<h1>Made by Shergazy</h1>");

        // Проходим по всем страницам книги
        for (int i = 1; i < 32; i++)
        {
            string url = $"http://loveread.ec/read_book.php?id=37343&p={i}";

            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(url);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='MsoNormal']|//div[@class='take_h1']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasClass("take_h1"))
                    {
                        if (sectionContent.Length > 0)
                        {
                            doc.AddSection(currentTitle, sectionContent.ToString());
                            sectionContent.Clear();
                        }

                        currentTitle = node.InnerText.Trim();
                    }
                    else if (doc.Sections.Count() == 0)
                    {
                        doc.AddSection(currentTitle, node.OuterHtml);
                    }
                    else
                    {
                        sectionContent.Append(node.OuterHtml);
                    }
                }
            }
            else
            {
                Console.WriteLine("Элементы <div> не найдены.");
            }
        }

        // Добавляем последнюю секцию, если есть контент
        if (sectionContent.Length > 0)
        {
            doc.AddSection(currentTitle, sectionContent.ToString());
        }

        // Путь для сохранения EPUB файла
        string outputFilePath = "C:\\Users\\malikov\\Pictures\\Saved Pictures\\Posters\\outputSample22.epub";

        // Экспортируем книгу в EPUB
        using (var fs = new FileStream(outputFilePath, FileMode.Create))
        {
            doc.Export(fs);
        }

        Console.WriteLine("Книга успешно сохранена в EPUB формате.");
    }
}
