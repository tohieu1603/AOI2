using EventMgrLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame.UI
{
    public class RefreshDataMessage:PubSubEvent<RefreshDataMessage>
    {
        public string Message { get; set; }

        public RefreshDataMessage()
        {
            
        }
    }
}
