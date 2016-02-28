using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace WebAPI_final.Models.Service
{
    [ServiceContract]
    /// <summary> Service des taux de change </summary>
    public interface IExchangeRateService
    {
        [OperationContract]
        /// <summary>
        /// Recherche les taux de change suivant les données en paramètre
        /// </summary>
        /// <param name="symbol">Nom de la monnaie étalon</param>
        /// <param name="columns">Listes des monnaies à comparer à la monnaie étalon</param>
        /// <param name="start">Date de début de sauvegarde des données</param>
        /// <param name="end">Date de fin</param>
        /// <param name="freq">Fréquence de sauvegarde, traitement différent si journalier ou mensuel/annuel</param>
        WebAPI_final.Models.Data.Data getExchangeRate(
            WebAPI_final.Models.Data.Data.Currency symbol, List<WebAPI_final.Models.Data.Data.Currency> columns,
            DateTime start, DateTime end, WebAPI_final.Models.Data.Data.Frequency freq, Data.DataRetour dretour);
    }
}
