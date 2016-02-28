using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebAPI_final.Models.Data
{
    [DataContract]
    /// <summary>
    /// Implémente Data dans le cas de la récupération des taux de change depuis FXTop
    /// </summary>
    public class DataExchangeRate : Data
    {
        /// <summary>
        /// Fréquence d'acquisition (d,m,y), ne sert que dans le cas de l'acquisition fxtop 
        /// </summary>
        public Frequency Freq { get; private set; }

        /// <summary>
        /// Constructeur pour recuperer les taux de change avec des string 
        /// Utile pour la gestion d'erreurs
        /// </summary>
        /// <param name="symbol">Nom de la monnaie étalon</param>
        /// <param name="columns">Listes des monnaies à comparer à la monnaie étalon</param>
        /// <param name="debut">Date de début de sauvegarde des données</param>
        /// <param name="fin">Date de fin</param>
        /// <param name="freq">Fréquence de sauvegarde, traitement différent si journalier ou mensuel/annuel</param>
        public DataExchangeRate(String symbol, List<String> columns, DateTime start, DateTime end, Frequency freq)
        {
            //On teste le bon ordre des dates
            if (end < start)
            {
                throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
            }
            
            Type = TypeData.ExchangeRate;

            Symbol = new List<string>();
            Columns = new List<string>();

            //si Journalier, les colonnes correspondent à chaque monnaie à comparer
            if (freq == Frequency.Daily)
            {
                Symbol.Add(symbol.ToString());
                foreach (var item in columns)
                {
                    if (item != symbol)
                        Columns.Add(symbol.ToString() + "/" + item.ToString());
                }

            }
            //Si Mensuel/Annuel, chaque symbole correspond à un couple de monnaie, les colonnes correspondant aux valeurs moyennes, minimales et maximales
            else
            {
                foreach (var item in columns)
                {
                    if (item != symbol)
                        Symbol.Add(symbol.ToString() + "/" + item.ToString());
                }
                Columns.Add("Average");
                Columns.Add("Min");
                Columns.Add("Max");
            }

            Start = start;
            End = end;
            Freq = freq;

            initDataSet();
        }

        public DataExchangeRate(Currency symbol, List<Currency> columns, DateTime start, DateTime end, Frequency freq)
        {
            //On teste le bon ordre des dates
            if (end < start)
            {
                throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
            }

            Type = TypeData.ExchangeRate;

            Symbol = new List<string>();
            Columns = new List<string>();

            //si Journalier, les colonnes correspondent à chaque monnaie à comparer
            if (freq == Frequency.Daily)
            {
                Symbol.Add(symbol.ToString());
                foreach (var item in columns)
                {
                    if (item != symbol)
                        Columns.Add(symbol.ToString() + "/" + item.ToString());
                }

            }
            //Si Mensuel/Annuel, chaque symbole correspond à un couple de monnaie, les colonnes correspondant aux valeurs moyennes, minimales et maximales
            else
            {
                foreach (var item in columns)
                {
                    if (item != symbol)
                        Symbol.Add(symbol.ToString() + "/" + item.ToString());
                }
                Columns.Add("Average");
                Columns.Add("Min");
                Columns.Add("Max");
            }

            Start = start;
            End = end;
            Freq = freq;

            initDataSet();
        }
        /// <summary>
        /// constructeur par recopie
        /// </summary>
        /// <param name="d"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="freq"></param>
        public DataExchangeRate(DataExchangeRate d, DateTime start, DateTime end)
        {
            //On teste le bon ordre des dates
            if (end < start)
            {
                throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
            }

            Type = TypeData.ExchangeRate;
            this.Symbol = d.Symbol;
            this.Start = start;
            this.End = end;
            this.Columns = d.Columns;
            this.Freq = d.Freq;

            initDataSet();
        }
    }
}
