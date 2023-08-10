using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryEditor2
{
    public class Tree
    {
        public Node BaseNode { set; get; }
        public Tree(Node node)
        {
            this.BaseNode = node;
        }
        public Node findNodeByPath(String path)
        {
           List<String> list = path.Split("\\").ToList<String>();
            list.RemoveAt(0);
            if (list.Count != 0)
            {
                Node nextNode = BaseNode.subNodes.GetValueOrDefault(list[0]);
                list.RemoveAt(0);
                return findNode(list, nextNode);
            }
            else return null;
        }
        private Node findNode(List<String> list, Node Node)
        {
            if (list.Count != 0)
            {
               Node nextNode= Node.subNodes.GetValueOrDefault(list[0]);
                list.RemoveAt(0);
                return findNode(list, nextNode);
            }
            else return Node;    
        }
    }
}
