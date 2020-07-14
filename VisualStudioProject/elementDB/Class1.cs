using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elementDB
{
    public class Node
    {
        private string m_data;
        private List<Node> m_children = new List<Node>();

        public Node(string data)
        {
            m_data = data;
        }

        public Node()
        {
          
        }

        public Node getChild (int i)
        {
            return m_children[i];
        }

        public Node getChild(string data)
        {
            foreach (Node node in m_children)
            {
                if (node.getData().Equals(data))
                {
                    return node;
                }
            }
            return new Node();
        }

        public Node getLastChild()
        {
            return m_children.Last();
        }

        public Node this[int i]
        {
            get { return m_children[i]; }
            set { m_children[i] = value; }
        }

        public void addNode(string data)
        {
            m_children.Add(new Node(data));
        }

        public void setData(string data)
        {
            m_data = data;
        }

        public string getData()
        {
            return m_data;
        }

        public List<string> getChildrensData()
        {
            List<string> dataList = new List<string>();
            foreach (Node node in m_children)
            {
                dataList.Add(node.m_data);
            }
            return dataList;
        }

        public int getChildrenCount()
        {
            return m_children.Count;
        }
    }

}
