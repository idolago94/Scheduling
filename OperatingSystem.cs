using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class OperatingSystem
    {
        public Disk Disk { get; private set; }
        public CPU CPU { get; private set; }
        private Dictionary<int, ProcessTableEntry> m_dProcessTable;
        private List<ReadTokenRequest> m_lReadRequests;
        private int m_cProcesses;
        private SchedulingPolicy m_spPolicy;
        private static int IDLE_PROCESS_ID = 0;

        public OperatingSystem(CPU cpu, Disk disk, SchedulingPolicy sp)
        {
            CPU = cpu;
            Disk = disk;
            m_dProcessTable = new Dictionary<int, ProcessTableEntry>();
            m_lReadRequests = new List<ReadTokenRequest>();
            cpu.OperatingSystem = this;
            disk.OperatingSystem = this;
            m_spPolicy = sp;
            if(m_spPolicy.quntom!=-1)
                cpu.RemainingTime = m_spPolicy.quntom;
            //create an "idle" process here :x
            this.CreateIdleProcess();
            
        }


        public void CreateProcess(string sCodeFileName)
        {
            Code code = new Code(sCodeFileName);
            m_dProcessTable[m_cProcesses] = new ProcessTableEntry(m_cProcesses, sCodeFileName, code);
            m_dProcessTable[m_cProcesses].StartTime = CPU.TickCount;
            m_spPolicy.AddProcess(m_cProcesses);
            m_cProcesses++;
        }
        public void CreateIdleProcess()
        {
            String sCodeFileName = "idle.code";
            IdleCode idleCode = new IdleCode(sCodeFileName);
            m_dProcessTable[m_cProcesses] = new ProcessTableEntry(m_cProcesses, sCodeFileName, idleCode);
            m_dProcessTable[m_cProcesses].StartTime = CPU.TickCount;
            m_dProcessTable[m_cProcesses].Priority = -9999999;
            m_spPolicy.AddProcess(m_cProcesses);
            m_cProcesses++;
        }
        public void CreateProcess(string sCodeFileName, int iPriority)
        {
            Code code = new Code(sCodeFileName);
            m_dProcessTable[m_cProcesses] = new ProcessTableEntry(m_cProcesses, sCodeFileName, code);
            m_dProcessTable[m_cProcesses].Priority = iPriority;
            m_dProcessTable[m_cProcesses].StartTime = CPU.TickCount;
            m_spPolicy.AddProcess(m_cProcesses);
            m_cProcesses++;
        }

        public void ProcessTerminated(Exception e)
        {
            if (e != null)
                Console.WriteLine("Process " + CPU.ActiveProcess + " terminated unexpectedly. " + e);
            m_dProcessTable[CPU.ActiveProcess].Done = true;
            m_dProcessTable[CPU.ActiveProcess].Console.Close();
            m_dProcessTable[CPU.ActiveProcess].EndTime = CPU.TickCount;
            ActivateScheduler();
        }

        public void TimeoutReached()
        {
            CPU.RemainingTime = m_spPolicy.quantom;
            ActivateScheduler();
        }

        public void ReadToken(string sFileName, int iTokenNumber, int iProcessId, string sParameterName)
        {
            ReadTokenRequest request = new ReadTokenRequest();
            request.ProcessId = iProcessId;
            request.TokenNumber = iTokenNumber;
            request.TargetVariable = sParameterName;
            request.Token = null;
            request.FileName = sFileName;
            m_dProcessTable[iProcessId].Blocked = true;
            if (Disk.ActiveRequest == null)
                Disk.ActiveRequest = request;
            else
                m_lReadRequests.Add(request);
            CPU.ProgramCounter = CPU.ProgramCounter + 1;
            ActivateScheduler();
        }

        public void Interrupt(ReadTokenRequest rFinishedRequest)
        {
            //implement an "end read request" interrupt handler.
            //translate the returned token into a value (double). 
            //when the token is null, EOF has been reached.
            //write the value to the appropriate address space of the calling process.
            //activate the next request in queue on the disk.
            double currentNumber;
            if (rFinishedRequest == null)
            {
                currentNumber = Double.NaN;
            }else
                {
                currentNumber = Convert.ToDouble(rFinishedRequest.Token);
                }
            bool hasValue = m_dProcessTable.TryGetValue(rFinishedRequest.ProcessId, out ProcessTableEntry process);
            if (hasValue && process!=null)
                process.AddressSpace[rFinishedRequest.TargetVariable] = currentNumber;

            if (m_lReadRequests.Any())// isEmpty?
                Disk.ActiveRequest = m_lReadRequests.First(); 

            if (m_spPolicy.RescheduleAfterInterrupt())
                ActivateScheduler();
        }

        private ProcessTableEntry ContextSwitch(int iEnteringProcessId)
        {
            //your code here
            //implement a context switch, switching between the currently active process on the CPU to the process with pid iEnteringProcessId
            //You need to switch the following: ActiveProcess, ActiveAddressSpace, ActiveConsole, ProgramCounter.
            //All values are stored in the process table (m_dProcessTable)
            //Our CPU does not have registers, so we do not store or switch register values.
            //returns the process table information of the outgoing process
            //After this method terminates, the execution continues with the new process
            bool hasValue = m_dProcessTable.TryGetValue(iEnteringProcessId, out ProcessTableEntry new_process);
            if(!hasValue || CPU.ActiveProcess==-1)
            {
                Console.WriteLine("``````````````````````````~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~````````````````````````");
                Console.WriteLine(new_process.ToString);
            }
            else
            {
                if (new_process != null)
                {
                    m_dProcessTable.TryGetValue(CPU.ActiveProcess, out ProcessTableEntry current_process);
                    current_process.LastCPUTime = CPU.TickCount;
                    CPU.ActiveProcess = new_process.ProcessId;
                    CPU.ActiveAddressSpace = new_process.AddressSpace;
                    CPU.ActiveConsole = new_process.Console;
                    CPU.ProgramCounter = current_process.ProgramCounter;
                
                return new_process;
                }
            }
            return null;
            //throw new NullReferenceException();
        }

        public void ActivateScheduler()
        {
            int iNextProcessId = m_spPolicy.NextProcess(m_dProcessTable);
            if (iNextProcessId == -1)
            {
                Console.WriteLine("All processes terminated or blocked.");
                CPU.Done = true;
            }
            else
            {//add code here to check if only the Idle process remains
                bool bOnlyIdleRemains = true;
                foreach(ProcessTableEntry process in m_dProcessTable.Values)
                {
                    if(process != null && process.Name!="idle.code" && process.Done==false)
                    {
                        bOnlyIdleRemains = false; break;
                    }
                }
                if(bOnlyIdleRemains)
                {
                    Console.WriteLine("Only idle remains.");
                    CPU.Done = true;
                }
                else
                    ContextSwitch(iNextProcessId);
            }
        }

        public double AverageTurnaround()
        {
            //Compute the average time from the moment that a process enters the system until it terminates.
            throw new NotImplementedException();
        }
        public int MaximalStarvation()
        {
            //Compute the maximal time that some project has waited in a ready stage without receiving CPU time.
            throw new NotImplementedException();
        }
    }
}
