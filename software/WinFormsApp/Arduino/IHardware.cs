using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino
{
    public interface IHardware
    {
        public void RegisterProperty(IProperty prop);
        public void SendValue(IProperty p);
        public void RequestValue(IProperty p);

        public void RequestAllValues();

        public void SendCommand(string cmd);

    }
}
