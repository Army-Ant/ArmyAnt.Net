using System.Collections.Generic;

namespace ArmyAnt.Collections {
    /// <summary>
    /// ���ı�����ʽö��
    /// </summary>
    public enum ETreeTraversalWay {
        Unknown,
        /// <summary>
        /// ������ֱ���ӽڵ�
        /// </summary>
        ChildrenOnly,
        /// <summary>
        /// ����������Ҷ�ӽڵ�
        /// </summary>
        LeavesOnly,
        /// <summary>
        /// �������
        /// </summary>
        RandomTraversal,
        /// <summary>
        /// ������ ( ������ȱ��� )
        /// </summary>
        LayerorderTraversal,
        /// <summary>
        /// ������� ( ������ȱ��� )
        /// </summary>
        PreorderTraversal,
        /// <summary>
        /// �������
        /// </summary>
        PostorderTraversal,
    }

    /// <summary>
    /// �����ݽṹ�ĳ���ӿڣ������˽ṹ�е�һ���ڵ�
    /// </summary>
    /// <typeparam name="T_Tag"> �������Ͳ��� </typeparam>
    /// <typeparam name="T_Val"> ֵ�����Ͳ��� </typeparam>
    public interface ITree<T_Tag, T_Val> : IEnumerable<ITree<T_Tag, T_Val>> {
        T_Tag Tag { get; set; }
        T_Val Value { get; set; }
        int ChildrenCount { get; }
        ETreeTraversalWay EnumeratorType { get; set; }
        string Json { get; }

        ITree<T_Tag, T_Val> GetParent();
        ITree<T_Tag, T_Val> GetRoot();
        /// <summary>
        /// ��ȡָ��tag����������. �粻ָ��tag, ���ȡ��������
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
