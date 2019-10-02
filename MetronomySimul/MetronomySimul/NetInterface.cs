using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MetronomySimul
{
    /// <summary>
    /// Klasa NetInterface jest implementacją interfejsu sieciowego metronomu. Klasa ta posiada swoją instancję klienta UDP, 
    /// oraz metody pozwalające na komunikację z innymi interfejsami.
    /// Schemat datagramu wykorzystywanego przez tą klasę znajduję się w klasie NetPacket.
    /// </summary>
    class NetInterface
	{
		protected UdpClient netClient;                                      //Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
        public IPEndPoint localEndPoint; 
        public IPEndPoint targetEndPoint;                                   //Instancje klasy IPEndPoint zawierają pary adres IPv4 oraz numer portu endpointu nadawcy i odbiorcy
		protected Queue<NetPacket> packetsToSend, packetsReceived;          //Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie
        protected Mutex sendMutex, receiveMutex;                            //Muteksy dla kolejek komnikatów
        private bool isConnected;                                           //Oznacza, czy interfejs nie ma już połączenia
		protected Thread senderThread, listenerThread;	                    //Uchwyty na wątki
        private int seq_number;

        public Form1 form; //Uchwyt na okno
        public int interfaceNumber; //Numer interfejsu

        /// <summary>
        /// Tworzy nową instancję interfejsu sieciowego do wymieniania danych oscylacji
        /// </summary>
        /// <param name="localAddress">Lokalny adres IP karty sieciowej</param>
        /// <param name="interfaceNumber">Na podstawie numeru interfejsu zostaje przypisany mu nr portu</param>
        /// <param name="form">Uchwyt na okno w celu wyświetlania logu</param>
		public NetInterface(string localAddress, int interfaceNumber, Form1 form)
		{
            this.form = form;
            seq_number = 0;
            sendMutex = new Mutex();
            receiveMutex = new Mutex();
            isConnected = false;
			packetsToSend = new Queue<NetPacket>(); //Inicjalizacja buforów na komunikaty
			packetsReceived = new Queue<NetPacket>();
            this.interfaceNumber = interfaceNumber;
            
			localEndPoint = new IPEndPoint(IPAddress.Parse(localAddress), 8080 + interfaceNumber); //Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
			netClient = new UdpClient(localEndPoint);	//Inicjalizacja klienta protokołu UDP
		}


        //Wątki====================================================================================
        /// <summary>
        /// Uruchamia wątki nasłuchujące i wysyłające dane
        /// </summary>
        private void StartThreads()
        {
            try
            {
                senderThread = new Thread(SenderThread);
                listenerThread = new Thread(ListenerThread);

                senderThread.Start();
                listenerThread.Start();
            }
            catch (System.Exception e)
            {
                this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\t exception caught while starting the threads: " + e.ToString());
            }
        }

        /// <summary>
        /// Zatrzymuje wątki nasłuchujące i wysyłające dane
        /// </summary>
        private void StopThreads()
        {
            try
            {
                senderThread.Abort();
                listenerThread.Abort();
            }
            catch (System.Exception e)
            {
                this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\t exception caught while starting the threads: " + e.ToString());
            }
        }
        /// <summary> 
        /// Wątek odbierający komunikaty z sieci
        /// </summary>
        private void ListenerThread()
		{
            byte[] receivedBytes;
            NetPacket receivedPacket = new NetPacket();

			while(true)
			{
                receivedPacket = new NetPacket();
                receivedBytes = netClient.Receive(ref targetEndPoint);
                receivedPacket.ReadReceivedMsg(receivedBytes);

                if (!receivedPacket.sender_IP.ToString().Equals(localEndPoint.Address.ToString()) && receivedPacket.sender_port != 8080)
                {
                    packetsReceived.Enqueue(receivedPacket);
                    Task.Run(async () => await Process());
                }
			}
		}

		/// <summary>
		/// Wątek wysyłający komunikaty o oscylacji w do połączonego metronomu
		/// </summary>
		private void SenderThread()     
		{
			while(true)
			{
				if(packetsToSend.Count > 0)
				{
                    byte[] bytesToSend = NetPacket.TranslateMsgToSend(GetAwaitingToSendPacket());
                    this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\tSending bytes: " + bytesToSend.ToString() + " to " + targetEndPoint.ToString()); 
                    netClient.Send(bytesToSend, bytesToSend.Length, targetEndPoint);
				}
			}
		}

        /// <summary>
        /// Zadanie uruchamiane asynchronicznie, podczas gdy odbierzemy pakiet od połączonego metronomu w wątku ListenerThread
        /// </summary>
        /// <returns></returns>
        private Task Process() 
        {
            var result = Task.Run(() => ProcessingThread());
            return result;
        }
             
		/// <summary>
		/// Wątek przetwarzający odebrane pakiety od połączonego metronomu, wywoływany asynchronicznie w metodzie Process()
		/// </summary>
		private void ProcessingThread()
        {
                //jak są jakieś otrzymane pakiety to je przetwarza
                if(packetsReceived.Count > 0)
                {
                    NetPacket toProcess = GetReceivedPacket();
                    this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\tReceived: " + toProcess.operation + " from " + toProcess.sender_IP);


                    if (toProcess.operation == Operations.SYNC)
                    {
                        OscillatorUpdator.GiveOscInfoForeign(NetPacket.ReadOscInfoFromData(toProcess.data));
                    }                    
				}
        }
        //=========================================================================================



        //Tworzenie i obsługa pakietów Net=========================================================
        /// <summary>
        /// Dodaje do kolejki pakietów do wysłania dane oscylacji
        /// </summary>
        /// <param name="oscilation_info"></param>
		public void sendSyncPacket(System.Tuple<double, double> oscilation_info)
        {
            AddAwaitingToSendPacket(MakeSyncPacket(oscilation_info));
        }

        /// <summary>
        /// Tworzenie pakietu z danymi oscylacji do synchronizacji z powiązanym na danym interfejsie metronomem
        /// </summary>
        /// <param name="oscilation_info"></param>
        /// <returns></returns>
        public NetPacket MakeSyncPacket(System.Tuple<double, double> oscilation_info)
        {
            return new NetPacket(
                        localEndPoint.Address,
                        targetEndPoint.Address,
                        localEndPoint.Port,
                        targetEndPoint.Port,
                        seq_number,
                        Operations.SYNC,
                        oscilation_info.Item1.ToString() + ";" + oscilation_info.Item2.ToString() + ";");
        }

        /// <summary>
        /// Zwraca i usuwa pierwszy pakiet w kolejce pakietów odebranych
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
        /// Zwraca i usuwa pierwszy pakiet w kolejce pakietów do wysłania
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
        //=========================================================================================



        //Nawiązywanie i przerywanie połączeń======================================================
        public bool IsConnected() => isConnected;

        /// <summary>
        /// Nawiązuje połączenie z metronomem o wskazanym endpoincie, jeżeli nie jest jeszcze połączony
        /// </summary>
        /// <param name="targetEndPoint">IPEndPoint docelowego metronomu</param>
	    public void SetConnection(IPEndPoint targetEndPoint)
		{
            if (isConnected == false)
            {
                //Inicjalizacja pól
                this.targetEndPoint = targetEndPoint;

                //Uruchomienie wątków
                StartThreads();

                //Sorki, mam chłopaka
                isConnected = true;

                //Log
                this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\t has connected to " + targetEndPoint.Address.ToString());
            }
            else
                this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\t tried to connect to endpoint " + targetEndPoint.Address.ToString() + "but is already connected!");
        }

        /// <summary>
        /// Przerywa obecne połączenie, jeżeli takie istnieje
        /// </summary>
		public void TerminateConnection()
		{
            if (isConnected == true)
            {
                //Wyłączenie wątków
                StopThreads();

                //Czyszczenie pól
                packetsToSend.Clear();
                packetsReceived.Clear();
                targetEndPoint = null;

                //Od dzisiaj jestem wolna
                isConnected = false;

                //Log
                this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\t has disconected from another metronome");
            }
            else
                this.form.DisplayOnLog("ETH" + this.interfaceNumber + ">$\t\t tried to disconnect, but wasn't connected anywhere!");
        }
        //=========================================================================================



        //Przydatne metody=========================================================================
        public int GetPortNumber() => localEndPoint.Port;

        public int GetInterfaceNumber() => localEndPoint.Port - 8080;

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
