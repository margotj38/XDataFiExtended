using System;
using System.Globalization;
using System.Collections.Generic;

namespace WebAPI_final.Models.ImportParse
{
    /// <summary>
    /// Récupération d'actifs depuis la plateforme de Yahoo
    /// (Récupération d'un fichier csv)
    /// Traitement via ParserCSV
    /// </summary>
    public class Yahoo : ImportParse
    {
        #region Constructeur
        /// <summary>
        /// Méthode de connexion
        /// </summary>
        public Yahoo()
        {
            _Parser = new Parser.ParserCSV("", CultureInfo.GetCultureInfo("EN"), true);

            // Test la connectivité réseau
            try
            {
                System.Net.IPHostEntry Test = System.Net.Dns.GetHostEntry("fr.finance.yahoo.com");
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
                        throw new WrongDates(@"La date de E ne peut être antérieure au début de l'acquisition");
                    }

                    // pour contenir la liste des entrées incorecte
                    List<string> listeErreur = new List<string>();
                    // Pour chaque symbol, on récupère le fichier et on le parse
                    foreach (string symbol in d.Symbol)
                    {
                        _Filepath = "Yahoo_" + symbol + "_" + d.Start.ToString("dd-MM-yy") + "_" + d.End.ToString("dd-MM-yy") + ".csv";

                        Uri siteUri;
                        // exemple d'url voulu : http://ichart.finance.yahoo.com/table.csv?s=GOOG&d=5&e=8&f=2013&g=d&a=5&b=8&c=2011&ignore=.csv
                        // http://ichart.finance.yahoo.com/table.csv?s=GOOG&d=5&e=3&f=2014&g=d&a=4&b=2&c=2014&ignore=.csv

                        int moiDebt = d.Start.Month - 1;
                        int moiFin = d.End.Month - 1;

                        siteUri = new Uri("http://ichart.finance.yahoo.com/table.csv?s=" + symbol
                                            + "&d=" + moiFin + "&e=" + d.End.Day + "&f=" + d.End.Year
                                            + "&g=d" + "&a=" + moiDebt + "&b=" + d.Start.Day + "&c=" + d.Start.Year + "&ignore=.csv");

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
