using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace banki_ru_Parser
{
    internal sealed class Bank
    {
        public string image;
        public string cardURL;
        public string cardName;
        public string[] cardType;
        public string bankURL;
        public string bankName;
        public string[] constraints;     // особые ограничения
        public string stavka;
        public string creditlimit;
        public string period;
        public string costPerYear;
        public string cashback;
        public string[] cashbackConditions;       // условия кешбека
        public string popularity;

        public Bank(string image, string cardURL, string cardName, string[] cardType, string bankURL, string bankName, 
                    string[] constraints, string stavka, string creditlimit, string period, string costPerYear, string cashback,
                    string[] cashbackConditions, string popularity)
        {
            this.image = image;
            this.cardURL = cardURL;
            this.cardName = cardName;
            this.cardType = cardType;
            this.bankURL = bankURL;
            this.bankName = bankName;
            this.constraints = constraints;     // особые ограничения
            this.stavka = stavka;
            this.creditlimit = creditlimit;
            this.period = period;
            this.costPerYear = costPerYear;
            this.cashback = cashback;
            this.cashbackConditions = cashbackConditions;       // условия кешбека
            this.popularity = popularity;
        }      
    }
}
