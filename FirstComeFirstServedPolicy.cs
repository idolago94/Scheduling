using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class FirstComeFirstServedPolicy : SchedulingPolicy
    {
        public Queue<int> queue { get; set; }
        public FirstComeFirstServedPolicy()
        {
            base.quantom = -1;
            queue = new Queue<int>();
        }

        public override int NextProcess(Dictionary<int, ProcessTableEntry> dProcessTable)
        {
            bool hasValue = queue.TryDequeue(out int nextProcessId);
            while(hasValue)
            {
                bool contains = dProcessTable.ContainsKey(nextProcessId);
                if(contains)
                {
                    ProcessTableEntry entry = dProcessTable[nextProcessId];
                    if(entry != null && !entry.Done && !entry.Blocked) {
                        return nextProcessId;
                    }
                }
                hasValue = queue.TryDequeue(out nextProcessId);
            }
            return -1;
        }

        public override void AddProcess(int iProcessId)
        {
            queue.Enqueue(iProcessId);
        }

        public override bool RescheduleAfterInterrupt()
        {
            return queue.TryPeek(out int id);
        }
    }
}
