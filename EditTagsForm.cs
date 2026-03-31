using System;
using System.Drawing;
using System.Windows.Forms;

namespace MP3Player
{
    public partial class EditTagsForm : Form
    {
        private string filePath;
        private System.Windows.Forms.TextBox titleTextBox;
        private System.Windows.Forms.TextBox artistTextBox;
        private System.Windows.Forms.TextBox albumTextBox;

        // 改为私有字段，避免序列化问题
        private string _title = string.Empty;
        private string _artist = string.Empty;
        private string _album = string.Empty;

        public string Title => _title;
        public string Artist => _artist;
        public string Album => _album;

        public EditTagsForm(string filePath)
        {
            this.filePath = filePath;
            InitializeComponent();
            LoadTags();
        }

        private void InitializeComponent()
        {
            this.Text = "编辑MP3标签";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 文件路径标签
            var fileLabel = new System.Windows.Forms.Label();
            fileLabel.Text = $"文件: {System.IO.Path.GetFileName(filePath)}";
            fileLabel.Location = new Point(20, 20);
            fileLabel.AutoSize = true;

            // 标题
            var titleLabel = new System.Windows.Forms.Label();
            titleLabel.Text = "标题:";
            titleLabel.Location = new Point(20, 60);
            titleLabel.Size = new Size(60, 20);

            titleTextBox = new System.Windows.Forms.TextBox();
            titleTextBox.Location = new Point(90, 57);
            titleTextBox.Size = new Size(270, 20);

            // 艺术家
            var artistLabel = new System.Windows.Forms.Label();
            artistLabel.Text = "艺术家:";
            artistLabel.Location = new Point(20, 90);
            artistLabel.Size = new Size(60, 20);

            artistTextBox = new System.Windows.Forms.TextBox();
            artistTextBox.Location = new Point(90, 87);
            artistTextBox.Size = new Size(270, 20);

            // 专辑
            var albumLabel = new System.Windows.Forms.Label();
            albumLabel.Text = "专辑:";
            albumLabel.Location = new Point(20, 120);
            albumLabel.Size = new Size(60, 20);

            albumTextBox = new System.Windows.Forms.TextBox();
            albumTextBox.Location = new Point(90, 117);
            albumTextBox.Size = new Size(270, 20);

            // 按钮
            var saveButton = new System.Windows.Forms.Button();
            saveButton.Text = "保存";
            saveButton.Location = new Point(180, 170);
            saveButton.Size = new Size(80, 30);
            saveButton.Click += SaveButton_Click;

            var cancelButton = new System.Windows.Forms.Button();
            cancelButton.Text = "取消";
            cancelButton.Location = new Point(270, 170);
            cancelButton.Size = new Size(80, 30);
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // 添加到窗体
            this.Controls.AddRange(new Control[] {
                fileLabel,
                titleLabel, titleTextBox,
                artistLabel, artistTextBox,
                albumLabel, albumTextBox,
                saveButton, cancelButton
            });
        }

        private void LoadTags()
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    titleTextBox.Text = file.Tag.Title ?? string.Empty;
                    artistTextBox.Text = file.Tag.FirstPerformer ?? string.Empty;
                    albumTextBox.Text = file.Tag.Album ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法读取标签信息: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    file.Tag.Title = titleTextBox.Text;
                    if (!string.IsNullOrEmpty(artistTextBox.Text))
                    {
                        file.Tag.Performers = new[] { artistTextBox.Text };
                    }
                    else
                    {
                        file.Tag.Performers = new string[0];
                    }
                    file.Tag.Album = albumTextBox.Text;
                    file.Save();
                }

                _title = titleTextBox.Text;
                _artist = artistTextBox.Text;
                _album = albumTextBox.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法保存标签信息: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}