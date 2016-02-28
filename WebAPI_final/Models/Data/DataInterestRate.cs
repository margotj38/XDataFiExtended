using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebAPI_final.Models.Data
{
    [DataContract]
    /// <summary>
    /// Implémentation de Data dans le cas des tauxs d'intérêts
    /// </summary>
    public class DataInterestRate : Data
    {
        #region Construction
        /// <summary>
        /// Constructeur pour taux d'intérêts
        /// </summary>
        /// <param name="symbol">Symbole à traiter</param>
        /// <param name="debut">Date de début</param>
        /// <param name="fin">Date de fin</param>
        public DataInterestRate(InterestRate symbol, DateTime start, DateTime end)
        {
            //On teste le bon ordre des dates
            if (end < start)
            {
                throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
            }

            Type = TypeData.InterestRate;

            Symbol = new List<string>();
            Symbol.Add(symbol.ToString());

            // Les colonnes seront ajoutées dynamiquement
            Columns = new List<string>();

            Start = start;
            End = end;

            initDataSet();
        }

        public DataInterestRate(DataInterestRate d, DateTime start, DateTime end)
        {
            //On teste le bon ordre des dates
            if (end < start)
            {
                throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
            }

            Type = TypeData.InterestRate;

            Symbol = d.Symbol;
            Columns = d.Columns;

            Start = start;
            End = end;

            initDataSet();
        }
        #endregion
    }
}
