namespace XMLtoCSVconvertor
{
    partial class XmlToCsvConverter
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
            this.inputFolderButtonEdit = new DevExpress.XtraEditors.ButtonEdit();
            this.outputFolderButtonEdit = new DevExpress.XtraEditors.ButtonEdit();
            ((System.ComponentModel.ISupportInitialize)(this.inputFolderButtonEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputFolderButtonEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Папка для выгрузки файлов и работы с ними";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(227, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Папка по умолчанию для диспансеризации";
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(118, 102);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 6;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // inputFolderButtonEdit
            // 
            this.inputFolderButtonEdit.EditValue = "\\\\mainserver\\vipnetprocess\\prof2021\\";
            this.inputFolderButtonEdit.Location = new System.Drawing.Point(15, 25);
            this.inputFolderButtonEdit.Name = "inputFolderButtonEdit";
            this.inputFolderButtonEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.inputFolderButtonEdit.Size = new System.Drawing.Size(281, 20);
            this.inputFolderButtonEdit.TabIndex = 9;
            this.inputFolderButtonEdit.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.BrowseFolderButtonEdit_ButtonClick);
            // 
            // outputFolderButtonEdit
            // 
            this.outputFolderButtonEdit.EditValue = "D:\\Data\\Диспансеризация\\2021\\Списки\\";
            this.outputFolderButtonEdit.Location = new System.Drawing.Point(15, 72);
            this.outputFolderButtonEdit.Name = "outputFolderButtonEdit";
            this.outputFolderButtonEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.outputFolderButtonEdit.Size = new System.Drawing.Size(281, 20);
            this.outputFolderButtonEdit.TabIndex = 10;
            this.outputFolderButtonEdit.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.BrowseFolderButtonEdit_ButtonClick);
            // 
            // XmlToCsvConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 135);
            this.Controls.Add(this.outputFolderButtonEdit);
            this.Controls.Add(this.inputFolderButtonEdit);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "XmlToCsvConverter";
            this.Text = "Конвертер XML в CSV";
            ((System.ComponentModel.ISupportInitialize)(this.inputFolderButtonEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputFolderButtonEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private DevExpress.XtraEditors.ButtonEdit inputFolderButtonEdit;
        private DevExpress.XtraEditors.ButtonEdit outputFolderButtonEdit;
    }
}

