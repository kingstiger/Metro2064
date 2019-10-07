using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MetronomySimul
{
    //TODO -> deoffer -> DONE
    //TODO -> offered To -> CHYBA DONE
    //TODO -> porty dobre w ack
    
    class Watchdog
    {
        private List<WNetInterface> interfaces = new List<WNetInterface>();
        public Form1 form;                                                  //Uchwyt na okno
        int discoverCounter = 0;
        private UdpClient netClient;                                        //Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
        private IPEndPoint localEndPoint;
        private Queue<NetPacket> packetsToSend, packetsReceived;            //Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie
        private Mutex sendMutex, receiveMutex, interfaceMutex;              //Muteksy dla kolejek komnikatów oraz listy interfejsów
        protected Thread senderThread, listenerThread, cyclicThread;	
        private IPEndPoint multicastReceivingEndpoint;
        //Uchwyty na wątki do wysyłania i odbierania danych, oraz cyklicznego dodawania danych do wysłania przez watchdoga

        public Watchdog(string localAddress, int localPort, int numberOfInterfaces, Form1 form)
        {
            this.form = form;

            sendMutex = new Mutex();
            receiveMutex = new Mutex();
            interfaceMutex = new Mutex();
            packetsToSend = new Queue<NetPacket>();
            packetsReceived = new Queue<NetPacket>();
            multicastReceivingEndpoint = new IPEndPoint(IPAddress.Any, localPort);

            localEndPoint = new IPEndPoint(IPAddress.Parse(localAddress), localPort);
            netClient = new UdpClient(localEndPoint);
            netClient.Client.EnableBroadcast = true;
            for (int i = 1; i <= numberOfInterfaces; ++i) //Inicjalizacja interfejsów siecowych
            {
                interfaces.Add(new WNetInterface(localAddress, localPort, i, form)); //Tworzy nowy interfejs sieciowy i przypisuje mu numer
            }
            cyclicThread = new Thread(CyclicThread);
            listenerThread = new Thread(ListenerThread);
            senderThread = new Thread(SenderThread);
            listenerThread.Start();
            senderThread.Start();
            cyclicThread.Start();
        }

        //=============================================================================================================

        public void StopThreads()
        {
            try
            {
                cyclicThread.Abort();
                senderThread.Abort();
                listenerThread.Abort();
            }
            catch (Exception) {; }
        }
        

        //Wątek wysyłający=============================================================================================

            /// <summary>
            /// Wątek odpowiadający za wysyłanie pakietów z kolejki
            /// </summary>
        private void SenderThread()
        {
            while (true)
            {
                if (packetsToSend.Count > 0)
                {
                    NetPacket toSendPacket = new NetPacket();
                    toSendPacket = GetAwaitingToSendPacket();
                    this.form.DisplayOnLog("WATCHDOG>#\tSending: " + toSendPacket.operation + " to " + toSendPacket.receiver_IP);
                    byte[] bytesToSend = NetPacket.TranslateMsgToSend(toSendPacket);
                    if (toSendPacket.operation.Equals(Operations.ACK))
                    {
                        netClient.Send(bytesToSend, bytesToSend.Length, new IPEndPoint(toSendPacket.receiver_IP, 8080));
                    }
                    else
                    {
                        netClient.Send(bytesToSend, bytesToSend.Length, new IPEndPoint(toSendPacket.receiver_IP, toSendPacket.receiver_port));
                    }
                }
            }
        }


        //=============================================================================================================

        //Wątek nasłuchujący wraz z funkcją przetwarzającą=============================================================

        /// <summary>
        /// Wątek nasłuchujący nadchodzących pakietów i translujący surowe dane na strukturę NetPacket
        /// </summary>
        private void ListenerThread()
        {
            byte[] receivedBytes;
            NetPacket receivedPacket = new NetPacket();

            while (true)
            {
                receivedPacket = new NetPacket();
                receivedBytes = netClient.Receive(ref multicastReceivingEndpoint);
                receivedPacket.ReadReceivedMsg(receivedBytes);
                if (!receivedPacket.sender_IP.ToString().Equals(localEndPoint.Address.ToString()))
                {
                    this.form.DisplayOnLog("WATCHDOG>#\tReceived: " + receivedPacket.operation + " from " + receivedPacket.sender_IP);
                    AddReceivedPacket(receivedPacket);
                    Task.Run(async () => await Process());
                }
            }
        }

        /// <summary>
        /// Implementacja asynchronicznego wywołania funkcji przetwarzającej odebrane pakiety
        /// </summary>
        /// <param name="receivedPacket"></param>
        /// <returns></returns>
        private Task Process()
        {
            var result = Task.Run(() => ProcessingThread());
            return result;
        }


        /// <summary>
        /// Funkcja przetwarzająca odebrane pakiety
        /// </summary>
        /// <param name="toProcess"></param>
        private void ProcessingThread()
        {
            NetPacket toProcess = null;
            while ((toProcess = GetReceivedPacket()) != null)
            {
                if (toProcess.operation.Equals(Operations.DISCOVER))
                {
                    foreach (WNetInterface wNetInterface in interfaces)
                    {
                        if (wNetInterface.IsAvaiable() && !wNetInterface.isOffered)
                        {
                            OfferInterface(wNetInterface, new IPEndPoint(toProcess.sender_IP, toProcess.sender_port));
                            goto End;
                        }
                    }
                }
                if (toProcess.operation.Equals(Operations.OFFER))
                {
                    foreach (WNetInterface wNetInterface in interfaces)
                    {
                        if (wNetInterface.IsAvaiable())
                        {
                            if(wNetInterface.SetConnection(new IPEndPoint(toProcess.sender_IP, toProcess.sender_port)))
                            AddAwaitingToSendPacket(MakeAckPacket(toProcess));
                            goto End;
                        }
                        else
                        {
                            //Wyślij NACK albo nie rób nic
                        }
                    }
                }
                if (toProcess.operation.Equals(Operations.PING))
                {
                    AddAwaitingToSendPacket(MakeAckPacket(toProcess));
                    foreach (WNetInterface wNetInterface in interfaces)
                    {
                        if (wNetInterface.IsConnected()
                            && wNetInterface.GetTargetEndpoint().Address.ToString()
#pragma warning disable CS0618 // Type or member is obsolete
                        .Equals(toProcess.sender_IP.Address.ToString()))
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            wNetInterface.ZeroPing();
                            wNetInterface.ResetPingCount();
                            goto End;
                        }
                    }
                }
                if (toProcess.operation.Equals(Operations.ACK))
                {
                    if (toProcess.data.Equals(Operations.PING))
                    {
                        foreach (WNetInterface wNetInterface in interfaces)
                        {
                            if (wNetInterface.IsConnected()
                            && wNetInterface.GetTargetEndpoint().Address.ToString()
#pragma warning disable CS0618 // Type or member is obsolete
                        .Equals(toProcess.sender_IP.Address.ToString()))
#pragma warning restore CS0618 // Type or member is obsolete
                            {
                                wNetInterface.ZeroPing();
                                wNetInterface.ResetPingCount();
                                goto End;
                            }
                        }
                    }
                    if (toProcess.data.Equals(Operations.OFFER))
                    {
                        foreach (WNetInterface wNetInterface in interfaces)
                        {
                            if (!wNetInterface.IsConnected() && wNetInterface.offeredTo != null)
                            {
                                if (wNetInterface.offeredTo.ToString().Equals(toProcess.sender_IP.Address.ToString()))
                                {
                                    if(wNetInterface.SetConnection(new IPEndPoint(toProcess.sender_IP, toProcess.sender_port)))
                                    StopOfferingInterface(wNetInterface);
                                    goto End;
                                }
                            }
                            else
                            {
                                //wyślij nack
                            }
                        }
                    }
                }
                End:
                toProcess = null;
            }
        }

        //=============================================================================================================

        //Wątek cykliczny i związane z nim metody======================================================================

        /// <summary>
        /// Wątek cyklicznie wysyłający pingi i oscylacje lub discovery
        /// </summary>
        private void CyclicThread()
        {
                                    
            while(true)
            {
                TryDeoffer();
                TryPing();
                TrySendOscillationInformation();
                DiscoverIfAlone();
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Próba pingowania wszystkich dostępnych metronomów lub zrywanie z nimi połączenia
        /// </summary>
        /// <returns></returns>
        private bool TryPing()
        {
            bool hasAnyoneBeenPinged = false;
            foreach (WNetInterface wNetInterface in interfaces)
            {
                if (wNetInterface.IsConnected())
                {
                    if (wNetInterface.IncrementLastPing())
                    {
                        if (wNetInterface.DoTerminate())
                        {
                            if(wNetInterface.TerminateConnection())
                            hasAnyoneBeenPinged = true;
                        }
                        else
                        {
                            AddAwaitingToSendPacket(MakePingPacket(wNetInterface));
                            wNetInterface.ZeroPing();
                            hasAnyoneBeenPinged = true;
                        }
                    }
                }
            }
            return hasAnyoneBeenPinged;
        }

        /// <summary>
        /// Próba zakończenia okresu ważności wysłanych ofert
        /// </summary>
        private void TryDeoffer()
        {
            foreach (WNetInterface wNetInterface in interfaces)
            {
                if(wNetInterface.isOffered)
                {
                    if(wNetInterface.DecrementOfferTime())
                    {
                        StopOfferingInterface(wNetInterface);
                    }
                }
            }
        }

        /// <summary>
        /// Próba wysłania informacji o oscylacji do wszystkich dostępnych metronomów
        /// </summary>
        private void TrySendOscillationInformation()
        {
            foreach (WNetInterface wNetInterface in interfaces)
            {
                if (wNetInterface.IsConnected())
                {
                    wNetInterface.SendOscilations(form.GetOscInfoToSend());
                }
            }
        }


        /// <summary>
        /// Jeśli żaden interfejs nie jest połączony ani zaoferowany, wysyła broadcastowo pakiet discover
        /// </summary>
        private void DiscoverIfAlone()
        {
            if (++discoverCounter >= 10)
            {
                discoverCounter = 0;
                foreach (WNetInterface wNetInterface in interfaces)
                {
                    if (!wNetInterface.IsAvaiable())
                    {
                        return;
                    }
                }
                AddAwaitingToSendPacket(MakeDiscoverPacket());
            }
        }

        //Oferowanie interfejsów=======================================================================================

       /// <summary>
       /// Metoda do oferowania wybranego interfejsu docelowemu adresowi
       /// </summary>
       /// <param name="wNet">Oferowany interfejs</param>
       /// <param name="targetEndpoint">Adres docelowy</param>
        public void OfferInterface(WNetInterface wNet, IPEndPoint targetEndpoint)
        {
            wNet.isOffered = true;
            wNet.offeredTo = targetEndpoint;
            wNet.SetOfferTimeout();
            wNet.ZeroPing();
            AddAwaitingToSendPacket(MakeOfferPacket(wNet, targetEndpoint));
            form.DisplayOnLog("WATCHDOG>#\tInterface " + wNet.eth.GetInterfaceNumber() + " offered. Awaiting ACK...");
        }

        /// <summary>
        /// Metoda do ustawienia statusu interfejsu na nie-oferowany
        /// </summary>
        /// <param name="wNet"></param>
        public void StopOfferingInterface(WNetInterface wNet)
        {
            wNet.isOffered = false;
            wNet.offeredTo = null;
            wNet.ZeroOfferTime();
            wNet.ZeroPing();
            this.form.DisplayOnLog("WATCHDOG>#\tInterface " + wNet.eth.GetInterfaceNumber() + " is no longer offered");
        }

        //=============================================================================================================

        //Metody do obsługi kolejek pakietów do wysłania i odebranych==================================================

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
        /// Zwraca i usuwa pierwszy pakiet w kolejce pakietów odebranych
        /// </summary>
        /// <returns></returns>
        protected NetPacket GetReceivedPacket()
        {
            NetPacket temp;
            receiveMutex.WaitOne();
            if (packetsReceived.Count > 0)
                temp = packetsReceived.Dequeue();
            else
                temp = null;
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

        //================================================================================================================

        //Metody do tworzenia gotowych pakietów ==========================================================================

        /// <summary>
        /// Tworzy pakiet z Pingiem dla podanego interfejsu
        /// </summary>
        /// <param name="wNetInterface"></param>
        /// <returns></returns>
        private NetPacket MakePingPacket(WNetInterface wNetInterface)
        {
            return new NetPacket(
                             wNetInterface.GetLocalEndpoint().Address,
                             wNetInterface.GetTargetEndpoint().Address,
                             wNetInterface.GetLocalEndpoint().Port,
                             8080,
                             0,
                             Operations.PING,
                             "");
        }

        /// <summary>
        /// Tworzy pakiet discover (broadcastowy)
        /// </summary>
        /// <returns></returns>
        private NetPacket MakeDiscoverPacket()
        {
            return new NetPacket(
                this.localEndPoint.Address,
                IPAddress.Broadcast,
                this.localEndPoint.Port,
                this.multicastReceivingEndpoint.Port,
                0,
                Operations.DISCOVER,
                ""
                );
        }

        /// <summary>
        /// Tworzy pakiet z ofertą wskazanego netInterface'u, adresowany na podany endpoint
        /// </summary>
        /// <param name="wNetInterface"></param>
        /// <param name="iPEndPoint"></param>
        /// <returns></returns>
        private NetPacket MakeOfferPacket(WNetInterface wNetInterface, IPEndPoint iPEndPoint)
        {
            return new NetPacket(
                wNetInterface.GetLocalEndpoint().Address,
                iPEndPoint.Address,
                wNetInterface.GetLocalEndpoint().Port,
                iPEndPoint.Port,
                0,
                Operations.OFFER,
                ""
                );
        }

        /// <summary>
        /// Tworzy pakiet ACK w odpowiedzi na podany pakiet
        /// </summary>
        /// <param name="wNetInterface"></param>
        /// <param name="acknowledgedPacket"></param>
        /// <returns></returns>
        private NetPacket MakeAckPacket(NetPacket acknowledgedPacket)
        {
            return new NetPacket(   
                acknowledgedPacket.receiver_IP,
                acknowledgedPacket.sender_IP,
                acknowledgedPacket.receiver_port,
                acknowledgedPacket.sender_port,
                0,
                Operations.ACK,
                acknowledgedPacket.operation
                );
        }
    }
}
