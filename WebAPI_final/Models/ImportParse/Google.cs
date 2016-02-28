using System;
using System.Globalization;
using System.Collections.Generic;

namespace WebAPI_final.Models.ImportParse
{
    /// <summary>
    /// Récupération d'actifs depuis la plateforme de Google
    /// (Récupération d'un fichier csv)
    /// Traitement via ParserCSV
    /// </summary>
    public class Google : ImportParse
    {
        #region Constructeur
        /// <summary>
        /// Méthode de connexion
        /// </summary>
        public Google()
        {
            _Parser = new Parser.ParserCSV("", CultureInfo.GetCultureInfo("EN"), true);

            // Test la connectivité réseau
            try
            {
                System.Net.IPHostEntry Test = System.Net.Dns.GetHostEntry("www.google.fr");
            }
            catch
            {
                throw new ConnectivityException(@"Il semble que votre connection réseau soit inactive ou qu'elle ne fonctionne pas correctement, veuillez vérifier vos paramètres de connexions ou contacter votre administrateur système");
            }
        }
        #endregion

        #region Méthodes
        /// <summary>
        /// Télécharge le fichier désiré
        /// et la parse, en remplissant les données
        /// </summary>
        /// <param name="d">Base de donnée, doit être de type HistoricalData</param>
        public override void ImportAndParse(Data.Data d, Data.DataRetour Erreur = null)
        {
            // On vérifie que les données soient de type historique
            switch (d.Type)
            {
                case Data.Data.TypeData.HistoricalData:

                    //On teste le bon ordre des dates
                    if (d.End < d.Start)
                    {
                        throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
                    }

                    // pour contenir la liste des entrées incorecte
                    List<string> listeErreur = new List<string>();
                    // Pour chaque symbol, on récupère le fichier et on le parse
                    foreach (string symbol in d.Symbol)
                    {
                        _Filepath = "Google_" + symbol + ".csv";

                        Uri siteUri;
                        // exemple d'url voulu : https://www.google.com/finance/historical?q=GOOG&startdate=May+8+2011&enddate=Jun+8%2C+2013&num=30&output=csv
                        //https://www.google.com/finance/historical?q=CA.PA&startdate=juin+1+2014&enddate=juin+3+2014&num=30&output=csv

                        siteUri = new Uri("http://www.google.com/finance/historical?q=" + symbol
                                           + "&startdate=" + d.Start.ToString("MMM+d+yyyy")
                                           + "&enddate=" + d.End.ToString("MMM+d+yyyy") + "&num=30&output=csv");

                        try
                        {
                            // Télécharge le fichier
                            ImportFile(siteUri);

                            // On indique au parser le symbol courant et le nom du fichier, puis on parse le fichier obtenu
                            _Parser.set(_Filepath, symbol);
                            _Parser.ParseFile(d);
                        }
                        catch
                        {
                            listeErreur.Add(symbol);
                        }
                        // On supprime le fichier
                        System.IO.File.Delete(@_Filepath);
                    }

                    // on renvoie la liste d'erreur
                    if (listeErreur.Count != 0)
                    {
                        Erreur.SetListeErreur(listeErreur);
                    }
                    break;
                default:
                    throw new Mauvaistype(@"Mauvais Type utilisé");
            }
        }
        #endregion
    }
}
