using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zadatak.Model;

namespace Zadatak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataTable table = new DataTable();
        HttpWebRequest myHttpWebRequest;
        public MainWindow()
        {
            InitializeComponent();
            GifLoader.Visibility = Visibility.Hidden;
            dgPrikazPodataka.DataContext = table;
        }

        private async void btnPotvrdi_Click(object sender, RoutedEventArgs e)
        {

            if (!String.IsNullOrEmpty(txtUnosTexta.Text.Trim()))
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    GifLoader.Visibility = Visibility.Visible;
                }));

                const string quote = "\"";
                string parametar = quote + txtUnosTexta.Text.Trim() + quote;
                string url = @"https://webservice.gvsi.com/query/json/GetDaily/tradedatetimegmt/open/high/low/close/volume?pricesymbol=" + parametar + "&daysBack=100";
                var odgovor = await GetRESTData(url, "desktopapp", "ThePa55word");

                dgPrikazPodataka.ItemsSource = odgovor;
                GifLoader.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("Molimo popunite polje predvidjeno za unos.");
            }
        }

        private void btnOdustani_Click(object sender, RoutedEventArgs e)
        {
            myHttpWebRequest.Abort();
        }
        private async Task<List<PoljaSaApija>> GetRESTData(string url, string username, string password)
        {
            try
            {
                Uri myUri = new Uri(url);
                WebRequest myWebRequest = HttpWebRequest.Create(myUri);
                myHttpWebRequest = (HttpWebRequest)myWebRequest;
                myHttpWebRequest.Method = WebRequestMethods.Http.Get;
                myHttpWebRequest.Accept = "application/json";

                NetworkCredential myNetworkCredential = new NetworkCredential(username, password);
                CredentialCache myCredentialCache = new CredentialCache();
                myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

                myHttpWebRequest.PreAuthenticate = true;
                myHttpWebRequest.Credentials = myCredentialCache;
                try
                {
                    using (var response = await myHttpWebRequest.GetResponseAsync()
                                               .ConfigureAwait(false))
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string content = reader.ReadToEnd();
                            var dataResult = JsonConvert.DeserializeObject<DataResult>(content);
                            return CalcualteMovingAvarage(dataResult.Results.Items);
                        }
                    }
                }
                catch (WebException e)
                {
                    if(e.Message!= "The request was aborted: The request was canceled.")
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            MessageBox.Show(e.Message);
                        }));
                    }
                    return new List<PoljaSaApija>();
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    MessageBox.Show(ex.Message);
                }));                
                return new List<PoljaSaApija>();
            }
        }

        private List<PoljaSaApija> CalcualteMovingAvarage(List<PoljaSaApija> items)
        {
            int n = 10;
            foreach (var item in items)
            {
                int brojValidnihDatuma = items.Where(x => x.Tradedatetimegmt <= item.Tradedatetimegmt).Count();

                double sumaValidnihDatuma = items
                    .Where(x => x.Tradedatetimegmt <= item.Tradedatetimegmt)
                    .Sum(x => x.Close);

                if (brojValidnihDatuma < 10)
                {
                    item.MovingAverage = Math.Round(sumaValidnihDatuma / brojValidnihDatuma ,2, MidpointRounding.ToEven);
                }
                else
                {
                    var vrednost = items
                        .Where(x => x.Tradedatetimegmt <= item.Tradedatetimegmt)
                        .OrderByDescending(x => x.Tradedatetimegmt)
                        .Take(n)
                        .Sum(x => x.Close) / n;

                    item.MovingAverage = Math.Round(vrednost, 2, MidpointRounding.ToEven);
                }
            }

            return items;
        }

        private void dgPrikazPodataka_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Tradedatetimegmt")
            {
                DataGridTextColumn column = e.Column as DataGridTextColumn;
                Binding binding = column.Binding as Binding;
                binding.StringFormat = "dd.MM.yyyy hh:mm:ss";
            }
        }
    }    
}
