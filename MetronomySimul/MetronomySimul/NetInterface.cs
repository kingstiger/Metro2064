using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

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
		private bool isAvailable;                                           //Oznacza, czy interfejs nie ma już połączenia
		Thread senderThread;												//Uchwyty na wątki
		Thread listenerThread;

		public NetInterface(int interfaceNumber)                    //interfaceNumber oznacza numer interfejsu w celu dobrania odpowiedniego portu
		{
			isAvailable = true;
			packetsToSend = new Queue<NetPacket>();                 //Inicjalizacja buforów na komunikaty
			packetsReceived = new Queue<NetPacket>();
			senderThread = new Thread(new ThreadStart(SenderThread));
			listenerThread = new Thread(new ThreadStart(ListenerThread));

			localEndPoint = new IPEndPoint(IPAddress.Any, GetPortNumber(interfaceNumber));      //Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
			client = new UdpClient(localEndPoint);                  //Inicjalizacja klienta protokołu UDP
		}

		private int GetPortNumber(int interfaceNumber) => 8080 + interfaceNumber;

		private bool ImAvailable() => this.isAvailable = true;

		private bool ImNotAvailable() => this.isAvailable = false;

		private void ListenerThread()   //Wątek odbierający komunikaty z sieci
		{

		}

		private void SenderThread()     //Wątek wysyłający komunikaty w sieć
		{

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
			ImNotAvailable();
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
			ImAvailable();
		}
	}
}
