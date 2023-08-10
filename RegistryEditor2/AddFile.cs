using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RegistryEditor2
{
    public partial class AddFile : Form
    {
        private RegistryKey registryKey;
        public String Name { get; set; }
        public object Value { get; set; }
        private String NAME;
        public AddFile(RegistryKey registryKey)
        {
            InitializeComponent();
            this.registryKey = registryKey;
        }
        public AddFile(RegistryKey registryKey, String Name)
        {
            InitializeComponent();
            this.registryKey = registryKey;
            this.NAME = Name;
        }
        private String getStringByType(RegistryValueKind kind, object value)
        {
            switch (kind)
            {
                case RegistryValueKind.Binary:
                    StringBuilder result = new StringBuilder();
                    foreach (byte b in (byte[])value) result.Append(Convert.ToString(b, 16) + " ");
                    return result.ToString();
                case RegistryValueKind.DWord:
                default: return value.ToString();
            }
        }
        private void AddFile_Load(object sender, EventArgs e)
        {
            buttonCancel.DialogResult = DialogResult.Cancel;
            comboBox1.Items.Add(RegistryValueKind.Binary);
            comboBox1.Items.Add(RegistryValueKind.DWord);
            comboBox1.Items.Add(RegistryValueKind.String);
            comboBox1.SelectedItem = comboBox1.Items[0];
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            if (NAME != null)
            {
                RegistryValueKind kind = registryKey.GetValueKind(NAME);
                textBox1.Text = NAME;
                textBox2.Text = getStringByType(kind, registryKey.GetValue(NAME));
                int index;
                if (kind == RegistryValueKind.Binary) index = 0;
                else if (kind == RegistryValueKind.DWord) index = 1;
                else if (kind == RegistryValueKind.String) index = 2; else
                 index = comboBox1.Items.Add(kind);
                comboBox1.SelectedItem = comboBox1.Items[index];
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            String name = textBox1.Text.Trim();
            String value = textBox2.Text.Trim();
            if (!String.IsNullOrEmpty(name) && (!registryKey.GetValueNames().Contains(name) || NAME!=null && NAME==name))
            {
                if (ValidateValue(value,comboBox1.Text, out object result))
                {
                    Name = name;
                    Value = result;
                    this.DialogResult = DialogResult.OK;
                }
                else errorProvider2.SetError(textBox2,"Значение параметра не соответствует выбранному типу!");
            }
            else errorProvider1.SetError(textBox1, "Строка пуста или параметр с так именем уже существует!");
        }
        private bool ValidateValue(String value, String Type, out object result)
        {
            try
            {
                switch (Type)
                {
                    case "Binary":
                        if (String.IsNullOrEmpty(value)) { result = null;return false; }
                        String[] strs = value.Split(" ");
                        byte[] bytes = new byte[strs.Length];
                        for (int i = 0; i < strs.Length; i++) bytes[i] = Convert.ToByte(strs[i].Trim(), 16);
                        result = bytes;
                        return true;
                    case "DWord": result = Convert.ToInt32(value); return true;
                    default: result = value; return true;
                }
            }
            catch (Exception e) { result = null;return false; }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            errorProvider2.Clear();
        }
    }
}
