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
            this.Update = new System.Windows.Forms.Button();
            this.downloadStatusLabel = new System.Windows.Forms.Label();
            this.login1C = new System.Windows.Forms.Button();
            this.richTextBoxVersions = new System.Windows.Forms.RichTextBox();
            this.pingTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Update
            // 
            this.Update.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Update.Location = new System.Drawing.Point(178, 241);
            this.Update.Name = "Update";
            this.Update.Size = new System.Drawing.Size(146, 50);
            this.Update.TabIndex = 1;
            this.Update.Text = "Обновить";
            this.Update.UseVisualStyleBackColor = true;
            this.Update.Click += new System.EventHandler(this.Update_Click);
            // 
            // downloadStatusLabel
            // 
            this.downloadStatusLabel.AutoSize = true;
            this.downloadStatusLabel.Location = new System.Drawing.Point(175, 294);
            this.downloadStatusLabel.Name = "downloadStatusLabel";
            this.downloadStatusLabel.Size = new System.Drawing.Size(0, 13);
            this.downloadStatusLabel.TabIndex = 4;
            // 
            // login1C
            // 
            this.login1C.Location = new System.Drawing.Point(371, 255);
            this.login1C.Name = "login1C";
            this.login1C.Size = new System.Drawing.Size(130, 23);
            this.login1C.TabIndex = 5;
            this.login1C.Text = "Авторизация";
            this.login1C.UseVisualStyleBackColor = true;
            this.login1C.Click += new System.EventHandler(this.login1C_Click);
            // 
            // richTextBoxVersions
            // 
            this.richTextBoxVersions.Location = new System.Drawing.Point(6, 12);
            this.richTextBoxVersions.Name = "richTextBoxVersions";
            this.richTextBoxVersions.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxVersions.Size = new System.Drawing.Size(502, 225);
            this.richTextBoxVersions.TabIndex = 6;
            this.richTextBoxVersions.Text = "";
            // 
            // pingTimer
            // 
            this.pingTimer.Enabled = true;
            this.pingTimer.Interval = 60000;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 322);
            this.Controls.Add(this.richTextBoxVersions);
            this.Controls.Add(this.login1C);
            this.Controls.Add(this.downloadStatusLabel);
            this.Controls.Add(this.Update);
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
    }
}

