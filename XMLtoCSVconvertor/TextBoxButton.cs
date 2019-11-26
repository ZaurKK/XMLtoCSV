using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XMLtoCSVconvertor
{
    public partial class TextBoxButton : UserControl
    {
        public TextBoxButton()
        {
            InitializeComponent();
        }

        [Category("Custom")]
        [Description("Text displayed in the textbox")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
            }
        }

        [Category("Custom")]
        [Description("Text displayed in the button")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ButtonText
        {
            get
            {
                return button.Text;
            }
            set
            {
                button.Text = value;
            }
        }

        [Category("Custom")]
        [Description("Folder browser dialog")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public FolderBrowserDialog FolderBrowserDialog
        {
            get
            {
                return folderBrowserDialog;
            }
            set
            {
                folderBrowserDialog = value;
            }
        }

        public new event EventHandler Click
        {
            add
            {
                base.Click += value;
                foreach (Control control in Controls)
                    control.Click += value;
            }
            remove
            {
                base.Click -= value;
                foreach (Control control in Controls)
                    control.Click -= value;
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = textBox.Text;
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                textBox.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
