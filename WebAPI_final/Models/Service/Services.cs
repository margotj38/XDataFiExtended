using System;
using System.Collections.Generic;

namespace WebAPI_final.Models.Service
{
    /// <summary>
    /// Contient toutes les fonctions de tous les services disponibles
    /// </summary>
    public class Services : IActifService, IExchangeRateService, IInterestRateService, IXMLService
    {
        #region IActifService
        /// <summary>
        /// Crée un DataActif, et le remplie de l'historique des actifs
        /// </summary>
        /// <param name="symbol">Nom des symboles à traiter</param>
        /// <param name="colums">Informations à fournir (high, low, ...)</param>
        /// <param name="start">Date de début</param>
        /// <param name="end">Date de fin</param>
       [System.Web.Http.HttpGet]
 public WebAPI_final.Models.Data.Data getActifHistorique(List<string> symbol, List<WebAPI_final.Models.Data.Data.HistoricalColumn> columns, DateTime start, DateTime end, WebAPI_final.Models.Data.DataRetour Retour = null)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start getActifHistorique", 0);
            
            // Création du DataActif
            WebAPI_final.Models.Data.DataActif d = new WebAPI_final.Models.Data.DataActif(symbol, columns, start, end);

            // Import des données désirées
            WebAPI_final.Models.ImportParse.Yahoo i = new WebAPI_final.Models.ImportParse.Yahoo();
            i.ImportAndParse(d, Retour);

            //WcfLibrary.Data.Data dd = (WcfLibrary.Data.Data)d;
            WebAPI_final.Models.Constantes.displayDEBUG("end getActifHistorique", 0);

            return d;
        }
        #endregion

        #region IExchangeRateService
        /// <summary>
        /// Recherche les taux de change suivant les données en paramètre
        /// </summary>
        /// <param name="symbol">Nom de la monnaie étalon</param>
        /// <param name="columns">Listes des monnaies à comparer à la monnaie étalon</param>
        /// <param name="start">Date de début de sauvegarde des données</param>
        /// <param name="end">Date de fin</param>
        /// <param name="freq">Fréquence de sauvegarde, traitement différent si journalier ou mensuel/annuel</param>
        public WebAPI_final.Models.Data.Data getExchangeRate(
            WebAPI_final.Models.Data.Data.Currency symbol, List<WebAPI_final.Models.Data.Data.Currency> columns,
            DateTime start, DateTime end, WebAPI_final.Models.Data.Data.Frequency freq, Data.DataRetour dretour )
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start getExchangeRate", 0);

            // Création du DataIRate
            WebAPI_final.Models.Data.DataExchangeRate d = new WebAPI_final.Models.Data.DataExchangeRate(symbol, columns, start, end, freq);

            // Import des données désirées
            WebAPI_final.Models.ImportParse.FXTop i = new WebAPI_final.Models.ImportParse.FXTop();
            i.ImportAndParse(d, dretour);

            WebAPI_final.Models.Constantes.displayDEBUG("end getExchangeRate", 0);

            return d;
        }
        #endregion

        #region IInterestRateService
        /// <summary>
        /// Recherche les taux d'intérêts suivant les données en paramètre
        /// </summary>
        /// <param name="symbol">Nom du symbol à traiter</param>
        /// <param name="start">Date de début</param>
        /// <param name="end">Date de fin</param>
        public WebAPI_final.Models.Data.Data getInterestRate(WebAPI_final.Models.Data.Data.InterestRate symbol, DateTime start, DateTime end, Data.DataRetour dretour)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start getInterestRate", 0);
            
            // Création du DataIRate
            WebAPI_final.Models.Data.DataInterestRate d = new WebAPI_final.Models.Data.DataInterestRate(symbol, start, end);

            // Import des données désirées
            WebAPI_final.Models.ImportParse.EBF i = new WebAPI_final.Models.ImportParse.EBF();
            i.ImportAndParse(d,dretour);

            WebAPI_final.Models.Constantes.displayDEBUG("end getInterestRate", 0);

            return d;
        }
        #endregion    

        #region IXML
        /// <summary>
        /// Exécute la fonction demandée dans le fichier XML
        /// </summary>
        /// <param name="content">contenu du fichier XML</param>
        public WebAPI_final.Models.Data.Data getXML(string s)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start getXML", 0);

            // Création du DataIRate
            WebAPI_final.Models.Data.DataXML d = new WebAPI_final.Models.Data.DataXML();

            // Import des données désirées
            WebAPI_final.Models.ImportParse.XML i = new WebAPI_final.Models.ImportParse.XML(s);
            i.ImportAndParse(d);

            WebAPI_final.Models.Constantes.displayDEBUG("end getXML", 0);

            return d;
        }
        #endregion
    }
}
