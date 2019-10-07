using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MetronomySimul
{
    class Metronome
    {
        private double wychylenie, frequency = 0; //wychylenie <-1, 1>, czestotliwosc (0Hz, 1Hz>
        private int kierunek; //kierunek {-1, 1}
        private Thread thread;
        private Mutex oscInfoMutex;
        public Form1 form;

        public Metronome(Form1 form)
        {
            this.form = form;
            Thread.Sleep(2000);
            Random r = new Random();
            wychylenie = r.NextDouble() * r.Next(-1, 1);
            while (frequency == 0)
            {
                frequency = r.NextDouble();
            }
            if (r.Next(0, 1) == 1)
                kierunek = 1;
            else kierunek = -1;

            oscInfoMutex = new Mutex();

            thread = new Thread(PendulumThread);
            thread.Start();
        }
        public Tuple<double, double> GetOscInfoToSend()
        {
            return new Tuple<double, double>(wychylenie, frequency);
        }

        public void ApplyGivenOscInfo(Tuple<double, double> osc_info)
        {
            oscInfoMutex.WaitOne();
            wychylenie = (wychylenie + osc_info.Item1) / 2;
            frequency = (frequency + osc_info.Item2) / 2;
            oscInfoMutex.ReleaseMutex();
        }

        private void PendulumThread()
        {


            //tu będzie oscylacja
            //nie wiem czy to zadziała
            //jak bedzie GUI to sie przekonamy
            while (true)
            {
                if (OscillatorUpdator.oscillation_info_foreign.Count > 0)
                {
                    Tuple<double, double> rcvd_info;
                    rcvd_info = OscillatorUpdator.GetOscInfoForeign();
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
                    form.TrySendOscillationInformation();
                }
                else
                {
                    //obsluga w oknie, domyslnie dwa progress bary - jeden normalny "przyklejony" to drugiego
                    //drugi z ustawionym rightToLeft = true, yes, whtvr
                    oscInfoMutex.WaitOne();
                    if (wychylenie > 0)
                    {
                        if (form.IsHandleCreated)
                        {
                            form.Invoke
                            (new Action(() =>
                            {
                                form.progressBar1.Value = (int)(wychylenie * 1000);
                                form.SetPitch(wychylenie);
                                form.SetFrequency(frequency);
                                form.progressBar2.Value = 0;
                            }));
                        }
                    }
                    if (wychylenie < 0)
                    {
                        if (form.IsHandleCreated)
                        {
                            form.Invoke
                            (new Action(() =>
                            {
                                form.progressBar2.Value = (-1) * (int)(wychylenie * 1000);
                                form.SetPitch(wychylenie);
                                form.SetFrequency(frequency);
                                form.progressBar1.Value = 0;
                            }));
                        }
                    }
                    oscInfoMutex.ReleaseMutex();
                }
            }
        }


    }
}
