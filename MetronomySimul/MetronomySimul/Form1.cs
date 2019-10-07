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
        public const string IP_ADDRESS = "192.168.1.10";
        public const int NUMBER_OF_INTERFACES = 4;
        public const int WATCHDOG_PORT = 8080;

        private Watchdog watchdog;
        private double wychylenie, frequency = 0; //wychylenie <-1, 1>, czestotliwosc (0Hz, 1Hz>
        private int kierunek; //kierunek {-1, 1}
        private Thread thread;
        private string[] connectionsConsole = new string[4];
        private Mutex oscInfoMutex;
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

            oscInfoMutex = new Mutex();
            watchdog = new Watchdog(IP_ADDRESS, WATCHDOG_PORT, NUMBER_OF_INTERFACES, this); //Tu zmieniaj ilosc interfejsow (domyslnie 4)
            progressBar1.Maximum = 1000;
            progressBar2.Maximum = 1000;
            
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
                    Tuple<double, double> infoToGive = new Tuple<double, double>(wychylenie, frequency);
                    OscillatorUpdator.GiveOscInfoDomestic(infoToGive);
                }
                else
                {
                    //obsluga w oknie, domyslnie dwa progress bary - jeden normalny "przyklejony" to drugiego
                    //drugi z ustawionym rightToLeft = true, yes, whtvr
                    oscInfoMutex.WaitOne();
                    if (wychylenie > 0)
                    {
                        if (IsHandleCreated)
                        {
                            Invoke
                            (new Action(() =>
                            {
                                progressBar1.Value = (int)(wychylenie * 1000);
                                textBox1.Text = wychylenie.ToString();
                                freqTextBox.Text = frequency.ToString();
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
                    oscInfoMutex.ReleaseMutex();
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
            } catch (ThreadStateException)
            {
                ;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        
        private void SetCheckBoxAndIp(string ipAddress, int interfaceNumber)
        {
            try
            {
                switch (interfaceNumber)
                {
                    case 1:
                        checkBox1.Invoke(new Action(() => checkBox1.Checked = !checkBox1.Checked));
                        if (checkBox1.Checked) textBoxIP1.Invoke(new Action(() => textBoxIP1.Text = ipAddress));
                        else textBoxIP1.Invoke(new Action(() => textBoxIP1.Text = ""));
                        break;
                    case 2:
                        checkBox2.Invoke(new Action(() => checkBox2.Checked = !checkBox2.Checked));
                        if (checkBox2.Checked) textBoxIP2.Invoke(new Action(() => textBoxIP2.Text = ipAddress));
                        else textBoxIP2.Invoke(new Action(() => textBoxIP2.Text = ""));
                        break;
                    case 3:
                        checkBox3.Invoke(new Action(() => checkBox3.Checked = !checkBox3.Checked));
                        if (checkBox3.Checked) textBoxIP3.Invoke(new Action(() => textBoxIP3.Text = ipAddress));
                        else textBoxIP3.Invoke(new Action(() => textBoxIP3.Text = ""));
                        break;
                    case 4:
                        checkBox4.Invoke(new Action(() => checkBox4.Checked = !checkBox4.Checked));
                        if (checkBox4.Checked) textBoxIP4.Invoke(new Action(() => textBoxIP4.Text = ipAddress));
                        else textBoxIP4.Invoke(new Action(() => textBoxIP4.Text = ""));
                        break;
                }
            } catch(Exception)
            {
                ;
            }
        }

        private void progressBar2_Click(object sender, EventArgs e)
        {

        }

        public void DisplayOnLog(string text)
        {
            try
            {
                Log.Invoke(new Action(() => Log.AppendText("\n" + text)));
            }
            catch (Exception) {; }
                
        }
       
    }
}
