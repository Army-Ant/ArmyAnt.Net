using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.Thread {

    /// <summary>
    /// 用于排查可能的死锁问题的简单锁封装
    /// </summary>
    public class SimpleLock {
        public SimpleLock(int initialCount, int maximumCount, bool consoleLog = false) {
            handle = new System.Threading.Semaphore(initialCount, maximumCount);
            id = Convert.ToUInt32(new Random().Next(0, 999999));
            ConsoleLog = consoleLog;
            if(ConsoleLog) {
                Console.WriteLine("Lock created id " + id);
            }
        }

        public virtual bool Lock() {
            if(ConsoleLog) {
                Console.WriteLine("Lock the id " + id);
            }
            return handle.WaitOne();
        }

        public int Unlock() {
            if(ConsoleLog) {
                Console.WriteLine("Unlock the id " + id);
            }
            return handle.Release(1);
        }

        public bool ConsoleLog { get; set; }

        private readonly System.Threading.Semaphore handle;
        private readonly uint id;
    }
}
