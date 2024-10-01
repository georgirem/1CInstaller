using System.Drawing;
using System.Windows.Forms;

namespace _1CInstaller
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Update = new System.Windows.Forms.Button();
            this.downloadStatusLabel = new System.Windows.Forms.Label();
            this.login1C = new System.Windows.Forms.Button();
            this.richTextBoxVersions = new System.Windows.Forms.RichTextBox();
            this.pingTimer = new System.Windows.Forms.Timer(this.components);
            this.Settings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Update
            // 
            this.Update.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Update.Location = new System.Drawing.Point(47, 189);
            this.Update.Name = "Update";
            this.Update.Size = new System.Drawing.Size(413, 50);
            this.Update.TabIndex = 1;
            this.Update.Text = "Обновить";
            this.Update.UseVisualStyleBackColor = true;
            this.Update.Click += new System.EventHandler(this.Update_Click);
            // 
            // downloadStatusLabel
            // 
            this.downloadStatusLabel.AutoSize = true;
            this.downloadStatusLabel.Location = new System.Drawing.Point(175, 246);
            this.downloadStatusLabel.Name = "downloadStatusLabel";
            this.downloadStatusLabel.Size = new System.Drawing.Size(0, 13);
            this.downloadStatusLabel.TabIndex = 4;
            // 
            // login1C
            // 
            this.login1C.Location = new System.Drawing.Point(6, 246);
            this.login1C.Name = "login1C";
            this.login1C.Size = new System.Drawing.Size(130, 23);
            this.login1C.TabIndex = 5;
            this.login1C.Text = "Авторизация";
            this.login1C.UseVisualStyleBackColor = true;
            this.login1C.Click += new System.EventHandler(this.login1C_Click);
            // 
            // richTextBoxVersions
            // 
            this.richTextBoxVersions.Location = new System.Drawing.Point(6, 3);
            this.richTextBoxVersions.Name = "richTextBoxVersions";
            this.richTextBoxVersions.ReadOnly = true;
            this.richTextBoxVersions.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxVersions.Size = new System.Drawing.Size(502, 171);
            this.richTextBoxVersions.TabIndex = 6;
            this.richTextBoxVersions.Text = "";
            // 
            // pingTimer
            // 
            this.pingTimer.Enabled = true;
            this.pingTimer.Interval = 60000;
            // 
            // Settings
            // 
            this.Settings.BackColor = System.Drawing.Color.Transparent;
            this.Settings.BackgroundImage = global::_1CInstaller.Properties.Resources.icons8_settings_32;
            this.Settings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Settings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Settings.Location = new System.Drawing.Point(476, 243);
            this.Settings.Margin = new System.Windows.Forms.Padding(0);
            this.Settings.Name = "Settings";
            this.Settings.Size = new System.Drawing.Size(32, 32);
            this.Settings.TabIndex = 7;
            this.Settings.UseVisualStyleBackColor = false;
            this.Settings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 278);
            this.Controls.Add(this.Settings);
            this.Controls.Add(this.richTextBoxVersions);
            this.Controls.Add(this.login1C);
            this.Controls.Add(this.downloadStatusLabel);
            this.Controls.Add(this.Update);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Установщик 1С";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button Update;
        private Label downloadStatusLabel;
        private Button login1C;
        private RichTextBox richTextBoxVersions;
        private Timer pingTimer;
        private Button Settings;
    }
}

