using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

/*
Klasa NetPacket to schemat komunikatu przesyłanego przez instancje klasy NetInterface przez protokół UDP. Klasa ta nie zawiera metod
służących do komunikacji przez sieć lokalną, a jedynie zbiór metod pozwalających złożyć komunikat z danych cząstkowych oraz odczytać
poszczególne dane komunikatu
*/

namespace MetronomySimul
{
	class NetPacket
	{
        IPAddress sender_IP;
        IPAddress receiver_IP;
        int sender_port;
        int receiver_port;
        int seq_number;
        string operation; //na pewno string?
        string data; //na pewno string?

        
        public string toOneStr()
        {
            string s = sender_IP.ToString().Select(n => n).Where(a => a != '.').ToString() + '<'
                + receiver_IP.ToString().Select(n => n).Where(a => a != '.').ToString() + '<'
                + sender_port.ToString() + '<' + receiver_port.ToString() + '<'
                + seq_number.ToString() + '<' + operation + '<' + data;
            return s;
        }

        public byte[] toByte()
        {
            string s = this.toOneStr();
            byte[] prep = Encoding.Unicode.GetBytes(s);
            byte[] p = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, prep);
            return p;
        }

        string ByteToStr(byte[] bytes)
        {
            string s = Encoding.ASCII.GetString(bytes);
            return s;
        }

        void ReadReceivedMsg(byte[] received_msg)
        {
            string msg = ByteToStr(received_msg);
            int i;

            //read IPs and seq number
            {
                string sendIP = "", recIP = "", sqnumber = "";
                for (i = 0; msg[i] != '<'; i++)
                {
                    sendIP += msg[i];
                    recIP += msg[i + 6];
                    sqnumber += msg[i + 12];
                }

                sender_IP = IPAddress.Parse(sendIP);
                receiver_IP = IPAddress.Parse(recIP);
                seq_number = Int32.Parse(sqnumber);
            }

            //read Ports
            {
                string sendPort = "", recPort = "";
                for (i = 4; msg[i] != '<'; i++)
                {
                    sendPort += msg[i];
                    recPort += msg[i + 6];
                }
                sender_port = Int32.Parse(sendPort);
                receiver_port = Int32.Parse(recPort);
            }

            //read Operation
            
            for (i = 16; msg[i] != '<'; i++)
            {
                operation += msg[i];
            }

            //read data (if any)

            if(received_msg.Count() > 19)
            {
                for (i = 19; i < msg.Count(); i++)
                {
                    data += msg[i];
                }
            }
        }

        byte[] WriteMsgToSend()
        {
            int size = 19 + (data.Count()*2);
            byte[] packet = new byte[;


            return packet;
        }
	}
}
