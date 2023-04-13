using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduling
{
    class RoundRobin : FirstComeFirstServedPolicy
    {
        public RoundRobin(int iQuantum) : base()
        {
            base.quantom = iQuantum;
        }
        public int getQuantom() { return quantom; }
    }
}
