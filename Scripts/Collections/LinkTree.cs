using System.Collections;
using System.Collections.Generic;

namespace ArmyAnt.Collections {

    public class LinkTree<TTag, TVal> : ITree<TTag, TVal> {
        // ʵ�ֽӿ�
        public LinkTree(TTag tag, TVal val, LinkTree<TTag, TVal> parent) {
            Value = val;
            if (parent != null) {
                parent[tag] = this;
            } else {
                Tag = tag;
            }
            IsCheckTag = false;
        }

        public ITree<TTag, TVal> this[TTag tag] {
            get {
                return children[tag];
            }
            set {
                if (value.GetType() != typeof(LinkTree<TTag, TVal>))
                    throw new System.ArgumentException("The child of the tree you are setting is not a linked tree!", "tag");
                children[tag] = ToLinkedTree(value);
                children[tag].parent = this;
                children[tag].Tag = tag;
                if (!CheckTreeRef(children[tag], true)) {
                    throw new System.ArgumentException("The parent has a node as this as its parent or one of grand parents !");
                }
            }
        }

        public TTag Tag { get; set; }
        public TVal Value { get; set; }

        public int ChildrenCount { get { return children.Count; } }
        public string Json {
            get {
                string ret = "{";
                foreach (var elem in children) {
                    ret += '"' + elem.Key.ToString() + '"' + ':' + elem.Value.ToString() + ',';
                }
                ret.Trim(',');
                return ret + '}';
            }
        }
        public override string ToString() {
            return Json;
        }

        public ETreeTraversalWay EnumeratorType { get; set; }

        public bool AddChild(TTag tag, TVal value) {
            if (IsCheckTag && (GetRoot().Tag.Equals(tag) || GetRoot().GetChildInTree(tag) != null))
                return false;
            if (children.ContainsKey(tag))
                return false;
            new LinkTree<TTag, TVal>(tag, value, this);
            return true;
        }

        public void ClearChildren() {
            foreach (var elem in children) {
                elem.Value.parent = null;
            }
            children.Clear();
        }

        public ITree<TTag, TVal> GetChild(TTag tag) {
            return children[tag];
        }

        public ITree<TTag, TVal> GetChildInTree(TTag tag) {
            var ret = GetChild(tag);
            if (ret == null) {
                var childrens = GetChildren();
                if (childrens == null)
                    return null;
                foreach (var elem in childrens) {
                    var finded = GetChildInTree(tag);
                    if (finded != null)
                        return finded;
                }
            }
            return ret;
        }

        public ITree<TTag, TVal>[] GetChildren() {
            if (children.Count == 0)
                return null;
            var ret = new ITree<TTag, TVal>[children.Count];
            var index = 0;
            foreach (var elem in children) {
                ret[index++] = elem.Value;
            }
            return ret;
        }

        public IEnumerator<ITree<TTag, TVal>> GetEnumerator() {
            return GetEnumerator(EnumeratorType);
        }

        public bool RemoveChild(TTag tag) {
            var target = children[tag];
            if (target != null)
                target.parent = null;
            return children.Remove(tag);
        }

