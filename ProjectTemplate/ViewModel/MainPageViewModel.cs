﻿using CommunityToolkit.Mvvm.ComponentModel;
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

        private PayCalculator _payCalculator;
        private PayCalculator PayCalculator
        {
            get
            {
                try
                {
                    if (_payCalculator == null && TaxRatesWithThreshold.Count > 0 && TaxRatesNoThreshold.Count > 0)
                    {
                        _payCalculator = new PayCalculator(TaxRatesWithThreshold, TaxRatesNoThreshold);
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during PayCalculator initialization
                    StatusMessage = $"Error initializing PayCalculator: {ex.Message}";
                }
                return _payCalculator;
            }
        }


        public async Task InitializeAsync()
        {
            try
            {
                await ImportTaxRatesWithThreshold();
                await ImportTaxRatesNoThreshold();

                if (TaxRatesWithThreshold.Count == 0 || TaxRatesNoThreshold.Count == 0)
                {
                    StatusMessage = "Failed to load tax rate data.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during initialization
                StatusMessage = $"Initialization failed: {ex.Message}";
            }
        }


        [ObservableProperty]
        private Person selectedEmployee;


        public MainPageViewModel()
        {
            Employees = new ObservableCollection<Person>();
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



        // MY CODE

        public ObservableCollection<TaxRate> TaxRatesWithThreshold { get; private set; } = new ObservableCollection<TaxRate>();
        public ObservableCollection<TaxRate> TaxRatesNoThreshold { get; private set; } = new ObservableCollection<TaxRate>();

        [RelayCommand]
        public async Task ImportTaxRatesWithThreshold()
        {
            await ImportTaxRates("taxratewiththreshold.csv", TaxRatesWithThreshold);
        }

        [RelayCommand]
        public async Task ImportTaxRatesNoThreshold()
        {
            await ImportTaxRates("taxratenothreshold.csv", TaxRatesNoThreshold);
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


        [ObservableProperty]
        private string nameDisplay;

        [ObservableProperty]
        private string roleDisplay;

        [ObservableProperty]
        private string taxThresholdDisplay;

        [ObservableProperty]
        private string hoursWorkedDisplay;

        [ObservableProperty]
        private string hourlyRateDisplay;

        [ObservableProperty]
        private string dateDisplay;

        [ObservableProperty]
        private string timeDisplay;

        [ObservableProperty]
        private string statusMessage;


        [RelayCommand]
        public void CalculatePay()
        {
            // Check if HoursWorked is within the valid range
            if (HoursWorked <= 0 || HoursWorked > 40)
            {
                StatusMessage = "Hours worked must be greater than 0 and no more than 40.";
                return; // Exit early if validation fails
            }

            if (SelectedEmployee != null && PayCalculator != null)
            {
                // Set UI-bound properties for employee details
                NameDisplay = $"{SelectedEmployee.firstName} {SelectedEmployee.lastName}";
                RoleDisplay = SelectedEmployee.typeEmployee;
                TaxThresholdDisplay = SelectedEmployee.taxthreshold == "Y" ? "Yes" : "No";
                HoursWorkedDisplay = HoursWorked.ToString();
                HourlyRateDisplay = SelectedEmployee.hourlyRate.ToString("C");

                // Perform calculations
                decimal grossPay = _payCalculator.CalculateGrossPay(HoursWorked, SelectedEmployee.hourlyRate);
                bool isTaxFreeThreshold = SelectedEmployee.taxthreshold == "Y";
                decimal tax = _payCalculator.CalculateTax(grossPay, isTaxFreeThreshold);
                decimal netPay = _payCalculator.CalculateNetPay(grossPay, tax);
                decimal superannuation = _payCalculator.CalculateSuperannuation(grossPay, 0.11M); // 11% super rate

                // Set UI-bound properties for calculation results
                GrossPay = grossPay;
                Tax = tax;
                NetPay = netPay;
                Superannuation = superannuation;
                DateDisplay = DateTime.Now.ToString("d");
                TimeDisplay = DateTime.Now.ToString("T");

                // Clear the hours worked input
                HoursWorked = 0;
                StatusMessage = "";
            }
            else
            {
                StatusMessage = "Unable to calculate pay. Please try again.";
            }
        }

        [RelayCommand]
        public async Task SaveData()
        {
            if (SelectedEmployee == null)
            {
                StatusMessage = "No employee selected.";
                return;
            }

            string filename = $"Pay-{SelectedEmployee.employeeID}-{SelectedEmployee.firstName}{SelectedEmployee.lastName}-{DateTime.Now:yyyyMMddHHmmss}.csv";
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            try
            {
                using var stream = File.OpenWrite(filePath);
                using var writer = new StreamWriter(stream);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                var records = new List<dynamic>
                {
                    new
                    {
                        EmployeeID = SelectedEmployee.employeeID,
                        HoursWorked,
                        HourlyRate = SelectedEmployee.hourlyRate,
                        TaxThreshold = SelectedEmployee.taxthreshold,
                        GrossPay,
                        Tax,
                        NetPay,
                        Superannuation
                    }
                };

                csv.WriteRecords(records);
                StatusMessage = "Data saved successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save data: {ex.Message}";
            }
        }

    }
}


