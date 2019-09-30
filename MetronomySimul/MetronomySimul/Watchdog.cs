using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using System;
/*
Klasa Watchdog dziedziczy po klasie NetInterface i jest odpowiedzialna za znajdywanie i wiązanie nowych metronomów
z wolnymi interfejsami. Dodatkowo Watchdog ma cyklicznie sprawdza czy metronomy są "połączone". Jeżeli nie, to zwalnia
interfejs sieciowy do którego nie przychodzą żadne odpowiedzi
*/

namespace MetronomySimul
{
	sealed class Watchdog : NetInterface
	{
        private List<NetInterface> interfaces = new List<NetInterface>();
        private List<int> offeredInterfacesNumbers;   //Lista interfejsów zaoferowanych do innych metronomów.
        public List<NetPacket> connectedInterfaces = new List<NetPacket>(); //Gotowe do wysłania NetPacket-y (do wstawienia PING albo DISCOVER)
        private IPEndPoint multicastReceivingEndpoint;
        private Mutex offeredMutex;
        public int seconds_elapsed_since_last_pings;
        private int[] seconds_to_disconnect;
        private Thread cyclic;

        public Watchdog(int amount_of_interfaces, Form1 form) : base(0, form)
		{
            multicastReceivingEndpoint = new IPEndPoint(IPAddress.Any, GetPortNumber(0));

            offeredMutex = new Mutex();
            offeredInterfacesNumbers = new List<int>();

           for (int i = 1; i <= amount_of_interfaces; i++)
           {
                interfaces.Add(new NetInterface(i, form));
           }
            netClient.Client.EnableBroadcast = true;
            SetConnection(new IPEndPoint(IPAddress.Broadcast, GetPortNumber(0)));
            cyclic = new Thread(Cyclic);
            cyclic.Start();
            seconds_to_disconnect = new int[amount_of_interfaces + 1];
        }