        public bool RemoveChildInTree(TTag tag) {
            var ret = RemoveChild(tag);
            if (!ret) {
                var childrens = GetChildren();
                if (childrens == null)
                    return false;
                foreach (var elem in childrens) {
                    var finded = RemoveChildInTree(tag);
                    if (finded)
                        return true;
                }
            }
            return ret;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public ITree<TTag, TVal> GetParent() {
            return parent;
        }

        public ITree<TTag, TVal> GetRoot() {
            var ret = this;
            while (ret.parent != null)
                ret = ret.parent;
            return ret;
        }

        // �Դ�����
        public bool IsCheckTag { get; set; }

        public bool AddChild(LinkTree<TTag, TVal> tree) {
            if (IsCheckTag && (GetRoot().Tag.Equals(tree.Tag) || GetRoot().GetChildInTree(tree.Tag) != null))
                return false;
            if (children.ContainsKey(tree.Tag))
                return false;
            this[tree.Tag] = tree;
            return true;
        }

        public IEnumerator<ITree<TTag, TVal>> GetEnumerator(ETreeTraversalWay type) {

            if (type == ETreeTraversalWay.Unknown)
                type = EnumeratorType;
            switch (type) {   // TODO : ��Ҫ���Ƶ����������㷨, ��Ŀǰ�ݲ���Ҫ
                case ETreeTraversalWay.ChildrenOnly:
                    foreach (var item in children) {
                        yield return item.Value;
                    }
                    break;
                case ETreeTraversalWay.LeavesOnly: {
                    if (ChildrenCount <= 0) {
                        yield return this;
                    } else {
                        var current = this;     // Ҫ����ĵ�������Ա
                        Stack<Dictionary<TTag, LinkTree<TTag, TVal>>.Enumerator> targetCurr = new Stack<Dictionary<TTag, LinkTree<TTag, TVal>>.Enumerator>();
                        do {
                            // Ѱ�ҵ���һ����, ��ѯ�Ƿ����ӽڵ�, ����, ��ת��Ϊ�ӽڵ�, ������֤����Ҷ�ӽڵ�, Ϊ����Ŀ��
                            for (var enumerator = current.children.GetEnumerator(); current.ChildrenCount > 0; enumerator = current.children.GetEnumerator()) {
                                enumerator.MoveNext();
                                targetCurr.Push(enumerator);
                                current = enumerator.Current.Value;
                            }
                            yield return current;
                            // ǰ����һ��, ������һ��, �򷵻ص����ڵ����һ��, �����ڵ�Ҳ��,�����Ѱ�Ҹ��ڵ�
                            var currEnum = targetCurr.Pop();
                            var ret = currEnum.MoveNext();
                            while (!ret) {
                                // ����һ��һֱ׷�ݵ����ڵ�Ҳû����һ��, ��֤���ѱ������
                                if (targetCurr.Count > 0) {
                                    currEnum = targetCurr.Pop();
                                    ret = currEnum.MoveNext();
                                } else {
                                    break;
                                }
                            }
                            if (ret) {
                                targetCurr.Push(currEnum);
                            }
                        } while (targetCurr.Count > 0);
                    }
                }
                break;
                case ETreeTraversalWay.RandomTraversal:
                    break;
                case ETreeTraversalWay.LayerorderTraversal:
                    break;
                case ETreeTraversalWay.PreorderTraversal:
                    break;
                case ETreeTraversalWay.PostorderTraversal:
                    break;
            }
        }

        public static LinkTree<TTag, TVal> ToLinkedTree(ITree<TTag, TVal> value) {
            if (value.GetType() != typeof(LinkTree<TTag, TVal>))
                return null;
            return (LinkTree<TTag, TVal>)value;
        }

        public int GetBranchDepth() {
            int ret = 0;
            foreach (var elem in children) {
                var elemDepth = elem.Value.GetBranchDepth();
                if (ret < elemDepth)
                    ret = elemDepth;
            }
            return 1 + ret;
        }

        public int GetDepthInRoot() {
            int ret = 1;
            var roadParent = parent;
            while (roadParent != null) {
                ret++;
                roadParent = roadParent.parent;
            }
            return ret;
        }

        public ITree<TTag, TVal>[] GetBranchRoad() {
            var list = new Stack<ITree<TTag, TVal>>();
            list.Push(this);
            var roadParent = parent;
            while (roadParent != null) {
                list.Push(roadParent);
                roadParent = roadParent.parent;
            }
            var ret = new ITree<TTag, TVal>[list.Count];
            var index = 0;
            while (list.Count > 0) {
                ret[index++] = list.Pop();
            }
            return ret;
        }

        /// <summary>
        ///     ������Ľڵ��Ƿ����ظ�
        /// </summary>
        /// <param name="node"> �������˲����Ҳ�Ϊnull, �����������Ƿ�����˽ڵ��ظ��Ľڵ� </param>
        /// <param name="parentOnly"> �������˲�����Ϊtrue, ������ýڵ�ĸ��ڵ��������Ƿ��иýڵ��Լ� </param>
        /// <returns> ���ظ�����true </returns>
        public bool CheckTreeRef(LinkTree<TTag, TVal> node = null, bool parentOnly = false) {
            if (parentOnly) {
                if (node != null) {
                    var p = node.parent;
                    while (p != null) {
                        if (p == node) {
                            return false;
                        }
                        p = p.parent;
                    }
                    return true;
                } else {
                    // TODO û��ָ���ڵ�, ����������Ƿ��и����ظ�
                    return false;
                }
            } else {
                if (node != null) {
                    // TODO ����������Ƿ�������ظ��Ľڵ�
                    return false;
                } else {
                    // TODO ����������Ƿ�������ڵ�֮�以���ظ�
                    return false;
                }
            }
        }

        // ��Ա
        private LinkTree<TTag, TVal> parent = null;
        private Dictionary<TTag, LinkTree<TTag, TVal>> children = new Dictionary<TTag, LinkTree<TTag, TVal>>();
    }

}
