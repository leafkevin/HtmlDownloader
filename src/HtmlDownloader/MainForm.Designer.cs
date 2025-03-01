using System.Drawing;
using System.Windows.Forms;

namespace HtmlRefactor;

partial class TransferForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        txtOutputDir = new TextBox();
        lblSavePath = new Label();
        btnBrowser = new Button();
        btnClose = new Button();
        lblUrl = new Label();
        txtUrl = new TextBox();
        btnDownload = new Button();
        fbdDir = new FolderBrowserDialog();
        SuspendLayout();
        // 
        // txtOutputDir
        // 
        txtOutputDir.Location = new Point(91, 61);
        txtOutputDir.Name = "txtOutputDir";
        txtOutputDir.Size = new Size(467, 23);
        txtOutputDir.TabIndex = 1;
        // 
        // lblSavePath
        // 
        lblSavePath.AutoSize = true;
        lblSavePath.Location = new Point(17, 64);
        lblSavePath.Name = "lblSavePath";
        lblSavePath.Size = new Size(68, 17);
        lblSavePath.TabIndex = 2;
        lblSavePath.Text = "保存路径：";
        // 
        // btnBrowser
        // 
        btnBrowser.Location = new Point(557, 59);
        btnBrowser.Name = "btnBrowser";
        btnBrowser.Size = new Size(32, 26);
        btnBrowser.TabIndex = 3;
        btnBrowser.Text = "...";
        btnBrowser.UseVisualStyleBackColor = true;
        btnBrowser.Click += btnBrowser_Click;
        // 
        // btnClose
        // 
        btnClose.Location = new Point(483, 111);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(75, 28);
        btnClose.TabIndex = 4;
        btnClose.Text = "关闭";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;
        // 
        // lblUrl
        // 
        lblUrl.AutoSize = true;
        lblUrl.Location = new Point(17, 26);
        lblUrl.Name = "lblUrl";
        lblUrl.Size = new Size(68, 17);
        lblUrl.TabIndex = 6;
        lblUrl.Text = "网络地址：";
        // 
        // txtUrl
        // 
        txtUrl.Location = new Point(91, 23);
        txtUrl.Name = "txtUrl";
        txtUrl.Size = new Size(467, 23);
        txtUrl.TabIndex = 5;
        txtUrl.Text = "https://amincods.com/html/flex-it/";
        // 
        // btnDownload
        // 
        btnDownload.Location = new Point(91, 111);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(75, 28);
        btnDownload.TabIndex = 7;
        btnDownload.Text = "开始下载";
        btnDownload.UseVisualStyleBackColor = true;
        btnDownload.Click += btnDownload_Click;
        // 
        // TransferForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(617, 173);
        Controls.Add(btnDownload);
        Controls.Add(lblUrl);
        Controls.Add(txtUrl);
        Controls.Add(btnClose);
        Controls.Add(btnBrowser);
        Controls.Add(lblSavePath);
        Controls.Add(txtOutputDir);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "TransferForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "转换";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private TextBox txtOutputDir;
    private Label lblSavePath;
    private Button btnBrowser;
    private Button btnClose;
    private Label lblUrl;
    private TextBox txtUrl;
    private Button btnDownload;
    private FolderBrowserDialog fbdDir;
}
