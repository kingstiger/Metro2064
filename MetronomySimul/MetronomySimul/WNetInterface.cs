using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MetronomySimul
{
    class WNetInterface
    {
        public NetInterface eth;   //Interfejs sieciowy
        public bool isOffered;     //Flaga oznaczająca, czy interfejs został zaoferowany, ale jeszcze nie utworzono z nim połączenia
        public int secondsElapsedLastPing;
        private Form1 form;         //Uchwyt na okno

        public WNetInterface(string localAddress, int interfaceNumber, Form1 form)
        {
            this.form = form;
            eth = new NetInterface(localAddress, interfaceNumber, form);
            isOffered = false;
            secondsElapsedLastPing = -1;    //Dopiero po nawiązaniu połączenia i wysłaniu pierwszego pinga uzupełniamy o pole o wartość większą/równą zero.
        }

        //Przydatne gettery
        public IPEndPoint GetLocalEndpoint() => eth.localEndPoint;
        public IPEndPoint GetTargetEndpoint() => eth.targetEndPoint;

        //Nawiązywanie/przerywanie połączeń
        public bool IsAvaiable() => !eth.IsConnected() && !isOffered;
        public void SetConnection(IPEndPoint targetEndPoint) //Łączy wybrany interfejs z wybranym endpointem
        {
            isOffered = false;

            eth.SetConnection(targetEndPoint);
        }
        public void TerminateConnection()   //Rozłącza wybrany interfejs
        {
            isOffered = false;
            secondsElapsedLastPing = -1;

            eth.TerminateConnection();
        }

        
        //Dalsze gówna
    }
}
