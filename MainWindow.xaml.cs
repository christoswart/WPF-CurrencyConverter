using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataTable dtCurrency = new DataTable();
        private const string API_KEY = "a143b7429c1be823bf5b5a14"; // Sign up at https://www.exchangerate-api.com
        private const string BASE_URL = "https://v6.exchangerate-api.com/v6/";
        // Define base currencies we want to display
        private List<string> currencies = new List<string> { "USD", "EUR", "GBP", "AUD", "ZAR" };

        public MainWindow()
        {
            InitializeComponent();
            lblCurrency.Content = "Select a currency";
            ClearControls();
            LoadCurrencyRates();
        }

        private async void LoadCurrencyRates()
        {
            await this.BindCurrencyAsync();
        }

        public async Task BindCurrencyAsync()
        {
            try
            {
                // Initialize DataTable structure
                dtCurrency.Columns.Add("Text");
                dtCurrency.Columns.Add("Value");
                dtCurrency.Rows.Add("--SELECT--", 0);

                // Fetch live rates
                using (var client = new HttpClient())
                {
                    // Get rates with USD as base currency
                    var response = await client.GetStringAsync($"{BASE_URL}{API_KEY}/latest/USD");
                    var ratesData = JsonSerializer.Deserialize<ExchangeRateResponse>(response);

                    if (ratesData?.conversion_rates != null)
                    {
                        foreach (var currency in currencies)
                        {
                            if (ratesData.conversion_rates.TryGetValue(currency, out double rate))
                            {
                                dtCurrency.Rows.Add(currency, rate);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to default values if API call fails
                Console.WriteLine($"Error fetching exchange rates: {ex.Message}");
                AddDefaultRates();
            }

            // Bind to ComboBoxes
            BindComboBoxes();
        }

        private void AddDefaultRates()
        {
            // Fallback rates in case API is unavailable
            dtCurrency.Rows.Add("USD", 1);
            dtCurrency.Rows.Add("EUR", 0.85);
            dtCurrency.Rows.Add("GBP", 0.73);
            dtCurrency.Rows.Add("AUD", 75);
            dtCurrency.Rows.Add("ZAR", 3.75);
        }

        private void BindComboBoxes()
        {
            // Bind From Currency ComboBox
            cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

            // Bind To Currency ComboBox
            cmbToCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }

        // Class to deserialize API response
        private class ExchangeRateResponse
        {
            public Dictionary<string, double> conversion_rates { get; set; }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            //Create a variable as ConvertedValue with double data type to store currency converted value
            double ConvertedValue;

            //Check amount textbox is Null or Blank
            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                //If amount textbox is Null or Blank it will show the below message box   
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //After clicking on message box OK sets the Focus on amount textbox
                txtCurrency.Focus();
                return;
            }
            //Else if the currency from is not selected or it is default text --SELECT--
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                //It will show the message
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //Set focus on From Combobox
                cmbFromCurrency.Focus();
                return;
            }
            //Else if Currency To is not Selected or Select Default Text --SELECT--
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                //It will show the message
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //Set focus on To Combobox
                cmbToCurrency.Focus();
                return;
            }

            //Check if From and To Combobox selected values are same
            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                //Amount textbox value set in ConvertedValue.
                //double.parse is used for converting the datatype String To Double.
                //Textbox text have string and ConvertedValue is double Datatype
                ConvertedValue = double.Parse(txtCurrency.Text);

                //Show the label converted currency and converted currency name and ToString("N3") is used to place 000 after the dot(.)
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
            else
            {
                //Calculation for currency converter is From Currency value multiply(*) 
                //With the amount textbox value and then that total divided(/) with To Currency value
                ConvertedValue = ConversionHelper.Convert(double.Parse(txtCurrency.Text), double.Parse(cmbFromCurrency.SelectedValue.ToString()), (double.Parse(cmbToCurrency.SelectedValue.ToString())));
                
                //Show the label converted currency and converted currency name.
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
        }

        //Allow only the integer value in TextBox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regular Expression to add regex add library using System.Text.RegularExpressions;
            Regex regex = new Regex("^[0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        //ClearControls used for clear all controls value
        private void ClearControls()
        {
            txtCurrency.Text = "0";
            if (cmbFromCurrency.Items.Count > 0)
                cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0)
                cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }
    }
}