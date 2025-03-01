using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HtmlRefactor;

class Downloader
{
    public async Task Download(string url, string outputDir)
    {
        var filePath = Path.Combine(outputDir, "index.html");
        (_, var resourceUrls) = await this.DownloadFile(url, filePath, true);
        //TODO:分析页面内容，下载所有引用资源文件
        int completed = 0;
        var rootPath = url;
        if (resourceUrls != null && resourceUrls.Count > 0)
        {
            foreach (var resourceUrl in resourceUrls)
            {
                if (resourceUrl.StartsWith(rootPath) || resourceUrl.StartsWith("http:")
                    || resourceUrl.StartsWith("https:")) continue;

                while (rootPath.Length > 0)
                {
                    rootPath = rootPath.Substring(0, rootPath.LastIndexOf('/'));
                    if (resourceUrl.StartsWith(rootPath))
                        break;
                }
            }
        }
        foreach (var resourceUrl in resourceUrls)
        {
            if (resourceUrl == url) continue;
            //_ = Task.Run(async () =>
            //{
            var savePath = this.GetSavePath(rootPath, resourceUrl, outputDir);
            if (string.IsNullOrEmpty(savePath)) continue;
            await this.DownloadFile(resourceUrl, savePath);
            Interlocked.Increment(ref completed);
            //});
        }
        SpinWait.SpinUntil(() => completed == resourceUrls.Count);
    }
    public async Task<(string, List<string>)> DownloadFile(string url, string filePath, bool isFormat = false)
    {
        using var client = new HttpClient();
        var msgResp = await client.GetAsync(url);
        msgResp.EnsureSuccessStatusCode();
        this.GetFileName(url, out var isTxtFile, out var isStreamFile);
        if (isTxtFile)
        {
            var codeScripts = await msgResp.Content.ReadAsStringAsync();
            //var codeScripts = await File.ReadAllTextAsync("C:\\Users\\leafkevin\\Desktop\\index.html");
            List<string> resourceUrls = null;
            if (isFormat)
            {
                var formater = new HtmlFormater();
                (codeScripts, resourceUrls) = formater.Transfer(url, codeScripts);
            }
            using var writerStream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(writerStream);
            writer.Write(codeScripts);
            writer.Flush();
            writerStream.Flush();
            writer.Close();
            writerStream.Close();
            return (codeScripts, resourceUrls);
        }
        else if (isStreamFile)
        {
            using var writerStream = File.OpenWrite(filePath);
            await msgResp.Content.CopyToAsync(writerStream);
            writerStream.Flush();
            writerStream.Close();
        }
        return (null, null);
    }
    public string GetFileName(string url, out bool isTxtFile, out bool isStreamFile)
    {
        var extName = ".html";
        isTxtFile = false;
        isStreamFile = false;
        var result = "index.html";
        var index = url.LastIndexOf("/");
        if (index > 0)
        {
            var fileName = url.Substring(index).TrimEnd('/');
            if (!string.IsNullOrEmpty(fileName))
            {
                result = fileName;
                index = fileName.LastIndexOf('.');
                extName = fileName.Substring(index);
            }
        }
        switch (extName)
        {
            case ".png":
            case ".jpg":
            case ".gif":
            case ".svg":
            case ".woff":
            case ".woff2":
            case ".ttf":
            case ".eot":
            case ".otf":
            case ".ico":
            case ".mp4":
            case ".mp3":
            case ".wav":
            case ".avi":
            case ".mov":
            case ".flv":
            case ".swf":
                isStreamFile = true;
                break;
            case ".css":
            case ".js":
            case ".html":
                isTxtFile = true;
                break;
            default:
                break;
        }
        return result;
    }
    private string GetSavePath(string rootPath, string resourceUrl, string outputRootPath)
    {
        var index = resourceUrl.IndexOf(rootPath);
        if (index < 0)
        {
            return null;
            //var endIndex = resourceUrl.IndexOf('/');
            //if (endIndex < 0) return null;
            //endIndex = resourceUrl.IndexOf('.', endIndex + 1);
            //if (endIndex < 0) return null;
        }
        index = index + rootPath.Length;
        var relativePath = resourceUrl.Substring(index).TrimStart('/');
        var filePath = outputRootPath;
        while (relativePath.Length > 0)
        {
            var endIndex = relativePath.IndexOf('/');
            if (endIndex < 0)
            {
                if (relativePath.Length > 0)
                    filePath = Path.Combine(filePath, relativePath);
                break;
            }
            filePath = Path.Combine(filePath, relativePath.Substring(0, endIndex));
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            relativePath = relativePath.Substring(endIndex + 1);
        }
        return filePath;
    }
}
