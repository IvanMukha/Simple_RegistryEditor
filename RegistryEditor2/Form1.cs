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
using System.Security.AccessControl;
using System.Security.Principal;

namespace RegistryEditor2
{
    public partial class Form1 : Form
    {
        private Tree tree;
        private int currentIndex=-1;
        public Form1()
        {
            RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"AxControls.PMFActiveX");
            RegistrySecurity rs = new RegistrySecurity();
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            rs.AddAccessRule(new RegistryAccessRule(new SecurityIdentifier(WellKnownSidType.AccountDomainUsersSid, id.User.AccountDomainSid), RegistryRights.FullControl, AccessControlType.Allow));
            key.SetAccessControl(rs);
            InitializeComponent();
            tree = new Tree(new Node(null, treeView1.Nodes.Add("Computer"),null));
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            tree.BaseNode.subNodes.Add(Registry.ClassesRoot.Name,new Node(tree.BaseNode, treeView1.Nodes[0].Nodes.Add(Registry.ClassesRoot.Name), Registry.ClassesRoot));
            tree.BaseNode.subNodes.Add(Registry.CurrentUser.Name,new Node(tree.BaseNode, treeView1.Nodes[0].Nodes.Add(Registry.CurrentUser.Name), Registry.CurrentUser));
            tree.BaseNode.subNodes.Add(Registry.LocalMachine.Name,new Node(tree.BaseNode, treeView1.Nodes[0].Nodes.Add(Registry.LocalMachine.Name), Registry.LocalMachine));
            tree.BaseNode.subNodes.Add(Registry.Users.Name,new Node(tree.BaseNode, treeView1.Nodes[0].Nodes.Add(Registry.Users.Name), Registry.Users));
            tree.BaseNode.subNodes.Add(Registry.CurrentConfig.Name,new Node(tree.BaseNode, treeView1.Nodes[0].Nodes.Add(Registry.CurrentConfig.Name), Registry.CurrentConfig));           
            treeView1.Nodes[0].Expand();
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode nowTreeNode = e.Node;
            Node node = tree.findNodeByPath(nowTreeNode.FullPath);
            if (node != null) node.Open();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentIndex = -1;
            dataGridView1.Rows.Clear();
            TreeNode nowTreeNode = e.Node;
            Node node = tree.findNodeByPath(nowTreeNode.FullPath);
            if (node != null && node.registryKey!=null)
            {
                string[] values = node.registryKey.GetValueNames();

                foreach (String str in values)
                {
                    RegistryValueKind kind = node.registryKey.GetValueKind(str);
                    String value = getStringByType(kind, node.registryKey.GetValue(str));
                    dataGridView1.Rows.Add(str, kind, value);
                }
            }
        }
        private String getStringByType(RegistryValueKind kind, object value)
        {
            switch(kind)
            {
                case RegistryValueKind.Binary:
                    StringBuilder result = new StringBuilder();
                    foreach (byte b in (byte[])value) result.Append(Convert.ToString(b, 16)+" ");
                    return result.ToString();
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    try
                    {
                        String resultStr;
                        if (kind == RegistryValueKind.DWord) resultStr = Convert.ToString((Int32)value, 16); else resultStr = Convert.ToString((Int64)value, 16);
                        return "0x" + resultStr + " (" + value + ")";
                    }catch(Exception e) { return "Недопустимый параметр Dword"; }
                default: return value.ToString();
            }
        }
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                DialogResult res = MessageBox.Show("Вы уверены что хотите удалить этот элемент?", "Подтверждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (res == DialogResult.OK)
                {
                    Node node = tree.findNodeByPath(treeView1.SelectedNode.FullPath);
                    node.Remove();
                }
            }
        }

        private void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ((TreeView)sender).SelectedNode = ((TreeView)sender).GetNodeAt(e.X, e.Y);
                ((TreeView)sender).Focus();
            }
        }

        private void addSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node node = tree.findNodeByPath(treeView1.SelectedNode.FullPath);
                EnterName enterName = new EnterName(node);
                enterName.ShowDialog();
                if (enterName.DialogResult == DialogResult.OK && enterName.Name != null)
                {
                    node.AddSubNode(enterName.Name);
                    treeView1.SelectedNode.Expand();
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node node = tree.findNodeByPath(treeView1.SelectedNode.FullPath);
                EnterName enterName = new EnterName(node.parentNode);
                enterName.ShowDialog();
                if (enterName.DialogResult == DialogResult.OK && enterName.Name != null)
                {
                    node.Rename(enterName.Name);
                    
                }
            }
        }

        private void addParameterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node node = tree.findNodeByPath(treeView1.SelectedNode.FullPath);
                if (node!=null && node.registryKey != null)
                {
                    AddFile addFile = new AddFile(node.registryKey);
                    addFile.ShowDialog();
                    if (addFile.DialogResult == DialogResult.OK && addFile.Name != null && addFile.Value != null)
                    {
                        String name = addFile.Name;
                        node.registryKey.SetValue(name, addFile.Value);
                        RegistryValueKind kind = node.registryKey.GetValueKind(name);
                        String value = getStringByType(kind, node.registryKey.GetValue(name));
                        dataGridView1.Rows.Add(name, kind, value);
                    }
                }
            }
        }

        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node node = tree.findNodeByPath(treeView1.SelectedNode.FullPath);
                if (node != null && node.registryKey != null && currentIndex != -1)
                {
                    DialogResult res = MessageBox.Show("Вы уверены что хотите удалить этот элемент?", "Подтверждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.OK)
                    {
                        node.registryKey.DeleteValue(dataGridView1.Rows[currentIndex].Cells[0].Value.ToString());
                        dataGridView1.Rows.RemoveAt(currentIndex);
                    }
                }
                currentIndex = -1;
            }
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            currentIndex = e.RowIndex;
        }

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Node node = tree.findNodeByPath(treeView1.SelectedNode.FullPath);
                if (node != null && node.registryKey != null && currentIndex != -1)
                {
                    String NAME = dataGridView1.Rows[currentIndex].Cells[0].Value.ToString();
                    AddFile addFile = new AddFile(node.registryKey,NAME);
                    addFile.ShowDialog();
                    if (addFile.DialogResult == DialogResult.OK && addFile.Name != null && addFile.Value != null)
                    {
                        String name = addFile.Name;
                        node.registryKey.DeleteValue(dataGridView1.Rows[currentIndex].Cells[0].Value.ToString());
                        dataGridView1.Rows.RemoveAt(currentIndex);
                        node.registryKey.SetValue(name, addFile.Value);
                        RegistryValueKind kind = node.registryKey.GetValueKind(name);
                        String value = getStringByType(kind, node.registryKey.GetValue(name));
                        dataGridView1.Rows.Add(name, kind, value);
                    }
                }
                currentIndex = -1;
            }
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Find().Show();
        }
    }
}
