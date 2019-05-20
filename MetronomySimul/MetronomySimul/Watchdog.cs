using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


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


	}
}
