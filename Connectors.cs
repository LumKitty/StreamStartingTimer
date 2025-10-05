using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamStartingTimer {
    public enum ConnectStatus {
        Disabled = -1,
        Disconnected = 0,
        Connecting = 1,
        Error = 2,
        Connected = 3
    }
    
    public abstract class Connector {
        ConnectStatus status;
        protected abstract void Connect();
        protected abstract void Disconnect();
    }

    public class VNyanConnector : Connector {
        protected override void Connect() {

        }
    }
}
