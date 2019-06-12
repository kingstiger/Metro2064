﻿using System;
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
            thread.Start();
            new Thread(ConnectionsThread).Start();           
            InitializeComponent();
        }

        private void ConnectionsThread()
        {
            while(true)
            {
                bool _modified = true;
                int number_of_interfaces_connected = 0;
                if(_modified)
                {
                    number_of_interfaces_connected = 0;
                    Invoke
                            (new Action(() =>
                            {
                                activeConnections.Text += "";
                            }));
                    foreach (NetPacket x in watchdog.connectedInterfaces)
                    {
                        connectionsConsole[watchdog.connectedInterfaces.IndexOf(x)] = $"IP Adrress: {x.receiver_IP}; Port: {x.receiver_port}; Time since last PING: {watchdog.seconds_elapsed_since_last_pings}";
                        number_of_interfaces_connected++;
                    }
                    for (int i = 0; i < connectionsConsole.Length; i++)
                    {
                        if (connectionsConsole[i] != "")

                            Invoke
                            (new Action(() =>
                            {
                                activeConnections.Text += connectionsConsole[i];
                            }));
                        else
                            Invoke
                            (new Action(() =>
                            {
                                activeConnections.Text += "Niepołączony";
                            }));

                    }
                    _modified = false;
                }
                if (watchdog.connectedInterfaces.Count != number_of_interfaces_connected)
                    _modified = true;
            }
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
                    if (OscillatorUpdator.oscillation_info_foreign.Any())
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
                            Invoke
                            (new Action(() =>
                            {
                                progressBar1.Value = (int)(wychylenie * 1000);
                                textBox1.Text = wychylenie.ToString();
                            }));
                        }
                        if (wychylenie < 0)
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
        private void Form1_Load(object sender, EventArgs e)
        {

        }

       
    }
}
