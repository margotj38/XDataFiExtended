using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI_final.Models.Data;
using WebAPI_final.Models;
using WebAPI_final.Models.Service;
using System.Data;

namespace WebAPI_final.Models.GestionErreurs
{
    public static class GestionVolatilite
    {

        /// <summary>
        /// Le type de donnees dans le parametre donnee .
        /// </summary>
        /// <param name="donnees"></param>
        /// <returns></returns>
        public static double volatilite(Data.Data donnees, string columnName)
        {
            //Initialisation de la volatilite
            double ecart_type = 0;
            int nbr_ligne = donnees.Ds.Tables[0].Rows.Count;

            //Traitement identique pour tous les types de donnees

            //On calcule la moyenne du cours
            double moyenne = 0;
            for (int i = 0; i < nbr_ligne; i++)
            {
                moyenne += (double)donnees.Ds.Tables[0].Rows[i][columnName];
            }
            moyenne /= nbr_ligne;

            //On calcule sont ecart type
            for (int i = 0; i < nbr_ligne; i++)
            {
                ecart_type += Math.Pow((double)donnees.Ds.Tables[0].Rows[i][columnName] - moyenne, 2);
            }

            ecart_type = Math.Sqrt(ecart_type / nbr_ligne);

            return ecart_type;
        }

        public static Data.Data donneesVolatilite(Data.Data d, DateTime debut, DateTime fin, int indiceFin)
        {

            Data.Data retour = d;
            // On veut le nb de date ouvrée entre debut et fin
            List<DateTime> listedate = GestionSimulation.listeDate(debut, fin);
            int nb_date = listedate.Count;
            // après fin (chronologique)
            int nb_date_suivant = indiceFin + 1;
            // avant debut
            int nb_date_precedant = d.Ds.Tables[0].Rows.Count - indiceFin - 1;

            // si on a suffisamment de date suivante
            if (nb_date_suivant - nb_date > 0)
            {
                //et suffisamment de date précedente
                if (nb_date_precedant - nb_date > 0)
                {
                    // on reccupère nb_date données à gauche et à droite
                    retour = recupereDonnees(d, indiceFin + 1 + nb_date, indiceFin - nb_date);
                }
                // si pas assez de donées précédemment
                else
                {
                    // on récuppère à partir du début de d
                    retour = recupereDonnees(d, d.Ds.Tables[0].Rows.Count-1, indiceFin - nb_date);
                }
            }
            // si pas suffisamment de date suivante
            else
            {
                // si assez de date précédante
                if ((nb_date_precedant - nb_date) > 0)
                {
                    retour = recupereDonnees(d, indiceFin + 1 + nb_date, 0);
                }
                else
                {
                    retour = recupereDonnees(d, d.Ds.Tables[0].Rows.Count-1, 0);
                }
            }

            return retour;
        }

        private static Data.Data recupereDonnees(Data.Data d, int indiceDebut, int indiceFin)
        {
            Data.Data retour = d;
            DateTime start = (DateTime)d.Ds.Tables[0].Rows[indiceDebut]["Date"];
            DateTime end = (DateTime)d.Ds.Tables[0].Rows[indiceFin]["Date"];

            switch (d.Type)
            {
                case Data.Data.TypeData.HistoricalData:
                    retour = new DataActif(d.Symbol, d.Columns, start, end);
                    break;

                case Data.Data.TypeData.ExchangeRate:
                    retour = new DataExchangeRate((DataExchangeRate)d, start, end);
                    break;

                case Data.Data.TypeData.InterestRate:
                    retour = new DataInterestRate((DataInterestRate)d, start, end);
                    break;

                default:
                    break;
            }
            for (int i = indiceFin; i <= indiceDebut; i++)
            {
                DataRow row = retour.Ds.Tables[0].NewRow();
                row["Symbol"] = d.Ds.Tables[0].Rows[0]["Symbol"];
                row["Date"] = d.Ds.Tables[0].Rows[i]["Date"];
                foreach (string column in d.Columns)
                {
                    row[column] = d.Ds.Tables[0].Rows[i][column];
                }
                retour.Ds.Tables[0].Rows.Add(row);
            }
            return retour;
        }
    }
}