using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using System.Linq;

namespace MP3Player
{
    public class MetadataSourceForm : Forms.Form
    {
        // 使用字段代替属性
        private string _selectedTitle = "";
        private string _selectedArtist = "";
        private string _selectedAlbum = "";
        
        // 公共获取方法
        public string GetSelectedTitle() => _selectedTitle;
        public string GetSelectedArtist() => _selectedArtist;
        public string GetSelectedAlbum() => _selectedAlbum;
        
        public MetadataSourceForm(string fileName, 
                                 string localTitle, string localArtist, string localAlbum,
                                 string cloudTitle, string cloudArtist, string cloudAlbum)
        {
            BuildForm(fileName, localTitle, localArtist, localAlbum, 
                     cloudTitle, cloudArtist, cloudAlbum);
        }
        
        private void BuildForm(string fileName, 
                              string localTitle, string localArtist, string localAlbum,
                              string cloudTitle, string cloudArtist, string cloudAlbum)
        {
            this.Text = "选择元数据来源";
            this.Size = new Drawing.Size(800, 500);
            this.StartPosition = Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // 文件信息
            var fileLabel = new Forms.Label
            {
                Text = $"文件: {fileName}",
                Location = new Drawing.Point(20, 20),
                AutoSize = true,
                Font = new Drawing.Font("微软雅黑", 10, Drawing.FontStyle.Bold)
            };
            
            // 创建对比表格
            var table = new Forms.DataGridView
            {
                Location = new Drawing.Point(20, 60),
                Size = new Drawing.Size(740, 300),
                ColumnCount = 4,
                RowCount = 3,
                ReadOnly = false,
                AllowUserToAddRows = false
            };
            
            // 设置列
            table.Columns[0].HeaderText = "字段";
            table.Columns[1].HeaderText = "本地元数据";
            table.Columns[2].HeaderText = "云端识别";
            table.Columns[3].HeaderText = "使用本地";
            table.Columns[3].ValueType = typeof(bool);
            
            // 填充数据
            table.Rows[0].Cells[0].Value = "标题";
            table.Rows[0].Cells[1].Value = localTitle;
            table.Rows[0].Cells[2].Value = cloudTitle;
            table.Rows[0].Cells[3].Value = true;
            
            table.Rows[1].Cells[0].Value = "艺术家";
            table.Rows[1].Cells[1].Value = localArtist;
            table.Rows[1].Cells[2].Value = cloudArtist;
            table.Rows[1].Cells[3].Value = true;
            
            table.Rows[2].Cells[0].Value = "专辑";
            table.Rows[2].Cells[1].Value = localAlbum;
            table.Rows[2].Cells[2].Value = cloudAlbum;
            table.Rows[2].Cells[3].Value = true;
            
            // 批量操作按钮
            var useLocalAllButton = new Forms.Button
            {
                Text = "全部使用本地",
                Location = new Drawing.Point(20, 370),
                Size = new Drawing.Size(120, 30)
            };
            useLocalAllButton.Click += (s, e) => SelectAllRows(table, true);
            
            var useCloudAllButton = new Forms.Button
            {
                Text = "全部使用云端",
                Location = new Drawing.Point(150, 370),
                Size = new Drawing.Size(120, 30)
            };
            useCloudAllButton.Click += (s, e) => SelectAllRows(table, false);
            
            // 确认按钮
            var confirmButton = new Forms.Button
            {
                Text = "确定",
                Location = new Drawing.Point(600, 370),
                Size = new Drawing.Size(80, 30),
                BackColor = Drawing.Color.LightGreen
            };
            confirmButton.Click += (s, e) => SaveSelections(table);
            
            // 取消按钮
            var cancelButton = new Forms.Button
            {
                Text = "取消",
                Location = new Drawing.Point(500, 370),
                Size = new Drawing.Size(80, 30)
            };
            cancelButton.Click += (s, e) => this.Close();
            
            // 添加到窗体
            this.Controls.Add(fileLabel);
            this.Controls.Add(table);
            this.Controls.Add(useLocalAllButton);
            this.Controls.Add(useCloudAllButton);
            this.Controls.Add(confirmButton);
            this.Controls.Add(cancelButton);
        }
        
        private void SelectAllRows(Forms.DataGridView table, bool useLocal)
        {
            foreach (Forms.DataGridViewRow row in table.Rows)
            {
                row.Cells[3].Value = useLocal;
            }
        }
        
        private void SaveSelections(Forms.DataGridView table)
        {
            // 保存选择结果
            _selectedTitle = (bool)table.Rows[0].Cells[3].Value 
                ? table.Rows[0].Cells[1].Value?.ToString() ?? ""
                : table.Rows[0].Cells[2].Value?.ToString() ?? "";
            
            _selectedArtist = (bool)table.Rows[1].Cells[3].Value 
                ? table.Rows[1].Cells[1].Value?.ToString() ?? ""
                : table.Rows[1].Cells[2].Value?.ToString() ?? "";
            
            _selectedAlbum = (bool)table.Rows[2].Cells[3].Value 
                ? table.Rows[2].Cells[1].Value?.ToString() ?? ""
                : table.Rows[2].Cells[2].Value?.ToString() ?? "";
            
            this.DialogResult = Forms.DialogResult.OK;
            this.Close();
        }
    }
}