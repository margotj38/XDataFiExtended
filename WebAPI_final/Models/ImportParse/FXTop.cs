using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace WebAPI_final.Models.ImportParse
{
    /// <summary>
    /// Récupération du code source HTML du site FXTop
    /// Traitement de celui-ci via le ParserFXTop
    /// </summary>
    public class FXTop : ImportParse
    {
        /// <summary> Correspond à la culture du fichier importé. le format varie en fonction de la langue utilisée </summary>
        private CultureInfo _Culture;

        #region Constructeur
        /// <summary> Constructeur du parser FXTop </summary>
        public FXTop()
        {
            _Culture = CultureInfo.GetCultureInfo("EN");


            string path = "./Schema/FxtopSchema.sch";
            //string path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sch";
            _Parser = new Parser.ParserGenerique(path);

            // Test la connectivité réseau
            try
            {
                System.Net.IPHostEntry Test = System.Net.Dns.GetHostEntry("www.fxtop.com");
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
        /// <param name="d">Base de donnée, doit être de type ExchangeRate</param>
        public override void ImportAndParse(Data.Data d, Data.DataRetour Erreur = null)
        {
            // On vérifie que les données soient de type ExchangeRate
            switch (d.Type)
            {
                case Data.Data.TypeData.ExchangeRate:

                    //On teste le bon ordre des dates
                    if (d.End < d.Start)
                    {
                        throw new WrongDates(@"La date de fin ne peut être antérieure au début de l'acquisition");
                    }

                    // Cast, pour obtenir la fréquence
                    Data.DataExchangeRate der = (Data.DataExchangeRate) d;
                    CultureInfo culture = CultureInfo.GetCultureInfo("EN");

                    List<string> list;

                    if (der.Freq == Data.Data.Frequency.Daily)
                        list = der.Columns;
                    else
                        list = der.Symbol;

                    // pour contenir la liste des entrées incorecte
                    List<string> listeErreur = new List<string>();
                    // Pour chaque symbol, on récupère le fichier et on le parse
                    foreach (var symb in list)
                    {
                        string[] monnaie = symb.Split('/');

                        //Récupération de l'url
                        //http://fxtop.com/en/historical-exchange-rates.php?A=1&C1=EUR&C2=USD&DD1=1&MM1=1&YYYY1=2012&B=1&P=&I=1&DD2=1&MM21&YYYY2=2013&btnOK=Go%21
                        //http://fxtop.com/en/historical-exchange-rates.php?A=1&C1=EUR&C2=USD&MA=1&DD1=01&MM1=05&YYYY1=2014&B=1&P=&I=1&DD2=01&MM2=06&YYYY2=2014&btnOK=Go%21
                        //http://fxtop.com/en/historical-exchange-rates.php?A=1&C1=ADF&C2=ALL&MA=1&DD1=1&MM1=4&YYYY1=2014&B=1&P=&I=1&DD2=1&MM2=5&YYYY2=2014&btnOK=Go%21
                        string url;

                        if (der.Freq == Data.Data.Frequency.Daily)
                            url = "http://fxtop.com/en/historical-exchange-rates.php?A=1&C1=" + monnaie[0] + "&C2=" + monnaie[1] + "&DD1=" + der.Start.ToString("dd") + "&MM1=" + der.Start.ToString("MM") + "&YYYY1=" + der.Start.ToString("yyyy") + "&B=1&P=&I=1&DD2=" + der.End.ToString("dd") + "&MM2=" + der.End.ToString("MM") + "&YYYY2=" + der.End.ToString("yyyy") + "&btnOK=Go%21";
                        else
                        {
                            string choixFreq;

                            if (der.Freq == Data.Data.Frequency.Monthly)
                                choixFreq = "&MA=1";
                            else
                                choixFreq = "&YA=1";

                            url = "http://fxtop.com/en/historical-exchange-rates.php?A=1&C1=" + monnaie[0] + "&C2=" + monnaie[1] + choixFreq + "&DD1=" + der.Start.ToString("dd") + "&MM1=" + der.Start.ToString("MM") + "&YYYY1=" + der.Start.ToString("yyyy") + "&B=1&P=&I=1&DD2=" + der.End.ToString("dd") + "&MM2=" + der.End.ToString("MM") + "&YYYY2=" + der.End.ToString("yyyy") + "&btnOK=Go%21";
                        }

                        _Filepath = "FxTop_" + monnaie[0] + "_" + monnaie[1] + "_" + der.Freq.ToString() 
                                  + "_" + der.Start.ToString("dd-MM-yy") + "_" + der.End.ToString("dd-MM-yy") + ".html";
                        Uri siteUri = new Uri(url);

                        try
                        {
                            // Télécharge le fichier
                            ImportFile(siteUri);

                            // On indique au parser le symbol courant et le nom du fichier, puis on parse le fichier obtenu
                            _Parser.set(_Filepath, symb);
                            _Parser.ParseFile(d);
                        }
                        catch
                        {
                            listeErreur.Add(symb);
                        }

                        // On insère les données dans d
                        Parser.ParserGenerique p = (Parser.ParserGenerique) _Parser;
                        DataSet ds = p.getDataSet();
                        insertData(d, ds, symb);

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

        /// <summary>
        /// Insère le contenu du DataSet dans le Data
        /// </summary>
        /// <param name="d">Base de donnée</param>
        /// <param name="ds">Données</param>
        /// <param name="symb">Symbol courant</param>
        private void insertData(Data.Data d, DataSet ds, string symb)
        {
            int nTable = ds.Tables.Count;
            int nRow = ds.Tables[nTable -1].Rows.Count;

            if (nRow == 0)
            {
                return;
            }

            // Détermine le nombre de colonne du DataSet
            int nbColDS = 1;
            int n = (int) ds.Tables[nTable - 1].Rows[0][1];

            while (nRow > nbColDS && n == (int) ds.Tables[nTable - 1].Rows[nbColDS][1] )
            {
                nbColDS++;
            }
            

            Data.DataExchangeRate der = (Data.DataExchangeRate) d;
            if (der.Freq == Data.Data.Frequency.Daily)
            {
                // Si aucune ligne, Alors on les crée
                if(d.Ds.Tables[0].Rows.Count == 0)
                {
                    for(int i=0; i < nRow/nbColDS; i++)
                    {
                        DataRow dr = d.Ds.Tables[0].NewRow();
                        dr["Symbol"] = symb.Substring(0, symb.IndexOf("/"));
                        dr["Date"] = DateTime.Parse(ds.Tables[nTable -1].Rows[i * nbColDS][0].ToString(), _Culture);
                        d.Ds.Tables[0].Rows.Add(dr);
                    }
                }

                for (int i = 0; i < nRow / nbColDS; i++)
                {
                    d.Ds.Tables[0].Rows[i][symb] = Double.Parse(ds.Tables[nTable - 1].Rows[i * nbColDS +1][0].ToString(), _Culture);
                }
            }
            else
            {
                for (int i = 0; i < nRow / nbColDS; i++)
                {
                    // On crée une nouvelle ligne
                    DataRow dr = d.Ds.Tables[0].NewRow();
                    dr["Symbol"] = symb;
					
                    // Récupération de la date
                    string date;
                    if (der.Freq == Data.Data.Frequency.Monthly)
                        date = ds.Tables[nTable - 1].Rows[i * nbColDS][0].ToString() + "-01";
                    else
                        date = ds.Tables[nTable - 1].Rows[i * nbColDS][0].ToString() + "-01-01";

                    dr["Date"] = DateTime.Parse(date, _Culture);

                    int k = 1;
                    foreach(string s in d.Columns)
                    {
                        dr[s] = Double.Parse(ds.Tables[nTable - 1].Rows[i * nbColDS + k][0].ToString(), _Culture);
                        k++;
                    }
                    d.Ds.Tables[0].Rows.Add(dr);
                }
            }
        }
        #endregion
    }
}
