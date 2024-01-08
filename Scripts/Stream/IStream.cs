namespace ArmyAnt.Stream {

    /// <summary>
    /// ���м�ʱ��ȡ���Ĺ����ӿڣ��������硢�ܵ���Ӳ�����ݿڣ��紮�ڣ����������ݿڣ���������
    /// </summary>
    public interface IStream {
        /// <summary>
        /// �Ƿ������У�ֻ���������в��ܷ��͡��������ݣ����ܼ����ͽ�������
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// ����, ���������ȡĬ��ֵ
        /// </summary>
        void Open();
        /// <summary>
        /// �ر���, �Ͽ���������
        /// </summary>
        void Close(bool nowait = false);
        /// <summary>
        /// �Ƿ�����ͣ��
        /// </summary>
        bool IsPausing { get; }
        /// <summary>
        /// ��ͣ�����̣߳���ͣ��Ϣ�Ľ��պ����ӽ���
        /// </summary>
        void Pause();
        /// <summary>
        /// �ָ������߳�
        /// </summary>
        void Resume();
        /// <summary>
        /// ��ȡ�ص�
        /// </summary>
        event System.Action<byte[]> OnReading;
    }
}
