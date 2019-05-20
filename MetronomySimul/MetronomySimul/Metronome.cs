using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
Klasa Metronome jest główną klasą całej symulacji. Zawiera ona szereg pól i metod potrzebnych do jej przeprowadzenia: Wartość wychylennia
i częstotliwość drgań, oraz funkcjonalność służącą do zmiany tych wartości na podstawie otrzymanych danych z innych metronomów
w sieci lokalnej. Główną metodą tej klasy jest wątek który cyklicznie symuluje drgania metronomu dla zadanych wartości i na bieżąco
aktualizuje parametry tych drgań.
*/

namespace MetronomySimul
{
	class Metronome
	{

        //watchdog powinien miec metode ze daje jej info 
        private Watchdog watchdog;
        private double wychylenie, frequency; //wychylenie <-1, 1>, czestotliwosc <10Hz, 100Hz>

        public Metronome()
        {
            Random r = new Random();
            wychylenie = r.NextDouble() * r.Next(-1, 1);
            frequency = r.Next(10, 100);
            watchdog = new Watchdog(4); //Tu zmieniaj ilosc interfejsow (domyslnie 4)
        }
        
        private void PendulumThread()
        {
            if (OscillatorUpdator.oscillation_info.Any())
            {
                Tuple<double, double> rcvd_info;
                rcvd_info = OscillatorUpdator.get_osc_info();
                wychylenie = rcvd_info.Item1;
                frequency = rcvd_info.Item2;
            }
            //tu bedzie proces oscylacji
        } 
	}
}
