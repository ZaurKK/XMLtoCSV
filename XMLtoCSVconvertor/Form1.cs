using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;


namespace XMLtoCSVconvertor
{
    public partial class Form1 : Form
    {
        XmlFilesProcessor xmlFilesProcessor = new XmlFilesProcessor();

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            //if (xmlFilesProcessor.ProcessFiles(inputFolderTextBox.Text, outputFolderTextBox.Text))
            if (xmlFilesProcessor.ProcessFiles(inputFolderTextBoxButton.Text, outputFolderTextBoxButton.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Файлы сформированы!\r\n\r\nВыйти из программы?", "Операция завершена", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                    Application.Exit();
            }
        }
    }

    class ProductComparer : EqualityComparer<XElement>
    {
        // XElements are equal if their names and product numbers are equal.
        public override bool Equals(XElement x, XElement y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the xelements' properties are equal.
            return x.Value == y.Value;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode(XElement element)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(element, null)) return 0;

            //Calculate the hash code for the product.
            return element.Value.GetHashCode();
        }
    }
}
