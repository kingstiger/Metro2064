using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;

/*
Klasa NetPacket to schemat komunikatu przesyłanego przez instancje klasy NetInterface przez protokół UDP. Klasa ta nie zawiera metod
służących do komunikacji przez sieć lokalną, a jedynie zbiór metod pozwalających złożyć komunikat z danych cząstkowych oraz odczytać
poszczególne dane komunikatu
*/

namespace MetronomySimul
{

    class NetPacket
	{
        public IPAddress sender_IP { get; set; }
        public IPAddress receiver_IP { get; set; }
        public int sender_port { get; set; }
        public int receiver_port { get; set; }
        public int seq_number { get; set; }
        public string operation { get; set; }
        public string data { get; set; }

        /// <summary>
        /// Dla budowania całego pakietu od zera
        /// </summary>
        /// <param name="senderIP"></param>
        /// <param name="receiverIP"></param>
        /// <param name="senderPort"></param>
        /// <param name="receiverPort"></param>
        /// <param name="sqnumber"></param>
        /// <param name="_operation"></param>
        /// <param name="_data"></param>
        public NetPacket(IPAddress senderIP, IPAddress receiverIP, int senderPort, int receiverPort, int sqnumber, string _operation, string _data)
        {
            sender_IP = senderIP;
            receiver_IP = receiverIP;
            sender_port = senderPort;
            receiver_port = receiverPort;
            seq_number = sqnumber;
            operation = _operation;
            data = _data;
        }

        /// <summary>
        /// Dla odpowiedzi na pakiet z komunikatem DISCOVER
        /// </summary>
        /// <param name="R"></param>
        /// <param name="sender_ip">Adres lokalnego endpointa</param>
        /// <param name="data">Tutaj idzie numer interfejsu, który jest oferowany</param>
        public NetPacket(NetPacket R, IPAddress local_ip, string data)
        {
            sender_IP = local_ip;
            sender_port = R.receiver_port;
            receiver_IP = R.sender_IP;
            receiver_port = R.sender_port;
            seq_number = R.seq_number;
            this.operation = Operations.OFFER;
            this.data = data;
        }

        /// <summary>
        /// Dla odpowiedzi w tej samej fazie - numer sekwencyjny pozostaje taki sam
        /// </summary>
        /// <param name="R"></param>
        /// <param name="operation"></param>
        public NetPacket(NetPacket R, string operation)  
        {
            sender_IP = R.receiver_IP;
            sender_port = R.receiver_port;
            receiver_IP = R.sender_IP;
            receiver_port = R.sender_port;
            seq_number = R.seq_number;
            this.operation = operation;
            //TODO: data = nic?
        }



        /// <summary>
        /// Z pakietu "wyciąga" informacje o oscylacji
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public Tuple<double, double> ReadOscInfoFromData(string data)
        {
            string wychylenie = "", czestotliwosc = "";
            Tuple<double, double> Osc;
            for (int i = 0; data[i] != ';'; i++)
                wychylenie += data[i];
            for (int i = 0; data[i] != ';'; i++)
                czestotliwosc += data[i];
            Osc = new Tuple<double, double>(double.Parse(wychylenie), double.Parse(czestotliwosc));
            return Osc;
        }

        /// <summary>
        /// Pusty pakiet
        /// </summary>
        public NetPacket()
        {
            sender_IP = null;
            sender_port = 0;
            receiver_port = 0;
            receiver_IP = null;
            seq_number = 0;
            operation = "";
            data = "";
        }


        /// <summary>
        /// Dla odpowiedzi z jakimis danymi (nr sekwencyjny pozostaje taki sam)
        /// </summary>
        /// <param name="R"></param>
        /// <param name="operation"></param>
        /// <param name="data"></param>
        public NetPacket(NetPacket R, string operation, string data)
        {
            sender_IP = R.receiver_IP;
            sender_port = R.receiver_port;
            receiver_IP = R.sender_IP;
            receiver_port = R.sender_port;
            seq_number = R.seq_number;
            this.operation = operation;
            this.data = data;
        }

        /// <summary>
        /// Dla odpowiedzi z jakimis danymi
        /// </summary>
        /// <param name="R"></param>
        /// <param name="operation"></param>
        /// <param name="data"></param>
        /// <param name="sq_number"> 0 -> zeruje, inne liczby -> powieksza o daną wartość </param>
        public NetPacket(NetPacket R, string operation, string data, int sq_number)
        {
            sender_IP = R.receiver_IP;
            sender_port = R.receiver_port;
            receiver_IP = R.sender_IP;
            receiver_port = R.sender_port;
            this.operation = operation;
            this.data = data;
            if (sq_number == 0) this.seq_number = 0;
            else this.seq_number = R.seq_number + sq_number;
        }

