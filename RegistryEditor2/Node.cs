using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RegistryEditor2
{
    public class Node
    {
        public RegistryKey registryKey;
        private TreeNode TreeNode;
        private String[] strs;
        public Node parentNode;
        public Dictionary<String, Node> subNodes;
        public Node(Node parentNode, TreeNode TreeNode, RegistryKey registryKey)
        {
            this.parentNode = parentNode;
            this.TreeNode = TreeNode;
            this.registryKey = registryKey;
            this.subNodes = new Dictionary<String, Node>();
            fill();
        }
        private void fill()
        {
            if (registryKey != null)
            {
                strs = registryKey.GetSubKeyNames();
                if (strs != null && strs.Length !=0) TreeNode.Nodes.Add("Nodes");
            }
        }
        public void AddSubNode(String name)
        {
            if (strs != null && subNodes.Count == 0) Open();
            TreeNode nowTreeNode = TreeNode.Nodes.Add(name);
            RegistryKey newRegistryKey = this.registryKey.CreateSubKey(name);
            subNodes.Add(name, new Node(this,nowTreeNode, newRegistryKey));
        }
        public void Rename(String newName)
        {
            
            RegisterOperations registerOperations = new RegisterOperations();
            registerOperations.CopyKey(parentNode.registryKey, TreeNode.Text, newName);
            Remove();
            parentNode.Open();

        }
        public void Remove()
        {
            if(subNodes!=null)foreach (Node node in subNodes.Values) { node.Remove(); }
            if (parentNode != null)
            {
                parentNode.subNodes.Remove(TreeNode.Text);
                if (registryKey != null) parentNode.registryKey.DeleteSubKeyTree(TreeNode.Text);
            }
            TreeNode.Remove();
        }
        public void Open()
        {
            strs = registryKey.GetSubKeyNames();
            if (strs != null && (subNodes.Count==0 || strs.Length!= TreeNode.Nodes.Count))
            {
                TreeNode.Nodes.Clear();
                subNodes.Clear();
                foreach (String str in strs)
                {

                    TreeNode nowTreeNode = TreeNode.Nodes.Add(str);
                    RegistryKey nowRegistryKey;
                    try
                    {
                        nowRegistryKey = registryKey.OpenSubKey(str, true);

                    }
                    catch (Exception e) { nowRegistryKey = null; }
                    subNodes.Add(str, new Node(this,nowTreeNode, nowRegistryKey));
                }
            }
        }
    }
}
