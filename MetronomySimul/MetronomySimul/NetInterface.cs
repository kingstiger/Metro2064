﻿using System.Collections.Generic;
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
		private UdpClient client;                                           //Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
		private IPEndPoint localEndPoint, targetEndPoint;                   //Instancje klasy IPEndPoint zawierają pary adres IPv4 oraz numer portu endpointu nadawcy i odbiorcy
		private Queue<NetPacket> packetsToSend, packetsReceived;            //Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie
		private bool isAvailable { get; set; }                              //Oznacza, czy interfejs nie ma już połączenia
		Thread senderThread, listenerThread, processingThread;				//Uchwyty na wątki

		public NetInterface(int interfaceNumber)                    //interfaceNumber oznacza numer interfejsu w celu dobrania odpowiedniego portu
		{
			isAvailable = true;
			packetsToSend = new Queue<NetPacket>();                 //Inicjalizacja buforów na komunikaty
			packetsReceived = new Queue<NetPacket>();
			senderThread = new Thread(new ThreadStart(SenderThread));
			listenerThread = new Thread(new ThreadStart(ListenerThread));
			processingThread = new Thread(new ThreadStart(ProcessingThread));

			localEndPoint = new IPEndPoint(IPAddress.Any, GetPortNumber(interfaceNumber));      //Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
			client = new UdpClient(localEndPoint);												//Inicjalizacja klienta protokołu UDP
		}

		private int GetPortNumber(int interfaceNumber) => 8080 + interfaceNumber;

		private void ListenerThread()   //Wątek odbierający komunikaty z sieci
		{
			while(true)
			{
				//odbieranie pakietu
				//dodanie go do kolejki pakietów odebranych
			}
		}

		private void SenderThread()     //Wątek wysyłający komunikaty w sieć
		{
			while(true)
			{
				if(packetsToSend.Count > 0)
				{
					//Wyślij pakiet z kolejki
				}
			}
		}

		public bool IsAvailable() => this.isAvailable;

		public void SetConnection(IPEndPoint targetEndPoint)
		{
			//Inicjalizacja pól
			this.targetEndPoint = targetEndPoint;

			//Uruchomienie wątków
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

			//Czyszczenie pól
			packetsToSend.Clear();
			packetsReceived.Clear();
			targetEndPoint = null;

			//Od dzisiaj jestem wolna
			isAvailable = true;
		}

		private void ProcessingThread()     //Przetwarzanie pakietów, ahhhhhhh to bdzie duże
		{
			return;
		}
	}
}
