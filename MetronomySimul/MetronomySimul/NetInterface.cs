using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

/*
Klasa NetInterface jest implementacją interfejsu sieciowego metronomu. Klasa ta posiada swoją instancję klienta UDP, oraz metody
pozwalające na komunikację z innymi interfejsami. Schemat datagramu wykorzystywanego przez tą klasę znajduję się w klasie NetPacket.
*/

namespace MetronomySimul
{
	class NetInterface
	{
		UdpClient client;											//Instancja klasy UdpClient do przesyłania danych przez sieć za pomocą protokołu UDP
		IPEndPoint localEndPoint, targetEndPoint;					//Instancje klasy IPEndPoint zawierają pary adres IPv4 oraz numer portu endpointu nadawcy i odbiorcy
		Queue<NetPacket> packetsToSend, packetsReceived;			//Kolejki (bufory) komunikatów przychodzących i oczekujących na wysłanie

		NetInterface()
		{
			packetsToSend = new Queue<NetPacket>();					//Inicjalizacja buforów na komunikaty
			packetsReceived = new Queue<NetPacket>();

			localEndPoint = new IPEndPoint(IPAddress.Any, 0);		//Lokalny endpoint otrzyma adres karty sieciowej i wolny numer portu
			targetEndPoint = new IPEndPoint(IPAddress.None, 0);		//Na początku nie znamy danych nadawcy, dlatego w konstruktorze przypisujemy pusty adres IPv4

			//localEndPoint.Address = IPAddress.Parse("192.168.0.1");

			
		}
	}
}
