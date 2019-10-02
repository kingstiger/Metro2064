using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MetronomySimul
{
    /// <summary>
    /// Klasa WNetInterface zawiera w sobie instancję interfejsu sieciowego, oraz szereg metod
    /// służacych do komunikacji między Watchdogiem a interfejsem, oraz dodatkowe pola do oznaczania interfejsu jako
    /// zaoferowanego lub połączonego i czas od ostatniego wysłania pakietu PING
    /// </summary>
    class WNetInterface
    {
        public NetInterface eth;   //Interfejs sieciowy
        public bool isOffered;     //Flaga oznaczająca, czy interfejs został zaoferowany, ale jeszcze nie utworzono z nim połączenia
        private int secondsElapsedLastPing;
        private Form1 form;        //Uchwyt na okno

        public WNetInterface(string localAddress, int interfaceNumber, Form1 form)
        {
            this.form = form;
            eth = new NetInterface(localAddress, interfaceNumber, form);
            isOffered = false;
            ZeroPing();    //Dopiero po nawiązaniu połączenia i wysłaniu pierwszego pinga uzupełniamy o pole o wartość większą od zera.
        }

        //Nawiązywanie/przerywanie połączeń i przesyłanie danych oscylacji=========================
        public bool IsConnected() => eth.IsConnected();
        public bool IsAvaiable() => !IsConnected() && !isOffered;

        /// <summary>
        /// Uruchamia zawarty w klasie interfejs sieciowy i zestawia połączenie na podany endpoint
        /// </summary>
        /// <param name="targetEndPoint">Endpoint, z którym zostanie nawiązanie połaczenie</param>
        public void SetConnection(IPEndPoint targetEndPoint)
        {
            isOffered = false;
            ZeroPing();
            eth.SetConnection(targetEndPoint);
        }

        /// <summary>
        /// Wyłącza zawarty w klasie interfejs
        /// </summary>
        public void TerminateConnection()
        {
            isOffered = false;
            ZeroPing();
            eth.TerminateConnection();
        }

        /// <summary>
        /// Wysyła dane o oscylacji do przesłania przez interfejs sieciowy
        /// </summary>
        /// <param name="oscilation_info">Para zmiennych typu double zawierająca informacje o oscylacji</param>
        public void SendOscilations(System.Tuple<double, double> oscilation_info) => eth.sendSyncPacket(oscilation_info);
        //=========================================================================================


        //Metody od sprawdzania pingu==============================================================
        /// <summary>
        /// Zwięsza licznik sekund od ostatniego pinga o 1. Jeżeli czas od ostatniego pinga przekroczy 10 sekund metoda zwraca wartość logiczną false
        /// </summary>
        /// <returns></returns>
        public bool IncrementLastPing() 
        {
            if (++secondsElapsedLastPing >= 10)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Zeruje licznik sekund od ostatniego pingu
        /// </summary>
        public void ZeroPing() => secondsElapsedLastPing = 0;
        //=========================================================================================



        //Przydatne metody=========================================================================
        public IPEndPoint GetLocalEndpoint() => eth.localEndPoint;
        public IPEndPoint GetTargetEndpoint() => eth.targetEndPoint;
        //=========================================================================================
    }
}
