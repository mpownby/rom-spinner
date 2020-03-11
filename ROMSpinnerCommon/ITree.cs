using System;
using System.Collections.Generic;
using System.Text;

namespace ROMSpinner.Common
{
    public interface ITreeView
    {
        void CollapseAll();

        ITreeNode NewNode();

        void AddChild(ITreeNode node);
    }

    public interface ITreeNode
    {
        string Text
        {
            get;
            set;
        }

        string Icon
        {
            get;
            set;
        }

        ITreeNode Parent
        {
            get;
        }

        /// <summary>
        /// Add a child node to this node.
        /// </summary>
        /// <param name="node"></param>
        void AddChild(ITreeNode node);

        void Expand();

        void Collapse();

        ITreeNode NewNode();

        ITreeNode NewNode(string Text);
    }
}
