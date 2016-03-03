using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using System.Net.Http;
using System.Web.Http;
using WebAPI_final.Models.Data;
using WebAPI_final.Models;
using WebAPI_final.Models.Service;
using WebAPI_final.Models.GestionErreurs;


namespace WebAPI_final.Controllers
{
    public class ActifController : ApiController
    {
        public DataRetour donnees = new DataRetour();

        //TODO: Gérer les erreurs dans les chiffres pour date, et choix d'options
        //      Virer les fonctions statics et les mettre dans un meilleur endroit
        //      BLoquer quand 2 fois le meme actif

        /// <summary>
        /// Methode qui set un Data d'actifs à partir de différents paramètres
        /// </summary>
        /// <param name="namesActif"> Liste de String contenant les noms (Yahoo) des actifs désirés </param>
        /// <param name="columns"> Liste contenant les options souhaitées (High, Low, Volume ...)</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public void setActif(List<String> namesActif, List<Data.HistoricalColumn> columns, DateTime startDate, DateTime endDate)
        {
            //Gestion du GetActif sans spécifier les options --> tout ajouter
            if (!columns.Any())
            {
                columns.Add(Data.HistoricalColumn.Open);
                columns.Add(Data.HistoricalColumn.High);
                columns.Add(Data.HistoricalColumn.Low);
                columns.Add(Data.HistoricalColumn.Close);
                columns.Add(Data.HistoricalColumn.Volume);

            }

            Services s = new Services();
            Data d = s.getActifHistorique(namesActif, columns, startDate, endDate, donnees);

            // gestion d'erreur
            try
            {
                donnees.SetData(GestionErreurs.actifErreur((DataActif)d, donnees));
                GestionErreurs.donneesIncomplètes(donnees, startDate, endDate);
            }
            catch
            {

            }
            //
        }



        /// <summary>
        /// Methode appelant HttpGet, qui permet grace aux paramètres dans l'Uri, d'obtenir les données en Json de certains actifs,
        /// entre 2 dates.
        /// Séparer les noms des actifs avec un charactères "~"
        /// Cette méthode retourne tous les paramètres possibles (Open, High, Low Close, Volume)
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        [Route("actif/{d1:datetime}/{d2:datetime}/{names}")]
        public DataRetour GetActif(DateTime d1, DateTime d2, String names)
        {
            setActif(Transformation(names), new List<Data.HistoricalColumn>(), d1, d2);
            return donnees;
        }


        /// <summary>
        /// Methode appelant HttpGet, qui permet grace aux paramètres dans l'Uri, d'obtenir les données en Json de certains actifs,
        /// entre 2 dates.
        /// Séparer les noms des actifs avec un charactères "~"
        /// Cette méthode permet de choisir les options: entrer High, Low etc .. séparé de "~"
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="options"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        [Route("actif/{d1:datetime}/{d2:datetime}/{options}/{names}")]
        public DataRetour GetActif(DateTime d1, DateTime d2,String options , String names)
        {
            setActif(Transformation(names), Transformation2(options), d1, d2);
            return donnees;
        }

        [Route("actif/realtime/{name}")]
        public DataRetour GetActif(String name)
        {
            string url = "http://finance.yahoo.com/webservice/v1/symbols/%5" + name + "/quote?format=json";
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
        /// Méthode transformant un string (contenant les noms d'actifs séparés par un ~) en une liste de String.
        /// </summary>
        /// <param name="Actifs"></param>
        /// <returns></returns>
        public static List<String> Transformation(string Actifs)
        {
            List<String> ActifsList = new List<String>();
            int l = 0;
            string content;
            for (int i = 0; i < Actifs.Length - 1; i++)
            {
                if (Actifs[i] == '~')
                {
                    content = Actifs.Substring(l, i - l);
                    ActifsList.Add(content.ToUpper());
                    l = i + 1;
                }

            }

            content = Actifs.Substring(l, Actifs.Length - l);
            ActifsList.Add(content.ToUpper());
            return ActifsList;
        }

        /// <summary>
        /// Méthode transformant un string (contenant les options séparées par un ~) en une Liste de Historical.Column
        /// </summary>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static List<Data.HistoricalColumn> Transformation2(string Params)
        {
            List<Data.HistoricalColumn> Parametres = new List<Data.HistoricalColumn>();
            int l = 0;
            string content;
            for (int i = 0; i < Params.Length - 1; i++)
            {
                if (Params[i] == '~')
                {
                    content = Params.Substring(l, i - l).ToLower();
                    content = content.First().ToString().ToUpper() + String.Join("", content.Skip(1));
                    Parametres.Add((Data.HistoricalColumn)Enum.Parse(typeof(Data.HistoricalColumn), content));
                    l = i + 1;
                }

            }

            content = Params.Substring(l, Params.Length - l).ToLower();
            content = content.First().ToString().ToUpper() + String.Join("", content.Skip(1));
            Parametres.Add((Data.HistoricalColumn)Enum.Parse(typeof(Data.HistoricalColumn), content));
            return Parametres;
        }

    }
}
