using CsvHelper;
using ProjectTemplate.ViewModel;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ProjectTemplate;

public partial class MainPage : ContentPage
{

    public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;

        viewModel.ImportSomeRecords();
        
    }

    private void PersonDataGrid_ItemSelected(object sender, SelectionChangedEventArgs e)
    {
        var selection = PersonDataGrid.SelectedItem as Person;
        if (selection != null)
        {
            (BindingContext as MainPageViewModel).SelectedEmployee = selection;
            DisplayAlert("Selected Employee", $" {selection.firstName} {selection.lastName} \n Is claiming a tax free threshold \n {selection.taxthreshold}", "OK");
        }
    }

    //BUTTON EVENT
    //var selection = PersonDataGrid.SelectedItem as Person;
    //PAYSLIP.id = PERSON.id DATA
}

