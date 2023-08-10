using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegistryEditor2
{
    public partial class EnterName : Form
    {
        private Node node;
        public String Name { get; set; }
        public EnterName(Node node)
        {
            InitializeComponent();
            this.node = node;
        }

        private void EnterName_Load(object sender, EventArgs e)
        {
            buttonCancel.DialogResult = DialogResult.Cancel;
            textBox1.Focus();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            String name = textBox1.Text.Trim();
            if (!String.IsNullOrEmpty(name) && !node.subNodes.ContainsKey(name))
            {
                Name = name;
                this.DialogResult = DialogResult.OK;
            }
            else errorProvider1.SetError(textBox1, "Строка пуста или узел с так именем уже существует!");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }
    }
}
