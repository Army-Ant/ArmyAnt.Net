using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArmyAnt.Thread {
    /// <summary>
    /// 任务（线程）管理器，集合并调度所有的线程对象（<see cref="ITaskQueue"/>），并统一分发消息
    /// </summary>
    /// <typeparam name="T"> event id type </typeparam>
    public class TaskPool<T> {
        /// <summary>
        /// 单独的消息接收对象，可以接收消息，每个这样的对象都会被分配至少1个任务（线程）
        /// </summary>
        public interface ITaskQueue {
            void OnTask<Input>(T _event, params Input[] data);
        }

        /// <summary>
        /// 添加一个对象到管理器中，并让这个对象开始接收消息
        /// </summary>
        /// <param name="queue"></param>
        /// <returns> 返回管理器为对象分配的 task ID </returns>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="queue"/> 传入 null 时引发 </exception>
        public long AddTaskQueue(ITaskQueue queue) {
            if(queue == null) {
                throw new ArgumentNullException("queue");
            }
            long taskId = 0;
            lock(pool) {
                while(pool.ContainsKey(++taskId)) {
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
        /// 从管理器中移除一个对象，并在这之前会停止该对象正在进行的消息接收行为
        /// </summary>
        /// <param name="taskId"> 对象的 task ID </param>
        /// <returns> async 返回该对象的任务运行结果 </returns>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="taskId"/> 传入 null 时引发 </exception>
        /// <exception cref="KeyNotFoundException"> 当 <paramref name="taskId"/> 不存在时引发 </exception>
        public async Task<bool> RemoveTaskQueue(long taskId) {
            await StopTaskQueue(taskId);
            lock(pool) {
                var queue = pool[taskId];
                var ret = pool.Remove(taskId);
                return ret;
            }
        }

        /// <summary>
        /// 清空管理器，移除所有对象
        /// </summary>
        public void ClearTaskQueue() {
            lock(pool) {
                pool.Clear();
            }
        }

        /// <summary>
        /// 查找管理器中是否存在某对象
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool IsTaskQueueExist(long taskId) {
            lock(pool) {
                return pool.ContainsKey(taskId);
            }
        }

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <typeparam name="Input"></typeparam>
        /// <param name="taskId"></param>
        /// <param name="_event"></param>
        /// <param name="param"></param>
        /// <returns> 如果 task ID 不存在，返回 false </returns>
        public bool EnqueueTaskTo<Input>(long taskId, T _event, params Input[] param) {
            if(IsTaskQueueExist(taskId)) {
                lock(pool) {
                    var queue = pool[taskId];
                    lock(queue.task) {
                        if(queue.end.Token.IsCancellationRequested) {
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
        /// 停止一个对象的消息接收
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="taskId"/> 传入 null 时引发 </exception>
        /// <exception cref="KeyNotFoundException"> 当 <paramref name="taskId"/> 不存在时引发 </exception>
        public async Task StopTaskQueue(long taskId) {
            TaskQueueInfo queue;
            lock(pool) {
                queue = pool[taskId];
                if(!queue.end.IsCancellationRequested) {
                    queue.end.Cancel();
                }
            }
            await queue.task;
        }

        /// <summary>
        /// 恢复一个对象的消息接收
        /// </summary>
        /// <param name="taskId"></param>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="taskId"/> 传入 null 时引发 </exception>
        /// <exception cref="KeyNotFoundException"> 当 <paramref name="taskId"/> 不存在时引发 </exception>
        public void ResumeTaskQueue(long taskId) {
            lock(pool) {
                var queue = pool[taskId];
                if(queue.end.Token.IsCancellationRequested) {
                    queue.end = new System.Threading.CancellationTokenSource();
                    queue.task = Task.Run(() => { }, queue.end.Token);
                }
            }
        }

        /// <summary>
        /// 查看指定索引的对象是否被停止了消息接收
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="taskId"/> 传入 null 时引发 </exception>
        /// <exception cref="KeyNotFoundException"> 当 <paramref name="taskId"/> 不存在时引发 </exception>
        public bool IsTaskQueueStopped(long taskId) {
            lock(pool) {
                return pool[taskId].end.IsCancellationRequested;
            }
        }

        /// <summary>
        /// 获取指定对象的 Task，用于对其 await
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="taskId"/> 传入 null 时引发 </exception>
        /// <exception cref="KeyNotFoundException"> 当 <paramref name="taskId"/> 不存在时引发 </exception>
        public Task GetTask(long taskId) => pool[taskId].task;

        /// <summary>
        /// 获取所有对象的 Task，用于 await 或者 AwaitAll
        /// </summary>
        /// <returns></returns>
        public Task[] GetAllTasks() {
            lock(pool) {
                var ret = new Task[pool.Count];
                int index = 0;
                foreach(var i in pool) {
                    ret[index++] = i.Value.task;
                }
                return ret;
            }
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns> 返回要获取的对象，如果不存在，返回 null </returns>
        public ITaskQueue GetQueue(long taskId) => pool?[taskId]?.queue;

        private class TaskQueueInfo {
            public ITaskQueue queue;
            public Task task;
            public System.Threading.CancellationTokenSource end;
        }
        private readonly IDictionary<long, TaskQueueInfo> pool = new Dictionary<long, TaskQueueInfo>();
    }
}
