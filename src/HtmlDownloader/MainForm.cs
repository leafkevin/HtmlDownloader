using System;
using System.IO;
using System.Windows.Forms;

namespace HtmlRefactor;

public partial class TransferForm : Form
{
    public TransferForm()
    {
        InitializeComponent();
        this.fbdDir.InitialDirectory = Directory.GetCurrentDirectory();
        this.txtOutputDir.Text = this.fbdDir.InitialDirectory;
    }

    //private void btnOk_Click(object sender, EventArgs e)
    //{
    //    var filePath = this.txtOutputDir.Text.Trim();
    //    if (string.IsNullOrEmpty(filePath))
    //    {
    //        MessageBox.Show("转换文件路径不能为空");
    //        this.txtOutputDir.Focus();
    //        return;
    //    }
    //    var formater = new HtmlFormater();
    //    var files = Directory.GetFiles("E:\\Templates", "*.html");
    //    Array.ForEach(files, f => formater.Transfer(f));
    //    MessageBox.Show("转换完成！");
    //}
    private void btnBrowser_Click(object sender, EventArgs e)
    {
        if (this.fbdDir.ShowDialog() == DialogResult.OK)
            this.txtOutputDir.Text = this.fbdDir.SelectedPath;
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private async void btnDownload_Click(object sender, EventArgs e)
    {
        var url = this.txtUrl.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("网络地址不能为空");
            this.txtUrl.Focus();
            return;
        }
        var outputDir = this.txtOutputDir.Text.Trim();
        if (string.IsNullOrEmpty(outputDir))
        {
            MessageBox.Show("转换文件路径不能为空");
            this.txtOutputDir.Focus();
            return;
        }
        this.lbList.Items.Clear();
        var downloader = new Downloader();
        await downloader.Download(url, outputDir, (mssage, completedText) => this.Invoke(() =>
        {
            this.lbList.Items.Add(mssage);
            this.lblProcess.Text = completedText;
        }));
    }
}