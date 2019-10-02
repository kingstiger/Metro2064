using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MetronomySimul
{
    
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
            Task.Run(async () => await Cyclic());
        }

        private Task Cyclic()
        {
            var result = Task.Run(() => CyclicThread());
            return result;
        }

        private void CyclicThread()
        {
            while(true)
            {

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
