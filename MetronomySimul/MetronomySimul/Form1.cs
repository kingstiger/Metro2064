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

namespace MetronomySimul
{
    public partial class Form1 : Form
    {
        //watchdog powinien miec metode ze daje jej info 

        private Watchdog watchdog;
        private double wychylenie, frequency = 0; //wychylenie <-1, 1>, czestotliwosc (0Hz, 1Hz>
        private int kierunek; //kierunek {-1, 1}
        private Thread thread;
        private string[] connectionsConsole = new string[4];
        public Form1()
        {
            InitializeComponent();
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

            watchdog = new Watchdog(4, this); //Tu zmieniaj ilosc interfejsow (domyslnie 4)
            progressBar1.Maximum = 1000;
            progressBar2.Maximum = 1000;
            
            thread = new Thread(PendulumThread);
            thread.Start();
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
                        OscillatorUpdator.GiveOscInfoDomestic(new Tuple<double, double>(wychylenie, frequency));
                    }
                    else
                    {
                        //obsluga w oknie, domyslnie dwa progress bary - jeden normalny "przyklejony" to drugiego
                        //drugi z ustawionym rightToLeft = true, yes, whtvr

                        if (wychylenie > 0)
                        {
                            if (IsHandleCreated)
                            {
                                Invoke
                                (new Action(() =>
                                {
                                    progressBar1.Value = (int)(wychylenie * 1000);
                                    textBox1.Text = wychylenie.ToString();
                                }));
                            }
                        }
                        if (wychylenie < 0)
                        {
                            if (IsHandleCreated)
                            {
                                Invoke
                                (new Action(() =>
                                {
                                    progressBar2.Value = (-1) * (int)(wychylenie * 1000);
                                    textBox1.Text = wychylenie.ToString();
                                }));
                            }
                        }

                    }

                }
            }
        }

        private void CLOSEAPP_Click(object sender, EventArgs e)
        {
            try
            {
                thread.Abort();
                watchdog.StopThreads();
                Application.Exit();
                Environment.Exit(0);
            } catch (ThreadAbortException)
            {
                ;
            }catch (ThreadStateException)
            {
                ;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void DisplayOnLog(string text)
        {
            try
            {
                Log.Invoke(new Action(() => Log.AppendText("\n" + text)));
            }
            catch (Exception e) {; }
                
        }
       
    }
}
