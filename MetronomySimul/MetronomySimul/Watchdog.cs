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
	class Watchdog : NetInterface
	{
        private List<NetInterface> interfaces;

		public Watchdog(int amount_of_interfaces) : base()
		{
           for (int i = 0; i < amount_of_interfaces; i++)
           {
                interfaces.Add(new NetInterface(i));
           }
        }

        protected override void ProcessingThread()
        {
            while (true)
            {
                //jak są jakieś otrzymane pakiety to je przetwarza
                if (packetsReceived.Count > 0)
                {
                    NetPacket toProcess = GetReceivedPacket();

                    if (toProcess.operation == Operations.DISCOVER)
                    {
                        
                        foreach(NetInterface x in interfaces)
                        {
                            if(x.IsAvailable())
                            {
                                NetPacket packetToSend = new NetPacket(toProcess, IPAddress.Any, (interfaces.IndexOf(x) + 1).ToString());
                                AddAwaitingToSendPacket(packetToSend);
                                break;
                            }
                           
                        }
                    }

                    if(toProcess.operation == Operations.OFFER)
                    {
                       
                        foreach(NetInterface x in interfaces)
                        {
                            if(x.IsAvailable())
                            {
                                x.SetConnection(new IPEndPoint(toProcess.sender_IP, GetPortNumber(Int32.Parse(toProcess.data))));
                                //Odpowiadając ACK na komunikat OFFER przesyłamy w polu danych nazwę operacji która zostaje potwierdzona (OFFER) i numer interfejsu na którym zestawiliśmy połączenie
                                NetPacket packetToSend = new NetPacket(toProcess, Operations.ACK, Operations.OFFER + ";" + (interfaces.IndexOf(x) + 1).ToString());
                                AddAwaitingToSendPacket(packetToSend);
                                break;
                            }
                        }

                    }

                    if (toProcess.operation == Operations.SYNC)
                    {
                        OscillatorUpdator.GiveOscInfoForeign(NetPacket.ReadOscInfoFromData(toProcess.data));
                    }

                    if (toProcess.operation == Operations.PING)
                    {
                        NetPacket packetToSend = new NetPacket(toProcess, Operations.ACK, Operations.PING);
                        AddAwaitingToSendPacket(packetToSend);
                    }
                    

                    if (toProcess.operation == Operations.ACK)
                    {
                        if(toProcess.data == Operations.PING)
                        {
                            //przestawiamy flagę oczekiwania na ACK po PINGU (go home, ur drunk)
                        } else
                        {
                            //w przeciwnym przypadku ACK otrzymujemy po wysłaniu pakietu OFFER

                        }
                    }

                    if (toProcess.operation == Operations.NACK)
                    {

                    }
                }

               
            }
            
        }
    }
}
