using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using SystemIO = System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Linq;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using TagLib;

namespace MP3Player
{
    public partial class MP3AnalyzerForm : Forms.Form
    {
        private List<MP3FileInfo> mp3Files = new List<MP3FileInfo>();
        private DataTable analysisData = new DataTable();
        //private CloudApiService _cloudService = new CloudApiService();
        private EncodingHelper _encodingHelper = new EncodingHelper();
        
        // 控件声明
        private Forms.DataGridView dataGridView;
        private Forms.TextBox filterTextBox;
        private Forms.Label statsLabel;
        private Forms.Label durationLabel;
        private Forms.Label sizeLabel;
        private Forms.ToolStripStatusLabel statusLabel;
        
        public MP3AnalyzerForm()
        {
            InitializeComponent();
            InitializeDataTable();
        }
        
        private void InitializeComponent()
        {
            this.Text = "MP3元数据分析工具";
            this.Size = new Drawing.Size(1200, 700);
            this.StartPosition = Forms.FormStartPosition.CenterParent;
            
            // 工具栏
            var toolStrip = new Forms.ToolStrip();
            
            // 文件操作按钮
            var loadFolderButton = new Forms.ToolStripButton("📁 加载文件夹");
            loadFolderButton.Click += LoadFolderButton_Click;
            
            var loadFilesButton = new Forms.ToolStripButton("📄 加载文件");
            loadFilesButton.Click += LoadFilesButton_Click;
            
            // 分析按钮
            var analyzeButton = new Forms.ToolStripButton("📊 分析");
            analyzeButton.Click += AnalyzeButton_Click;
            
            // 导出按钮
            var exportCsvButton = new Forms.ToolStripButton("📤 导出CSV");
            exportCsvButton.Click += ExportCsvButton_Click;
            
            // 重复检测
            var findDuplicatesButton = new Forms.ToolStripButton("🔍 查找重复");
            findDuplicatesButton.Click += FindDuplicatesButton_Click;
            
            // 编码修复按钮
            var fixEncodingButton = new Forms.ToolStripButton("🔧 修复乱码");
            fixEncodingButton.Click += FixEncodingButton_Click;
            
            // 元数据补全
            //var autoCompleteButton = new Forms.ToolStripButton("⚙️ 自动补全");
            //autoCompleteButton.Click += AutoCompleteButton_Click;
            
            // 重命名
            var renameFilesButton = new Forms.ToolStripButton("✏️ 重命名文件");
            renameFilesButton.Click += RenameFilesButton_Click;
            
            // 云分析按钮
            //var cloudAnalyzeButton = new Forms.ToolStripButton("☁️ 云宝分析");
            //cloudAnalyzeButton.Click += CloudAnalyzeButton_Click;
            
            // 智能分析按钮
            //var smartAnalyzeButton = new Forms.ToolStripButton("🤖 智能分析");
            //smartAnalyzeButton.Click += SmartAnalyzeButton_Click;
            
            // 测试修复按钮
            var testFixButton = new Forms.ToolStripButton("🧪 测试修复");
            testFixButton.Click += TestFixButton_Click;

            var specificGBKFixButton = new Forms.ToolStripButton("🎯 修复GBK双重编码");
            specificGBKFixButton.Click += SpecificGBKFixButton_Click;
            specificGBKFixButton.ToolTipText = "专门修复'鐜嬭彶'等GBK双重编码";

            var deepAnalyzeButton = new Forms.ToolStripButton("🔬 深度分析");
            deepAnalyzeButton.Click += DeepAnalyzeButton_Click;
            deepAnalyzeButton.ToolTipText = "深度分析文件编码";

            // 添加验证按钮
            var verifyButton = new Forms.ToolStripButton("✅ 验证修复");
            verifyButton.Click += VerifyButton_Click;
            verifyButton.ToolTipText = "验证修复结果";
            
            toolStrip.Items.AddRange(new Forms.ToolStripItem[] {
                loadFolderButton, loadFilesButton, 
                new Forms.ToolStripSeparator(),
                analyzeButton, exportCsvButton, findDuplicatesButton,
                new Forms.ToolStripSeparator(),
                fixEncodingButton, renameFilesButton,
                //fixEncodingButton, autoCompleteButton, renameFilesButton,
                new Forms.ToolStripSeparator(),
                //cloudAnalyzeButton, smartAnalyzeButton, testFixButton,
                testFixButton,
                deepAnalyzeButton,specificGBKFixButton, verifyButton
            });
            
            // 统计面板
            var statsPanel = new Forms.Panel();
            statsPanel.Dock = Forms.DockStyle.Top;
            statsPanel.Height = 80;
            statsPanel.BackColor = Drawing.Color.LightGray;
            statsPanel.Padding = new Forms.Padding(10);
            
            statsLabel = new Forms.Label();
            statsLabel.Text = "已加载: 0 个文件";
            statsLabel.Location = new Drawing.Point(20, 20);
            statsLabel.AutoSize = true;
            statsLabel.Font = new Drawing.Font("微软雅黑", 9, Drawing.FontStyle.Bold);
            
            durationLabel = new Forms.Label();
            durationLabel.Text = "总时长: 00:00:00";
            durationLabel.Location = new Drawing.Point(200, 20);
            durationLabel.AutoSize = true;
            durationLabel.Font = new Drawing.Font("微软雅黑", 9);
            
            sizeLabel = new Forms.Label();
            sizeLabel.Text = "总大小: 0 MB";
            sizeLabel.Location = new Drawing.Point(400, 20);
            sizeLabel.AutoSize = true;
            sizeLabel.Font = new Drawing.Font("微软雅黑", 9);
            
            var missingInfoLabel = new Forms.Label();
            missingInfoLabel.Text = "缺失元数据: 0 个";
            missingInfoLabel.Location = new Drawing.Point(600, 20);
            missingInfoLabel.AutoSize = true;
            missingInfoLabel.Font = new Drawing.Font("微软雅黑", 9, Drawing.FontStyle.Bold);
            missingInfoLabel.ForeColor = Drawing.Color.Red;
            missingInfoLabel.Name = "missingInfoLabel";
            
            statsPanel.Controls.AddRange(new Forms.Control[] { 
                statsLabel, durationLabel, sizeLabel, missingInfoLabel 
            });
            
            // 数据表格
            dataGridView = new Forms.DataGridView();
            dataGridView.Dock = Forms.DockStyle.Fill;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = false;
            dataGridView.SelectionMode = Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView.AutoSizeColumnsMode = Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.CellContentClick += DataGridView_CellContentClick;
            
            // 过滤面板
            var filterPanel = new Forms.Panel();
            filterPanel.Dock = Forms.DockStyle.Bottom;
            filterPanel.Height = 40;
            filterPanel.BackColor = Drawing.Color.FromArgb(240, 240, 240);
            filterPanel.Padding = new Forms.Padding(10, 5, 10, 5);
            
            var filterLabel = new Forms.Label();
            filterLabel.Text = "过滤:";
            filterLabel.Location = new Drawing.Point(10, 10);
            filterLabel.AutoSize = true;
            
            filterTextBox = new Forms.TextBox();
            filterTextBox.Location = new Drawing.Point(60, 7);
            filterTextBox.Size = new Drawing.Size(200, 20);
            filterTextBox.TextChanged += FilterTextBox_TextChanged;
            
            var filterColumnCombo = new Forms.ComboBox();
            filterColumnCombo.Location = new Drawing.Point(270, 7);
            filterColumnCombo.Size = new Drawing.Size(150, 20);
            filterColumnCombo.Items.AddRange(new string[] {
                "所有列", "文件名", "标题", "艺术家", "专辑", "年份", "流派"
            });
            filterColumnCombo.SelectedIndex = 0;
            
            filterPanel.Controls.AddRange(new Forms.Control[] { 
                filterLabel, filterTextBox, filterColumnCombo 
            });
            
            // 底部状态栏
            var statusStrip = new Forms.StatusStrip();
            statusLabel = new Forms.ToolStripStatusLabel("就绪");
            statusStrip.Items.Add(statusLabel);
            statusStrip.Dock = Forms.DockStyle.Bottom;
            
            // 添加到窗体
            this.Controls.AddRange(new Forms.Control[] { 
                dataGridView, filterPanel, statsPanel, toolStrip, statusStrip 
            });
        }
        
