using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArmyAnt.Thread {
    /// <summary>
    /// �����̣߳������������ϲ��������е��̶߳���<see cref="ITaskQueue"/>������ͳһ�ַ���Ϣ
    /// </summary>
    /// <typeparam name="T"> event id type </typeparam>
    public class TaskPool<T> {
        /// <summary>
        /// ��������Ϣ���ն��󣬿��Խ�����Ϣ��ÿ�������Ķ��󶼻ᱻ��������1�������̣߳�
        /// </summary>
        public interface ITaskQueue {
            void OnTask<Input>(T _event, params Input[] data);
        }

        /// <summary>
        /// ���һ�����󵽹������У������������ʼ������Ϣ
        /// </summary>
        /// <param name="queue"></param>
        /// <returns> ���ع�����Ϊ�������� task ID </returns>
        /// <exception cref="ArgumentNullException"> �� <paramref name="queue"/> ���� null ʱ���� </exception>
        public long AddTaskQueue(ITaskQueue queue) {
            if (queue == null) {
                throw new ArgumentNullException("queue");
            }
            long taskId = 0;
            lock (pool) {
                while (pool.ContainsKey(++taskId)) {
                }
                var end = new System.Threading.CancellationTokenSource();
                var info = new TaskQueueInfo() {
                    queue = queue,
                    end = end,
                    task = Task.Run(() => { }, end.Token),
                };
                pool.Add(taskId, info);
            }
            return taskId;

        }

        /// <summary>
        /// �ӹ��������Ƴ�һ�����󣬲�����֮ǰ��ֹͣ�ö������ڽ��е���Ϣ������Ϊ
        /// </summary>
        /// <param name="taskId"> ����� task ID </param>
        /// <returns> async ���ظö�����������н�� </returns>
        /// <exception cref="ArgumentNullException"> �� <paramref name="taskId"/> ���� null ʱ���� </exception>
        /// <exception cref="KeyNotFoundException"> �� <paramref name="taskId"/> ������ʱ���� </exception>
        public async Task<bool> RemoveTaskQueue(long taskId) {
            await StopTaskQueue(taskId);
            lock (pool) {
                var queue = pool[taskId];
                var ret = pool.Remove(taskId);
                return ret;
            }
        }

        /// <summary>
        /// ��չ��������Ƴ����ж���
        /// </summary>
        public void ClearTaskQueue() {
            lock (pool) {
                pool.Clear();
            }
        }

        /// <summary>
        /// ���ҹ��������Ƿ����ĳ����
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool IsTaskQueueExist(long taskId) {
            lock (pool) {
                return pool.ContainsKey(taskId);
            }
        }

        /// <summary>
        /// �ַ���Ϣ
        /// </summary>
        /// <typeparam name="Input"></typeparam>
        /// <param name="taskId"></param>
        /// <param name="_event"></param>
        /// <param name="param"></param>
        /// <returns> ��� task ID �����ڣ����� false </returns>
        public bool EnqueueTaskTo<Input>(long taskId, T _event, params Input[] param) {
            if (IsTaskQueueExist(taskId)) {
                lock (pool) {
                    var queue = pool[taskId];
                    lock (queue.task) {
                        if (queue.end.Token.IsCancellationRequested) {
                            throw new ObjectDisposedException("The task queue has been stopped, cannot enqueue any more before it was resumed", default(Exception));
                        }
                        queue.task = queue.task.ContinueWith((lastTask) => {
                            queue.queue.OnTask(_event, param);
                        }, queue.end.Token);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// ֹͣһ���������Ϣ����
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> �� <paramref name="taskId"/> ���� null ʱ���� </exception>
        /// <exception cref="KeyNotFoundException"> �� <paramref name="taskId"/> ������ʱ���� </exception>
        public async Task StopTaskQueue(long taskId) {
            TaskQueueInfo queue;
            lock (pool) {
                queue = pool[taskId];
                if (!queue.end.IsCancellationRequested) {
                    queue.end.Cancel();
                }
            }
            await queue.task;
        }

        /// <summary>
        /// �ָ�һ���������Ϣ����
        /// </summary>
        /// <param name="taskId"></param>
        /// <exception cref="ArgumentNullException"> �� <paramref name="taskId"/> ���� null ʱ���� </exception>
        /// <exception cref="KeyNotFoundException"> �� <paramref name="taskId"/> ������ʱ���� </exception>
        public void ResumeTaskQueue(long taskId) {
            lock (pool) {
                var queue = pool[taskId];
                if (queue.end.Token.IsCancellationRequested) {
                    queue.end = new System.Threading.CancellationTokenSource();
                    queue.task = Task.Run(() => { }, queue.end.Token);
                }
            }
        }

        /// <summary>
        /// �鿴ָ�������Ķ����Ƿ�ֹͣ����Ϣ����
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> �� <paramref name="taskId"/> ���� null ʱ���� </exception>
        /// <exception cref="KeyNotFoundException"> �� <paramref name="taskId"/> ������ʱ���� </exception>
        public bool IsTaskQueueStopped(long taskId) {
            lock (pool) {
                return pool[taskId].end.IsCancellationRequested;
            }
        }

        /// <summary>
        /// ��ȡָ������� Task�����ڶ��� await
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> �� <paramref name="taskId"/> ���� null ʱ���� </exception>
        /// <exception cref="KeyNotFoundException"> �� <paramref name="taskId"/> ������ʱ���� </exception>
        public Task GetTask(long taskId) => pool[taskId].task;

        /// <summary>
        /// ��ȡ���ж���� Task������ await ���� AwaitAll
        /// </summary>
        /// <returns></returns>
        public Task[] GetAllTasks() {
            lock (pool) {
                var ret = new Task[pool.Count];
                int index = 0;
                foreach (var i in pool) {
                    ret[index++] = i.Value.task;
                }
                return ret;
            }
        }

        /// <summary>
        /// ��ȡһ������
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns> ����Ҫ��ȡ�Ķ�����������ڣ����� null </returns>
        public ITaskQueue GetQueue(long taskId) => pool?[taskId]?.queue;

        private class TaskQueueInfo {
            public ITaskQueue queue;
            public Task task;
            public System.Threading.CancellationTokenSource end;
        }
        private readonly IDictionary<long, TaskQueueInfo> pool = new Dictionary<long, TaskQueueInfo>();
    }
}