        /// <summary>
        /// Dla odpowiedzi z nowym numerem sekwencyjnym
        /// </summary>
        /// <param name="R"></param>
        /// <param name="operation"></param>
        /// <param name="sq_number"> 0 -> zeruje, inne liczby -> powieksza o daną wartość </param>
        public NetPacket(NetPacket R, string operation, int sq_number)
        {
            sender_IP = R.receiver_IP;
            sender_port = R.receiver_port;
            receiver_IP = R.sender_IP;
            receiver_port = R.sender_port;
            if (sq_number == 0) this.seq_number = 0;
            else this.seq_number = R.seq_number + sq_number;
            this.operation = operation;
            //TODO: data = nic?
        }


        /// <summary>
        /// Odwraca odbiorcę z nadawcą, resztę parametrów zeruje
        /// </summary>
        /// <param name="R"></param>
        public void ReverseMsgDir(NetPacket R)
        {
            sender_IP = R.receiver_IP;
            sender_port = R.receiver_port;
            receiver_IP = R.sender_IP;
            receiver_port = R.sender_port;
        }

        //Konwertuje zmienną typu IPAddress na jej reprezentację w 4 znakach (string)
        private string IptoStr(IPAddress ip)
        {
            string s = sender_IP.ToString();
            string wyn = "";
            string temp = "";
            for (int i = 0; i < s.Count(); i++)
            {
                if (s[i] != '.' && i == s.Count() - 1)
                {
                    temp += s[i];
                } else
                {
                    wyn += Convert.ToChar(Int32.Parse(temp));
                    temp = "";
                }
            }
            return wyn;

        }


        //Konwertuje wszystkie dane pakietu na jednen ciąg znaków
        private string ToOneStr()
        {
            string s = IptoStr(sender_IP) + '<' + IptoStr(receiver_IP) + '<'
                + sender_port.ToString() + '<' + receiver_port.ToString() + '<'
                + seq_number.ToString() + '<' + operation + '<' + data;
            return s;
        }


        //Konwertuje 4 znaki odpowiadające 32-bitowemu adresowi IP na adres IP
        private IPAddress StringtoIP(string address_short)
        {
            string address_long = "";
            foreach(char x in address_short)
            {
                address_long += Convert.ToInt32(x).ToString();
            }
            return IPAddress.Parse(address_long);
        }


        //Konwertuje string zawierajacy wszystkie dane pakietu na tablicę bajtów (zmieniając przy okazji kodowanie na ASCII)
        private byte[] ToByte()
        {
            string s = this.ToOneStr();
            byte[] prep = Encoding.Unicode.GetBytes(s);
            byte[] p = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, prep);
            return p;
        }


        //Konwertuje tablicę bajtów na ciąg znaków ASCII
        private string ByteToStr(byte[] bytes)
        {
            string s = Encoding.ASCII.GetString(bytes);
            return s;
        }

        
        public void ReadReceivedMsg(byte[] received_msg)
        {
            string msg = ByteToStr(received_msg);
            int i;

            //read IPs, ports, eq number
            {
                string sendIP = "", recIP = "", sqnumber = "", sendPort = "", recPort = "";
                for (i = 0; msg[i] != '<'; i++)
                {
                    sendIP += msg[i];
                    sendPort += msg[i + 5];
                    recIP += msg[i + 10];
                    recPort += msg[i + 15];
                    sqnumber += msg[i + 20];
                }
                sender_IP = StringtoIP(sendIP);
                receiver_IP = StringtoIP(recIP);
                sender_port = Int32.Parse(sendPort);
                receiver_port = Int32.Parse(recPort);
                seq_number = Int32.Parse(sqnumber);
            }
            
            //read Operation
            
            for (i = 25; msg[i] != '<'; i++)
            {
                operation += msg[i];
            }

            //read data (if any)

            if(received_msg.Count() > 19)
            {
                for (i++; i < msg.Count(); i++)
                {
                    data += msg[i];
                }
            }
        }

        static public byte[] TranslateMsgToSend(NetPacket p)
        {
            byte[] packet = p.ToByte();
            return packet;
        }
	}
}