        private void InitializeDataTable()
        {
            // 添加复选框列
            analysisData.Columns.Add("选择", typeof(bool));
            analysisData.Columns.Add("文件名", typeof(string));
            analysisData.Columns.Add("路径", typeof(string));
            analysisData.Columns.Add("标题", typeof(string));
            analysisData.Columns.Add("艺术家", typeof(string));
            analysisData.Columns.Add("专辑", typeof(string));
            analysisData.Columns.Add("年份", typeof(int));
            analysisData.Columns.Add("音轨号", typeof(int));
            analysisData.Columns.Add("碟片号", typeof(int));
            analysisData.Columns.Add("流派", typeof(string));
            analysisData.Columns.Add("时长", typeof(string));
            analysisData.Columns.Add("比特率", typeof(int));
            analysisData.Columns.Add("采样率", typeof(int));
            analysisData.Columns.Add("大小(MB)", typeof(double));
            analysisData.Columns.Add("修改时间", typeof(DateTime));
            analysisData.Columns.Add("是否有封面", typeof(bool));
            analysisData.Columns.Add("作曲者", typeof(string));
            analysisData.Columns.Add("备注", typeof(string));
            analysisData.Columns.Add("编码质量", typeof(string));
            
            dataGridView.DataSource = analysisData;
            
            // 设置列宽
            if (dataGridView.Columns["选择"] != null)
                dataGridView.Columns["选择"].Width = 50;
        }
        
        // ==================== 文件加载和分析 ====================
        
