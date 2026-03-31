using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP3Player
{
    public class MetadataChooserForm : Form
    {
        private string _titleResult = "";
        private string _artistResult = "";
        private string _albumResult = "";
        
        public string TitleResult { get { return _titleResult; } }
        public string ArtistResult { get { return _artistResult; } }
        public string AlbumResult { get { return _albumResult; } }
        
        public MetadataChooserForm(string fileName, 
                                  string localTitle, string localArtist, string localAlbum,
                                  string cloudTitle, string cloudArtist, string cloudAlbum)
        {
            InitializeUI(fileName, localTitle, localArtist, localAlbum, 
                        cloudTitle, cloudArtist, cloudAlbum);
        }
        
        private void InitializeUI(string fileName, 
                                 string localTitle, string localArtist, string localAlbum,
                                 string cloudTitle, string cloudArtist, string cloudAlbum)
        {
            this.Text = "选择元数据";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // 文件信息
            Label fileLabel = new Label
            {
                Text = $"文件: {fileName}",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };
            
            // 标题选择
            GroupBox titleGroup = new GroupBox
            {
                Text = "标题",
                Location = new Point(20, 60),
                Size = new Size(450, 60)
            };
            
            RadioButton titleLocalRadio = new RadioButton
            {
                Text = $"本地: {localTitle}",
                Location = new Point(10, 20),
                Size = new Size(200, 20),
                Checked = true
            };
            
            RadioButton titleCloudRadio = new RadioButton
            {
                Text = $"云端: {cloudTitle}",
                Location = new Point(220, 20),
                Size = new Size(200, 20)
            };
            
            titleGroup.Controls.Add(titleLocalRadio);
            titleGroup.Controls.Add(titleCloudRadio);
            
            // 艺术家选择
            GroupBox artistGroup = new GroupBox
            {
                Text = "艺术家",
                Location = new Point(20, 130),
                Size = new Size(450, 60)
            };
            
            RadioButton artistLocalRadio = new RadioButton
            {
                Text = $"本地: {localArtist}",
                Location = new Point(10, 20),
                Size = new Size(200, 20),
                Checked = true
            };
            
            RadioButton artistCloudRadio = new RadioButton
            {
                Text = $"云端: {cloudArtist}",
                Location = new Point(220, 20),
                Size = new Size(200, 20)
            };
            
            artistGroup.Controls.Add(artistLocalRadio);
            artistGroup.Controls.Add(artistCloudRadio);
            
            // 专辑选择
            GroupBox albumGroup = new GroupBox
            {
                Text = "专辑",
                Location = new Point(20, 200),
                Size = new Size(450, 60)
            };
            
            RadioButton albumLocalRadio = new RadioButton
            {
                Text = $"本地: {localAlbum}",
                Location = new Point(10, 20),
                Size = new Size(200, 20),
                Checked = true
            };
            
            RadioButton albumCloudRadio = new RadioButton
            {
                Text = $"云端: {cloudAlbum}",
                Location = new Point(220, 20),
                Size = new Size(200, 20)
            };
            
            albumGroup.Controls.Add(albumLocalRadio);
            albumGroup.Controls.Add(albumCloudRadio);
            
            // 确定按钮
            Button okButton = new Button
            {
                Text = "确定",
                Location = new Point(350, 270),
                Size = new Size(60, 25)
            };
            okButton.Click += (s, e) => 
            {
                _titleResult = titleLocalRadio.Checked ? localTitle : cloudTitle;
                _artistResult = artistLocalRadio.Checked ? localArtist : cloudArtist;
                _albumResult = albumLocalRadio.Checked ? localAlbum : cloudAlbum;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            
            // 取消按钮
            Button cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(280, 270),
                Size = new Size(60, 25)
            };
            cancelButton.Click += (s, e) => 
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            
            // 批量操作：全部使用本地
            Button useLocalAllButton = new Button
            {
                Text = "全部使用本地",
                Location = new Point(20, 270),
                Size = new Size(100, 25)
            };
            useLocalAllButton.Click += (s, e) => 
            {
                titleLocalRadio.Checked = true;
                artistLocalRadio.Checked = true;
                albumLocalRadio.Checked = true;
            };
            
            // 批量操作：全部使用云端
            Button useCloudAllButton = new Button
            {
                Text = "全部使用云端",
                Location = new Point(130, 270),
                Size = new Size(100, 25)
            };
            useCloudAllButton.Click += (s, e) => 
            {
                titleCloudRadio.Checked = true;
                artistCloudRadio.Checked = true;
                albumCloudRadio.Checked = true;
            };
            
            // 添加到窗体
            this.Controls.Add(fileLabel);
            this.Controls.Add(titleGroup);
            this.Controls.Add(artistGroup);
            this.Controls.Add(albumGroup);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(useLocalAllButton);
            this.Controls.Add(useCloudAllButton);
        }
    }
}