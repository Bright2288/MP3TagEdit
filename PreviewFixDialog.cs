// PreviewFixDialog.cs
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using System.Collections.Generic;

namespace MP3Player
{
    public class PreviewFixDialog : Forms.Form
    {
        // 使用完整类型名：MP3AnalyzerForm.MP3FileInfo
        private List<MP3AnalyzerForm.MP3FileInfo> _selectedFiles = new List<MP3AnalyzerForm.MP3FileInfo>();
        
        public List<MP3AnalyzerForm.MP3FileInfo> GetSelectedFiles() => _selectedFiles;
        
        public PreviewFixDialog(List<MP3AnalyzerForm.MP3FileInfo> files)
        {
            InitializeComponent(files);
        }
        
        private void InitializeComponent(List<MP3AnalyzerForm.MP3FileInfo> files)
        {
            this.Text = "预览修复结果";
            this.Size = new Drawing.Size(600, 400);
            
            var listBox = new Forms.ListBox
            {
                Dock = Forms.DockStyle.Fill,
                SelectionMode = Forms.SelectionMode.MultiExtended
            };
            
            foreach (var file in files)
            {
                listBox.Items.Add($"{file.FileName} - 将修复为: 艺术家={file.Artist}, 专辑={file.Album}");
            }
            
            var okButton = new Forms.Button
            {
                Text = "确认修复",
                Dock = Forms.DockStyle.Bottom,
                Height = 30
            };
            okButton.Click += (s, e) => 
            {
                _selectedFiles = files;
                this.DialogResult = Forms.DialogResult.OK;
                this.Close();
            };
            
            var cancelButton = new Forms.Button
            {
                Text = "取消",
                Dock = Forms.DockStyle.Bottom,
                Height = 30
            };
            cancelButton.Click += (s, e) => 
            {
                this.DialogResult = Forms.DialogResult.Cancel;
                this.Close();
            };
            
            this.Controls.Add(listBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }
    }
}