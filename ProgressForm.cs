using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace MP3Player
{
    public partial class ProgressForm : Forms.Form
    {
        private Forms.ProgressBar progressBar;
        private Forms.Label statusLabel;
        private Forms.Button cancelButton;
        
        // 改为字段，而不是自动属性
        private bool _cancelled = false;
        public bool Cancelled => _cancelled;
        
        public ProgressForm(string title, int maxValue)
        {
            InitializeComponent(title, maxValue);
        }
        
        private void InitializeComponent(string title, int maxValue)
        {
            this.Text = title;
            this.Size = new Drawing.Size(400, 150);
            this.StartPosition = Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            
            // 进度条
            progressBar = new Forms.ProgressBar();
            progressBar.Location = new Drawing.Point(20, 20);
            progressBar.Size = new Drawing.Size(350, 30);
            progressBar.Minimum = 0;
            progressBar.Maximum = maxValue;
            progressBar.Value = 0;
            
            // 状态标签
            statusLabel = new Forms.Label();
            statusLabel.Location = new Drawing.Point(20, 60);
            statusLabel.Size = new Drawing.Size(350, 20);
            statusLabel.Text = "准备中...";
            
            // 取消按钮
            cancelButton = new Forms.Button();
            cancelButton.Text = "取消";
            cancelButton.Location = new Drawing.Point(150, 85);
            cancelButton.Size = new Drawing.Size(100, 30);
            cancelButton.Click += (s, e) => 
            {
                _cancelled = true;
                this.Close();
            };
            
            this.Controls.AddRange(new Forms.Control[] { progressBar, statusLabel, cancelButton });
        }
        
        public void UpdateProgress(int value, string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new System.Action<int, string>(UpdateProgress), value, status);
                return;
            }
            
            progressBar.Value = System.Math.Min(value, progressBar.Maximum);
            statusLabel.Text = status;
            Forms.Application.DoEvents(); // 更新UI
        }
    }
}