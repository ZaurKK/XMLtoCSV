namespace XMLtoCSVconvertor
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
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
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonRun = new System.Windows.Forms.Button();
            this.outputFolderTextBoxButton = new XMLtoCSVconvertor.TextBoxButton();
            this.inputFolderTextBoxButton = new XMLtoCSVconvertor.TextBoxButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(423, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Папка для выгрузки файлов и работы с ними";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(423, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(227, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Папка по умолчанию для диспансеризации";
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(299, 64);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 6;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // outputFolderTextBoxButton
            // 
            this.outputFolderTextBoxButton.ButtonText = "...";
            this.outputFolderTextBoxButton.Location = new System.Drawing.Point(12, 38);
            this.outputFolderTextBoxButton.Name = "outputFolderTextBoxButton";
            this.outputFolderTextBoxButton.Size = new System.Drawing.Size(405, 20);
            this.outputFolderTextBoxButton.TabIndex = 8;
            this.outputFolderTextBoxButton.Text = "D:\\Data\\Диспансеризация\\2019\\Списки от МО\\22.11.2019\\";
            // 
            // inputFolderTextBoxButton
            // 
            this.inputFolderTextBoxButton.ButtonText = "...";
            this.inputFolderTextBoxButton.Location = new System.Drawing.Point(12, 12);
            this.inputFolderTextBoxButton.Name = "inputFolderTextBoxButton";
            this.inputFolderTextBoxButton.Size = new System.Drawing.Size(405, 20);
            this.inputFolderTextBoxButton.TabIndex = 7;
            this.inputFolderTextBoxButton.Text = "\\\\mainserver\\vipnetprocess\\prof2019\\";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 93);
            this.Controls.Add(this.outputFolderTextBoxButton);
            this.Controls.Add(this.inputFolderTextBoxButton);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonRun;
        private TextBoxButton inputFolderTextBoxButton;
        private TextBoxButton outputFolderTextBoxButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}

