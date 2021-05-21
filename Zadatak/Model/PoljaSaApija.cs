using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadatak.Model
{
    public class DataResult
    {
        public Data Results { get; set; }
    }

    public class Data
    {
        public List<PoljaSaApija> Items { get; set; }
    }

    public class PoljaSaApija
    {
        public DateTime Tradedatetimegmt { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public int Volume { get; set; }
        public double MovingAverage { get; set; }
    }
}
