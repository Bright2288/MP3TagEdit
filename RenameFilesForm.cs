using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace MP3Player
{
    public partial class RenameFilesForm : Forms.Form
    {
        private List<MP3AnalyzerForm.MP3FileInfo> filesToRename;
        private Forms.TextBox patternTextBox;
        private Forms.ListBox previewListBox;
        private Forms.Label exampleLabel;
        
        public RenameFilesForm(List<MP3AnalyzerForm.MP3FileInfo> files)
        {
            this.filesToRename = files;
            InitializeComponent();
            UpdatePreview();
        }
        
        private void InitializeComponent()
        {
            this.Text = "批量重命名MP3文件";
            this.Size = new Drawing.Size(800, 600);
            this.StartPosition = Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // 模式输入
            var patternPanel = new Forms.Panel();
            patternPanel.Dock = Forms.DockStyle.Top;
            patternPanel.Height = 120;
            patternPanel.Padding = new Forms.Padding(10);
            
            var patternLabel = new Forms.Label();
            patternLabel.Text = "重命名模式:";
            patternLabel.Location = new Drawing.Point(10, 15);
            patternLabel.AutoSize = true;
            
            patternTextBox = new Forms.TextBox();
            patternTextBox.Location = new Drawing.Point(100, 12);
            patternTextBox.Size = new Drawing.Size(650, 20);
            patternTextBox.Text = "{artist} - {title}";
            patternTextBox.TextChanged += (s, e) => UpdatePreview();
            
            exampleLabel = new Forms.Label();
            exampleLabel.Text = "可用变量: {artist}, {title}, {album}, {year}, {track}, {disc}, {genre}";
            exampleLabel.Location = new Drawing.Point(10, 40);
            exampleLabel.AutoSize = true;
            exampleLabel.ForeColor = Drawing.Color.Gray;
            
            var examplesLabel = new Forms.Label();
            examplesLabel.Text = "示例:\n{artist} - {title}.mp3\n{track:00} - {title}.mp3\n{album}\\{artist} - {title}.mp3";
            examplesLabel.Location = new Drawing.Point(10, 70);
            examplesLabel.AutoSize = true;
            examplesLabel.ForeColor = Drawing.Color.Blue;
            
            patternPanel.Controls.AddRange(new Forms.Control[] { 
                patternLabel, patternTextBox, exampleLabel, examplesLabel 
            });
            
            // 预览列表
            var previewLabel = new Forms.Label();
            previewLabel.Text = "重命名预览:";
            previewLabel.Dock = Forms.DockStyle.Top;
            previewLabel.Height = 30;
            previewLabel.Padding = new Forms.Padding(10, 10, 0, 0);
            
            previewListBox = new Forms.ListBox();
            previewListBox.Dock = Forms.DockStyle.Fill;
            previewListBox.Font = new Drawing.Font("Consolas", 9);
            
            // 按钮面板
            var buttonPanel = new Forms.Panel();
            buttonPanel.Dock = Forms.DockStyle.Bottom;
            buttonPanel.Height = 50;
            buttonPanel.Padding = new Forms.Padding(10);
            
            var renameButton = new Forms.Button();
            renameButton.Text = "执行重命名";
            renameButton.Size = new Drawing.Size(100, 30);
            renameButton.Location = new Drawing.Point(580, 10);
            renameButton.Click += RenameButton_Click;
            
            var cancelButton = new Forms.Button();
            cancelButton.Text = "取消";
            cancelButton.Size = new Drawing.Size(100, 30);
            cancelButton.Location = new Drawing.Point(690, 10);
            cancelButton.Click += (s, e) => this.DialogResult = Forms.DialogResult.Cancel;
            
            buttonPanel.Controls.AddRange(new Forms.Control[] { renameButton, cancelButton });
            
            // 添加到窗体
            this.Controls.AddRange(new Forms.Control[] { 
                previewListBox, previewLabel, patternPanel, buttonPanel 
            });
        }
        
        private void UpdatePreview()
        {
            previewListBox.Items.Clear();
            
            var pattern = patternTextBox.Text;
            if (string.IsNullOrEmpty(pattern))
                return;
            
            foreach (var file in filesToRename)
            {
                try
                {
                    var newName = ApplyPattern(pattern, file);
                    previewListBox.Items.Add($"{file.FileName}  ->  {newName}");
                }
                catch (Exception ex)
                {
                    previewListBox.Items.Add($"{file.FileName}  ->  错误: {ex.Message}");
                }
            }
        }
        
        private string ApplyPattern(string pattern, MP3AnalyzerForm.MP3FileInfo file)
        {
            var result = pattern;
            
            // 替换所有变量
            result = result.Replace("{artist}", CleanFileName(file.Artist));
            result = result.Replace("{title}", CleanFileName(file.Title));
            result = result.Replace("{album}", CleanFileName(file.Album));
            result = result.Replace("{year}", file.Year > 0 ? file.Year.ToString() : "");
            result = result.Replace("{track}", file.Track > 0 ? file.Track.ToString("00") : "");
            result = result.Replace("{disc}", file.Disc > 0 ? file.Disc.ToString() : "");
            result = result.Replace("{genre}", CleanFileName(file.Genre.Split(',')[0]));
            
            // 清理多余的分隔符
            result = Regex.Replace(result, @"\s*-\s*-", " - ");
            result = Regex.Replace(result, @"\s{2,}", " ");
            result = result.Trim();
            
            // 确保有扩展名
            if (!result.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                result += ".mp3";
                
            return result;
        }
        
        private string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Unknown";
                
            // 移除非法文件名字符
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "");
            }
            
            return fileName.Trim();
        }
        
        private void RenameButton_Click(object sender, EventArgs e)
        {
            var result = Forms.MessageBox.Show(
                $"确定要重命名 {filesToRename.Count} 个文件吗？此操作不可撤销。",
                "确认重命名",
                Forms.MessageBoxButtons.YesNo,
                Forms.MessageBoxIcon.Warning);
            
            if (result != Forms.DialogResult.Yes)
                return;
            
            int successCount = 0;
            int failCount = 0;
            var errors = new List<string>();
            
            using (var progressForm = new ProgressForm("重命名文件", filesToRename.Count))
            {
                progressForm.Show();
                
                for (int i = 0; i < filesToRename.Count; i++)
                {
                    var file = filesToRename[i];
                    
                    try
                    {
                        var newName = ApplyPattern(patternTextBox.Text, file);
                        var directory = Path.GetDirectoryName(file.FilePath);
                        var newPath = Path.Combine(directory, newName);
                        
                        // 如果目标文件已存在，添加数字后缀
                        if (File.Exists(newPath))
                        {
                            int counter = 1;
                            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(newName);
                            var extension = Path.GetExtension(newName);
                            
                            while (File.Exists(newPath))
                            {
                                newName = $"{fileNameWithoutExt} ({counter}){extension}";
                                newPath = Path.Combine(directory, newName);
                                counter++;
                            }
                        }
                        
                        File.Move(file.FilePath, newPath);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errors.Add($"{Path.GetFileName(file.FilePath)}: {ex.Message}");
                    }
                    
                    progressForm.UpdateProgress(i + 1, $"正在重命名: {Path.GetFileName(file.FilePath)}");
                    
                    if (progressForm.Cancelled)
                        break;
                }
                
                progressForm.Close();
            }
            
            if (errors.Count > 0)
            {
                var errorMessage = $"重命名完成\n\n成功: {successCount} 个\n失败: {failCount} 个\n\n错误列表:\n{string.Join("\n", errors.Take(10))}";
                if (errors.Count > 10)
                    errorMessage += $"\n... 还有 {errors.Count - 10} 个错误";
                    
                Forms.MessageBox.Show(errorMessage, "重命名结果", 
                    Forms.MessageBoxButtons.OK, failCount > 0 ? Forms.MessageBoxIcon.Warning : Forms.MessageBoxIcon.Information);
            }
            else
            {
                Forms.MessageBox.Show($"成功重命名 {successCount} 个文件", "完成", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
            }
            
            this.DialogResult = Forms.DialogResult.OK;
        }
    }
}