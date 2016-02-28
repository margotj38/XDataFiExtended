using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace WebAPI_final.Models.Service
{
    [ServiceContract]
    /// <summary> Service de l'historique des actifs </summary>
    public interface IActifService
    {
        [OperationContract]
        /// <summary>
        /// Crée un DataActif, et le remplie de l'historique des actifs
        /// </summary>
        /// <param name="symbol">Nom des symboles à traiter</param>
        /// <param name="colums">Informations à fournir (high, low, ...)</param>
        /// <param name="start">Date de début</param>
        /// <param name="end">Date de fin</param>
        WebAPI_final.Models.Data.Data getActifHistorique(List<string> symbol, List<WebAPI_final.Models.Data.Data.HistoricalColumn> columns, DateTime start, DateTime end, Models.Data.DataRetour Retour = null);
    }
}
