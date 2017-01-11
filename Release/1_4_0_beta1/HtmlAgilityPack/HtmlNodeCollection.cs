// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;

namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a combined list and collection of HTML nodes.
    /// </summary>
    public class HtmlNodeCollection : IList<HtmlNode>
    {
        private readonly HtmlNode _parentnode;
        private readonly List<HtmlNode> items = new List<HtmlNode>();

        public HtmlNodeCollection(HtmlNode parentnode)
        {
            _parentnode = parentnode; // may be null
        }

        /// <summary>
        /// Gets a given node from the list.
        /// </summary>
        public int this[HtmlNode node]
        {
            get
            {
                int index = GetNodeIndex(node);
                if (index == -1)
                {
                    throw new ArgumentOutOfRangeException("node",
                                                          "Node \"" + node.CloneNode(false).OuterHtml +
                                                          "\" was not found in the collection");
                }
                return index;
            }
        }

        /// <summary>
        /// Get node with tag name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public HtmlNode this[string nodeName]
        {
            get
            {
                nodeName = nodeName.ToLower();
                for (int i = 0; i < items.Count; i++)
                    if (items[i].Equals(nodeName))
                        return items[i];

                return null;
            }
            }

        #region IList<HtmlNode> Members

        /// <summary>
        /// Gets the number of elements actually contained in the list.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        public void Clear()
        {
            foreach (HtmlNode node in items)
            {
                node._parentnode = null;
                node._nextnode = null;
                node._prevnode = null;
            }
            items.Clear();
        }

        /// <summary>
        /// Remove HtmlNode at index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            HtmlNode oldnode = items[index];

            if (index > 0)
            {
                prev = items[index - 1];
            }

            if (index < (items.Count - 1))
            {
                next = items[index + 1];
            }

            items.RemoveAt(index);

            if (prev != null)
            {
                if (next == prev)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                prev._nextnode = next;
            }

            if (next != null)
            {
                next._prevnode = prev;
            }

            oldnode._prevnode = null;
            oldnode._nextnode = null;
            oldnode._parentnode = null;
        }

        /// <summary>
        /// Insert node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Insert(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;

            if (index > 0)
            {
                prev = items[index - 1];
            }

            if (index < items.Count)
            {
                next = items[index];
            }

            items.Insert(index, node);

            if (prev != null)
            {
                if (node == prev)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                prev._nextnode = node;
            }

            if (next != null)
            {
                next._prevnode = node;
            }

            node._prevnode = prev;
            if (next == node)
            {
                throw new InvalidProgramException("Unexpected error.");
            }
            node._nextnode = next;
            node._parentnode = _parentnode;
        }

        /// <summary>
        /// Add node to the collection
        /// </summary>
        /// <param name="node"></param>
        public void Add(HtmlNode node)
        {
            items.Add(node);
        }

        /// <summary>
        /// Gets the node at the specified index.
        /// </summary>
        public HtmlNode this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        /// <summary>
        /// Get index of node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(HtmlNode item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        /// Gets existence of node in collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(HtmlNode item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Copy collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(HtmlNode[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Is collection readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(HtmlNode item)
        {
            int i = items.IndexOf(item);
            RemoveAt(i);
            return true;
        }

        /// <summary>
        /// Get Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator<HtmlNode> IEnumerable<HtmlNode>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Get Explicit Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Replace node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Replace(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            HtmlNode oldnode = items[index];

            if (index > 0)
            {
                prev = items[index - 1];
            }

            if (index < (items.Count - 1))
            {
                next = items[index + 1];
            }

            items[index] = node;

            if (prev != null)
            {
                if (node == prev)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                prev._nextnode = node;
            }

            if (next != null)
            {
                next._prevnode = node;
            }

            node._prevnode = prev;

            if (next == node)
            {
                throw new InvalidProgramException("Unexpected error.");
            }

            node._nextnode = next;
            node._parentnode = _parentnode;

            oldnode._prevnode = null;
            oldnode._nextnode = null;
            oldnode._parentnode = null;
        }

        /// <summary>
        /// Add node to the end of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Append(HtmlNode node)
        {
            HtmlNode last = null;
            if (items.Count > 0)
            {
                last = items[items.Count - 1];
            }

            items.Add(node);
            node._prevnode = last;
            node._nextnode = null;
            node._parentnode = _parentnode;
            if (last != null)
            {
                if (last == node)
                {
                    throw new InvalidProgramException("Unexpected error.");
                }
                last._nextnode = node;
            }
        }

        /// <summary>
        /// Add node to the beginning of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Prepend(HtmlNode node)
        {
            HtmlNode first = null;
            if (items.Count > 0)
            {
                first = items[0];
            }

            items.Insert(0, node);

            if (node == first)
            {
                throw new InvalidProgramException("Unexpected error.");
            }
            node._nextnode = first;
            node._prevnode = null;
            node._parentnode = _parentnode;
            if (first != null)
            {
                first._prevnode = node;
            }
        }

        /// <summary>
        /// Get index of node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeIndex(HtmlNode node)
        {
            // TODO: should we rewrite this? what would be the key of a node?
            for (int i = 0; i < items.Count; i++)
            {
                if (node == (items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Remove node at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Remove(int index)
        {
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Get first instance of node with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlNode FindFirst(string name)
        {
            return FindFirst(this, name);
        }

        /// <summary>
        /// Get first instance of node in supplied collection
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HtmlNode FindFirst(HtmlNodeCollection items, string name)
        {
            foreach (HtmlNode node in items)
        {
                if (node.Name.ToLower().Contains(name))
                    return node;
                if (node.HasChildNodes)
                {
                    HtmlNode returnNode = FindFirst(node.ChildNodes, name);
                    if (returnNode != null)
                        return returnNode;
                }
            }
            return null;
        }

        #region LINQ Methods

        /// <summary>
        /// Get all node descended from this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> DescendantNodes()
        {
            foreach (HtmlNode item in items)
                foreach (HtmlNode n in item.DescendantNodes())
                    yield return n;
            }

            /// <summary>
        /// Get all node descended from this collection
            /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants()
            {
            foreach (HtmlNode item in items)
                foreach (HtmlNode n in item.Descendants())
                    yield return n;
            }

            /// <summary>
        /// Get all node descended from this collection with matching name
            /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(string name)
            {
            foreach (HtmlNode item in items)
                foreach (HtmlNode n in item.Descendants(name))
                    yield return n;
            }

            /// <summary>
        /// Gets all first generation elements in collection
            /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements()
                {
            foreach (HtmlNode item in items)
                foreach (HtmlNode n in item.ChildNodes)
                    yield return n;
                }

        /// <summary>
        /// Gets all first generation elements matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements(string name)
        {
            foreach (HtmlNode item in items)
                foreach (HtmlNode n in item.Elements(name))
                    yield return n;
            }

            /// <summary>
        /// All first generation nodes in collection
            /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Nodes()
                {
            foreach (HtmlNode item in items)
                foreach (HtmlNode n in item.ChildNodes)
                    yield return n;
    }

        #endregion
    }
}
