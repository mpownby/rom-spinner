using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ROMSpinner.Common;

namespace ROMSpinner.Win
{
    public class WinTreeView : ITreeView
    {
        private TreeView m_treeView;
        private ImageList m_ImageList = new ImageList();
        private Hashtable m_hashImages = new Hashtable();

        public WinTreeView(TreeView view)
        {
            m_treeView = view;
            m_treeView.ImageList = m_ImageList;
        }

        public void CollapseAll()
        {
            m_treeView.CollapseAll();
        }

        public ITreeNode NewNode()
        {
            return new WinTreeNode(this);
        }

        public void AddChild(ITreeNode node)
        {
            TreeNode nodeWin = ((WinTreeNode)node).Me;
            m_treeView.Nodes.Add(nodeWin);
        }

        public int GetImageIdx(string strImgPath)
        {
            int iResult = 0;

            // if the image has already been loaded, then return its index
            if (m_hashImages.ContainsKey(strImgPath))
            {
                iResult = (int) m_hashImages[strImgPath];
            }
                // else the image hasn't been loaded, so load it, and add it to our stuff
            else
            {
                iResult = m_ImageList.Images.Count;
                m_hashImages[strImgPath] = iResult;
                Image img = Image.FromFile(strImgPath);
                m_ImageList.Images.Add(img);
            }
            return iResult;
        }
    }

    public class WinTreeNode : ITreeNode
    {
        TreeNode m_node;
        WinTreeView m_view;

        public WinTreeNode(WinTreeView view)
        {
            m_node = new TreeNode();
            m_view = view;
            Icon = "blank16.png";
        }

        public string Text
        {
            get
            {
                return m_node.Text;
            }
            set
            {
                m_node.Text = value;
            }
        }

        public string Icon
        {
            get
            {
                return "";
            }
            set
            {
                m_node.ImageIndex = m_node.SelectedImageIndex = m_view.GetImageIdx(value);
            }
        }

        public ITreeNode Parent
        {
            get
            {
                WinTreeNode nodeWin = null;
                if (m_node.Parent != null)
                {
                    nodeWin = new WinTreeNode(m_view);
                    nodeWin.m_node = m_node.Parent;
                }
                return nodeWin;
            }
        }

        public TreeNode Me
        {
            get
            {
                return m_node;
            }
        }

        public void AddChild(ITreeNode node)
        {
            TreeNode nodeWin = ((WinTreeNode)node).m_node;
            m_node.Nodes.Add(nodeWin);
        }

        public void Expand()
        {
            m_node.Expand();
        }

        public void Collapse()
        {
            m_node.Collapse();
        }

        public ITreeNode NewNode()
        {
            return new WinTreeNode(m_view);
        }

        public ITreeNode NewNode(string Text)
        {
            WinTreeNode node = new WinTreeNode(m_view);
            node.Text = Text;
            return node;
        }
    }
}
