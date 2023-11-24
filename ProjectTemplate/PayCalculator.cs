using System;
using System.Collections.ObjectModel;

namespace ProjectTemplate
{
    public class PayCalculator
    {
        private readonly ObservableCollection<TaxRate> _taxRatesWithThreshold;
        private readonly ObservableCollection<TaxRate> _taxRatesNoThreshold;

        public PayCalculator(ObservableCollection<TaxRate> taxRatesWithThreshold, ObservableCollection<TaxRate> taxRatesNoThreshold)
        {
            _taxRatesWithThreshold = taxRatesWithThreshold;
            _taxRatesNoThreshold = taxRatesNoThreshold;
        }

        public decimal CalculateGrossPay(int hoursWorked, int hourlyRate)
        {
            return hoursWorked * hourlyRate;
        }

        public decimal CalculateTax(decimal grossPay, bool isTaxFreeThreshold)
        {
            var taxRates = isTaxFreeThreshold ? _taxRatesWithThreshold : _taxRatesNoThreshold;
            foreach (var rate in taxRates)
            {
                if (grossPay >= rate.Min && grossPay <= rate.Max)
                {
                    return grossPay * rate.Rate + rate.Constant;
                }
            }
            return 0;
        }

        public decimal CalculateNetPay(decimal grossPay, decimal tax)
        {
            return grossPay - tax;
        }

        public decimal CalculateSuperannuation(decimal grossPay, decimal superRate)
        {
            return grossPay * superRate;
        }
    }

}

