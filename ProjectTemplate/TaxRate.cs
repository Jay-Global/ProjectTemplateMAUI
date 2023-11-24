using System;
using CsvHelper.Configuration;

namespace ProjectTemplate
{
    public class TaxRate
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public decimal Rate { get; set; }
        public decimal Constant { get; set; }
    }

    public sealed class TaxRateMap : ClassMap<TaxRate>
    {
        public TaxRateMap()
        {
            Map(m => m.Min).Index(0);
            Map(m => m.Max).Index(1);
            Map(m => m.Rate).Index(2);
            Map(m => m.Constant).Index(3);
        }
    }

}