        //cyklicznie wysyła PING oraz DISCOVER
        private void Cyclic()
        {

            seconds_elapsed_since_last_pings = 0;
            while (true)
            {
                
                if (seconds_elapsed_since_last_pings > 10)
                {
                    NetPacket cyclic;
                    if (connectedInterfaces.Count > 0)
                    {
                        

                        foreach (NetInterface x in interfaces)
                        {
                            foreach (NetPacket y in connectedInterfaces)
                            {
                                if (y.receiver_IP.ToString().Equals(x.GetTargetEndpoint().Address.ToString()))
                                {
                                    cyclic = new NetPacket(y);
                                    cyclic.data = "";
                                    cyclic.operation = Operations.PING;
                                    AddAwaitingToSendPacket(cyclic);
                                }
                            }

                            if (!x.IsAvailable())
                            {
                                if (OscillatorUpdator.oscillation_info_domestic.Count > 0)
                                {
                                    Tuple<double, double> oscilation_info = OscillatorUpdator.GetOscInfoDomestic();
                                    x.AddAwaitingToSendPacket(x.MakeSyncPacket(oscilation_info)); 
                                }

                                if (seconds_to_disconnect[interfaces.IndexOf(x)+1] <= 0)
                                {
                                    int indexToDelete = -1;
                                    foreach(NetPacket y in connectedInterfaces)
                                    {
                                        if (y.receiver_IP.ToString().Equals(x.GetTargetEndpoint().Address.ToString()))
                                        {
                                            //connectedInterfaces.Remove(y);
                                            indexToDelete = connectedInterfaces.IndexOf(y);
                                        }
                                    }
                                    if (indexToDelete != -1)
                                    {
                                        connectedInterfaces.RemoveAt(indexToDelete);
                                    }
                                    RemoveOfferedInterface(x.interfaceNumber);
                                    x.TerminateConnection();
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        cyclic = MakeDiscoverPacket();
                        AddAwaitingToSendPacket(cyclic);
                    }
                    seconds_elapsed_since_last_pings = 0;
                } else
                {
                    seconds_elapsed_since_last_pings++;
                    for (int i = 1; i < seconds_to_disconnect.Length; i++)
                    {
                        seconds_to_disconnect[i]--;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        protected override void ProcessingThread()
        {
            
                //jak są jakieś otrzymane pakiety to je przetwarza
                if (packetsReceived.Count > 0)
                {
                    NetPacket toProcess = GetReceivedPacket();

                    //Na pakiet DISCOVER odpowiadamy OFFER
                    if (toProcess.operation == Operations.DISCOVER)
                    {
                        foreach (NetInterface x in interfaces)
                        {
                            if (x.IsAvailable() && !(offeredInterfacesNumbers.Contains(interfaces.IndexOf(x) + 1)))
                            {
                                NetPacket packetToSend = new NetPacket(toProcess, localEndPoint.Address, x.GetInterfaceNumber().ToString());
                                AddOfferedInterface(interfaces.IndexOf(x) + 1);
                                AddAwaitingToSendPacket(packetToSend);
                                break;
                            }

                        }
                    }

                    //Na pakiet OFFER odpowiadamy ACK
                    if (toProcess.operation == Operations.OFFER)
                    {
                        foreach (NetInterface x in interfaces)
                        {
                            if (x.IsAvailable() && !(offeredInterfacesNumbers.Contains(interfaces.IndexOf(x))))
                            {
                                x.SetConnection(new IPEndPoint(toProcess.sender_IP, GetPortNumber(ParseToInt(toProcess.data))));
                                connectedInterfaces.Add(new NetPacket(
                                toProcess.receiver_IP,
                                toProcess.sender_IP,
                                toProcess.receiver_port,
                                toProcess.sender_port,
                                0,
                                Operations.PING,
                                toProcess.data
                                ));
                                //Odpowiadając ACK na komunikat OFFER przesyłamy w polu danych nazwę operacji która zostaje potwierdzona (OFFER) i numer interfejsu na którym zestawiliśmy połączenie
                                NetPacket packetToSend = new NetPacket(toProcess, Operations.ACK, Operations.OFFER + ";" + (interfaces.IndexOf(x) + 1).ToString());
                                AddAwaitingToSendPacket(packetToSend);
                                break;
                            }
                        }

                    }

                    //Na pakiet PING odpowiadamy ACK
                    if (toProcess.operation == Operations.PING)
                    {
                        NetPacket packetToSend = new NetPacket(toProcess, Operations.ACK, Operations.PING);
                        AddAwaitingToSendPacket(packetToSend);
                    }

                    //Na pakiet ACK odpowiadamy.... HMMMMMM to zależu NIEEEEEEEEEEEEEEEEE
                    if (toProcess.operation == Operations.ACK)
                    {
                        if (toProcess.data == Operations.PING)
                        {
                            foreach (NetPacket y in connectedInterfaces)
                            {
                                if (y.receiver_port == toProcess.sender_port)
                                {
                                    seconds_to_disconnect[connectedInterfaces.IndexOf(y)] = 20;
                                }
                            }
                            //przestawiamy flagę oczekiwania na ACK po PINGU (go home, ur drunk)
                        }
                        else
                        {
                            //w przeciwnym przypadku ACK otrzymujemy po wysłaniu pakietu OFFER

                        }
                    }

                    if (toProcess.operation == Operations.NACK)
                    {

                    }
                }

            
        }

        public override void StopThreads()
        {
            try
            {
                base.StopThreads();
                foreach (NetInterface n in interfaces)
                {
                    try
                    {
                        n.StopThreads();
                    }
                    catch(ThreadStateException ex)
                    {
                        continue;
                    }
                }
                //cyclic.Abort();
            } catch(ThreadStateException ex)
            {
                throw ex;
            }
}

        protected override void SenderThread()
        {
            while (true)
            {
                if (packetsToSend.Count > 0)
                {
                    NetPacket toSendPacket = new NetPacket();
                    toSendPacket = GetAwaitingToSendPacket();
                    this.form.DisplayOnLog("WATCHDOG>#\tSending: " + toSendPacket.operation + " to " + toSendPacket.receiver_IP);
                    byte[] bytesToSend = NetPacket.TranslateMsgToSend(toSendPacket);
                    netClient.Send(bytesToSend, bytesToSend.Length, new IPEndPoint(toSendPacket.receiver_IP, toSendPacket.receiver_port));
                }
            }
        }

        protected override void ListenerThread()
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
        private Task Process()
        {
            var result = Task.Run(() => ProcessingThread());
            return result;
        }
        /// <summary>
        /// Tworzenie pakietu do wysłania komunikatu DISCOVER
        /// </summary>
        /// <param name="oscilation_info"></param>
        /// <returns></returns>
        private NetPacket MakeDiscoverPacket()
        {
            NetPacket discover = new NetPacket(
                        localEndPoint.Address, IPAddress.Broadcast, localEndPoint.Port, GetPortNumber(0),
                        0, Operations.DISCOVER, "");
            return discover;
        }

        /// <summary>
        /// Dodaje parę adres oferenta i numer oferowanego interfejsu do listy zaoferowanych interfejsów
        /// </summary>
        /// <param name="bidderAddress">-> Adres oferenta interfejsu sieciowego</param>
        /// <param name="offeredInterfaceNumber">-> Numer oferowanego interfejsu</param>
        private void AddOfferedInterface(int offeredInterfaceNumber)
            {
                offeredMutex.WaitOne();
                offeredInterfacesNumbers.Add(offeredInterfaceNumber);
                form.DisplayOnLog("WATCHDOG>#\tInterface " + offeredInterfaceNumber + " offered. Awaiting connection...");
                offeredMutex.ReleaseMutex();
            }

        /// <summary>
        /// Usuwa interfejs z listy zaoferowanych interfejsów o podanym numerze
        /// </summary>
        /// <param name="offeredInterfaceNumber"></param>
        private void RemoveOfferedInterface(int offeredInterfaceNumber)
        {
            offeredMutex.WaitOne();
            int indexToDelete = -1;
            foreach (int n in offeredInterfacesNumbers)
            {
                if (n == offeredInterfaceNumber)
                {
                    indexToDelete = offeredInterfacesNumbers.IndexOf(n);
                    this.form.DisplayOnLog("WATCHDOG>#\tInterface " + offeredInterfaceNumber + " is no longer offered");
                }
            }
            if(indexToDelete != -1)
            {
                offeredInterfacesNumbers.RemoveAt(indexToDelete);
            }

            offeredMutex.ReleaseMutex();
        }

        /// <summary>
        /// Zwraca wartosć true jeżeli metronom ma co najmniej jedno zestawione połączenie
        /// </summary>
        /// <returns></returns>
        private bool AmIConnectedSomewhere()
        {
            foreach (NetInterface x in interfaces)
            {
                if (x.IsAvailable() == true)
                    return true;
            }

            return false;
        }



    }
}
