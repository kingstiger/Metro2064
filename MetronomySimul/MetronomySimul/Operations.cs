using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetronomySimul
{
    /// <summary>
    /// Klasa z dostępnymi operacjami
    /// </summary>
    public static class Operations
    {
        /// <summary>
        /// Discover - Nowy metronom pragnie podłączyć się pod inne
        /// </summary>
        public static string DISCOVER = "DIS";
        
        /// <summary>
        /// Działający już metronom oferuje połączenie
        /// </summary>
        public static string OFFER = "OFR";

        /// <summary>
        /// Sprawdzenie czy metronom odpowiada
        /// </summary>
        public static string PING = "PNG";

        /// <summary>
        /// Błąd??? Do ustalenia
        /// </summary>
        public static string ERROR = "ERR";

        /// <summary>
        /// Potwierdzenie
        /// </summary>
        public static string ACK = "ACK";

        /// <summary>
        /// Odrzucenie
        /// </summary>
        public static string NACK = "NCK";

        /// <summary>
        /// Żądanie synchronizacji
        /// </summary>
        public static string SYNC = "SNC";
    }
}
