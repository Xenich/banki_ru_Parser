using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace banki_ru_Parser
{
    delegate void delegBankParsed(string bankName);
    delegate void delegParseEnd(List<Bank> banks);
    internal sealed class Parser
    {
        public event delegBankParsed BankParsed;
        public event delegParseEnd ParseEnd;

        static List<Bank> banks = new List<Bank>();
        public void Parse(object _countOfPages)
        {
            int countOfPages = (int)_countOfPages;
            Thread[] threads = new Thread[countOfPages];
            for (int i = 0; i < countOfPages; i++)             // в нулевом элементе массива - ерунда, его пропускаем
            {
                threads[i] = new Thread(new ParameterizedThreadStart(ParsePage));
                threads[i].IsBackground = true;
                threads[i].Start(i + 1);
                Thread.Sleep(500);      // чтоб не забанили на серваке
            }

            for (int i = 0; i < countOfPages; i++)             // в нулевом элементе массива - ерунда, его пропускаем
            {
                threads[i].Join();
            }
            ParseEnd(banks);
            //int k = 1; // стоп
        }

        private void ParsePage(object _page)
        {
            int page = (int)_page;
            string site = "http://www.banki.ru/products/creditcards/search/?page=" + page.ToString();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string response = reader.ReadToEnd();

            int tbodyStart;
            int tbodyEnd;

            tbodyStart = response.IndexOf("<tbody>", 0);
            tbodyEnd = response.IndexOf("</tbody>", tbodyStart);
            response = response.Substring(tbodyStart, tbodyEnd - tbodyStart);

            string[] banks = response.Split(new string[] { "<tr class=" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < banks.Length; i++)
            {
                ParseBank(banks[i]);
            }
        }

        private void ParseBank(string _bank)
        {
            string[] columns = _bank.Split(new string[] { "<td style=" }, StringSplitOptions.RemoveEmptyEntries);
            int amount;

            string image;
            string cardURL;
            string cardName;
            string[] cardType;
            string bankURL;
            string bankName;
            string[] constraints;     // особые ограничения
            string stavka;
            string creditlimit;
            string period;
            string costPerYear;
            string cashback;
            string[] cashbackConditions;       // условия кешбека
            string popularity;

            int start = 0;
            int end = 0;
            string str;

            image = ParseBetweenStrings(columns[1], "<img src=\"", "\">", ref start, ref end);
            cardURL = "https://www.banki.ru" + ParseBetweenStrings(columns[1], "<a class=\"font-bold\" href=\"", "\">", ref start, ref end);
            cardName = (ParseBetweenStrings(columns[1], ">", "</a>", ref start, ref end)).Trim();
                // тип карт
            str = "<div class=\"font-size-default\">";
            amount = new Regex(str).Matches(columns[1]).Count;
            cardType = new string[amount];
            for (int i = 0; i < amount; i++)
            {
                cardType[i] = ParseBetweenStrings(columns[1], str, "</div>", ref start, ref end);
            }

            bankURL = "https://www.banki.ru" + ParseBetweenStrings(columns[1], "href=\"", "\">", ref start, ref end);
            bankName = (ParseBetweenStrings(columns[1], ">", "</a>", ref start, ref end)).Trim();
                // Особые ограничения
            amount = new Regex("<li>").Matches(columns[1]).Count;
            constraints = new string[amount];
            for (int i = 0; i < amount; i++)
            {
                constraints[i] = ParseBetweenStrings(columns[1], new string[] { "<li>" }, new string[] { "</li>", "<ul>" }, ref end);
            }

            stavka = (ParseBetweenStrings(columns[2], "width: 10%;\">", "</td>")).Trim();
            creditlimit = (ParseBetweenStrings(columns[3], "width: 10%;\">", "</td>")).Trim();
            period = (ParseBetweenStrings(columns[4], "width: 10%;\">", "</td>")).Trim();
            end = 0;
            costPerYear = (ParseBetweenStrings(columns[5], new string[] { "<span style=\"white-space: nowrap;\">", "<span class=\"color - gray - blue\">" }, new string[] { "</span>" }, ref end)).Trim();
            end = 0;
            cashback = (ParseBetweenStrings(columns[6], new string[] { "width: 10%;\" class=\"text-align-right\">" }, new string[] { "<span", "</td>" }, ref end)).Trim();
                // условия кешбека
            amount = new Regex("<li>").Matches(columns[6]).Count;
            cashbackConditions = new string[amount];
            start = 0; end = 0;
            for (int i = 0; i < amount; i++)
            {
                cashbackConditions[i] = ParseBetweenStrings(columns[6], new string[] { "<li>", "<p>" }, new string[] { "</li>", "</p>" }, ref end);
            }
            popularity = (ParseBetweenStrings(columns[7], "<span class=\"font-size-default\">", "</span")).Trim();
            Bank bank = new Bank(image, cardURL, cardName, cardType, bankURL, bankName, constraints, stavka, creditlimit, period, costPerYear, cashback, cashbackConditions, popularity);
            object thrd = new object();
            lock (thrd)         // в тело lock может зайти только 1 поток
            {
                banks.Add(bank);
            }
            BankParsed(bank.bankName);                
        }

        private string ParseBetweenStrings(string inputString, string startStr, string EndString, ref int start, ref int end)
        {
            start = inputString.IndexOf(startStr, end);
            if (start == -1)
                return "";
            end = inputString.IndexOf(EndString, start);
            return inputString.Substring(start + startStr.Length, end - start - startStr.Length);
        }

        private string ParseBetweenStrings(string inputString, string[] startStrAny, string[] endStringAny, ref int end)
        {
            int length = 0;
            int start = int.MaxValue;

            foreach (string str in startStrAny)
            {
                int n = inputString.IndexOf(str, end);
                if (n < start && n != -1)
                {
                    start = n;
                    length = str.Length;
                }
            }
            if (start == int.MaxValue)
                return "";

            int temp = end;
            end = int.MaxValue;
            foreach (string str in endStringAny)
            {
                int n = inputString.IndexOf(str, start);
                if (n < end && n != -1)
                {
                    end = n;
                }
            }
            if (end == int.MaxValue)
            {
                end = temp;
                return "";
            }

            return inputString.Substring(start + length, end - start - length);
        }

        private string ParseBetweenStrings(string inputString, string startStr, string EndString)
        {
            int start = inputString.IndexOf(startStr, 0);
            if (start == -1)
                return "";
            int end = inputString.IndexOf(EndString, start);
            return inputString.Substring(start + startStr.Length, end - start - startStr.Length);
        }
    }
}