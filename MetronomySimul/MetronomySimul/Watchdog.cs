using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
    class Watchdog
    {
        private List<WNetInterface> interfaces = new List<WNetInterface>();
        public Form1 form;                                                  //Uchwyt na okno

        private UdpClient netClient;                                        //Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
        private IPEndPoint localEndPoint;
        private IPEndPoint targetEndPoint;
        private Queue<NetPacket> packetsToSend, packetsReceived;            //Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie
        private Mutex sendMutex, receiveMutex, interfaceMutex;              //Muteksy dla kolejek komnikatów oraz listy interfejsów
        protected Thread senderThread, listenerThread, cyclicThread;	    //Uchwyty na wątki do wysyłania i odbierania danych, oraz cyklicznego dodawania danych do wysłania przez watchdoga

        public Watchdog(string localAddress, int numberOfInterfaces, Form1 form)
        {
            this.form = form;

            sendMutex = new Mutex();
            receiveMutex = new Mutex();
            interfaceMutex = new Mutex();
            packetsToSend = new Queue<NetPacket>();
            packetsReceived = new Queue<NetPacket>();

            localEndPoint = new IPEndPoint(IPAddress.Parse(localAddress), 8080);
            netClient = new UdpClient(localEndPoint);

            for (int i = 1; i <= numberOfInterfaces; ++i) //Inicjalizacja interfejsów siecowych
            {
                interfaces.Add(new WNetInterface(localAddress, i, form)); //Tworzy nowy interfejs sieciowy i przypisuje mu numer
            }
        }

        //Oferowanie interfejsów
        public void OfferInterface(WNetInterface wNet, IPEndPoint targetEndpoint)
        {
            wNet.isOffered = true;
            wNet.secondsElapsedLastPing = -1;
            //wyślij do kolejki rzeczy do wysłania....

            form.DisplayOnLog("WATCHDOG>#\tInterface " + wNet.eth.GetInterfaceNumber() + " offered. Awaiting ACK...");
        }
        public void StopOfferingInterface(WNetInterface wNet)
        {
            wNet.isOffered = false;
            wNet.secondsElapsedLastPing = -1;

            this.form.DisplayOnLog("WATCHDOG>#\tInterface " + wNet.eth.GetInterfaceNumber() + " is no longer offered");
        }
    }
}
