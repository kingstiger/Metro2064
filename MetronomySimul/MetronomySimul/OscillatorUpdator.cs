using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetronomySimul
{
    public static class OscillatorUpdator
    {
        public static Queue<Tuple<double, double>> oscillation_info;
        public static Mutex m;

        /// <summary>
        /// Metoda do pobierania z kolejki informacji o oscylacji otrzymanych od innych metronomów w sieci
        /// </summary>
        /// <returns></returns>
        public static Tuple<double, double> get_osc_info()
        {
            Tuple<double, double> info;
            m.WaitOne();
            info = oscillation_info.Dequeue();
            m.ReleaseMutex();
            return info;
        }

        /// <summary>
        /// Metoda do wstawiania w kolejkę informacji o oscylacji otrzymanych od innych metronomów w sieci
        /// </summary>
        /// <param name="info"></param>
        public static void give_osc_info(Tuple<double, double> info)
        {
            m.WaitOne();
            oscillation_info.Enqueue(info);
            m.ReleaseMutex();
        }
    }
}
