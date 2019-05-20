using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


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
        private double wychylenie, frequency = 0; //wychylenie <-1, 1>, czestotliwosc (0Hz, 1Hz>
        private int kierunek; //kierunek {-1, 1}
        private Thread thread;
        public Metronome()
        {
            Random r = new Random();
            wychylenie = r.NextDouble() * r.Next(-1, 1);
            while (frequency == 0)
            {
                frequency = r.NextDouble();
            }
            if (r.Next(0, 1) == 1)
                kierunek = 1;
            else kierunek = -1;
            watchdog = new Watchdog(4); //Tu zmieniaj ilosc interfejsow (domyslnie 4)
            thread = new Thread(PendulumThread);
        }
        
        private void PendulumThread()
        {
            while (true)
            {
                
                //tu będzie oscylacja
                //nie wiem czy to zadziała
                //jak bedzie GUI to sie przekonamy
                while (true)
                {
                    if (OscillatorUpdator.oscillation_info.Any())
                    {
                        Tuple<double, double> rcvd_info;
                        rcvd_info = OscillatorUpdator.get_osc_info();
                        wychylenie = (wychylenie + rcvd_info.Item1) / 2;
                        frequency = (frequency + rcvd_info.Item2) / 2;
                    }
                    Thread.Sleep((int)(1000 / (frequency * 1000)));
                    wychylenie += (0.001 * kierunek);
                    if (wychylenie > 1 || wychylenie < -1)
                    {
                        kierunek *= -1;
                        if (wychylenie > 1)
                            wychylenie = 1;
                        else wychylenie = -1;
                    }
                    else
                    { 
                        //obsluga w oknie, domyslnie dwa progress bary - jeden normalny "przyklejony" to drugiego
                        //drugi z ustawionym rightToLeft = true, yes, whtvr
                        /*
                        if (wychylenie > 0)
                        {
                            Invoke
                            (new Action(() =>
                            {
                                Form1.progressBar1.Value = (int)(wychylenie * 1000);
                            }));
                        }
                        if (wychylenie < 0)
                        {
                            Invoke(new Action(() =>
                            {
                                progressBar2.Value = (-1) * (int)(wychylenie * 1000);
                            }));

                        }
                        */
                    }

                }
            }
        } 
	}
}