        private void LoadFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new Forms.FolderBrowserDialog())
            {
                folderDialog.Description = "选择包含MP3文件的文件夹";
                if (folderDialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    LoadMP3FilesFromFolder(folderDialog.SelectedPath);
                }
            }
        }
        
        private void LoadFilesButton_Click(object sender, EventArgs e)
        {
            using (var openDialog = new Forms.OpenFileDialog())
            {
                openDialog.Filter = "MP3文件|*.mp3|所有文件|*.*";
                openDialog.Multiselect = true;
                openDialog.Title = "选择MP3文件";
                
                if (openDialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    LoadMP3Files(openDialog.FileNames);
                }
            }
        }
        
        private void LoadMP3FilesFromFolder(string folderPath)
        {
            try
            {
                var mp3Files = SystemIO.Directory.GetFiles(folderPath, "*.mp3", SystemIO.SearchOption.AllDirectories);
                LoadMP3Files(mp3Files);
            }
            catch (Exception ex)
            {
                Forms.MessageBox.Show($"加载文件夹失败: {ex.Message}", "错误", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
            }
        }
        
        private void LoadMP3Files(string[] filePaths)
        {
            mp3Files.Clear();
            analysisData.Rows.Clear();
            
            int processed = 0;
            int total = filePaths.Length;
            
            using (var progressForm = new ProgressForm("加载MP3文件", total))
            {
                progressForm.Show();
                
                foreach (var filePath in filePaths)
                {
                    try
                    {
                        var fileInfo = AnalyzeMP3File(filePath);
                        mp3Files.Add(fileInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"无法分析文件 {filePath}: {ex.Message}");
                    }
                    
                    processed++;
                    progressForm.UpdateProgress(processed, $"正在分析: {SystemIO.Path.GetFileName(filePath)}");
                    
                    if (progressForm.Cancelled)
                        break;
                }
                
                progressForm.Close();
            }
            
            UpdateStats();
            PopulateDataGrid();
            statusLabel.Text = $"已加载 {mp3Files.Count} 个MP3文件";
        }
        
        private MP3FileInfo AnalyzeMP3File(string filePath)
        {
            var fileInfo = new SystemIO.FileInfo(filePath);
            var mp3Info = new MP3FileInfo
            {
                FilePath = filePath,
                FileName = SystemIO.Path.GetFileName(filePath),
                FileSizeMB = Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2),
                ModifiedTime = fileInfo.LastWriteTime
            };
            
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    // 修改点1：直接读取，不进行额外编码修复
                    mp3Info.Title = file.Tag.Title ?? SystemIO.Path.GetFileNameWithoutExtension(filePath);
                    mp3Info.Artist = file.Tag.FirstPerformer ?? "未知艺术家";
                    mp3Info.Album = file.Tag.Album ?? "未知专辑";
                    
                    // 修改点2：使用正确的方法读取其他字段
                    mp3Info.Year = (int)file.Tag.Year;
                    mp3Info.Track = (int)file.Tag.Track;
                    mp3Info.Disc = (int)file.Tag.Disc;
                    mp3Info.Genre = string.Join(", ", file.Tag.Genres) ?? string.Empty;
                    mp3Info.Composer = file.Tag.FirstComposer ?? string.Empty;
                    mp3Info.Comment = file.Tag.Comment ?? string.Empty;
                    
                    // 修改点3：移除编码质量评估，因为文件已经是正确的
                    mp3Info.EncodingQuality = "正确";
                    
                    // 技术信息
                    mp3Info.Duration = file.Properties.Duration.ToString(@"hh\:mm\:ss");
                    mp3Info.Bitrate = file.Properties.AudioBitrate;
                    mp3Info.SampleRate = file.Properties.AudioSampleRate;
                    mp3Info.Channels = file.Properties.AudioChannels;
                    mp3Info.HasCoverArt = file.Tag.Pictures.Length > 0;
                    
                    mp3Info.CalculateMissingScore();
                }
            }
            catch
            {
                mp3Info.Title = SystemIO.Path.GetFileNameWithoutExtension(filePath);
                mp3Info.EncodingQuality = "读取错误";
            }
            
            return mp3Info;
        }
        
        private string EvaluateEncodingQuality(string title, string artist, string album)
        {
            bool titleValid = _encodingHelper.IsValidChineseText(title);
            bool artistValid = _encodingHelper.IsValidChineseText(artist);
            bool albumValid = _encodingHelper.IsValidChineseText(album);
            
            if (titleValid && artistValid && albumValid)
                return "良好";
            else if (titleValid || artistValid || albumValid)
                return "部分良好";
            else
                return "需修复";
        }
        
        private void PopulateDataGrid()
        {
            analysisData.Rows.Clear();
            
            foreach (var mp3 in mp3Files)
            {
                // 在填充前应用显示修复
                string displayTitle = FixDisplayEncoding(mp3.Title);
                string displayArtist = FixDisplayEncoding(mp3.Artist);
                string displayAlbum = FixDisplayEncoding(mp3.Album);
                string displayGenre = FixDisplayEncoding(mp3.Genre);
                string displayComposer = FixDisplayEncoding(mp3.Composer);
                string displayComment = FixDisplayEncoding(mp3.Comment);
                
                analysisData.Rows.Add(
                    false,  // 选择
                    mp3.FileName,
                    SystemIO.Path.GetDirectoryName(mp3.FilePath),
                    displayTitle,     // 使用修复后的显示文本
                    displayArtist,    // 使用修复后的显示文本
                    displayAlbum,     // 使用修复后的显示文本
                    mp3.Year,
                    mp3.Track,
                    mp3.Disc,
                    displayGenre,     // 使用修复后的显示文本
                    mp3.Duration,
                    mp3.Bitrate,
                    mp3.SampleRate,
                    mp3.FileSizeMB,
                    mp3.ModifiedTime,
                    mp3.HasCoverArt,
                    displayComposer,  // 使用修复后的显示文本
                    displayComment,   // 使用修复后的显示文本
                    mp3.EncodingQuality
                );
            }
        }
        
        private void UpdateStats()
        {
            if (mp3Files.Count == 0)
            {
                statsLabel.Text = "已加载: 0 个文件";
                durationLabel.Text = "总时长: 00:00:00";
                sizeLabel.Text = "总大小: 0 MB";
                
                var missingLabelControl = this.Controls.OfType<Forms.Control>()
                    .FirstOrDefault(c => c.Name == "missingInfoLabel") as Forms.Label;
                if (missingLabelControl != null)
                    missingLabelControl.Text = "缺失元数据: 0 个";
                    
                return;
            }
            
            statsLabel.Text = $"已加载: {mp3Files.Count} 个文件";
            
            // 计算总时长
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (var mp3 in mp3Files)
            {
                if (TimeSpan.TryParseExact(mp3.Duration, @"hh\:mm\:ss", 
                    CultureInfo.InvariantCulture, out var duration))
                {
                    totalDuration = totalDuration.Add(duration);
                }
            }
            durationLabel.Text = $"总时长: {totalDuration:hh\\:mm\\:ss}";
            
            // 计算总大小
            double totalSize = mp3Files.Sum(f => f.FileSizeMB);
            sizeLabel.Text = $"总大小: {totalSize:F2} MB";
            
            // 统计缺失元数据
            int missingCount = mp3Files.Count(f => f.MissingScore > 0);
            var missingLabelControl2 = this.Controls.OfType<Forms.Control>()
                .FirstOrDefault(c => c.Name == "missingInfoLabel") as Forms.Label;
            if (missingLabelControl2 != null)
                missingLabelControl2.Text = $"缺失元数据: {missingCount} 个";
        }
        
        // ==================== 编码修复功能 ====================
        
        private void FixEncodingButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("请先选择要修复的文件（勾选第一列）", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var result = Forms.MessageBox.Show($"确定要修复 {selectedFiles.Count} 个文件的乱码问题吗？", "确认", 
                Forms.MessageBoxButtons.YesNo, Forms.MessageBoxIcon.Question);
            
            if (result == Forms.DialogResult.Yes)
            {
                FixSelectedFilesEncoding(selectedFiles);
            }
        }
        
        private void FixSelectedFilesEncoding(List<MP3FileInfo> files)
        {
            int fixedCount = 0;
            int failedCount = 0;
            
            using (var progressForm = new ProgressForm("修复文件编码", files.Count))
            {
                progressForm.Show();
                
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    
                    try
                    {
                        if (FixSingleFileEncoding(file.FilePath))
                            fixedCount++;
                        else
                            failedCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                    
                    progressForm.UpdateProgress(i + 1, $"正在修复: {file.FileName}");
                    
                    if (progressForm.Cancelled)
                        break;
                }
            }
            
            Forms.MessageBox.Show($"修复完成\n\n成功: {fixedCount} 个\n失败: {failedCount} 个", "结果", 
                Forms.MessageBoxButtons.OK, fixedCount > 0 ? Forms.MessageBoxIcon.Information : Forms.MessageBoxIcon.Warning);
            
            // 重新加载文件
            var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
            LoadMP3Files(filePaths);
        }
        
        private bool FixSingleFileEncoding(string filePath)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    bool changed = false;
                    
                    // 修复标题
                    var fixedTitle = _encodingHelper.FixMP3TagEncoding(file.Tag.Title);
                    if (fixedTitle != file.Tag.Title && !string.IsNullOrEmpty(fixedTitle))
                    {
                        file.Tag.Title = fixedTitle;
                        changed = true;
                    }
                    
                    // 修复艺术家
                    if (file.Tag.Performers?.Length > 0)
                    {
                        var fixedArtists = new List<string>();
                        foreach (var artist in file.Tag.Performers)
                        {
                            fixedArtists.Add(_encodingHelper.FixMP3TagEncoding(artist));
                        }
                        
                        if (!fixedArtists.SequenceEqual(file.Tag.Performers))
                        {
                            file.Tag.Performers = fixedArtists.ToArray();
                            changed = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(file.Tag.FirstPerformer))
                    {
                        var fixedArtist = _encodingHelper.FixMP3TagEncoding(file.Tag.FirstPerformer);
                        if (fixedArtist != file.Tag.FirstPerformer)
                        {
                            file.Tag.Performers = new[] { fixedArtist };
                            changed = true;
                        }
                    }
                    
                    // 修复专辑
                    var fixedAlbum = _encodingHelper.FixMP3TagEncoding(file.Tag.Album);
                    if (fixedAlbum != file.Tag.Album && !string.IsNullOrEmpty(fixedAlbum))
                    {
                        file.Tag.Album = fixedAlbum;
                        changed = true;
                    }
                    
                    // 修复作曲者
                    if (file.Tag.Composers?.Length > 0)
                    {
                        var fixedComposers = new List<string>();
                        foreach (var composer in file.Tag.Composers)
                        {
                            fixedComposers.Add(_encodingHelper.FixMP3TagEncoding(composer));
                        }
                        
                        if (!fixedComposers.SequenceEqual(file.Tag.Composers))
                        {
                            file.Tag.Composers = fixedComposers.ToArray();
                            changed = true;
                        }
                    }
                    
                    if (changed)
                    {
                        file.Save();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            
            return false;
        }
        
        // ==================== 测试修复功能 ====================
        
        private void TestFixButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("请先选择一个文件进行测试", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var file = selectedFiles.First();
            var result = TestAndFixFile(file.FilePath);
            
            var resultForm = new Forms.Form
            {
                Text = "修复测试结果",
                Size = new Drawing.Size(600, 400),
                StartPosition = Forms.FormStartPosition.CenterParent
            };
            
            var textBox = new Forms.TextBox
            {
                Multiline = true,
                ScrollBars = Forms.ScrollBars.Both,
                Dock = Forms.DockStyle.Fill,
                Font = new Drawing.Font("Consolas", 10),
                Text = result,
                ReadOnly = true
            };
            
            resultForm.Controls.Add(textBox);
            resultForm.ShowDialog();
        }
        
        private string TestAndFixFile(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MP3文件编码修复测试 ===");
            sb.AppendLine($"文件: {SystemIO.Path.GetFileName(filePath)}");
            sb.AppendLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    // 显示原始值
                    sb.AppendLine("=== 原始元数据 ===");
                    sb.AppendLine($"标题: {file.Tag.Title}");
                    sb.AppendLine($"艺术家: {file.Tag.FirstPerformer}");
                    sb.AppendLine($"专辑: {file.Tag.Album}");
                    
                    // 显示原始字节
                    sb.AppendLine();
                    sb.AppendLine("=== 原始字节分析 ===");
                    sb.AppendLine($"标题字节: {GetBytesHex(file.Tag.Title)}");
                    sb.AppendLine($"艺术家字节: {GetBytesHex(file.Tag.FirstPerformer)}");
                    sb.AppendLine($"专辑字节: {GetBytesHex(file.Tag.Album)}");
                    
                    // 测试修复
                    sb.AppendLine();
                    sb.AppendLine("=== 修复测试 ===");
                    
                    // 使用专门的修复方法
                    string fixedTitle = _encodingHelper.FixSpecificDoubleEncodedGBK(file.Tag.Title);
                    string fixedArtist = _encodingHelper.FixSpecificDoubleEncodedGBK(file.Tag.FirstPerformer);
                    string fixedAlbum = _encodingHelper.FixSpecificDoubleEncodedGBK(file.Tag.Album);
                    
                    // 如果没有修复，尝试从文件名提取
                    if (string.IsNullOrEmpty(fixedTitle) || fixedTitle == file.Tag.Title)
                    {
                        fixedTitle = ExtractTitleFromFileName(filePath);
                    }
                    
                    sb.AppendLine($"标题修复: {file.Tag.Title} -> {fixedTitle}");
                    sb.AppendLine($"艺术家修复: {file.Tag.FirstPerformer} -> {fixedArtist}");
                    sb.AppendLine($"专辑修复: {file.Tag.Album} -> {fixedAlbum}");
                    
                    // 检查修复结果
                    sb.AppendLine();
                    sb.AppendLine("=== 修复结果评估 ===");
                    bool titleValid = _encodingHelper.IsValidChineseText(fixedTitle) || !string.IsNullOrEmpty(fixedTitle);
                    bool artistValid = _encodingHelper.IsValidChineseText(fixedArtist) || !string.IsNullOrEmpty(fixedArtist);
                    bool albumValid = _encodingHelper.IsValidChineseText(fixedAlbum) || !string.IsNullOrEmpty(fixedAlbum);
                    
                    sb.AppendLine($"标题有效: {titleValid}");
                    sb.AppendLine($"艺术家有效: {artistValid}");
                    sb.AppendLine($"专辑有效: {albumValid}");
                    
                    // 如果修复有效，询问是否应用
                    if (titleValid || artistValid || albumValid)
                    {
                        sb.AppendLine();
                        sb.AppendLine("✅ 检测到有效的修复结果！");
                        
                        // 显示详细对比
                        var previewText = new StringBuilder();
                        previewText.AppendLine("修复前:");
                        if (!string.IsNullOrEmpty(file.Tag.Title)) previewText.AppendLine($"  标题: {file.Tag.Title}");
                        if (!string.IsNullOrEmpty(file.Tag.FirstPerformer)) previewText.AppendLine($"  艺术家: {file.Tag.FirstPerformer}");
                        if (!string.IsNullOrEmpty(file.Tag.Album)) previewText.AppendLine($"  专辑: {file.Tag.Album}");
                        
                        previewText.AppendLine("\n修复后:");
                        if (!string.IsNullOrEmpty(fixedTitle)) previewText.AppendLine($"  标题: {fixedTitle}");
                        if (!string.IsNullOrEmpty(fixedArtist)) previewText.AppendLine($"  艺术家: {fixedArtist}");
                        if (!string.IsNullOrEmpty(fixedAlbum)) previewText.AppendLine($"  专辑: {fixedAlbum}");
                        
                        var dialogResult = Forms.MessageBox.Show(
                            previewText.ToString() + "\n是否应用修复？", 
                            "确认修复", 
                            Forms.MessageBoxButtons.YesNo, 
                            Forms.MessageBoxIcon.Question);
                        
                        if (dialogResult == Forms.DialogResult.Yes)
                        {
                            // 保存修复
                            if (!string.IsNullOrEmpty(fixedTitle))
                                file.Tag.Title = fixedTitle;
                            if (!string.IsNullOrEmpty(fixedArtist))
                                file.Tag.Performers = new[] { fixedArtist };
                            if (!string.IsNullOrEmpty(fixedAlbum))
                                file.Tag.Album = fixedAlbum;
                            
                            file.Save();
                            
                            sb.AppendLine("✅ 修复已保存到文件！");
                            
                            // 重新加载文件
                            var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
                            LoadMP3Files(filePaths);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ 错误: {ex.Message}");
            }
            
            return sb.ToString();
        }

        private string GetBytesHex(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";
            
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        private string ExtractTitleFromFileName(string filePath)
        {
            var fileName = SystemIO.Path.GetFileNameWithoutExtension(filePath);
            
            // 处理模式: wangfei_12.mp3 -> 12.天使
            if (fileName.StartsWith("wangfei_"))
            {
                var number = fileName.Replace("wangfei_", "");
                if (int.TryParse(number, out int trackNum))
                {
                    return $"{trackNum}.天使";
                }
            }
            
            return fileName;
        }
        
        /*
        // ==================== 云分析和智能分析 ====================
        
        private async void CloudAnalyzeButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                selectedFiles = mp3Files.Where(f => f.MissingScore > 0).ToList();
            }
            
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("没有找到需要云分析的文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var result = Forms.MessageBox.Show(
                $"确定要对 {selectedFiles.Count} 个文件进行云分析吗？\n\n" +
                "注意：云分析需要网络连接，可能消耗较长时间。", 
                "云分析确认", 
                Forms.MessageBoxButtons.YesNo, 
                Forms.MessageBoxIcon.Question);
            
            if (result == Forms.DialogResult.Yes)
            {
                await PerformCloudAnalysis(selectedFiles, false);
            }
        }*/
        
        private void SpecificGBKFixButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("请先选择要修复的文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            // 专门修复带有双重编码GBK的文件
            var filesToFix = selectedFiles.Where(f => 
                f.Artist.Contains("鐜嬭彶") || 
                f.Title.Contains("澶╀娇") || 
                f.Album.Contains("鎴戠殑鐜嬭彶鏃朵唬")
            ).ToList();
            
            if (filesToFix.Count == 0)
            {
                Forms.MessageBox.Show("没有找到需要专门修复的文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var result = Forms.MessageBox.Show(
                $"将修复 {filesToFix.Count} 个文件的GBK双重编码问题。\n\n" +
                "此修复专门针对以下模式：\n" +
                "• 鐜嬭彶 → 王菲\n" +
                "• 澶╀娇 → 天使\n" +
                "• 鎴戠殑鐜嬭彶鏃朵唬 → 我的王菲时代\n\n" +
                "是否继续？", 
                "GBK双重编码修复", 
                Forms.MessageBoxButtons.YesNo, 
                Forms.MessageBoxIcon.Question);
            
            if (result == Forms.DialogResult.Yes)
            {
                FixSpecificGBKDoubleEncoding(filesToFix);
            }
        }

        private void FixSpecificGBKDoubleEncoding(List<MP3FileInfo> files)
        {
            int fixedCount = 0;
            var changedFiles = new List<string>();
            
            using (var progressForm = new ProgressForm("修复GBK双重编码", files.Count))
            {
                progressForm.Show();
                
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    
                    try
                    {
                        if (FixSpecificGBKInFile(file.FilePath))
                        {
                            fixedCount++;
                            changedFiles.Add(file.FileName);
                        }
                    }
                    catch
                    {
                        // 忽略错误
                    }
                    
                    progressForm.UpdateProgress(i + 1, $"正在修复: {file.FileName}");
                }
            }
            
            // 重新加载文件
            var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
            LoadMP3Files(filePaths);
            
            // 显示结果
            var resultMessage = new StringBuilder();
            resultMessage.AppendLine("=== GBK双重编码修复结果 ===");
            resultMessage.AppendLine($"成功修复: {fixedCount} 个文件");
            
            if (changedFiles.Count > 0)
            {
                resultMessage.AppendLine("\n修复的文件:");
                foreach (var fileName in changedFiles)
                {
                    resultMessage.AppendLine($"  • {fileName}");
                }
            }
            
            Forms.MessageBox.Show(resultMessage.ToString(), "修复完成", 
                Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
        }

        private bool FixSpecificGBKInFile(string filePath)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    bool changed = false;
                    
                    // 修复标题
                    var originalTitle = file.Tag.Title;
                    var fixedTitle = _encodingHelper.FixSpecificDoubleEncodedGBK(originalTitle);
                    if (fixedTitle != originalTitle && !string.IsNullOrEmpty(fixedTitle))
                    {
                        file.Tag.Title = fixedTitle;
                        changed = true;
                    }
                    
                    // 修复艺术家
                    var originalArtist = file.Tag.FirstPerformer;
                    var fixedArtist = _encodingHelper.FixSpecificDoubleEncodedGBK(originalArtist);
                    if (fixedArtist != originalArtist && !string.IsNullOrEmpty(fixedArtist))
                    {
                        file.Tag.Performers = new[] { fixedArtist };
                        changed = true;
                    }
                    
                    // 修复专辑
                    var originalAlbum = file.Tag.Album;
                    var fixedAlbum = _encodingHelper.FixSpecificDoubleEncodedGBK(originalAlbum);
                    if (fixedAlbum != originalAlbum && !string.IsNullOrEmpty(fixedAlbum))
                    {
                        file.Tag.Album = fixedAlbum;
                        changed = true;
                    }
                    
                    if (changed)
                    {
                        file.Save();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            
            return false;
        }

        private void VerifyButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("请先选择一个文件进行验证", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var file = selectedFiles.First();
            var result = MP3RepairVerifier.VerifyRepair(file.FilePath);
            
            Forms.MessageBox.Show(result.ToString(), "修复验证结果", 
                Forms.MessageBoxButtons.OK, 
                result.IsRepaired ? Forms.MessageBoxIcon.Information : Forms.MessageBoxIcon.Warning);
        }
        private void DeepAnalyzeButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("请先选择一个文件进行分析", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var file = selectedFiles.First();
            var analysisResult = MP3EncodingAnalyzer.AnalyzeFile(file.FilePath);
            
            var resultForm = new Forms.Form
            {
                Text = "深度编码分析",
                Size = new Drawing.Size(800, 600),
                StartPosition = Forms.FormStartPosition.CenterParent
            };
            
            var textBox = new Forms.TextBox
            {
                Multiline = true,
                ScrollBars = Forms.ScrollBars.Both,
                Dock = Forms.DockStyle.Fill,
                Font = new Drawing.Font("Consolas", 9),
                Text = analysisResult,
                ReadOnly = true
            };
            
            var copyButton = new Forms.Button
            {
                Text = "复制结果",
                Dock = Forms.DockStyle.Bottom,
                Height = 30
            };
            copyButton.Click += (s, e2) => Forms.Clipboard.SetText(analysisResult);
            
            resultForm.Controls.Add(textBox);
            resultForm.Controls.Add(copyButton);
            resultForm.ShowDialog();
        }
        /*
        private async void SmartAnalyzeButton_Click(object sender, EventArgs e)
        {
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                selectedFiles = mp3Files.Where(f => f.MissingScore > 2).ToList();
            }
            
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("没有找到需要智能分析的文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var result = Forms.MessageBox.Show(
                $"确定要对 {selectedFiles.Count} 个文件进行智能分析吗？\n\n" +
                "智能分析会结合本地和云端信息，提供最佳元数据建议。", 
                "智能分析确认", 
                Forms.MessageBoxButtons.YesNo, 
                Forms.MessageBoxIcon.Question);
            
            if (result == Forms.DialogResult.Yes)
            {
                await PerformSmartAnalysis(selectedFiles);
            }
        }*/
        
        // 添加这个方法来正确处理中文字符显示
        private string FixDisplayEncoding(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            try
            {
                // 尝试多种编码解码
                Encoding[] encodings = {
                    Encoding.UTF8,
                    Encoding.GetEncoding("GB2312"),
                    Encoding.GetEncoding("GBK"),
                    Encoding.GetEncoding("GB18030"),
                    Encoding.Default
                };
                
                // 先转换为字节
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                
                // 尝试用各种编码解码
                foreach (var encoding in encodings)
                {
                    try
                    {
                        string decoded = encoding.GetString(bytes);
                        
                        // 检查是否包含中文字符
                        if (ContainsChinese(decoded))
                        {
                            return decoded;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                return text;
            }
            catch
            {
                return text;
            }
        }

        private bool ContainsChinese(string text)
        {
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF)
                    return true;
            }
            return false;
        }
        /*
        private async Task PerformCloudAnalysis(List<MP3FileInfo> files, bool askForSelection = false)
        {
            int successCount = 0;
            int failCount = 0;
            
            using (var progressForm = new ProgressForm("云宝分析", files.Count))
            {
                progressForm.Show();
                
                for (int i = 0; i < files.Count; i++)
                {
                    var fileInfo = files[i];
                    
                    try
                    {
                        var cloudData = await _cloudService.GetMetadataFromAcoustId(fileInfo.FilePath);
                        
                        if (cloudData != null && cloudData.Results?.Count > 0)
                        {
                            var recording = cloudData.Results[0].Recordings[0];
                            
                            if (askForSelection)
                            {
                                // 显示选择界面
                                var chooserForm = new MetadataChooserForm(
                                    fileInfo.FileName,
                                    fileInfo.Title, fileInfo.Artist, fileInfo.Album,
                                    recording.Title, 
                                    recording.Artists?.FirstOrDefault()?.Name ?? "未知",
                                    recording.ReleaseGroups?.FirstOrDefault()?.Title ?? "未知专辑"
                                );
                                
                                if (chooserForm.ShowDialog() == Forms.DialogResult.OK)
                                {
                                    UpdateFileMetadata(fileInfo.FilePath,
                                        chooserForm.TitleResult,
                                        chooserForm.ArtistResult,
                                        chooserForm.AlbumResult);
                                    successCount++;
                                }
                            }
                            else
                            {
                                // 直接使用云端数据覆盖
                                UpdateFileMetadata(fileInfo.FilePath,
                                    recording.Title,
                                    recording.Artists?.FirstOrDefault()?.Name ?? "未知",
                                    recording.ReleaseGroups?.FirstOrDefault()?.Title ?? "未知专辑");
                                successCount++;
                            }
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch
                    {
                        failCount++;
                    }
                    
                    progressForm.UpdateProgress(i + 1, $"正在分析: {fileInfo.FileName}");
                    
                    // API速率限制
                    await Task.Delay(1000);
                    
                    if (progressForm.Cancelled)
                        break;
                }
            }
            
            // 重新加载文件
            var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
            LoadMP3Files(filePaths);
            
            Forms.MessageBox.Show($"云分析完成\n\n成功: {successCount} 个\n失败: {failCount} 个", "结果", 
                Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
        }
        
        private async Task PerformSmartAnalysis(List<MP3FileInfo> files)
        {
            int successCount = 0;
            int failCount = 0;
            var metadataManager = new MetadataManager(_cloudService);
            
            using (var progressForm = new ProgressForm("智能分析", files.Count))
            {
                progressForm.Show();
                
                for (int i = 0; i < files.Count; i++)
                {
                    var fileInfo = files[i];
                    
                    try
                    {
                        var options = new MetadataOptions
                        {
                            TitleStrategy = MergeStrategy.CloudIfBetter,
                            ArtistStrategy = MergeStrategy.CloudIfBetter,
                            AlbumStrategy = MergeStrategy.CloudIfBetter,
                            AllowCloud = true
                        };
                        
                        var bestMetadata = await metadataManager.GetBestMetadata(fileInfo.FilePath, options);
                        
                        if (bestMetadata != null)
                        {
                            // 显示选择界面
                            var chooserForm = new MetadataChooserForm(
                                fileInfo.FileName,
                                fileInfo.Title, fileInfo.Artist, fileInfo.Album,
                                bestMetadata.Title, bestMetadata.Artist, bestMetadata.Album
                            );
                            
                            if (chooserForm.ShowDialog() == Forms.DialogResult.OK)
                            {
                                UpdateFileMetadata(fileInfo.FilePath,
                                    chooserForm.TitleResult,
                                    chooserForm.ArtistResult,
                                    chooserForm.AlbumResult);
                                successCount++;
                            }
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch
                    {
                        failCount++;
                    }
                    
                    progressForm.UpdateProgress(i + 1, $"正在分析: {fileInfo.FileName}");
                    await Task.Delay(1000);
                    
                    if (progressForm.Cancelled)
                        break;
                }
            }
            
            // 重新加载文件
            var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
            LoadMP3Files(filePaths);
            
            Forms.MessageBox.Show($"智能分析完成\n\n成功: {successCount} 个\n失败: {failCount} 个", "结果", 
                Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
        }
        
        private void UpdateFileMetadata(string filePath, string title, string artist, string album)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    bool changed = false;
                    
                    if (!string.IsNullOrEmpty(title) && file.Tag.Title != title)
                    {
                        file.Tag.Title = title;
                        changed = true;
                    }
                    
                    if (!string.IsNullOrEmpty(artist) && file.Tag.FirstPerformer != artist)
                    {
                        file.Tag.Performers = new[] { artist };
                        changed = true;
                    }
                    
                    if (!string.IsNullOrEmpty(album) && file.Tag.Album != album)
                    {
                        file.Tag.Album = album;
                        changed = true;
                    }
                    
                    if (changed)
                    {
                        file.Save();
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }*/
        
        // ==================== 其他功能 ====================
        
        private void AnalyzeButton_Click(object sender, EventArgs e)
        {
            if (mp3Files.Count == 0)
            {
                Forms.MessageBox.Show("请先加载MP3文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            ShowAnalysisReport();
        }
        
        private void ShowAnalysisReport()
        {
            var report = new StringBuilder();
            report.AppendLine("=== MP3文件分析报告 ===");
            report.AppendLine($"分析时间: {DateTime.Now}");
            report.AppendLine($"文件总数: {mp3Files.Count}");
            report.AppendLine();
            
            // 编码质量统计
            var encodingStats = mp3Files.GroupBy(f => f.EncodingQuality)
                                       .Select(g => new { Quality = g.Key, Count = g.Count() })
                                       .ToList();
            
            report.AppendLine("【编码质量统计】");
            foreach (var stat in encodingStats)
            {
                report.AppendLine($"  {stat.Quality}: {stat.Count} 个");
            }
            report.AppendLine();
            
            // 艺术家统计
            var artists = mp3Files.Where(f => !string.IsNullOrEmpty(f.Artist))
                                 .GroupBy(f => f.Artist)
                                 .OrderByDescending(g => g.Count())
                                 .Take(10);
            
            if (artists.Any())
            {
                report.AppendLine("【艺术家统计（前10）】");
                foreach (var artist in artists)
                {
                    report.AppendLine($"  {artist.Key}: {artist.Count()} 首");
                }
                report.AppendLine();
            }
            
            // 缺失信息统计
            int missingTitles = mp3Files.Count(f => string.IsNullOrEmpty(f.Title) || 
                                                  f.Title == SystemIO.Path.GetFileNameWithoutExtension(f.FilePath));
            int missingArtists = mp3Files.Count(f => string.IsNullOrEmpty(f.Artist));
            int missingAlbums = mp3Files.Count(f => string.IsNullOrEmpty(f.Album));
            
            report.AppendLine("【缺失信息统计】");
            report.AppendLine($"  缺失标题: {missingTitles} 个");
            report.AppendLine($"  缺失艺术家: {missingArtists} 个");
            report.AppendLine($"  缺失专辑: {missingAlbums} 个");
            report.AppendLine($"  有封面: {mp3Files.Count(f => f.HasCoverArt)} 个");
            
            Forms.MessageBox.Show(report.ToString(), "分析报告", 
                Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
        }
        
        private void ExportCsvButton_Click(object sender, EventArgs e)
        {
            if (mp3Files.Count == 0)
            {
                Forms.MessageBox.Show("没有数据可导出", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            using (var saveDialog = new Forms.SaveFileDialog())
            {
                saveDialog.Filter = "CSV文件|*.csv|Excel文件|*.xlsx";
                saveDialog.FileName = $"MP3分析_{DateTime.Now:yyyyMMdd_HHmmss}";
                
                if (saveDialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    ExportToCsv(saveDialog.FileName);
                }
            }
        }
        
        private void ExportToCsv(string filePath)
        {
            try
            {
                using (var writer = new SystemIO.StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // 写入标题行
                    var headers = new string[] {
                        "文件名", "路径", "标题", "艺术家", "专辑", "年份", "音轨号", 
                        "碟片号", "流派", "时长", "比特率(kbps)", "采样率(Hz)", 
                        "大小(MB)", "修改时间", "是否有封面", "作曲者", "备注", "编码质量"
                    };
                    writer.WriteLine(string.Join(",", headers));
                    
                    // 写入数据行
                    foreach (var mp3 in mp3Files)
                    {
                        var row = new string[] {
                            EscapeCsv(mp3.FileName),
                            EscapeCsv(SystemIO.Path.GetDirectoryName(mp3.FilePath)),
                            EscapeCsv(mp3.Title),
                            EscapeCsv(mp3.Artist),
                            EscapeCsv(mp3.Album),
                            mp3.Year.ToString(),
                            mp3.Track.ToString(),
                            mp3.Disc.ToString(),
                            EscapeCsv(mp3.Genre),
                            mp3.Duration,
                            mp3.Bitrate.ToString(),
                            mp3.SampleRate.ToString(),
                            mp3.FileSizeMB.ToString("F2"),
                            mp3.ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            mp3.HasCoverArt ? "是" : "否",
                            EscapeCsv(mp3.Composer),
                            EscapeCsv(mp3.Comment),
                            EscapeCsv(mp3.EncodingQuality)
                        };
                        writer.WriteLine(string.Join(",", row));
                    }
                }
                
                Forms.MessageBox.Show($"导出成功！文件已保存到: {filePath}", "成功", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Forms.MessageBox.Show($"导出失败: {ex.Message}", "错误", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
            }
        }
        
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
                
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
                
            return value;
        }
        
        private void FindDuplicatesButton_Click(object sender, EventArgs e)
        {
            if (mp3Files.Count == 0)
            {
                Forms.MessageBox.Show("请先加载MP3文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            FindDuplicateSongs();
        }
        
        private void FindDuplicateSongs()
        {
            // 基于标题和艺术家查找重复
            var duplicates = mp3Files
                .Where(f => !string.IsNullOrEmpty(f.Title) && !string.IsNullOrEmpty(f.Artist))
                .GroupBy(f => $"{f.Title.ToLower()}_{f.Artist.ToLower()}")
                .Where(g => g.Count() > 1)
                .ToList();
            
            if (duplicates.Count == 0)
            {
                Forms.MessageBox.Show("未找到重复歌曲", "信息", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var report = new StringBuilder();
            report.AppendLine("=== 重复歌曲检测结果 ===");
            report.AppendLine($"找到 {duplicates.Count} 组重复歌曲");
            report.AppendLine();
            
            foreach (var group in duplicates)
            {
                report.AppendLine($"【{group.First().Title} - {group.First().Artist}】");
                foreach (var file in group)
                {
                    report.AppendLine($"  • {SystemIO.Path.GetFileName(file.FilePath)}");
                    report.AppendLine($"    大小: {file.FileSizeMB:F2} MB, 时长: {file.Duration}");
                    if (file.Album != string.Empty)
                        report.AppendLine($"    专辑: {file.Album}");
                    if (file.Bitrate > 0)
                        report.AppendLine($"    比特率: {file.Bitrate} kbps");
                }
                report.AppendLine();
            }
            
            var resultForm = new Forms.Form
            {
                Text = "重复歌曲检测结果",
                Size = new Drawing.Size(800, 600),
                StartPosition = Forms.FormStartPosition.CenterParent
            };
            
            var textBox = new Forms.TextBox
            {
                Multiline = true,
                ScrollBars = Forms.ScrollBars.Both,
                Dock = Forms.DockStyle.Fill,
                Font = new Drawing.Font("Consolas", 10),
                Text = report.ToString(),
                ReadOnly = true
            };
            
            var copyButton = new Forms.Button
            {
                Text = "复制到剪贴板",
                Dock = Forms.DockStyle.Bottom,
                Height = 30
            };
            copyButton.Click += (s, e) => Forms.Clipboard.SetText(report.ToString());
            
            resultForm.Controls.Add(textBox);
            resultForm.Controls.Add(copyButton);
            resultForm.ShowDialog();
        }
        
        /*
        private void AutoCompleteButton_Click(object sender, EventArgs e)
        {
            if (mp3Files.Count == 0)
            {
                Forms.MessageBox.Show("请先加载MP3文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            // 显示选项对话框
            var optionForm = new Forms.Form
            {
                Text = "选择自动补全方式",
                Size = new Drawing.Size(300, 200),
                StartPosition = Forms.FormStartPosition.CenterParent,
                FormBorderStyle = Forms.FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            var label = new Forms.Label
            {
                Text = "请选择自动补全方式：",
                Location = new Drawing.Point(20, 20),
                Size = new Drawing.Size(250, 20)
            };
            
            var localButton = new Forms.Button
            {
                Text = "本地分析（从文件名提取）",
                Location = new Drawing.Point(20, 50),
                Size = new Drawing.Size(240, 30)
            };
            
            var cloudButton = new Forms.Button
            {
                Text = "云宝分析（智能识别）",
                Location = new Drawing.Point(20, 90),
                Size = new Drawing.Size(240, 30)
            };
            
            var cancelButton = new Forms.Button
            {
                Text = "取消",
                Location = new Drawing.Point(20, 130),
                Size = new Drawing.Size(240, 30)
            };
            
            Forms.DialogResult result = Forms.DialogResult.Cancel;
            
            localButton.Click += (s, ev) => 
            {
                result = Forms.DialogResult.Yes;
                optionForm.Close();
            };
            
            cloudButton.Click += (s, ev) => 
            {
                result = Forms.DialogResult.No;
                optionForm.Close();
            };
            
            cancelButton.Click += (s, ev) => 
            {
                result = Forms.DialogResult.Cancel;
                optionForm.Close();
            };
            
            optionForm.Controls.AddRange(new Forms.Control[] { label, localButton, cloudButton, cancelButton });
            optionForm.ShowDialog();
            
            if (result == Forms.DialogResult.Yes)
            {
                AutoCompleteMetadataLocal();
            }
            else if (result == Forms.DialogResult.No)
            {
                // 异步调用云分析
                var filesToAnalyze = mp3Files.Where(f => 
                    string.IsNullOrEmpty(f.Artist) || 
                    string.IsNullOrEmpty(f.Album) || 
                    (string.IsNullOrEmpty(f.Title) || f.Title == SystemIO.Path.GetFileNameWithoutExtension(f.FilePath))
                ).ToList();
                
                if (filesToAnalyze.Count > 0)
                {
                    _ = Task.Run(async () => 
                    {
                        await PerformCloudAnalysis(filesToAnalyze, true);
                    });
                }
            }
        }*/
        
        private void AutoCompleteMetadataLocal()
        {
            var filesToFix = mp3Files.Where(f => 
                string.IsNullOrEmpty(f.Artist) || 
                string.IsNullOrEmpty(f.Album) || 
                (string.IsNullOrEmpty(f.Title) || f.Title == SystemIO.Path.GetFileNameWithoutExtension(f.FilePath))
            ).ToList();
            
            if (filesToFix.Count == 0)
            {
                Forms.MessageBox.Show("所有文件的元数据都完整，无需自动补全", "信息", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var result = Forms.MessageBox.Show(
                $"发现 {filesToFix.Count} 个文件需要补全元数据。是否继续？", 
                "自动补全确认", 
                Forms.MessageBoxButtons.YesNo, 
                Forms.MessageBoxIcon.Question);
            
            if (result == Forms.DialogResult.Yes)
            {
                AutoCompleteMetadata(filesToFix);
            }
        }
        
        private void AutoCompleteMetadata(List<MP3FileInfo> filesToFix)
        {
            int fixedCount = 0;
            
            using (var progressForm = new ProgressForm("自动补全元数据", filesToFix.Count))
            {
                progressForm.Show();
                
                for (int i = 0; i < filesToFix.Count; i++)
                {
                    var fileInfo = filesToFix[i];
                    
                    try
                    {
                        using (var file = TagLib.File.Create(fileInfo.FilePath))
                        {
                            bool changed = false;
                            
                            // 从文件名提取信息
                            var fileName = SystemIO.Path.GetFileNameWithoutExtension(fileInfo.FileName);
                            
                            // 尝试各种分隔符
                            string[] separators = { " - ", "-", " – ", " _ ", "_" };
                            string artist = null;
                            string title = null;
                            
                            foreach (var separator in separators)
                            {
                                if (fileName.Contains(separator))
                                {
                                    var parts = fileName.Split(new[] { separator }, 2, StringSplitOptions.None);
                                    if (parts.Length == 2)
                                    {
                                        artist = parts[0].Trim();
                                        title = parts[1].Trim();
                                        break;
                                    }
                                }
                            }
                            
                            if (artist != null && title != null)
                            {
                                if (string.IsNullOrEmpty(fileInfo.Artist) || fileInfo.Artist == "未知艺术家")
                                {
                                    file.Tag.Performers = new[] { artist };
                                    changed = true;
                                }
                                
                                if (string.IsNullOrEmpty(fileInfo.Title) || 
                                    fileInfo.Title == SystemIO.Path.GetFileNameWithoutExtension(fileInfo.FilePath))
                                {
                                    file.Tag.Title = title;
                                    changed = true;
                                }
                            }
                            
                            // 从文件夹名获取专辑
                            if (string.IsNullOrEmpty(fileInfo.Album) || fileInfo.Album == "未知专辑")
                            {
                                var dirName = SystemIO.Path.GetFileName(SystemIO.Path.GetDirectoryName(fileInfo.FilePath));
                                if (!string.IsNullOrEmpty(dirName) && 
                                    !dirName.Equals("Music", StringComparison.OrdinalIgnoreCase) &&
                                    !dirName.Equals("MP3", StringComparison.OrdinalIgnoreCase))
                                {
                                    file.Tag.Album = dirName;
                                    changed = true;
                                }
                            }
                            
                            if (changed)
                            {
                                file.Save();
                                fixedCount++;
                            }
                        }
                    }
                    catch
                    {
                        // 忽略错误
                    }
                    
                    progressForm.UpdateProgress(i + 1, $"正在处理: {SystemIO.Path.GetFileName(fileInfo.FilePath)}");
                    
                    if (progressForm.Cancelled)
                        break;
                }
            }
            
            // 重新加载文件
            var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
            LoadMP3Files(filePaths);
            
            Forms.MessageBox.Show($"已补全 {fixedCount} 个文件的元数据", "完成", 
                Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
        }
        
        private void RenameFilesButton_Click(object sender, EventArgs e)
        {
            if (mp3Files.Count == 0)
            {
                Forms.MessageBox.Show("请先加载MP3文件", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                Forms.MessageBox.Show("请先选择要重命名的文件（勾选第一列）", "提示", 
                    Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Information);
                return;
            }
            
            var renameForm = new RenameFilesForm(selectedFiles);
            if (renameForm.ShowDialog() == Forms.DialogResult.OK)
            {
                // 重新加载文件
                var filePaths = mp3Files.Select(f => f.FilePath).ToArray();
                LoadMP3Files(filePaths);
            }
        }
        
        // ==================== 辅助方法 ====================
        
        private void DataGridView_CellContentClick(object sender, Forms.DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                dataGridView.CommitEdit(Forms.DataGridViewDataErrorContexts.Commit);
            }
        }
        
        private List<MP3FileInfo> GetSelectedFiles()
        {
            var selectedFiles = new List<MP3FileInfo>();
            
            foreach (Forms.DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value is bool isSelected && isSelected)
                {
                    var fileName = row.Cells["文件名"].Value?.ToString();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var fileInfo = mp3Files.FirstOrDefault(f => f.FileName == fileName);
                        if (fileInfo != null)
                            selectedFiles.Add(fileInfo);
                    }
                }
            }
            
            return selectedFiles;
        }
        
        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }
        
        private void ApplyFilter()
        {
            var filterText = filterTextBox.Text.ToLower();
            if (string.IsNullOrEmpty(filterText))
            {
                dataGridView.DataSource = analysisData;
                return;
            }
            
            try
            {
                var filteredRows = analysisData.AsEnumerable()
                    .Where(row => row.ItemArray.Any(cell => 
                        cell != null && cell.ToString().ToLower().Contains(filterText)))
                    .CopyToDataTable();
                
                dataGridView.DataSource = filteredRows;
            }
            catch
            {
                // 如果没有匹配的行，显示空表
                dataGridView.DataSource = analysisData.Clone();
            }
        }
        
        // ==================== 嵌套类 ====================
        
        public class MP3FileInfo
        {
            public string FilePath { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Artist { get; set; } = string.Empty;
            public string Album { get; set; } = string.Empty;
            public int Year { get; set; }
            public int Track { get; set; }
            public int Disc { get; set; }
            public string Genre { get; set; } = string.Empty;
            public string Duration { get; set; } = "00:00:00";
            public int Bitrate { get; set; }
            public int SampleRate { get; set; }
            public int Channels { get; set; }
            public double FileSizeMB { get; set; }
            public DateTime ModifiedTime { get; set; }
            public bool HasCoverArt { get; set; }
            public string Composer { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string EncodingQuality { get; set; } = "正常";
            public int MissingScore { get; private set; }
            
            public void CalculateMissingScore()
            {
                MissingScore = 0;
                if (string.IsNullOrEmpty(Title) || Title == SystemIO.Path.GetFileNameWithoutExtension(FileName))
                    MissingScore += 3;
                if (string.IsNullOrEmpty(Artist) || Artist == "未知艺术家")
                    MissingScore += 2;
                if (string.IsNullOrEmpty(Album) || Album == "未知专辑")
                    MissingScore += 1;
                if (Year == 0)
                    MissingScore += 1;
                if (!HasCoverArt)
                    MissingScore += 1;
                
                // 根据编码质量调整分数
                if (EncodingQuality == "良好")
                {
                    // 编码质量好，不扣分
                }
                else if (EncodingQuality == "部分良好")
                {
                    MissingScore += 1;
                }
                else if (EncodingQuality == "需修复")
                {
                    MissingScore += 2;
                }
                else if (EncodingQuality == "读取错误")
                {
                    MissingScore += 3;
                }
            }
        }
    }
}