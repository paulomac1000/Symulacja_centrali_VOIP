using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Telefon
{
    class IPPort
    {
        public IPAddress address { get; set; }
        public int port { get; set; }

        public IPPort(string adrress)
        {
            string bufor = adrress;
            int i = 0;
            for (; i < bufor.Length; i++)
            {
                if (bufor[i] == ':')
                    break;
            }
            bufor = adrress.Substring(0, i);
            port = Convert.ToInt32(adrress.Substring(i + 1));
            address = IPAddress.Parse(bufor);
        }
        public IPPort(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
        }

    }

    class User
    {
        public string nick { get; set; }
        public string mail { get; set; }

    }

    class CallID
    {
        public string callid { get; set; }

        public CallID()
        {
            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                callid += (char)rnd.Next(97, 122);
            }
            callid += "@domena.pl";
        }
        public CallID(string callid)
        {
            this.callid = callid;
        }

    }

    class CSeq //numer polecenia i polecenie
    {
        public int numer { get; set; }
        public string polecenie { get; set; }
    }

    class Contact
    {
        public string nick { get; set; }
        public string mail { get; set; }
    }
}
