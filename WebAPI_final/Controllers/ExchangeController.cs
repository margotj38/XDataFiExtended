using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI_final.Models.Data;
using WebAPI_final.Models;
using WebAPI_final.Models.GestionErreurs;
using WebAPI_final.Models.Service;
using System.Data;


namespace WebAPI_final.Controllers
{
    public class ExchangeController : ApiController
    {
        public DataRetour donnees = new DataRetour();



        /// <summary>
        /// Méthode qui set un Data de taux de change, à partir de certains paramètres
        /// </summary>
        /// <param name="refCurr"> Currency de référence</param>
        /// <param name="comparedCurr"> Liste de Currency qu'on compare à la référene</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="frequence">Fréquence de récupération des données: 0 = Daily, 1 = Monthly, 2 = Yearly</param>
        public void setExchangeRate(String refCurr, List<Data.Currency> comparedCurr, DateTime startDate, DateTime endDate, int frequence)
        {
            Data.Currency nameExchangeRate = (Data.Currency)Enum.Parse(typeof(Data.Currency), refCurr.ToUpper());
            
            Data.Frequency freq = Data.Frequency.Daily;
            if (frequence == 0) freq = Data.Frequency.Daily;
            if (frequence == 1) freq = Data.Frequency.Monthly;
            if (frequence == 2) freq = Data.Frequency.Yearly;


            Services s = new Services();

            Data d = s.getExchangeRate(nameExchangeRate, comparedCurr, startDate, endDate, freq, donnees);

            // gestion d'erreur
            donnees.SetData(GestionErreurs.exchangeErreur((DataExchangeRate)d, donnees));
            GestionErreurs.donneesIncomplètes(donnees, startDate, endDate);
            //
        }

        /// <summary>
        /// Methode appelant HttpGet, qui permet grace aux paramètres dans l'Uri, d'obtenir les données en Json de certains taux de change(Fxtop),
        /// entre 2 dates.
        /// Séparer les noms de monnaies avec un charactères "~"
        /// Nous devons choisir une fréquence via un entier (0=Daily, 1=Monthly, 2=Yearly)
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="nameBasis"></param>
        /// <param name="namesComp"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        [Route("exchange/{d1:datetime}/{d2:datetime}/{freq}/{nameBasis}/{namesComp}")]
        public DataRetour GetExchange(DateTime d1, DateTime d2, String nameBasis, String namesComp, int freq)
        {
            setExchangeRate(nameBasis, Transformation(namesComp), d1, d2, freq);
            return donnees;
        }

        [Route("exchange/realtime/{nameBasis}/{namesComp}")]
        public DataRetour GetExchange(String nameBasis, String namesComp)
        {
            string url = "http://finance.yahoo.com/webservice/v1/symbols/" + nameBasis + namesComp + "=X/quote?format=json";
            var model = JsonMapper._download_serialized_json_data<RootObject>(url);
            Data data = new Data();
            DataSet dataSet = new DataSet();
            DataTable dt = new DataTable();
            DataColumn[] dataColumns = new DataColumn[1];
            dataColumns[0] = new DataColumn("Price", System.Type.GetType("System.String"));
            dt.Columns.Add(dataColumns[0]);
            dt.Rows.Add(model.list.resources[0].resource.fields.price);
            dataSet.Tables.Add(dt);
            data.set(dataSet, new List<string>(), new List<string>(), DateTime.Now, DateTime.Now, Data.TypeData.RealTime);
            donnees.SetData(data);
            return donnees;
        }

        /// <summary>
        /// Transforme un string (monnaies à comparer séparer par un ~) en Liste de Currency
        /// </summary>
        /// <param name="Params"></param>
        /// <returns></returns>
        private List<Data.Currency> Transformation(String Params)
        {
            List<Data.Currency> Parametres = new List<Data.Currency>();
            int l = 0;
            string content;
            for (int i = 0; i < Params.Length -1 ; i++)
            {
                if (Params[i] == '~')
                {
                    content = Params.Substring(l, i - l).ToUpper();
                    try
                    {
                        Parametres.Add((Data.Currency)Enum.Parse(typeof(Data.Currency), content));
                    }
                    catch
                    {
                        donnees.GetListeErreur().Add(content);
                    }
                    l = i + 1;
                } 
            }
            content = Params.Substring(l, Params.Length - l).ToUpper();
            try
            {
                Parametres.Add((Data.Currency)Enum.Parse(typeof(Data.Currency), content));
            }
            catch
            {
                donnees.GetListeErreur().Add(content);
            }
            return Parametres;
        }



    }
}