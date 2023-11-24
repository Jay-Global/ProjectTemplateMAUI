using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ProjectTemplate.ViewModel
{

    public class Person : ObservableObject
    {
        private int id;
        public int employeeID
        {
            get => id;
            set
            {
                if (SetProperty(ref id, value))
                    OnPropertyChanged(nameof(employeeID));
            }
        }

        private string firstname;
        public string firstName
        {
            get => firstname;
            set
            {
                if (SetProperty(ref firstname, value))
                    OnPropertyChanged(nameof(firstName));
            }
        }

        private string lastname;
        public string lastName
        {
            get => lastname;
            set
            {
                if (SetProperty(ref lastname, value))
                    OnPropertyChanged(nameof(lastName));
            }
        }

        private string tEmp;
        public string typeEmployee
        {
            get => tEmp;
            set
            {
                if (SetProperty(ref tEmp, value))
                    OnPropertyChanged(nameof(typeEmployee));
            }
        }

        private int hrlyRate;
        public int hourlyRate
        {
            get => hrlyRate;
            set
            {
                if (SetProperty(ref hrlyRate, value))
                    OnPropertyChanged(nameof(hourlyRate)); ;
            }
        }

        private string txThr;
        public string taxthreshold
        {
            get => txThr;
            set
            {
                if (SetProperty(ref txThr, value))
                    OnPropertyChanged(nameof(taxthreshold));
            }
        }
    }
    public sealed class CsvMap : ClassMap<Person>
    {
        public CsvMap()
        {
            Map(m => m.employeeID).Index(0);
            Map(m => m.firstName).Index(1);
            Map(m => m.lastName).Index(2);
            Map(m => m.typeEmployee).Index(3);
            Map(m => m.hourlyRate).Index(4);
            Map(m => m.taxthreshold).Index(5);
        }
    }


    public partial class MainPageViewModel: ObservableObject
    {
        private readonly PayCalculator _payCalculator;
        [ObservableProperty]
        private Person selectedEmployee;


        public MainPageViewModel()
        {
            Employees = new ObservableCollection<Person>();

            // Import tax rates
            ImportTaxRatesWithThreshold();
            ImportTaxRatesNoThreshold();

            // Initialize PayCalculator
            _payCalculator = new PayCalculator(TaxRatesWithThreshold, TaxRatesNoThreshold);
        }

        [ObservableProperty]
        ObservableCollection<Person> employees = new ObservableCollection<Person>();


        [RelayCommand]
        public async void ImportSomeRecords()
        {
            string fileName = "employee.csv";
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(fileName);

            using (var reader = new StreamReader(fileStream))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<CsvMap>();


                    int employeeID;
                    string firstName;
                    string lastName;
                    string typeEmployee;
                    int hourlyRate;
                    string taxthreshold;


                    while (csv.Read())
                    {
                        employeeID = csv.GetField<int>(0);
                        firstName = csv.GetField<string>(1);
                        lastName = csv.GetField<string>(2);
                        typeEmployee = csv.GetField<string>(3);
                        hourlyRate = csv.GetField<int>(4);
                        taxthreshold = csv.GetField<string>(5);
                        employees.Add(CreateRecord(employeeID, firstName, lastName, typeEmployee, hourlyRate, taxthreshold));

                    }

                }

            }
        }

        public static Person CreateRecord(int employeeID, string firstName, string lastName, string typeEmployee, int hourlyRate, string taxthreshold)
        {
            Person record = new Person();

            record.employeeID = employeeID;
            record.firstName = firstName;
            record.lastName = lastName;
            record.typeEmployee = typeEmployee;
            record.hourlyRate = hourlyRate;
            record.taxthreshold = taxthreshold;

            return record;
        }





        public ObservableCollection<TaxRate> TaxRatesWithThreshold { get; private set; } = new ObservableCollection<TaxRate>();
        public ObservableCollection<TaxRate> TaxRatesNoThreshold { get; private set; } = new ObservableCollection<TaxRate>();

        [RelayCommand]
        public async Task ImportTaxRatesWithThreshold()
        {
            await ImportTaxRates("taxrate-withthreshold.csv", TaxRatesWithThreshold);
        }

        [RelayCommand]
        public async Task ImportTaxRatesNoThreshold()
        {
            await ImportTaxRates("taxrate-nothreshold.csv", TaxRatesNoThreshold);
        }

        private async Task ImportTaxRates(string fileName, ObservableCollection<TaxRate> taxRatesCollection)
        {
            using var fileStream = await FileSystem.Current.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<TaxRateMap>();

            while (csv.Read())
            {
                var taxRateRecord = csv.GetRecord<TaxRate>();
                taxRatesCollection.Add(taxRateRecord);
            }
        }

        [ObservableProperty]
        private int hoursWorked;

        [ObservableProperty]
        private decimal grossPay;

        [ObservableProperty]
        private decimal tax;

        [ObservableProperty]
        private decimal netPay;

        [ObservableProperty]
        private decimal superannuation;


        [RelayCommand]
        public void CalculatePay()
        {
            if (SelectedEmployee != null)
            {
                decimal grossPay = _payCalculator.CalculateGrossPay(hoursWorked, SelectedEmployee.hourlyRate);
                bool isTaxFreeThreshold = SelectedEmployee.taxthreshold == "Y";
                decimal tax = _payCalculator.CalculateTax(grossPay, isTaxFreeThreshold);
                decimal netPay = _payCalculator.CalculateNetPay(grossPay, tax);
                decimal superannuation = _payCalculator.CalculateSuperannuation(grossPay, 0.11M); // 11% super rate

                // Update UI-bound properties
                GrossPay = grossPay;
                Tax = tax;
                NetPay = netPay;
                Superannuation = superannuation;
            }
        }


    }
}


