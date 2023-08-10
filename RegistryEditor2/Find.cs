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
using System.Text.RegularExpressions;
using System.Threading;
namespace RegistryEditor2
{
    public partial class Find : Form
    {

        public Find()
        {
            InitializeComponent();
        }

        volatile List<Tuple<string, string, string>> FindResult = new();

        private static Regex regex;
        private void FindRegex(object parent)
        {
            RegistryKey Parent = (RegistryKey)parent;
            try
            {
                foreach (var nameVar in Parent.GetValueNames())
                {
                    if (regex.IsMatch(nameVar))
                    {
                        var n = Tuple.Create("Key(" + nameVar + ")", Parent.Name + "->" + nameVar, Parent.GetValue(nameVar).ToString());
                        FindResult.Add(n);
                    }
                    if (regex.IsMatch(Parent.GetValue(nameVar).ToString()))
                    {
                        var n = Tuple.Create("Value(" + Parent.GetValue(nameVar).ToString() + ")", Parent.Name + "->" + nameVar, Parent.GetValue(nameVar).ToString());
                        FindResult.Add(n);
                    }
                }

            }
            catch (Exception ex) { }
            try
            {
                foreach (var nameKey in Parent.GetSubKeyNames())
                {
                    if (regex.IsMatch(nameKey))
                    {
                        var n = Tuple.Create("Key(" + nameKey + ")", Parent.Name + "->" + nameKey, "<Dictionary>");
                        FindResult.Add(n);
                    }
                    FindRegex(Parent.OpenSubKey(nameKey));
                }
            }
            catch (Exception ex) { }
        }


        private void btnFind_Click(object sender, EventArgs e)
        {
            btnFind.Text="Processing find...";
            FindResult.Clear();
            dataGridView1.Rows.Clear();
            Thread[] threads = {
                new Thread(FindRegex),
                new Thread(FindRegex),
                new Thread(FindRegex),
                new Thread(FindRegex),
                new Thread(FindRegex),
            };
            RegistryKey[] Roots ={
                Registry.LocalMachine,
                Registry.ClassesRoot,
                Registry.CurrentConfig,
                Registry.CurrentUser,
                Registry.Users
            };
            string pattern = txtpattern.Text;
            regex = new Regex("^" + pattern + "$");
            for (int i = 0; i < 5; i++)
            {
                threads[i].Start(Roots[i]);
            }

            for (int i = 0; i < 5; i++)
            {
                threads[i].Join();
            }
            foreach (var line in FindResult)
            {
                string[] n = { line.Item1, line.Item2, line.Item3 };
                dataGridView1.Rows.Add(n);
            }
            btnFind.Text = "Start Find";
           
        }
        private void Search()
        {

        }
    }
}
