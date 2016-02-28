using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI_final.Models.Data;
using WebAPI_final.Models;
using WebAPI_final.Models.Service;
using WebAPI_final.Models.GestionErreurs;
namespace WebAPI_final.Controllers
{
    public class InterestController : ApiController
    {
        public DataRetour donnees = new DataRetour();

        /// <summary>
        /// Methode qui set un Data d'un taux d'intêrets à partir de certains paramètres
        /// </summary>
        /// <param name="NameRate"> Nom du taux voulu</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public void setInterestRate(String NameRate, DateTime startDate, DateTime endDate)
        {
            Data.InterestRate nameInterest = (Data.InterestRate)Enum.Parse(typeof(Data.InterestRate), NameRate.ToUpper());
                     
            Services s = new Services();

            Data d = s.getInterestRate(nameInterest, startDate, endDate, donnees);

            // gestion d'erreur
            donnees.SetData(GestionErreurs.interestErreur((DataInterestRate)d, donnees));
            GestionErreurs.donneesIncomplètes(donnees, startDate, endDate);
            //
        }

        /// <summary>
        /// Methode appelant HttpGet, qui permet grace aux paramètres dans l'Uri, d'obtenir les données en Json d'un taux d'intêret,
        /// entre 2 dates.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        [Route("interest/{d1:datetime}/{d2:datetime}/{names}")]
        public DataRetour GetInterest(DateTime d1, DateTime d2,string names)
        {
            setInterestRate(names, d1, d2);
            return donnees;
        }
    }
}