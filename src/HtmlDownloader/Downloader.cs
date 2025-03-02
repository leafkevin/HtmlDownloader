using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HtmlRefactor;

class Downloader
{
    public async Task Download(string url, string outputDir, Action<string, string> completedCallback)
    {
        var filePath = Path.Combine(outputDir, "index.html");
        (_, var resourceUrls) = await this.DownloadFile(true, url, url, filePath);
        resourceUrls = resourceUrls.FindAll(f => f.Trim('/') != url.Trim('/'));
        int total = resourceUrls.Count + 1;
        var completedText = $"下载进度：1/{total}";
        completedCallback.Invoke($"下载完成： {url}", completedText);
        completedCallback.Invoke("开始下载引用资源文件", completedText);

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
        var rootUrl = url;
        var newResouceUrls = new List<string>();
        foreach (var resourceUrl in resourceUrls)
        {
            _ = Task.Run(async () =>
            {
                (var isKnown, var savePath) = this.GetSavePath(rootPath, resourceUrl, outputDir);
                (var message, var myResouceUrls) = await this.DownloadFile(isKnown, rootUrl, resourceUrl, savePath);
                if (isKnown && myResouceUrls != null)
                {
                    lock (newResouceUrls)
                    {
                        myResouceUrls.ForEach(f =>
                        {
                            if (!newResouceUrls.Contains(f))
                                newResouceUrls.Add(f);
                        });
                    }
                }
                var completedCount = Interlocked.Increment(ref completed);
                completedCallback.Invoke(message, $"下载进度：{completedCount + 1}/{total}");
                if (completedCount == resourceUrls.Count)
                {
                    completedCallback.Invoke("引用资源文件下载完成", $"下载完成：{completedCount + 1}/{total}");

                    newResouceUrls = newResouceUrls.Except(resourceUrls)
                        .Except([url, url.TrimEnd('/')]).Distinct().ToList();
                    newResouceUrls = newResouceUrls.FindAll(f => !f.Contains("data:image/svg+xml") && !f.Contains("data:application/"));
                    completed = 0;
                    total = newResouceUrls.Count;
                    completedCallback.Invoke($"新增引引用资源文件: {total}", $"新增文件下载完成：0/{total}");
                    foreach (var newResourceUrl in newResouceUrls)
                    {
                        _ = Task.Run(async () =>
                        {
                            (isKnown, savePath) = this.GetSavePath(rootPath, newResourceUrl, outputDir);
                            (message, myResouceUrls) = await this.DownloadFile(isKnown, rootUrl, newResourceUrl, savePath);
                            //不再下探资源了
                            completedCount = Interlocked.Increment(ref completed);
                            completedCallback.Invoke(message, $"新增文件下载进度：{completedCount}/{total}");
                            if (completedCount == total)
                                completedCallback.Invoke("新增引用资源文件下载完成", $"新增文件下载完成：{completedCount}/{total}");
                        });
                    }
                }
            });
        }
    }
    public async Task<(string, List<string>)> DownloadFile(bool isKnown, string rootUrl, string fileUrl, string filePath)
    {
        var message = $"下载完成：{fileUrl}";
        List<string> resourceUrls = null;
        using var client = new HttpClient();
        HttpResponseMessage msgResp = null;
        try
        {
            msgResp = await client.GetAsync(fileUrl);
            msgResp.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"下载文件{fileUrl}失败，错误信息：{ex.Message}");
            return ($"下载失败：{fileUrl}", resourceUrls);
        }
        var fileType = this.GetFileType(filePath, out var extName);
        if (fileType == FileType.Text)
        {
            var codeScripts = await msgResp.Content.ReadAsStringAsync();
            if (isKnown && extName == ".html" || extName == ".css")
            {
                var formater = new HtmlFormater();
                string url = fileUrl.Substring(0, fileUrl.LastIndexOf('/'));
                if (extName == ".html")
                    codeScripts = formater.Transfer(codeScripts);
                resourceUrls = formater.GetRefResourceUrls(url, codeScripts);
            }
            using var writerStream = File.OpenWrite(filePath);
            using var writer = new StreamWriter(writerStream);
            writer.Write(codeScripts);
            writer.Flush();
            writerStream.Flush();
            writer.Close();
            writerStream.Close();
        }
        else
        {
            using var writerStream = File.OpenWrite(filePath);
            await msgResp.Content.CopyToAsync(writerStream);
            writerStream.Flush();
            writerStream.Close();
        }
        return (message, resourceUrls);
    }
    private FileType GetFileType(string filePath, out string extName)
    {
        extName = Path.GetExtension(filePath);
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
                return FileType.Stream;
            case ".css":
            case ".js":
            case ".html":
            default: return FileType.Text;
        }
    }
    private (bool, string) GetSavePath(string rootPath, string resourceUrl, string outputRootPath)
    {
        var filePath = outputRootPath;
        string relativePath = null;
        var index = resourceUrl.IndexOf(rootPath);
        if (index < 0)
        {
            //以http: https:开头的网址
            index = resourceUrl.IndexOf("//");
            relativePath = resourceUrl.Substring(index + 2);
            index = relativePath.IndexOf("?");
            if (index > 0) relativePath = relativePath.Substring(0, index);
            relativePath = relativePath.TrimEnd('/');
            index = relativePath.LastIndexOf('/');
            if (index > 0)
            {
                filePath = relativePath.Substring(index + 1);
                index = filePath.IndexOf('.');
                if (index < 0) filePath += ".html";
            }
            else filePath = relativePath + ".html";
            return (false, filePath);
        }
        index = index + rootPath.Length;
        relativePath = resourceUrl.Substring(index).TrimStart('/');
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
        return (true, filePath);
    }
    enum FileType
    {
        Text,
        Stream
    }
}
