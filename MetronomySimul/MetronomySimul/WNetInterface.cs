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
        public IPEndPoint offeredTo;
        private int secondsElapsedLastPing;
        private int offerTimeLeft;
        private Form1 form;        //Uchwyt na okno
        private int triedPings;
        private Mutex pingMutex;

        public WNetInterface(string localAddress, int localPort, int interfaceNumber, Form1 form)
        {
            this.form = form;
            eth = new NetInterface(localAddress, localPort, interfaceNumber, form);
            pingMutex = new Mutex();
            isOffered = false;
            offeredTo = null;
            ZeroOfferTime();
            ZeroPing();
        }

        //Nawiązywanie/przerywanie połączeń i przesyłanie danych oscylacji=========================
        public bool IsConnected() => eth.IsConnected();
        public bool IsAvaiable() => !IsConnected() && !isOffered;

        /// <summary>
        /// Uruchamia zawarty w klasie interfejs sieciowy i zestawia połączenie na podany endpoint
        /// </summary>
        /// <param name="targetEndPoint">Endpoint, z którym zostanie nawiązanie połaczenie</param>
        public bool SetConnection(IPEndPoint targetEndPoint)
        {
            ZeroPing();
            ResetPingCount();
            eth.SetConnection(targetEndPoint);

            isOffered = false;
            return true;
        }

        /// <summary>
        /// Wyłącza zawarty w klasie interfejs
        /// </summary>
        public bool TerminateConnection()
        {
            ResetPingCount();
            ZeroPing();
            eth.TerminateConnection();

            isOffered = false;
            offeredTo = null;
            return true;
        }

        /// <summary>
        /// Wysyła dane o oscylacji do przesłania przez interfejs sieciowy
        /// </summary>
        /// <param name="oscilation_info">Para zmiennych typu double zawierająca informacje o oscylacji</param>
        public void SendOscilations(System.Tuple<double, double> oscilation_info) => eth.sendSyncPacket(oscilation_info);
        //=========================================================================================


        //Metody od sprawdzania pingu i oferowania interfejsu======================================
        /// <summary>
        /// Zwięsza licznik sekund od ostatniego pinga o 1. Jeżeli czas od ostatniego pinga przekroczy 10 sekund metoda zwraca wartość logiczną false
        /// </summary>
        /// <returns></returns>
        public bool IncrementLastPing()
        {
            pingMutex.WaitOne();
            bool result = (++secondsElapsedLastPing >= 10) ? true : false;
            pingMutex.ReleaseMutex();
            return result;
        }

        /// <summary>
        /// Zmniejsza licznik pozostałego czasu oferty
        /// </summary>
        /// <returns></returns>
        public bool DecrementOfferTime() => (--offerTimeLeft <= 0) ? true : false;

        /// <summary>
        /// Stwierdza, czy trzeba kończyć połączenie (po 3 pingach bez odpowiedzi)
        /// </summary>
        /// <returns></returns>
        public bool DoTerminate()
        {
            pingMutex.WaitOne();
            bool result = (++triedPings >= 3) ? true : false;
            pingMutex.ReleaseMutex();
            return result;
        }

        /// <summary>
        /// Resetuje licznik wysłanych pingów
        /// </summary>
        public void ResetPingCount()
        {
            pingMutex.WaitOne();
            triedPings = 0;
            pingMutex.ReleaseMutex();
        }

        /// <summary>
        /// Zeruje licznik sekund od ostatniego pingu
        /// </summary>
        public void ZeroPing()
        {
            pingMutex.WaitOne();
            secondsElapsedLastPing = 0;
            pingMutex.ReleaseMutex();
        }


        /// <summary>
        /// Ustawia timeout ofertowania interfejsu po jej wysłaniu
        /// </summary>
        public void SetOfferTimeout() => offerTimeLeft = 5;

        /// <summary>
        /// Zeruje licznik czasu ważności oferty
        /// </summary>
        public void ZeroOfferTime() => offerTimeLeft = 0;
        //=========================================================================================



        //Przydatne metody=========================================================================
        public IPEndPoint GetLocalEndpoint() => eth.localEndPoint;
        public IPEndPoint GetTargetEndpoint() => eth.targetEndPoint;
        //=========================================================================================
    }
}
