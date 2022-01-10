using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino
{
    public class HardwareMessage
    {
        public ushort Offset { get; set; }
        public byte[] Value { get; set; }
    }
}
