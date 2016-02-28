using System;
using System.ServiceModel;

namespace WebAPI_final.Models.Service
{
    [ServiceContract]
    /// <summary> Service des taux d'intérêts </summary>
    public interface IInterestRateService
    {
        [OperationContract]
        /// <summary>
        /// Recherche les taux d'intérêts suivant les données en paramètre
        /// </summary>
        /// <param name="symbol">Nom du symbol à traiter</param>
        /// <param name="start">Date de début</param>
        /// <param name="end">Date de fin</param>
        WebAPI_final.Models.Data.Data getInterestRate(WebAPI_final.Models.Data.Data.InterestRate symbol, DateTime start, DateTime end, Data.DataRetour dretour);
    }
}
