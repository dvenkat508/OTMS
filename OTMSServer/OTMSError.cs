using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTMSServer
{
    class OTMSError
    {

    }
    public class InvalidData
    {
        private bool emailid;
        public InvalidData()
        {}
        public bool Emailid
        {
            get { return emailid; }
            set { emailid = value; }
        }
    }
}
