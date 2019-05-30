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
        public static Queue<Tuple<double, double>> oscillation_info_foreign; //kolejka z informacjami od innych metronomów w sieci
        public static Queue<Tuple<double, double>> oscillation_info_domestic; //kolejka z informacjami dla innych metronomów w sieci
        public static Mutex m;

        /// <summary>
        /// Metoda do pobierania z kolejki informacji o oscylacji otrzymanych od innych metronomów w sieci
        /// </summary>
        /// <returns></returns>
        public static Tuple<double, double> GetOscInfoForeign()
        {
            Tuple<double, double> info;
            m.WaitOne();
            info = oscillation_info_foreign.Dequeue();
            m.ReleaseMutex();
            return info;
        }

        /// <summary>
        /// Metoda do wstawiania w kolejkę informacji o oscylacji otrzymanych od innych metronomów w sieci
        /// </summary>
        /// <param name="info"></param>
        public static void GiveOscInfoForeign(Tuple<double, double> info)
        {
            m.WaitOne();
            oscillation_info_foreign.Enqueue(info);
            m.ReleaseMutex();
        }

        /// <summary>
        /// Metoda do otrzymywania z kolejki informacji o oscylacji "domowego" metronomu
        /// </summary>
        /// <returns></returns>
        public static Tuple<double, double> GetOscInfoDomestic()
        {
            Tuple<double, double> info;
            m.WaitOne();
            info = oscillation_info_foreign.Dequeue();
            m.ReleaseMutex();
            return info;
        }
        /// <summary>
        /// Metoda do wstawiania w kolejkę informacji o oscylacji "domowego" metronomu
        /// </summary>
        /// <param name="info"></param>
        public static void GiveOscInfoDomestic(Tuple<double, double> info)
        {
            m.WaitOne();
            oscillation_info_foreign.Enqueue(info);
            m.ReleaseMutex();
        }
    }
}
