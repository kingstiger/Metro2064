using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
/*
Klasa NetInterface jest implementacją interfejsu sieciowego metronomu. Klasa ta posiada swoją instancję klienta UDP, oraz metody
pozwalające na komunikację z innymi interfejsami. Schemat datagramu wykorzystywanego przez tą klasę znajduję się w klasie NetPacket.
*/

namespace MetronomySimul
{
	class NetInterface
	{
		protected UdpClient netClient;                                           //Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
		protected IPEndPoint localEndPoint, targetEndPoint;                   //Instancje klasy IPEndPoint zawierają pary adres IPv4 oraz numer portu endpointu nadawcy i odbiorcy
		protected Queue<NetPacket> packetsToSend, packetsReceived;            //Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie
        protected Mutex sendMutex, receiveMutex;                              //Muteksy dla kolejek komnikatów
        private bool isAvailable { get; set; }                                //Oznacza, czy interfejs nie ma już połączenia
		protected Thread senderThread, listenerThread, processingThread;	  //Uchwyty na wątki
        
        /// <summary>
        /// Tworzy nowy inerfejs sieciowy o numerze zgodnym z interfaceNumber
        /// </summary>
        /// <param name="interfaceNumber"></param>
		public NetInterface(int interfaceNumber)                              //interfaceNumber oznacza numer interfejsu w celu dobrania odpowiedniego portu
		{
            sendMutex = new Mutex();
            receiveMutex = new Mutex();
            isAvailable = true;
			packetsToSend = new Queue<NetPacket>();                           //Inicjalizacja buforów na komunikaty
			packetsReceived = new Queue<NetPacket>();
			senderThread = new Thread(new ThreadStart(SenderThread));
			listenerThread = new Thread(new ThreadStart(ListenerThread));
			processingThread = new Thread(new ThreadStart(ProcessingThread));

			localEndPoint = new IPEndPoint(IPAddress.Any, GetPortNumber(interfaceNumber));      //Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
			netClient = new UdpClient(localEndPoint);												//Inicjalizacja klienta protokołu UDP
		}

        /// <summary>
        /// Tworzy nowy interfejs sieciowy z numerem 0. Użyj tylko w przypadku tworzenia instancji klasy Watchdog
        /// </summary>
        protected NetInterface()
        {
            sendMutex = new Mutex();
            receiveMutex = new Mutex();
            packetsToSend = new Queue<NetPacket>();                           //Inicjalizacja buforów na komunikaty
            packetsReceived = new Queue<NetPacket>();
            senderThread = new Thread(new ThreadStart(SenderThread));
            listenerThread = new Thread(new ThreadStart(ListenerThread));
            processingThread = new Thread(new ThreadStart(ProcessingThread));

            localEndPoint = new IPEndPoint(IPAddress.Any, GetPortNumber(0));      //Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
            netClient = new UdpClient(localEndPoint);
        }

        /// <summary>
        /// Wątek odbierający komunikaty z sieci
        /// </summary>
        virtual protected void ListenerThread()
		{
            byte[] receivedBytes;
            NetPacket receivedPacket;

			while(true)
			{
                receivedBytes = netClient.Receive(ref targetEndPoint);
                receivedPacket = .ReadReceivedMsg(receivedBytes);
				//dodanie go do kolejki pakietów odebranych
			}
		}

        /// <summary>
        /// Wątek wysyłający komunikaty w sieć 
        /// </summary>
        virtual protected void SenderThread()     
		{
			while(true)
			{
				if(packetsToSend.Count > 0)
				{
					//Wyślij pakiet z kolejki
				}
			}
		}

        /// <summary>
        /// Wątek przetwarzający odebrane pakiety
        /// </summary>
        protected void ProcessingThread()
        {
            return;
        }

        /// <summary>
        /// Zwraca pierwszy pakiet w kolejce pakietów odebranych
        /// </summary>
        /// <returns></returns>
        protected NetPacket GetReceivedPacket()
        {
            receiveMutex.WaitOne();
            NetPacket temp = packetsReceived.Dequeue();
            receiveMutex.ReleaseMutex();

            return temp;
        }

        /// <summary>
        /// Dodaje nowy pakiet do kolejki pakietów odebranych
        /// </summary>
        /// <param name="newPacket"></param>
        protected void AddReceivedPacket(NetPacket newPacket)
        {
            receiveMutex.WaitOne();
            packetsReceived.Enqueue(newPacket);
            receiveMutex.ReleaseMutex();
        }

        /// <summary>
        /// Zwraca pierwszy pakiet w kolejce pakietów do wysłania
        /// </summary>
        /// <returns></returns>
        protected NetPacket GetAwaitingToSendPacket()
        {
            sendMutex.WaitOne();
            NetPacket temp = packetsToSend.Dequeue();
            sendMutex.ReleaseMutex();

            return temp;
        }

        /// <summary>
        /// Dodaje nowy pakiet do kolejki pakietów oczekujących na wysłanie
        /// </summary>
        /// <param name="newPacket"></param>
        protected void AddAwaitingToSendPacket(NetPacket newPacket)
        {
            sendMutex.WaitOne();
            packetsToSend.Enqueue(newPacket);
            sendMutex.ReleaseMutex();
        }

        virtual public int GetPortNumber(int interfaceNumber) => 8080 + interfaceNumber;

        public bool IsAvailable() => this.isAvailable;

	    public void SetConnection(IPEndPoint targetEndPoint)
		{
			//Inicjalizacja pól
			this.targetEndPoint = targetEndPoint;

            //Uruchomienie wątków
            processingThread.Start();
			senderThread.Start();
			listenerThread.Start();

			//Sorki, mam chłopaka
			isAvailable = false;
		}

		public void TerminateConnection()
		{
			//Wyłączenie wątków
			senderThread.Suspend();
			listenerThread.Suspend();
            processingThread.Suspend();

			//Czyszczenie pól
			packetsToSend.Clear();
			packetsReceived.Clear();
			targetEndPoint = null;

			//Od dzisiaj jestem wolna
			isAvailable = true;
		}
	}
}
