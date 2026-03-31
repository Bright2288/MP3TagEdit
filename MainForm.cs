using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;

namespace MP3Player
{
    public partial class MainForm : Form
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private List<string> playlist = new List<string>();
        private int currentTrackIndex = -1;
        private System.Windows.Forms.Timer playbackTimer;

        private System.Windows.Forms.ListView playlistListView;
        private System.Windows.Forms.TrackBar progressBar;
        private System.Windows.Forms.TrackBar volumeBar;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.Label currentTrackLabel;
        private System.Windows.Forms.Panel currentTrackPanel;

        public MainForm()
        {
            InitializeComponent();
            InitializePlaybackTimer();
        }

        private void InitializePlaybackTimer()
        {
            playbackTimer = new System.Windows.Forms.Timer();
            playbackTimer.Interval = 500;
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void InitializeComponent()
        {
            this.Text = "MP3TagEdit v1.0";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建菜单栏
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("文件(&F)");
            var editMenu = new ToolStripMenuItem("手动编辑(&E)");
            var toolsMenu = new ToolStripMenuItem("工具(&T)");  // 新增工具菜单
            var helpMenu = new ToolStripMenuItem("帮助(&H)");

            // 文件菜单项
            var openFileItem = new ToolStripMenuItem("打开文件(&O)");
            openFileItem.Click += OpenFile_Click;
            var openFolderItem = new ToolStripMenuItem("打开文件夹(&F)");
            openFolderItem.Click += OpenFolder_Click;
            var exitItem = new ToolStripMenuItem("退出(&X)");
            exitItem.Click += (s, e) => Application.Exit();

            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { openFileItem, openFolderItem, new ToolStripSeparator(), exitItem });

            // 编辑菜单项
            var editTagsItem = new ToolStripMenuItem("编辑标签(&T)");
            editTagsItem.Click += EditTags_Click;
            editMenu.DropDownItems.Add(editTagsItem);

            // 工具菜单项
            var analyzeItem = new ToolStripMenuItem("MP3分析工具(&A)");
            analyzeItem.Click += AnalyzeItem_Click;  
            // 新增事件处理
            toolsMenu.DropDownItems.Add(analyzeItem);

            // 帮助菜单项
            var aboutItem = new ToolStripMenuItem("关于(&A)");
            aboutItem.Click += About_Click;
            helpMenu.DropDownItems.Add(aboutItem);

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, toolsMenu,helpMenu });
            this.MainMenuStrip = menuStrip;

            // 工具栏
            var toolStrip = new ToolStrip();
            var playButton = new ToolStripButton("播放", null, Play_Click);
            var pauseButton = new ToolStripButton("暂停", null, Pause_Click);
            var stopButton = new ToolStripButton("停止", null, Stop_Click);
            var prevButton = new ToolStripButton("上一首", null, Prev_Click);
            var nextButton = new ToolStripButton("下一首", null, Next_Click);

            toolStrip.Items.AddRange(new ToolStripItem[] { playButton, pauseButton, stopButton, new ToolStripSeparator(), prevButton, nextButton });

            // 播放列表
            playlistListView = new System.Windows.Forms.ListView();
            playlistListView.Dock = DockStyle.Fill;
            playlistListView.View = View.Details;
            playlistListView.FullRowSelect = true;
            playlistListView.MultiSelect = false;
            playlistListView.Columns.Add("标题", 200);
            playlistListView.Columns.Add("艺术家", 150);
            playlistListView.Columns.Add("专辑", 150);
            playlistListView.Columns.Add("时长", 80);
            playlistListView.Columns.Add("文件路径", 300);
            playlistListView.DoubleClick += PlaylistListView_DoubleClick;

            // 当前播放信息
            currentTrackPanel = new System.Windows.Forms.Panel();
            currentTrackPanel.Dock = DockStyle.Top;
            currentTrackPanel.Height = 60;
            currentTrackPanel.BackColor = Color.FromArgb(240, 240, 240);
            currentTrackPanel.Padding = new Padding(10);

            currentTrackLabel = new System.Windows.Forms.Label();
            currentTrackLabel.Dock = DockStyle.Fill;
            currentTrackLabel.Text = "当前未播放";
            currentTrackLabel.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            currentTrackLabel.TextAlign = ContentAlignment.MiddleLeft;

            currentTrackPanel.Controls.Add(currentTrackLabel);

            // 播放控制面板
            var controlPanel = new System.Windows.Forms.Panel();
            controlPanel.Dock = DockStyle.Bottom;
            controlPanel.Height = 80;
            controlPanel.BackColor = Color.LightGray;

            // 进度条
            progressBar = new System.Windows.Forms.TrackBar();
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 40;
            progressBar.TickStyle = TickStyle.None;
            progressBar.Scroll += ProgressBar_Scroll;

            // 音量控制
            var volumePanel = new System.Windows.Forms.Panel();
            volumePanel.Dock = DockStyle.Right;
            volumePanel.Width = 150;
            volumePanel.Padding = new Padding(10, 5, 10, 5);

            var volumeLabel = new System.Windows.Forms.Label();
            volumeLabel.Text = "音量:";
            volumeLabel.Dock = DockStyle.Left;
            volumeLabel.AutoSize = false;
            volumeLabel.Width = 40;
            volumeLabel.TextAlign = ContentAlignment.MiddleLeft;

            volumeBar = new System.Windows.Forms.TrackBar();
            volumeBar.Minimum = 0;
            volumeBar.Maximum = 100;
            volumeBar.Value = 50;
            volumeBar.TickStyle = TickStyle.None;
            volumeBar.Dock = DockStyle.Fill;
            volumeBar.Scroll += VolumeBar_Scroll;

            volumePanel.Controls.Add(volumeBar);
            volumePanel.Controls.Add(volumeLabel);

            // 时间标签
            timeLabel = new System.Windows.Forms.Label();
            timeLabel.Dock = DockStyle.Left;
            timeLabel.Width = 120;
            timeLabel.Text = "00:00 / 00:00";
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;

            controlPanel.Controls.Add(volumePanel);
            controlPanel.Controls.Add(timeLabel);
            controlPanel.Controls.Add(progressBar);

            // 布局容器
            var mainPanel = new System.Windows.Forms.Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(5);

            mainPanel.Controls.Add(playlistListView);
            mainPanel.Controls.Add(currentTrackPanel);

            // 添加到窗体
            this.Controls.AddRange(new Control[] { mainPanel, controlPanel, toolStrip, menuStrip });
        }

        // 文件打开事件
        private void OpenFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "MP3文件|*.mp3|所有文件|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    AddFilesToPlaylist(openFileDialog.FileNames);
                }
            }
        }

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    var mp3Files = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.mp3", SearchOption.AllDirectories);
                    AddFilesToPlaylist(mp3Files);
                }
            }
        }

        private void AddFilesToPlaylist(string[] files)
        {
            foreach (var file in files)
            {
                if (!playlist.Contains(file))
                {
                    playlist.Add(file);
                    AddFileToListView(file);
                }
            }
        }

        private void AddFileToListView(string filePath)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    var title = string.IsNullOrEmpty(file.Tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : file.Tag.Title;
                    var artist = string.IsNullOrEmpty(file.Tag.FirstPerformer) ? "未知艺术家" : file.Tag.FirstPerformer;
                    var album = string.IsNullOrEmpty(file.Tag.Album) ? "未知专辑" : file.Tag.Album;
                    var duration = file.Properties.Duration.ToString(@"mm\:ss");

                    var item = new ListViewItem(title);
                    item.SubItems.Add(artist);
                    item.SubItems.Add(album);
                    item.SubItems.Add(duration);
                    item.SubItems.Add(filePath);
                    item.Tag = filePath;

                    playlistListView.Items.Add(item);
                }
            }
            catch
            {
                // 如果无法读取标签，使用文件名
                var item = new ListViewItem(Path.GetFileNameWithoutExtension(filePath));
                item.SubItems.Add("未知");
                item.SubItems.Add("未知");
                item.SubItems.Add("--:--");
                item.SubItems.Add(filePath);
                item.Tag = filePath;
                playlistListView.Items.Add(item);
            }
        }

        // 播放控制事件
        private void Play_Click(object sender, EventArgs e)
        {
            if (playlistListView.SelectedItems.Count > 0)
            {
                PlaySelectedTrack();
            }
            else if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                playbackTimer.Start();
            }
        }

        private void Pause_Click(object sender, EventArgs e)
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();
                playbackTimer.Stop();
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }

        private void Prev_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 0)
            {
                currentTrackIndex--;
                if (currentTrackIndex < 0) currentTrackIndex = playlist.Count - 1;
                PlayTrack(currentTrackIndex);
            }
        }

        private void Next_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 0)
            {
                currentTrackIndex++;
                if (currentTrackIndex >= playlist.Count) currentTrackIndex = 0;
                PlayTrack(currentTrackIndex);
            }
        }

        // 新增事件处理方法
        private void AnalyzeItem_Click(object sender, EventArgs e)
        {
            var analyzerForm = new MP3AnalyzerForm();
            analyzerForm.ShowDialog();
        }

        private void PlaySelectedTrack()
        {
            var selectedIndex = playlistListView.SelectedIndices[0];
            PlayTrack(selectedIndex);
        }

        private void PlayTrack(int index)
        {
            StopPlayback();
            currentTrackIndex = index;

            var filePath = playlist[index];
            try
            {
                audioFile = new AudioFileReader(filePath);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFile);
                outputDevice.Play();
                outputDevice.Volume = volumeBar.Value / 100f;

                // 更新UI
                UpdateCurrentTrackInfo(filePath);
                playlistListView.Items[index].Selected = true;
                playlistListView.Items[index].EnsureVisible();
                playbackTimer.Start();

                // 设置进度条
                progressBar.Maximum = (int)audioFile.TotalTime.TotalSeconds;
                progressBar.Value = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法播放文件: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopPlayback()
        {
            playbackTimer.Stop();
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;
            audioFile?.Dispose();
            audioFile = null;

            progressBar.Value = 0;
            timeLabel.Text = "00:00 / 00:00";
            currentTrackLabel.Text = "当前未播放";
        }

        // 进度条事件
        private void ProgressBar_Scroll(object sender, EventArgs e)
        {
            if (audioFile != null && outputDevice != null)
            {
                audioFile.CurrentTime = TimeSpan.FromSeconds(progressBar.Value);
            }
        }

        private void VolumeBar_Scroll(object sender, EventArgs e)
        {
            if (outputDevice != null)
            {
                outputDevice.Volume = volumeBar.Value / 100f;
            }
        }

        // 播放列表双击事件
        private void PlaylistListView_DoubleClick(object sender, EventArgs e)
        {
            if (playlistListView.SelectedItems.Count > 0)
            {
                PlaySelectedTrack();
            }
        }

        // 定时器事件
        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (audioFile != null && outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                var currentTime = audioFile.CurrentTime;
                var totalTime = audioFile.TotalTime;

                progressBar.Value = (int)currentTime.TotalSeconds;
                timeLabel.Text = $"{currentTime:mm\\:ss} / {totalTime:mm\\:ss}";

                // 检查是否播放完毕
                if (currentTime.TotalSeconds >= totalTime.TotalSeconds - 1)
                {
                    Next_Click(null, EventArgs.Empty);
                }
            }
        }

        // 编辑标签事件
        private void EditTags_Click(object sender, EventArgs e)
        {
            if (playlistListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择一个MP3文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var filePath = playlistListView.SelectedItems[0].Tag.ToString();
            if (filePath == null) return;

            var editForm = new EditTagsForm(filePath);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // 更新列表视图中的信息
                var item = playlistListView.SelectedItems[0];
                item.SubItems[0].Text = editForm.Title;
                item.SubItems[1].Text = editForm.Artist;
                item.SubItems[2].Text = editForm.Album;

                // 如果当前正在播放这个文件，更新显示信息
                if (currentTrackIndex == playlistListView.SelectedIndices[0])
                {
                    UpdateCurrentTrackInfo(filePath);
                }
            }
        }

        private void UpdateCurrentTrackInfo(string filePath)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    var title = string.IsNullOrEmpty(file.Tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : file.Tag.Title;
                    var artist = string.IsNullOrEmpty(file.Tag.FirstPerformer) ? "未知艺术家" : file.Tag.FirstPerformer;
                    currentTrackLabel.Text = $"正在播放: {title} - {artist}";
                }
            }
            catch
            {
                currentTrackLabel.Text = $"正在播放: {Path.GetFileName(filePath)}";
            }
        }

        // 关于窗口
        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "MP3播放器 v1.0\n\n" +
                "功能：\n" +
                "- 播放MP3文件\n" +
                "- 支持播放列表\n" +
                "- 编辑MP3标签信息\n" +
                "- 音量控制\n\n" +
                "使用TagLibSharp和NAudio库",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // 窗体关闭事件
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopPlayback();
            base.OnFormClosing(e);
        }
    }
}