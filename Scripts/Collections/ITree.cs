using System.Collections.Generic;

namespace ArmyAnt.Collections {
    /// <summary>
    /// 树的遍历方式枚举
    /// </summary>
    public enum ETreeTraversalWay {
        Unknown,
        /// <summary>
        /// 仅遍历直接子节点
        /// </summary>
        ChildrenOnly,
        /// <summary>
        /// 仅遍历所有叶子节点
        /// </summary>
        LeavesOnly,
        /// <summary>
        /// 随机遍历
        /// </summary>
        RandomTraversal,
        /// <summary>
        /// 逐层遍历 ( 广度优先遍历 )
        /// </summary>
        LayerorderTraversal,
        /// <summary>
        /// 先序遍历 ( 深度优先遍历 )
        /// </summary>
        PreorderTraversal,
        /// <summary>
        /// 后序遍历
        /// </summary>
        PostorderTraversal,
    }

    /// <summary>
    /// 树数据结构的抽象接口，亦代表此结构中的一个节点
    /// </summary>
    /// <typeparam name="T_Tag"> 键的类型参数 </typeparam>
    /// <typeparam name="T_Val"> 值的类型参数 </typeparam>
    public interface ITree<T_Tag, T_Val> : IEnumerable<ITree<T_Tag, T_Val>> {
        T_Tag Tag { get; set; }
        T_Val Value { get; set; }
        int ChildrenCount { get; }
        ETreeTraversalWay EnumeratorType { get; set; }
        string Json { get; }

        ITree<T_Tag, T_Val> GetParent();
        ITree<T_Tag, T_Val> GetRoot();
        /// <summary>
        /// 获取指定tag的所有子树. 如不指定tag, 则获取所有子树
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        ITree<T_Tag, T_Val> GetChild(T_Tag tag);
        ITree<T_Tag, T_Val> GetChildInTree(T_Tag tag);
        ITree<T_Tag, T_Val>[] GetChildren();

        bool AddChild(T_Tag tag, T_Val value);

        bool RemoveChild(T_Tag tag);
        bool RemoveChildInTree(T_Tag tag);
        void ClearChildren();

        int GetBranchDepth();
        int GetDepthInRoot();
        ITree<T_Tag, T_Val>[] GetBranchRoad();

        ITree<T_Tag, T_Val> this[T_Tag tag] { get; set; }

    }

}
