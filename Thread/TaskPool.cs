﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArmyAnt.Thread {
    public class TaskPool<T> {
        public interface ITaskQueue {
            void OnTask<Input>(T _event, params Input[] data);
        }

        public long AddTaskQueue(ITaskQueue queue) {
            if(queue == null) {
                throw new ArgumentNullException();
            }
            long index = 0;
            lock(pool) {
                while(pool.ContainsKey(++index)) {
                }
                var end = new System.Threading.CancellationTokenSource();
                var info = new TaskQueueInfo() {
                    queue = queue,
                    end = end,
                    task = Task.Run(() => { }, end.Token),
                };
                pool.Add(index, info);
            }
            return index;

        }

        public async Task<bool> RemoveTaskQueue(long taskId) {
            await StopTaskQueue(taskId);
            lock(pool) {
                var queue = pool[taskId];
                var ret = pool.Remove(taskId);
                return ret;
            }
        }

        public void ClearTaskQueue() {
            lock(pool) {
                pool.Clear();
            }
        }

        public bool IsTaskQueueExist(long index) {
            lock(pool) {
                return pool.ContainsKey(index);
            }
        }

        public bool EnqueueTaskTo<Input>(long index, T _event, params Input[] param) {
            if(IsTaskQueueExist(index)) {
                lock(pool) {
                    var queue = pool[index];
                    lock(queue.task) {
                        if(queue.end.Token.IsCancellationRequested) {
                            throw new System.ObjectDisposedException("The task queue has been stopped, cannot enqueue any more before it was resumed", default(System.Exception));
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

        public async Task StopTaskQueue(long index) {
            TaskQueueInfo queue;
            lock(pool) {
                queue = pool[index];
                if(!queue.end.IsCancellationRequested) {
                    queue.end.Cancel();
                }
            }
            await queue.task;
        }

        public void ResumeTaskQueue(long index) {
            lock(pool) {
                var queue = pool[index];
                if(queue.end.Token.IsCancellationRequested) {
                    queue.end = new System.Threading.CancellationTokenSource();
                    queue.task = Task.Run(() => { }, queue.end.Token);
                }
            }
        }

        public bool IsTaskQueueStopped(long index) {
            lock(pool) {
                return pool[index].end.IsCancellationRequested;
            }
        }

        public Task GetTask(long index) => pool[index].task;
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

        public ITaskQueue GetQueue(long index) => pool[index].queue;

        private struct TaskQueueInfo {
            public ITaskQueue queue;
            public Task task;
            public System.Threading.CancellationTokenSource end;
        }
        private readonly IDictionary<long, TaskQueueInfo> pool = new Dictionary<long, TaskQueueInfo>();
    }
}