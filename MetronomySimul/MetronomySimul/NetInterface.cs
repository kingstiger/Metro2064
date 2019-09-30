using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
		protected UdpClient netClient;                                        //Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
        protected IPEndPoint localEndPoint; 
        private IPEndPoint targetEndPoint;                                    //Instancje klasy IPEndPoint zawierają pary adres IPv4 oraz numer portu endpointu nadawcy i odbiorcy
		protected Queue<NetPacket> packetsToSend, packetsReceived;            //Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie
        protected Mutex sendMutex, receiveMutex;                              //Muteksy dla kolejek komnikatów
        private bool isAvailable { get; set; }                                //Oznacza, czy interfejs nie ma już połączenia
		protected Thread senderThread, listenerThread;	  //Uchwyty na wątki
        private int seq_number;

        public Form1 form; //Uchwyt na okno
        public int interfaceNumber; //Numer interfejsu

        /// <summary>
        /// Tworzy nowy inerfejs sieciowy o numerze zgodnym z interfaceNumber
        /// </summary>
        /// <param name="interfaceNumber">  numer interfejsu w celu dobrania odpowiedniego portu </param>
		public NetInterface(int interfaceNumber, Form1 form)                              //interfaceNumber oznacza numer interfejsu w celu dobrania odpowiedniego portu
		{
            this.form = form;
            seq_number = 0;
            sendMutex = new Mutex();
            receiveMutex = new Mutex();
            isAvailable = true;
			packetsToSend = new Queue<NetPacket>();                           //Inicjalizacja buforów na komunikaty
			packetsReceived = new Queue<NetPacket>();
			senderThread = new Thread(SenderThread);
			listenerThread = new Thread(ListenerThread);
            this.interfaceNumber = interfaceNumber;
            
			localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.9"), GetPortNumber(this.interfaceNumber));      //Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
			netClient = new UdpClient(localEndPoint);											//Inicjalizacja klienta protokołu UDP
		}

        /// <summary> 
        /// Wątek odbierający komunikaty z sieci
        /// </summary>
        virtual protected void ListenerThread()
		{
            byte[] receivedBytes;
            NetPacket receivedPacket = new NetPacket();

			while(true)
			{
                receivedPacket = new NetPacket();
                receivedBytes = netClient.Receive(ref targetEndPoint);
                receivedPacket.ReadReceivedMsg(receivedBytes);
                if (receivedPacket.sender_IP != localEndPoint.Address && receivedPacket.sender_port != GetPortNumber(0))
                {
                    packetsReceived.Enqueue(receivedPacket);
                    Task.Run(async () => await Process());
                }
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
                    byte[] bytesToSend = NetPacket.TranslateMsgToSend(GetAwaitingToSendPacket());
                    this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\tSending bytes: " + bytesToSend.ToString() + " to " + targetEndPoint.ToString()); 
                    netClient.Send(bytesToSend, bytesToSend.Length, targetEndPoint);
				}
			}
		}

        private Task Process() {
            var result = Task.Run(() => ProcessingThread());
            return result;
        }
             

		/// <summary>
		/// Wątek przetwarzający odebrane pakiety
		/// </summary>
		virtual protected void ProcessingThread()
        {
        
                //jak są jakieś otrzymane pakiety to je przetwarza
                if(packetsReceived.Count > 0)
                {
                    NetPacket toProcess = GetReceivedPacket();
                    this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\tReceived: " + toProcess.operation + " from " + toProcess.sender_IP);


                    if (toProcess.operation == Operations.SYNC)
                    {
                        OscillatorUpdator.GiveOscInfoForeign(NetPacket.ReadOscInfoFromData(toProcess.data));
                    }

					if (toProcess.operation == Operations.PING)
					{
						NetPacket packetToSend = new NetPacket(toProcess, Operations.ACK);
						AddAwaitingToSendPacket(packetToSend);
					}

					if (toProcess.operation == Operations.ERROR)
					{

					}

					if (toProcess.operation == Operations.ACK)
					{

					}

					if (toProcess.operation == Operations.NACK)
					{

					}
                    
				}

                //jak są jakieś informacje do wysłania, to je wsadza do kolejki do wysłania
                if(OscillatorUpdator.oscillation_info_domestic.Count > 0)
                {
                    System.Tuple<double, double> oscilation_info = OscillatorUpdator.GetOscInfoDomestic();
                    AddAwaitingToSendPacket(MakeSyncPacket(oscilation_info)); //tu nie jestem pewien co z nr sekwencyjnym

                }
            
        }

		
        /// <summary>
        /// Tworzenie pakietu do synchronizacji z powiązanym na danym interfejsie metronomem
        /// </summary>
        /// <param name="oscilation_info"></param>
        /// <returns></returns>
        public NetPacket MakeSyncPacket(System.Tuple<double, double> oscilation_info)
        {
            NetPacket sync = new NetPacket(
                        localEndPoint.Address, targetEndPoint.Address, localEndPoint.Port, targetEndPoint.Port,
                        seq_number, Operations.SYNC, oscilation_info.Item1.ToString() + ";" + oscilation_info.Item2.ToString() + ";");
            return sync;
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
        public void AddAwaitingToSendPacket(NetPacket newPacket)
        {
            sendMutex.WaitOne();
            packetsToSend.Enqueue(newPacket);
            sendMutex.ReleaseMutex();
        }

        public int GetPortNumber(int interfaceNumber) => 8080 + interfaceNumber;

        public int GetInterfaceNumber() => localEndPoint.Port - 8080;

        public bool IsAvailable() => isAvailable;

	    public void SetConnection(IPEndPoint targetEndPoint)
		{
			//Inicjalizacja pól
			this.targetEndPoint = targetEndPoint;

            //Uruchomienie wątków
			senderThread.Start();
			listenerThread.Start();

			//Sorki, mam chłopaka
			isAvailable = false;

            //Log
            this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t has connected to " + targetEndPoint.Address.ToString());
        }

		public void TerminateConnection()
		{
            //Wyłączenie wątków
#pragma warning disable CS0618 // Type or member is obsolete
            senderThread.Suspend();
            listenerThread.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete

            //Czyszczenie pól
            packetsToSend.Clear();
			packetsReceived.Clear();
			targetEndPoint = null;

			//Od dzisiaj jestem wolna
			isAvailable = true;

            //Log
            this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t has disconected from another metronome");
        }
        virtual public void StopThreads()
        {
            try
            {
                senderThread.Abort();
                listenerThread.Abort();
            } catch(ThreadStateException ex)
            {
                throw ex;
            }
        }
        
        public int ParseToInt(string str)
        {
            int result = 0;
            foreach(char x in str) {
                switch (x)
                {
                    case '0':
                        result *= 10;
                        break;
                    case '1':
                        result += 1;
                        result *= 10;
                        break;
                    case '2':
                        result += 2;
                        result *= 10;
                        break;
                    case '3':
                        result += 3;
                        result *= 10;
                        break;
                    case '4':
                        result += 4;
                        result *= 10;
                        break;
                    case '5':
                        result += 5;
                        result *= 10;
                        break;
                    case '6':
                        result += 6;
                        result *= 10;
                        break;
                    case '7':
                        result += 7;
                        result *= 10;
                        break;
                    case '8':
                        result += 8;
                        result *= 10;
                        break;
                    case '9':
                        result += 9;
                        result *= 10;
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
	}
}
