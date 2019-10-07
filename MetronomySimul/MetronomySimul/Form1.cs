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
        private const string IP_ADDRESS = "192.168.1.9";
        private const int NUMBER_OF_INTERFACES = 4;
        private const int WATCHDOG_PORT = 8080;

        public int GetWatchdogPort() => WATCHDOG_PORT;

        private Watchdog watchdog;
        private Metronome metronome;
        
        private string[] connectionsConsole = new string[4];
        public Form1()
        {
            InitializeComponent();
            
            watchdog = new Watchdog(IP_ADDRESS, WATCHDOG_PORT, NUMBER_OF_INTERFACES, this); //Tu zmieniaj ilosc interfejsow (domyslnie 4)
            progressBar1.Maximum = 1000;
            progressBar2.Maximum = 1000;
            metronome = new Metronome(this);
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public Tuple<double, double> GetOscInfoToSend()
        {
            return metronome.GetOscInfoToSend();
        }

        public void ApplyGivenOscInfo(Tuple<double, double> osc_info)
        {
            metronome.ApplyGivenOscInfo(osc_info);
        }

        public void SetCheckBoxAndIp(string ipAddress, int interfaceNumber)
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

        public void SetPitch(double wychylenie)
        {
            textBox1.Text = wychylenie.ToString();
        }

        public void SetFrequency(double frequency)
        {
            freqTextBox.Text = frequency.ToString();
        }

        private void CLOSEAPP_Click(object sender, EventArgs e)
        {
            try
            {
                watchdog.StopThreads();
                Application.Exit();
                Environment.Exit(0);
            }
            catch (ThreadAbortException)
            {
                ;
            }
            catch (ThreadStateException)
            {
                ;
            }
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
