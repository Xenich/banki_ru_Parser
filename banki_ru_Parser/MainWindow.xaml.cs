using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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


namespace banki_ru_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
           // delegBankParsed deleg;      // экземпляр делегата
            int totalItems;
            int itemsPerPage;
            int countOfPages;
            int start;
            int end;
            string site = "http://www.banki.ru/products/creditcards/search/";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);            
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string response = reader.ReadToEnd();

            start = response.IndexOf("totalItems", 0);
            end = response.IndexOf(";", start);
            string str = response.Substring(start+12, end- start - 12);
            totalItems= Int32.Parse(str);

            start = response.IndexOf("itemsPerPage", 0);
            end = response.IndexOf(";", start);
            str = response.Substring(start + 14, end - start - 14);
            itemsPerPage = Int32.Parse(str);

            countOfPages = Convert.ToInt32(Math.Ceiling(((double)totalItems / itemsPerPage))) ;

            Parser parser = new Parser();
            parser.BankParsed += Parser_BankParsed;
            parser.ParseEnd += Parser_ParseEnd;
            Thread thread = new Thread(new ParameterizedThreadStart(parser.Parse));
            thread.Start(countOfPages);
        }



        private void Parser_BankParsed(string bankName)
        {
            Dispatcher.Invoke(new Action(
                                            () =>
                                            {
                                                textBox.AppendText(bankName + Environment.NewLine);
                                            }
                                        )
                             );
        }
        private void Parser_ParseEnd(List<Bank> banks)
        {
            Dispatcher.Invoke(new Action(
                                            () =>
                                            {
                                                textBox.Text = "";
                                                foreach (Bank bank in banks)
                                                {
                                                    textBox.AppendText("Банк: "+bank.bankName+", карта: " + bank.cardName + Environment.NewLine);
                                                }
                                            }
                                        )
                             );
        }

    }
}
