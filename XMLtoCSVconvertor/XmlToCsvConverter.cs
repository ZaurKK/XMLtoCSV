using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Windows.Forms;
using System.Xml.Linq;


namespace XMLtoCSVconvertor
{
    public partial class XmlToCsvConverter : Form
    {
        static XmlFilesProcessor XmlFilesProcessor { get; set; } = new XmlFilesProcessor();

        public XmlToCsvConverter()
        {
            InitializeComponent();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            if (XmlFilesProcessor.ProcessFiles(inputFolderButtonEdit.Text, outputFolderButtonEdit.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Файлы сформированы!\r\n\r\nВыйти из программы?", "Операция завершена", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                    Application.Exit();
            }
        }

        private void BrowseFolderButtonEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            var buttonEdit = sender as ButtonEdit;

            folderBrowserDialog.SelectedPath = buttonEdit.Text;
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                buttonEdit.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
