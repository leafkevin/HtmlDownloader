using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HtmlRefactor;

class Downloader
{
    public async Task Download(string url, string outputDir)
    {
        using var client = new HttpClient();
        var msgResp = await client.GetAsync(url);
        msgResp.EnsureSuccessStatusCode();
        var codeScripts = await msgResp.Content.ReadAsStringAsync();
        //var codeScripts = await File.ReadAllTextAsync("C:\\Users\\leafkevin\\Desktop\\index.html");
        var formater = new HtmlFormater();
        codeScripts = formater.Transfer(codeScripts);
        var destPath = Path.Combine(outputDir, "index.html");
        using var writerStream = File.OpenWrite(destPath);
        using var writer = new StreamWriter(writerStream);
        writer.Write(codeScripts);
        writer.Flush();
        writerStream.Flush();
        writer.Close();
        writerStream.Close();

        //TODO:分析页面内容，下载所有引用资源文件
    }
}
