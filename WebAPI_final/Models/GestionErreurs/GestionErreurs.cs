﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI_final.Models.Data;
using WebAPI_final.Models;
using WebAPI_final.Models.Service;
using System.Data;

namespace WebAPI_final.Models.GestionErreurs
{

    public static class GestionErreurs
    {
        #region Erreur de saisie de date
        public static void donneesIncomplètes(DataRetour d, DateTime start, DateTime end)
        {
            foreach (string nom in d.GetData().Symbol)
            {
                DateTime s = start;
                DateTime e = end;
                bool ok = false;
                DataTable t = d.GetData().Ds.Tables[0];

                for (int i = 0; i < t.Rows.Count; i++)
                {
                    if (((string)t.Rows[i]["Symbol"]).Equals(nom))
                    {
                        if (!ok)
                        {
                            switch (d.GetData().Type)
                            {
                                case Data.Data.TypeData.InterestRate:
                                    s = (DateTime)t.Rows[i]["Date"];
                                    break;
                                case Data.Data.TypeData.ExchangeRate:
                                    e = (DateTime)t.Rows[i]["Date"];
                                    switch (((DataExchangeRate)d.GetData()).Freq)
                                    {
                                        case Data.Data.Frequency.Monthly:
                                            if (((DateTime)t.Rows[i]["Date"]).AddMonths(1) > end)
                                            {
                                                end = e;
                                            }
                                            break;
                                        case Data.Data.Frequency.Yearly:
                                            if (((DateTime)t.Rows[i]["Date"]).AddYears(1) > end)
                                            {
                                                end = e;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                default:
                                    e = (DateTime)t.Rows[i]["Date"];
                                    break;
                            }
                            ok = true;
                        }
                        else
                        {
                            switch (d.GetData().Type)
                            {
                                case Data.Data.TypeData.InterestRate:
                                    e = (DateTime)t.Rows[i]["Date"];
                                    break;
                                default:
                                    s = (DateTime)t.Rows[i]["Date"];
                                    break;
                            }
                        }
                    }
                }

                if (s != start)
                {
                    d.GetWarning().Add(nom + " : données incomplètes : date de debut d aquisition inférieure à celle du début des cotations : " + s.ToString("dd/MM/yyyy"));
                }
                if (e != end)
                {
                    d.GetWarning().Add(nom + " : données incomplètes : date de fin d aquisition supérieure à celle de fin des cotations : " + e.ToString("dd/MM/yyyy"));
                }
            }
        }
        #endregion

        #region Methode de recherche d'absence de donnees si liste de symbol
        static private bool pasDeValeur(Data.Data d, DateTime lendemain, Data.Data.Frequency freq = Data.Data.Frequency.Daily)
        {
            foreach (DataRow r in d.Ds.Tables[0].Rows){
                switch (freq)
                {
                    // si on a une valeur pour un symbol
                    case Data.Data.Frequency.Daily:
                        if ((((DateTime)r["Date"]) - lendemain).TotalDays == 0)
                        {
                            return true;
                        }
                        break;
                    case Data.Data.Frequency.Monthly:
                        if (((DateTime)r["Date"]).Month == lendemain.Month)
                        {
                            return true;
                        }
                        break;
                    case Data.Data.Frequency.Yearly:
                        if (((DateTime)r["Date"]).Year == lendemain.Year)
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }
            }
            return false;
        }
        #endregion

        #region Taux de change
        public static DataExchangeRate exchangeErreur_test(DataExchangeRate d, DataRetour dretour)
        {
            DataExchangeRate retour = new DataExchangeRate(d, d.Start, d.End);
            // on lit les données d
            int nb_ligne = d.Ds.Tables[0].Rows.Count;
            DateTime date1 = (DateTime)d.Ds.Tables[0].Rows[0]["Date"];
            DataRow newRow = retour.Ds.Tables[0].NewRow();
            int nbrColonne = 0;

            // recopie la 1ère ligne
            foreach (string colonne in d.Columns)
            {
                newRow[colonne] = d.Ds.Tables[0].Rows[0][colonne];
                nbrColonne++;
            }
            newRow["Symbol"] = d.Ds.Tables[0].Rows[0]["Symbol"];
            newRow["Date"] = d.Ds.Tables[0].Rows[0]["Date"];
            retour.Ds.Tables[0].Rows.Add(newRow);

            //Les colonnes définissent les rapports entre devises
            string[] nomMonnaie = new string[nbrColonne];
            for (int i = 0; i < nbrColonne; i++)
            {
                nomMonnaie[i] = d.Columns[i];

            }

            DateTime date2;
            for (int ligne = 1; ligne < nb_ligne; ligne++)
            {
                date2 = (DateTime)d.Ds.Tables[0].Rows[ligne]["Date"];
                // on test la fréquence 
                switch (d.Freq)
                {
                    case Data.Data.Frequency.Daily:
                        #region Daily
                        if (d.Columns.Count == 1)
                        {
                            #region Si un seul taux
                            if ((GestionSimulation.AddBusinessDays(date2, 2) < date1)/*&&(ligne<d.Ds.Tables[0].Rows.Count-1)*/)
                            {
                                List<GestionSimulation.evenement>[] tabListeCompa = new List<GestionSimulation.evenement>[nbrColonne];
                                // Initialisation des simulations utiles ***********************************************
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    tabListeCompa[i] = new List<GestionSimulation.evenement>();
                                }

                                string nomMonnaie1;
                                string nomMonnaie2;
                                string nomCourrant;
                                // On effectue une simulation                                                       
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nomCourrant = nomMonnaie[i];
                                    nomMonnaie1 = nomCourrant.Substring(0,3);
                                    nomMonnaie2 = nomCourrant.Substring(4,7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Daily);
                                        tabListeCompa[i] = GestionSimulation.calculTauxExchange(dcompa, date1, date2);
                                    }
                                        //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else 
                                    {
                                        break;
                                    }
                                    
                                }

                                // fin init simulation  ********************************************************************

                                // On doit maintenant ajouter autant de row qu'il y a de date.
                                int nb_date = 0;
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nb_date = Math.Max(nb_date, tabListeCompa[i].Count);
                                }
                                for (int i = nb_date - 1; i >= 0; i--)
                                {
                                    // on crée la ligne
                                    newRow = retour.Ds.Tables[0].NewRow();
                                    DateTime dateCour = date1;

                                    // on la complète
                                    for (int j = 0; j < nbrColonne; j++)
                                    {

                                        newRow[nomMonnaie[j]] = tabListeCompa[j][i].getCours();
                                        dateCour = tabListeCompa[j][i].getDate();
                                    }

                                    //on ajoute la ligne
                                    newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                    newRow["Date"] = dateCour;
                                    retour.Ds.Tables[0].Rows.Add(newRow);
                                }

                                // on recopie les valeurs de date2
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);

                                dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                    ": Simulation de " +
                                    date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on simule aléatoirement cette valeur :
                                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                        double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                        double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                        newRow[colonne] = valeur;

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Si plusieurs taux
                            DateTime lendemain = GestionSimulation.AddBusinessDays(date2, 1);
                            if ((lendemain < date1) && (pasDeValeur(d, lendemain)))
                            {
                                List<GestionSimulation.evenement>[] tabListeCompa = new List<GestionSimulation.evenement>[nbrColonne];
                                // Initialisation des simulations utiles ***********************************************
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    tabListeCompa[i] = new List<GestionSimulation.evenement>();
                                }

                                string nomMonnaie1;
                                string nomMonnaie2;
                                string nomCourrant;
                                // On effectue une simulation                                                       
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nomCourrant = nomMonnaie[i];
                                    nomMonnaie1 = nomCourrant.Substring(0, 3);
                                    nomMonnaie2 = nomCourrant.Substring(4, 7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        //verifier pour les dates
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Daily);
                                        tabListeCompa[i] = GestionSimulation.calculTauxExchange(dcompa, date1, date2);
                                    }
                                    //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else
                                    {
                                        break;
                                    }

                                }

                                // fin init simulation  ********************************************************************

                                // On doit maintenant ajouter autant de row qu'il y a de date.
                                int nb_date = 0;
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nb_date = Math.Max(nb_date, tabListeCompa[i].Count);
                                }
                                for (int i = nb_date - 1; i >= 0; i--)
                                {
                                    // on crée la ligne
                                    newRow = retour.Ds.Tables[0].NewRow();
                                    DateTime dateCour = date1;

                                    // on la complète
                                    for (int j = 0; j < nbrColonne; j++)
                                    {

                                        newRow[nomMonnaie[j]] = tabListeCompa[j][i].getCours();
                                        dateCour = tabListeCompa[j][i].getDate();
                                    }

                                    //on ajoute la ligne
                                    newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                    newRow["Date"] = dateCour;
                                    retour.Ds.Tables[0].Rows.Add(newRow);
                                }

                                // on recopie les valeurs de date2
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);

                                dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                    ": Simulation de " +
                                    date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on calcule cette valeur :
                                        String nomCourrant = colonne;
                                        String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                        String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                        if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                        {
                                            List<String> columnsCompa = new List<string>();
                                            columnsCompa.Add(nomMonnaie1);
                                            columnsCompa.Add(nomMonnaie2);
                                            //verifier pour les dates
                                            DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Daily);
                                            double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                             (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                            newRow[colonne] = valeur;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        break;
                        #endregion
                    case Data.Data.Frequency.Monthly:
                        #region Monthly
                        // s'il manque un mois
                        // 61 = 31+30
                        if (d.Columns.Count == 1)
                        {
                            #region S'il n'y a qu'un taux
                            if ((date1 - date2).TotalDays >= 61)
                            {
                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on calcule cette valeur :

                                    String nomCourrant = colonne;
                                    String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                    String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        //verifier pour les dates
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Monthly);
                                        double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                         (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                        newRow[colonne] = valeur;
                                    }
                                    //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else
                                    {
                                        break;
                                    }

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddMonths(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on calcule cette valeur :
                                        String nomCourrant = colonne;
                                        String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                        String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                        if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                        {
                                            List<String> columnsCompa = new List<string>();
                                            columnsCompa.Add(nomMonnaie1);
                                            columnsCompa.Add(nomMonnaie2);
                                            DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Monthly);
                                            double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                             (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                            newRow[colonne] = valeur;
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Si plusieurs taux
                            DateTime moissuiv = date1.AddMonths(1);
                            if (((date1 - date2).TotalDays >= 31) && (pasDeValeur(d, moissuiv, Data.Data.Frequency.Monthly)))
                            {
                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    String nomCourrant = colonne;
                                    String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                    String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Monthly);
                                        double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                         (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                        newRow[colonne] = valeur;
                                    }
                                    //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else
                                    {
                                        break;
                                    }

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + (((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddMonths(1).ToString("dd/MM/yyyy")));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddMonths(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on calcule cette valeur :
                                        String nomCourrant = colonne;
                                        String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                        String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                        if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                        {
                                            List<String> columnsCompa = new List<string>();
                                            columnsCompa.Add(nomMonnaie1);
                                            columnsCompa.Add(nomMonnaie2);
                                            DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Monthly);
                                            double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                             (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                            newRow[colonne] = valeur;
                                        }
                                        //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                        else
                                        {
                                            break;
                                        }

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        break;
                        #endregion
                    case Data.Data.Frequency.Yearly:
                        #region Yearly
                        if (d.Columns.Count == 1)
                        {
                            #region S'in n'y a qu'un taux
                            // s'il manque un an
                            //731 = 366+365
                            if ((date1 - date2).TotalDays >= 731)
                            {

                                // on calcule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    String nomCourrant = colonne;
                                    String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                    String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Yearly);
                                        double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                         (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                        newRow[colonne] = valeur;
                                    }
                                    //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else
                                    {
                                        break;
                                    }

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddYears(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            // sinon
                            // on recopie les valeurs
                            newRow = retour.Ds.Tables[0].NewRow();
                            foreach (string colonne in d.Columns)
                            {
                                // on test si il ne manque pas juste une colonne
                                if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                else
                                {
                                    String nomCourrant = colonne;
                                    String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                    String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Yearly);
                                        double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]] /
                                                         (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]]);
                                        newRow[colonne] = valeur;
                                    }
                                    //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else
                                    {
                                        break;
                                    }

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                            }
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                            retour.Ds.Tables[0].Rows.Add(newRow);
                            #endregion
                        }
                        else
                        {
                            #region Si plusieurs taux
                            DateTime anneesuiv = date2.AddYears(1);
                            if (((date1 - date2).TotalDays >= 366) && pasDeValeur(d, anneesuiv, Data.Data.Frequency.Yearly))
                            {

                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddYears(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            // sinon
                            // on recopie les valeurs
                            newRow = retour.Ds.Tables[0].NewRow();
                            foreach (string colonne in d.Columns)
                            {
                                // on test si il ne manque pas juste une colonne
                                if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                else
                                {
                                    String nomCourrant = colonne;
                                    String nomMonnaie1 = nomCourrant.Substring(0, 3);
                                    String nomMonnaie2 = nomCourrant.Substring(4, 7);
                                    if ((nomMonnaie1 != "USD") && (nomMonnaie2 != "USD"))
                                    {
                                        List<String> columnsCompa = new List<string>();
                                        columnsCompa.Add(nomMonnaie1);
                                        columnsCompa.Add(nomMonnaie2);
                                        DataExchangeRate dcompa = new DataExchangeRate("USD", columnsCompa, date1, date2, Data.Data.Frequency.Yearly);
                                        double valeur = ((double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[0]] /
                                                         (double)dcompa.Ds.Tables[0].Rows[ligne - 1][dcompa.Columns[1]]);
                                        newRow[colonne] = valeur;
                                    }
                                    //Il n'y a donc pas de trou on est pas cencé être rentré ici
                                    else
                                    {
                                        break;
                                    }

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                            }
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                            retour.Ds.Tables[0].Rows.Add(newRow);
                            #endregion
                        }
                        break;
                        #endregion
                    default:
                        break;
                }
                date1 = date2;
            }
            return retour;
        }

        /// <summary>
        /// Gere les possibles erreurs pour les taux de change : manque de valeurs, manque de donnees en entree
        /// </summary>
        /// <param name="d"> Taux de change au format DataExchangeRate avec des donnees eventuellement erronnées</param>
        /// <returns>Taux de change au format DataExchangeRate avec des donnees complète</returns>
        public static DataExchangeRate exchangeErreur(DataExchangeRate d, DataRetour dretour)
        {
            DataExchangeRate retour = new DataExchangeRate(d, d.Start, d.End);
            // on lit les données d
            int nb_ligne = d.Ds.Tables[0].Rows.Count;
            DateTime date1 = (DateTime)d.Ds.Tables[0].Rows[0]["Date"];
            DataRow newRow = retour.Ds.Tables[0].NewRow();
            int nbrColonne = 0;

            // recopie la 1ère ligne
            foreach (string colonne in d.Columns)
            {
                newRow[colonne] = d.Ds.Tables[0].Rows[0][colonne];
                nbrColonne++;
            }
            newRow["Symbol"] = d.Ds.Tables[0].Rows[0]["Symbol"];
            newRow["Date"] = d.Ds.Tables[0].Rows[0]["Date"];
            retour.Ds.Tables[0].Rows.Add(newRow);

            //Les colonnes définissent les rapports entre devises
            string[] nomMonnaie = new string[nbrColonne];
            for (int i = 0; i < nbrColonne; i++)
            {
                nomMonnaie[i] = d.Columns[i];

            }

            DateTime date2;
            for (int ligne = 1; ligne < nb_ligne; ligne++)
            {
                date2 = (DateTime)d.Ds.Tables[0].Rows[ligne]["Date"];
                // on test la fréquence 
                switch (d.Freq)
                {
                    case Data.Data.Frequency.Daily:
                        #region Daily
                        if (d.Columns.Count == 1)
                        {
                            #region Si un seul taux
                            if ((GestionSimulation.AddBusinessDays(date2, 2) < date1)/*&&(ligne<d.Ds.Tables[0].Rows.Count-1)*/)
                            {
                                double volatilite = 0;
                                List<GestionSimulation.evenement>[] tabListeCompa = new List<GestionSimulation.evenement>[nbrColonne];
                                // Initialisation des simulations utiles ***********************************************
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    tabListeCompa[i] = new List<GestionSimulation.evenement>();
                                }

                                double val1;
                                double val2;
                                string nomCourrant;
                                // On effectue une simulation                                                       
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nomCourrant = nomMonnaie[i];
                                    val1 = (double)d.Ds.Tables[0].Rows[ligne][nomCourrant];
                                    val2 = (double)d.Ds.Tables[0].Rows[ligne - 1][nomCourrant];
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date2, date1, ligne);                   
                                    volatilite = GestionVolatilite.volatilite(dv, nomCourrant);                 
                                    tabListeCompa[i] = GestionSimulation.pontBrownien(date2, date1, val1, val2, volatilite);
                                }

                                // fin init simulation  ********************************************************************

                                // On doit maintenant ajouter autant de row qu'il y a de date.
                                int nb_date = 0;
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nb_date = Math.Max(nb_date, tabListeCompa[i].Count);
                                }
                                for (int i = nb_date - 1; i >= 0; i--)
                                {
                                    // on crée la ligne
                                    newRow = retour.Ds.Tables[0].NewRow();
                                    DateTime dateCour = date1;

                                    // on la complète
                                    for (int j = 0; j < nbrColonne; j++)
                                    {

                                        newRow[nomMonnaie[j]] = tabListeCompa[j][i].getCours();
                                        dateCour = tabListeCompa[j][i].getDate();
                                    }

                                    //on ajoute la ligne
                                    newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                    newRow["Date"] = dateCour;
                                    retour.Ds.Tables[0].Rows.Add(newRow);
                                }

                                // on recopie les valeurs de date2
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);

                                dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                    ": Simulation de " +
                                    date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on simule aléatoirement cette valeur :
                                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                        double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                        double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                        newRow[colonne] = valeur;

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Si plusieurs taux
                            DateTime lendemain = GestionSimulation.AddBusinessDays(date2, 1);
                            if ((lendemain < date1)&&(pasDeValeur(d,lendemain)))
                            {
                                double volatilite = 0;
                                List<GestionSimulation.evenement>[] tabListeCompa = new List<GestionSimulation.evenement>[nbrColonne];
                                // Initialisation des simulations utiles ***********************************************
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    tabListeCompa[i] = new List<GestionSimulation.evenement>();
                                }

                                double val1;
                                double val2;
                                string nomCourrant;
                                // On effectue une simulation                                                       
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nomCourrant = nomMonnaie[i];
                                    val1 = (double)d.Ds.Tables[0].Rows[ligne][nomCourrant];                        //
                                    val2 = (double)d.Ds.Tables[0].Rows[ligne - 1][nomCourrant];                    //
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date2, date1, ligne);                         //
                                    volatilite = GestionVolatilite.volatilite(dv, nomCourrant);                             //
                                    tabListeCompa[i] = GestionSimulation.pontBrownien(date2, date1, val1, val2, volatilite);
                                }

                                // fin init simulation  ********************************************************************

                                // On doit maintenant ajouter autant de row qu'il y a de date.
                                int nb_date = 0;
                                for (int i = 0; i < nbrColonne; i++)
                                {
                                    nb_date = Math.Max(nb_date, tabListeCompa[i].Count);
                                }
                                for (int i = nb_date - 1; i >= 0; i--)
                                {
                                    // on crée la ligne
                                    newRow = retour.Ds.Tables[0].NewRow();
                                    DateTime dateCour = date1;

                                    // on la complète
                                    for (int j = 0; j < nbrColonne; j++)
                                    {

                                        newRow[nomMonnaie[j]] = tabListeCompa[j][i].getCours();
                                        dateCour = tabListeCompa[j][i].getDate();
                                    }

                                    //on ajoute la ligne
                                    newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                    newRow["Date"] = dateCour;
                                    retour.Ds.Tables[0].Rows.Add(newRow);
                                }

                                // on recopie les valeurs de date2
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);

                                dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                    ": Simulation de " +
                                    date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on simule aléatoirement cette valeur :
                                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                        double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                        double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                        newRow[colonne] = valeur;

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        break;
                        #endregion
                    case Data.Data.Frequency.Monthly:
                        #region Monthly
                        // s'il manque un mois
                        // 31 = 31+30
                        if (d.Columns.Count == 1)
                        {
                            #region S'il n'y a qu'un taux
                            if ((date1 - date2).TotalDays >= 61)
                            {
                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddMonths(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on simule aléatoirement cette valeur :
                                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                        double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                        double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                        newRow[colonne] = valeur;

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Si plusieurs taux    
                            DateTime moissuiv = date1.AddMonths(1);
                            if (((date1 - date2).TotalDays >= 31)&&(pasDeValeur(d,moissuiv,Data.Data.Frequency.Monthly)))
                            {
                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + (((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddMonths(1).ToString("dd/MM/yyyy")));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddMonths(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            else
                            {
                                // sinon
                                // on recopie les valeurs
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on test si il ne manque pas juste une colonne
                                    if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                    {
                                        newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                    }
                                    else
                                    {
                                        // sinon on simule aléatoirement cette valeur :
                                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                        double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                        double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                        newRow[colonne] = valeur;

                                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                            ": Simulation de " +
                                            colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                    }
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            #endregion
                        }
                        break;
                        #endregion
                    case Data.Data.Frequency.Yearly:
                        #region Yearly
                        if (d.Columns.Count == 1)
                        {
                            #region S'in n'y a qu'un taux
                            // s'il manque un an
                            //731 = 366+365
                            if ((date1 - date2).TotalDays >= 731)
                            {

                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddYears(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            // sinon
                            // on recopie les valeurs
                            newRow = retour.Ds.Tables[0].NewRow();
                            foreach (string colonne in d.Columns)
                            {
                                // on test si il ne manque pas juste une colonne
                                if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                else
                                {
                                    // sinon on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                            }
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                            retour.Ds.Tables[0].Rows.Add(newRow);
                            #endregion
                        }
                        else
                        {
                            #region Si plusieurs taux
                            DateTime anneesuiv = date2.AddYears(1);
                            if (((date1 - date2).TotalDays >= 366)&& pasDeValeur(d,anneesuiv, Data.Data.Frequency.Yearly))
                            {

                                // on simule
                                newRow = retour.Ds.Tables[0].NewRow();
                                foreach (string colonne in d.Columns)
                                {
                                    // on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                                newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                                newRow["Date"] = ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).AddYears(1);
                                retour.Ds.Tables[0].Rows.Add(newRow);
                            }
                            // sinon
                            // on recopie les valeurs
                            newRow = retour.Ds.Tables[0].NewRow();
                            foreach (string colonne in d.Columns)
                            {
                                // on test si il ne manque pas juste une colonne
                                if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                                {
                                    newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                                }
                                else
                                {
                                    // sinon on simule aléatoirement cette valeur :
                                    Data.Data dv = GestionVolatilite.donneesVolatilite(d, date1.AddDays(-ligne), date2, ligne);
                                    double volatilite = GestionVolatilite.volatilite(dv, colonne);
                                    double valeur = GestionSimulation.normale((double)d.Ds.Tables[0].Rows[ligne - 1][colonne], volatilite);
                                    newRow[colonne] = valeur;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)newRow["Date"]).ToString("dd/MM/yyyy"));
                                }
                            }
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                            retour.Ds.Tables[0].Rows.Add(newRow);
                            #endregion
                        }
                        break;
                        #endregion
                    default:
                        break;
                }
                date1 = date2;
            }
            return retour;
        }
        #endregion

        #region Taux d'interet
        public static DataInterestRate interestErreur(DataInterestRate d, DataRetour dretour)
        {
            //On copie les donnees d dans une var locale
            DataInterestRate retour = new DataInterestRate(d, d.Start, d.End);

            // on lit les données d
            int nb_ligne = d.Ds.Tables[0].Rows.Count;
            DateTime date1 = (DateTime)d.Ds.Tables[0].Rows[0]["Date"];
            DataRow newRow = retour.Ds.Tables[0].NewRow();

            // on recopie la 1ère ligne
            foreach (string colonne in d.Columns)
            {
                newRow[colonne] = d.Ds.Tables[0].Rows[0][colonne];
            }
            newRow["Symbol"] = d.Ds.Tables[0].Rows[0]["Symbol"];
            newRow["Date"] = d.Ds.Tables[0].Rows[0]["Date"];
            retour.Ds.Tables[0].Rows.Add(newRow);
            DateTime date2;
            for (int ligne = 1; ligne < nb_ligne; ligne++)
            {
                date2 = (DateTime)d.Ds.Tables[0].Rows[ligne]["Date"];
                if ((d.Symbol.Count == 1) && (!((string)d.Symbol[0]).ToUpper().Equals("EONIA")))
                {
                    #region Si un seul taux
                    if ((GestionSimulation.AddBusinessDays(date1, 2) < date2)/*&&(ligne<d.Ds.Tables[0].Rows.Count-1)*/)
                    {
                        // Initialisation des simulations utiles **********************************************
                        List<GestionSimulation.evenement> liste1W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste1M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste3M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste6M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste9M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste12M = new List<GestionSimulation.evenement>();       // 


                        //On copie l'evenement precedent dans la liste
                        GestionSimulation.evenement event1w_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["1w"], date1);
                        GestionSimulation.evenement event2w_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["2w"], date1);
                        GestionSimulation.evenement event1m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["1m"], date1);
                        GestionSimulation.evenement event2m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["2m"], date1);
                        GestionSimulation.evenement event3m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["3m"], date1);
                        GestionSimulation.evenement event6m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["6m"], date1);
                        GestionSimulation.evenement event9m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["9m"], date1);
                        GestionSimulation.evenement event12m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["12m"], date1);
                        //On copie l'evenement suivant dans la liste
                        GestionSimulation.evenement event1w_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["1w"], date2);
                        GestionSimulation.evenement event2w_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["2w"], date2);
                        GestionSimulation.evenement event1m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["1m"], date2);
                        GestionSimulation.evenement event2m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["2m"], date2);
                        GestionSimulation.evenement event3m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["3m"], date2);
                        GestionSimulation.evenement event6m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["6m"], date2);
                        GestionSimulation.evenement event9m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["9m"], date2);
                        GestionSimulation.evenement event12m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["12m"], date2);

                        //On applique notre interpolation
                        liste1W = GestionSimulation.interpolationLineraire(event1w_1, event1w_2);
                        liste2W = GestionSimulation.interpolationLineraire(event2w_1, event2w_2);
                        liste1M = GestionSimulation.interpolationLineraire(event1m_1, event1m_2);
                        liste2M = GestionSimulation.interpolationLineraire(event2m_1, event2m_2);
                        liste3M = GestionSimulation.interpolationLineraire(event3m_1, event3m_2);
                        liste6M = GestionSimulation.interpolationLineraire(event6m_1, event6m_2);
                        liste9M = GestionSimulation.interpolationLineraire(event9m_1, event9m_2);
                        liste12M = GestionSimulation.interpolationLineraire(event12m_1, event12m_2);

                        // On doit maintenant ajouter autant de row qu'il y a de date.
                        int nb_date = Math.Max(liste1W.Count, Math.Max(liste2W.Count,                          //
                                      Math.Max(liste1M.Count, Math.Max(liste2M.Count,                          //
                                      Math.Max(liste3M.Count, Math.Max(liste6M.Count,                          //
                                      Math.Max(liste9M.Count, liste12M.Count)))))));                           // 

                        for (int i = 0; i < nb_date; i++)
                        {
                            // on crée la ligne
                            newRow = retour.Ds.Tables[0].NewRow();
                            DateTime dateCour = date1;
                            // on la complète
                            foreach (string colonne in d.Columns)
                            {
                                dateCour = liste1M[i].getDate();

                                if (colonne.Equals("1w"))
                                {
                                    newRow["1w"] = liste1W[i].getCours();
                                }
                                else if (colonne.Equals("2w"))
                                {
                                    newRow["2w"] = liste2W[i].getCours();
                                }
                                else if (colonne.Equals("1m"))
                                {
                                    newRow["1m"] = liste1M[i].getCours();
                                }
                                else if (colonne.Equals("2m"))
                                {
                                    newRow["2m"] = liste2M[i].getCours();
                                }
                                else if (colonne.Equals("3m"))
                                {
                                    newRow["3m"] = liste3M[i].getCours();
                                }
                                else if (colonne.Equals("6m"))
                                {
                                    newRow["6m"] = liste6M[i].getCours();
                                }
                                else if (colonne.Equals("9m"))
                                {
                                    newRow["9m"] = liste9M[i].getCours();
                                }
                                else if (colonne.Equals("12m"))
                                {
                                    newRow["12m"] = liste12M[i].getCours();
                                }
                            }
                            //on ajoute la ligne
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = dateCour;
                            retour.Ds.Tables[0].Rows.Add(newRow);
                        }
                        // on recopie les valeurs de date2
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);

                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                            ": Simulation de " +
                            date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        // sinon
                        // on recopie les valeurs
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                            {
                                newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                            }
                            else
                            {
                                if ((ligne != nb_ligne - 1) && (d.Ds.Tables[0].Rows[ligne + 1].Table.Columns.Contains(colonne)))
                                {
                                    double valeur = ((double)d.Ds.Tables[0].Rows[ligne - 1][colonne]) + ((double)d.Ds.Tables[0].Rows[ligne + 1][colonne]);
                                    newRow[colonne] = valeur / 2;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).ToString("dd/MM/yyyy"));
                                }
                                else
                                {
                                    newRow[colonne] = retour.Ds.Tables[0].Rows[retour.Ds.Tables[0].Rows.Count][colonne];
                                }
                            }
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);
                    }
                    #endregion
                }
                else
                {
                    #region Si liste de taux
                    DateTime lendemain = GestionSimulation.AddBusinessDays(date1, 1);
                    if (((lendemain < date2)&&(pasDeValeur(d,lendemain)))
                        &&(!(((List<string>)d.Symbol).Contains("EONIA"))||((List<string>)d.Symbol).Contains("eonia")))
                    {
                        // Initialisation des simulations utiles **********************************************
                        List<GestionSimulation.evenement> liste1W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste1M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste3M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste6M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste9M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste12M = new List<GestionSimulation.evenement>();       // 


                        //On copie l'evenement precedent dans la liste
                        GestionSimulation.evenement event1w_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["1w"], date1);
                        GestionSimulation.evenement event2w_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["2w"], date1);
                        GestionSimulation.evenement event1m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["1m"], date1);
                        GestionSimulation.evenement event2m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["2m"], date1);
                        GestionSimulation.evenement event3m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["3m"], date1);
                        GestionSimulation.evenement event6m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["6m"], date1);
                        GestionSimulation.evenement event9m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["9m"], date1);
                        GestionSimulation.evenement event12m_1 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne - 1]["12m"], date1);
                        //On copie l'evenement suivant dans la liste
                        GestionSimulation.evenement event1w_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["1w"], date2);
                        GestionSimulation.evenement event2w_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["2w"], date2);
                        GestionSimulation.evenement event1m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["1m"], date2);
                        GestionSimulation.evenement event2m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["2m"], date2);
                        GestionSimulation.evenement event3m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["3m"], date2);
                        GestionSimulation.evenement event6m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["6m"], date2);
                        GestionSimulation.evenement event9m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["9m"], date2);
                        GestionSimulation.evenement event12m_2 = new GestionSimulation.evenement((double)d.Ds.Tables[0].Rows[ligne]["12m"], date2);

                        //On applique notre interpolation
                        liste1W = GestionSimulation.interpolationLineraire(event1w_1, event1w_2);
                        liste2W = GestionSimulation.interpolationLineraire(event2w_1, event2w_2);
                        liste1M = GestionSimulation.interpolationLineraire(event1m_1, event1m_2);
                        liste2M = GestionSimulation.interpolationLineraire(event2m_1, event2m_2);
                        liste3M = GestionSimulation.interpolationLineraire(event3m_1, event3m_2);
                        liste6M = GestionSimulation.interpolationLineraire(event6m_1, event6m_2);
                        liste9M = GestionSimulation.interpolationLineraire(event9m_1, event9m_2);
                        liste12M = GestionSimulation.interpolationLineraire(event12m_1, event12m_2);

                        // On doit maintenant ajouter autant de row qu'il y a de date.
                        int nb_date = Math.Max(liste1W.Count, Math.Max(liste2W.Count,                          //
                                      Math.Max(liste1M.Count, Math.Max(liste2M.Count,                          //
                                      Math.Max(liste3M.Count, Math.Max(liste6M.Count,                          //
                                      Math.Max(liste9M.Count, liste12M.Count)))))));                           // 

                        for (int i = 0; i < nb_date; i++)
                        {
                            // on crée la ligne
                            newRow = retour.Ds.Tables[0].NewRow();
                            DateTime dateCour = date1;
                            // on la complète
                            foreach (string colonne in d.Columns)
                            {
                                dateCour = liste1M[i].getDate();

                                if (colonne.Equals("1w"))
                                {
                                    newRow["1w"] = liste1W[i].getCours();
                                }
                                else if (colonne.Equals("2w"))
                                {
                                    newRow["2w"] = liste2W[i].getCours();
                                }
                                else if (colonne.Equals("1m"))
                                {
                                    newRow["1m"] = liste1M[i].getCours();
                                }
                                else if (colonne.Equals("2m"))
                                {
                                    newRow["2m"] = liste2M[i].getCours();
                                }
                                else if (colonne.Equals("3m"))
                                {
                                    newRow["3m"] = liste3M[i].getCours();
                                }
                                else if (colonne.Equals("6m"))
                                {
                                    newRow["6m"] = liste6M[i].getCours();
                                }
                                else if (colonne.Equals("9m"))
                                {
                                    newRow["9m"] = liste9M[i].getCours();
                                }
                                else if (colonne.Equals("12m"))
                                {
                                    newRow["12m"] = liste12M[i].getCours();
                                }
                            }
                            //on ajoute la ligne
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = dateCour;
                            retour.Ds.Tables[0].Rows.Add(newRow);
                        }
                        // on recopie les valeurs de date2
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);

                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                            ": Simulation de " +
                            date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        // sinon
                        // on recopie les valeurs
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            if (d.Ds.Tables[0].Rows[ligne].Table.Columns.Contains(colonne))
                            {
                                newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                            }
                            else
                            {
                                if ((ligne != nb_ligne - 1) && (d.Ds.Tables[0].Rows[ligne + 1].Table.Columns.Contains(colonne)))
                                {
                                    double valeur = ((double)d.Ds.Tables[0].Rows[ligne - 1][colonne]) + ((double)d.Ds.Tables[0].Rows[ligne + 1][colonne]);
                                    newRow[colonne] = valeur / 2;

                                    dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                                        ": Simulation de " +
                                        colonne + " à " + ((DateTime)d.Ds.Tables[0].Rows[ligne]["Date"]).ToString("dd/MM/yyyy"));
                                }
                                else
                                {
                                    newRow[colonne] = retour.Ds.Tables[0].Rows[retour.Ds.Tables[0].Rows.Count][colonne];
                                }
                            }
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);
                    }
                    #endregion
                }
                date1 = date2;
            }
            return retour;
        }

        /// <summary>
        /// ATTENTION les dates sont croissantes dans les DataInterest
        /// Gere les possibles erreurs pour les taux de change : manque de valeurs, manque de donnees en entree
        /// </summary>
        /// <param name="d">Taux d'interet au format DataInterest avec des donnees eventuellement erronnées</param>
        /// <returns>Taux de change au format DataExchangeRate avec des donnees complète</returns>
        public static DataInterestRate interestErreur_bis(DataInterestRate d, DataRetour dretour)
        {
            //On copie les donnees d dans une var locale
            DataInterestRate retour = new DataInterestRate(d, d.Start, d.End);

            // on lit les données d
            int nb_ligne = d.Ds.Tables[0].Rows.Count;
            // on parcours colonne par colonne
            DateTime date1 = (DateTime)d.Ds.Tables[0].Rows[0]["Date"];
            DataRow newRow = retour.Ds.Tables[0].NewRow();

            foreach (string colonne in d.Columns)
            {
                newRow[colonne] = d.Ds.Tables[0].Rows[0][colonne];
            }
            newRow["Symbol"] = d.Ds.Tables[0].Rows[0]["Symbol"];
            newRow["Date"] = d.Ds.Tables[0].Rows[0]["Date"];
            retour.Ds.Tables[0].Rows.Add(newRow);
            DateTime date2;
            for (int ligne = 1; ligne < nb_ligne; ligne++)
            {
                date2 = (DateTime)d.Ds.Tables[0].Rows[ligne]["Date"];
                if (d.Symbol.Count == 1)
                {
                    #region Si un seul taux
                    if ((GestionSimulation.AddBusinessDays(date1, 2) < date2)/*&&(ligne<d.Ds.Tables[0].Rows.Count-1)*/)
                    {
                        double volatilite = 0;
                        // Initialisation des simulations utiles **********************************************
                        List<GestionSimulation.evenement> liste1W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste1M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste3M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste6M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste9M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste12M = new List<GestionSimulation.evenement>();       // 

                        //On effectue la simulation pour chacune des colonnes. 

                        //On selectionne la plage de donnee sur laquelle on calculera la variance             //
                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date2, date1, ligne - 1);  //peu etre ligne-1       //

                        //Pour 1 semaine
                        double W1_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["1w"];                               //
                        double W1_2 = (double)d.Ds.Tables[0].Rows[ligne]["1w"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "1w");                                  //
                        liste1W = GestionSimulation.pontBrownien(date1, date2, W1_1, W1_2, volatilite);          //
                        //Pour 2 semaine
                        double W2_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["2w"];                               //
                        double W2_2 = (double)d.Ds.Tables[0].Rows[ligne]["2w"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "2w");                                  //
                        liste2W = GestionSimulation.pontBrownien(date1, date2, W2_1, W2_2, volatilite);          //
                        //Pour 1 mois
                        double M1_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["1m"];                               //
                        double M1_2 = (double)d.Ds.Tables[0].Rows[ligne]["1m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "1m");                                  //
                        liste1M = GestionSimulation.pontBrownien(date1, date2, M1_1, M1_2, volatilite);          //
                        //Pour 2 mois
                        double M2_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["2m"];                               //
                        double M2_2 = (double)d.Ds.Tables[0].Rows[ligne]["2m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "2m");                                  //
                        liste2M = GestionSimulation.pontBrownien(date1, date2, M2_1, M2_2, volatilite);          //
                        //Pour 3 mois
                        double M3_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["3m"];                               //
                        double M3_2 = (double)d.Ds.Tables[0].Rows[ligne]["3m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "3m");                                  //
                        liste3M = GestionSimulation.pontBrownien(date1, date2, M3_1, M3_2, volatilite);          //
                        //Pour 6 mois
                        double M6_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["6m"];                               //
                        double M6_2 = (double)d.Ds.Tables[0].Rows[ligne]["6m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "6m");                                  //
                        liste6M = GestionSimulation.pontBrownien(date1, date2, M6_1, M6_2, volatilite);          //
                        //Pour 9 mois
                        double M9_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["9m"];                               //
                        double M9_2 = (double)d.Ds.Tables[0].Rows[ligne]["9m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "9m");                                  //
                        liste9M = GestionSimulation.pontBrownien(date1, date2, M9_1, M9_2, volatilite);          //
                        //Pour 12 mois
                        double M12_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["12m"];                             //
                        double M12_2 = (double)d.Ds.Tables[0].Rows[ligne]["12m"];                         //
                        volatilite = GestionVolatilite.volatilite(dv, "12m");                                 //
                        liste12M = GestionSimulation.pontBrownien(date1, date2, M12_1, M12_2, volatilite);       //

                        // On doit maintenant ajouter autant de row qu'il y a de date.
                        int nb_date = Math.Max(liste1W.Count, Math.Max(liste2W.Count,                         //
                                      Math.Max(liste1M.Count, Math.Max(liste2M.Count,                          //
                                      Math.Max(liste3M.Count, Math.Max(liste6M.Count,                          //
                                      Math.Max(liste9M.Count, liste12M.Count)))))));                           // 

                        for (int i = 0; i < nb_date; i++)
                        {
                            // on crée la ligne
                            newRow = retour.Ds.Tables[0].NewRow();
                            DateTime dateCour = date1;
                            // on la complète
                            foreach (string colonne in d.Columns)
                            {
                                dateCour = liste1W[i].getDate();

                                if (colonne.Equals("1w"))
                                {
                                    newRow["1w"] = liste1W[i].getCours();
                                }
                                else if (colonne.Equals("2w"))
                                {
                                    newRow["2w"] = liste2W[i].getCours();
                                }
                                else if (colonne.Equals("1m"))
                                {
                                    newRow["1m"] = liste1M[i].getCours();
                                }
                                else if (colonne.Equals("2m"))
                                {
                                    newRow["2m"] = liste2M[i].getCours();
                                }
                                else if (colonne.Equals("3m"))
                                {
                                    newRow["3m"] = liste3M[i].getCours();
                                }
                                else if (colonne.Equals("6m"))
                                {
                                    newRow["6m"] = liste6M[i].getCours();
                                }
                                else if (colonne.Equals("9m"))
                                {
                                    newRow["9m"] = liste9M[i].getCours();
                                }
                                else if (colonne.Equals("12m"))
                                {
                                    newRow["12m"] = liste12M[i].getCours();
                                }
                            }
                            //on ajoute la ligne
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = dateCour;
                            retour.Ds.Tables[0].Rows.Add(newRow);
                        }
                        // on recopie les valeurs de date2
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);

                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                            ": Simulation de " +
                            date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        // sinon
                        // on recopie les valeurs
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);
                    }
                    #endregion
                }
                else
                {
                    #region Si une liste de taux
                    DateTime lendemain =GestionSimulation.AddBusinessDays(date1, 1) ;
                    if ((lendemain< date2)&&(pasDeValeur(d,lendemain)))
                    {
                        double volatilite = 0;
                        // Initialisation des simulations utiles **********************************************
                        List<GestionSimulation.evenement> liste1W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2W = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste1M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste2M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste3M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste6M = new List<GestionSimulation.evenement>();        // 
                        List<GestionSimulation.evenement> liste9M = new List<GestionSimulation.evenement>();        //
                        List<GestionSimulation.evenement> liste12M = new List<GestionSimulation.evenement>();       // 

                        //On effectue la simulation pour chacune des colonnes. 

                        //On selectionne la plage de donnee sur laquelle on calculera la variance             //
                        Data.Data dv = GestionVolatilite.donneesVolatilite(d, date2, date1, ligne - 1);  //peu etre ligne-1       //

                        //Pour 1 semaine
                        double W1_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["1w"];                               //
                        double W1_2 = (double)d.Ds.Tables[0].Rows[ligne]["1w"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "1w");                                  //
                        liste1W = GestionSimulation.pontBrownien(date1, date2, W1_1, W1_2, volatilite);          //
                        //Pour 2 semaine
                        double W2_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["2w"];                               //
                        double W2_2 = (double)d.Ds.Tables[0].Rows[ligne]["2w"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "2w");                                  //
                        liste2W = GestionSimulation.pontBrownien(date1, date2, W2_1, W2_2, volatilite);          //
                        //Pour 1 mois
                        double M1_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["1m"];                               //
                        double M1_2 = (double)d.Ds.Tables[0].Rows[ligne]["1m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "1m");                                  //
                        liste1M = GestionSimulation.pontBrownien(date1, date2, M1_1, M1_2, volatilite);          //
                        //Pour 2 mois
                        double M2_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["2m"];                               //
                        double M2_2 = (double)d.Ds.Tables[0].Rows[ligne]["2m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "2m");                                  //
                        liste2M = GestionSimulation.pontBrownien(date1, date2, M2_1, M2_2, volatilite);          //
                        //Pour 3 mois
                        double M3_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["3m"];                               //
                        double M3_2 = (double)d.Ds.Tables[0].Rows[ligne]["3m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "3m");                                  //
                        liste3M = GestionSimulation.pontBrownien(date1, date2, M3_1, M3_2, volatilite);          //
                        //Pour 6 mois
                        double M6_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["6m"];                               //
                        double M6_2 = (double)d.Ds.Tables[0].Rows[ligne]["6m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "6m");                                  //
                        liste6M = GestionSimulation.pontBrownien(date1, date2, M6_1, M6_2, volatilite);          //
                        //Pour 9 mois
                        double M9_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["9m"];                               //
                        double M9_2 = (double)d.Ds.Tables[0].Rows[ligne]["9m"];                           //
                        volatilite = GestionVolatilite.volatilite(dv, "9m");                                  //
                        liste9M = GestionSimulation.pontBrownien(date1, date2, M9_1, M9_2, volatilite);          //
                        //Pour 12 mois
                        double M12_1 = (double)d.Ds.Tables[0].Rows[ligne - 1]["12m"];                             //
                        double M12_2 = (double)d.Ds.Tables[0].Rows[ligne]["12m"];                         //
                        volatilite = GestionVolatilite.volatilite(dv, "12m");                                 //
                        liste12M = GestionSimulation.pontBrownien(date1, date2, M12_1, M12_2, volatilite);       //

                        // On doit maintenant ajouter autant de row qu'il y a de date.
                        int nb_date = Math.Max(liste1W.Count, Math.Max(liste2W.Count,                         //
                                      Math.Max(liste1M.Count, Math.Max(liste2M.Count,                          //
                                      Math.Max(liste3M.Count, Math.Max(liste6M.Count,                          //
                                      Math.Max(liste9M.Count, liste12M.Count)))))));                           // 

                        for (int i = 0; i < nb_date; i++)
                        {
                            // on crée la ligne
                            newRow = retour.Ds.Tables[0].NewRow();
                            DateTime dateCour = date1;
                            // on la complète
                            foreach (string colonne in d.Columns)
                            {
                                dateCour = liste1W[i].getDate();

                                if (colonne.Equals("1w"))
                                {
                                    newRow["1w"] = liste1W[i].getCours();
                                }
                                else if (colonne.Equals("2w"))
                                {
                                    newRow["2w"] = liste2W[i].getCours();
                                }
                                else if (colonne.Equals("1m"))
                                {
                                    newRow["1m"] = liste1M[i].getCours();
                                }
                                else if (colonne.Equals("2m"))
                                {
                                    newRow["2m"] = liste2M[i].getCours();
                                }
                                else if (colonne.Equals("3m"))
                                {
                                    newRow["3m"] = liste3M[i].getCours();
                                }
                                else if (colonne.Equals("6m"))
                                {
                                    newRow["6m"] = liste6M[i].getCours();
                                }
                                else if (colonne.Equals("9m"))
                                {
                                    newRow["9m"] = liste9M[i].getCours();
                                }
                                else if (colonne.Equals("12m"))
                                {
                                    newRow["12m"] = liste12M[i].getCours();
                                }
                            }
                            //on ajoute la ligne
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = dateCour;
                            retour.Ds.Tables[0].Rows.Add(newRow);
                        }
                        // on recopie les valeurs de date2
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);

                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                            ": Simulation de " +
                            date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        // sinon
                        // on recopie les valeurs
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);
                    }
                    #endregion
                }
                date1 = date2;
            }
            return retour;
        }
        #endregion

        #region Actif
        /// <summary>
        /// Gere les possibles erreurs pour les actifs : manque de valeurs, donnée(s) erroné(s) en entrée.
        /// </summary>
        /// <param name="d">Actifs au format DataActif avec des donnees eventuellement erronnées</param>
        /// <returns>Actifs au format DataActif avec des donnees complète</returns>
        public static Data.Data actifErreur(DataActif d, DataRetour dretour)
        {
            DataActif retour = new DataActif(d.Symbol, d.Columns, d.Start, d.End);
            // on lit les données d
            int nb_ligne = d.Ds.Tables[0].Rows.Count;
            DateTime date1 = (DateTime)d.Ds.Tables[0].Rows[0]["Date"];
            DataRow newRow = retour.Ds.Tables[0].NewRow();
            // permete de voir si on a déjà simulé l'un ou l'autre
            bool ouverture = false;

            // on recopie la première ligne
            foreach (string colonne in d.Columns)
            {
                newRow[colonne] = d.Ds.Tables[0].Rows[0][colonne];
            }
            newRow["Symbol"] = d.Ds.Tables[0].Rows[0]["Symbol"];
            newRow["Date"] = d.Ds.Tables[0].Rows[0]["Date"];
            retour.Ds.Tables[0].Rows.Add(newRow);
            DateTime date2;

            // puis on parcours toutes les autres lignes
            // en se souvenant de la date précédante.
            for (int ligne = 1; ligne < nb_ligne; ligne++)
            {
                date2 = (DateTime)d.Ds.Tables[0].Rows[ligne]["Date"];
                bool onlylow = false;
                bool onlyhigh = false;
                if (d.Symbol.Count == 1)
                {
                    #region si un seul titre
                    // si absence de données sur plus de 1 jour ouvré 
                    // date1 > date2 car dates décroissantes
                    if ((GestionSimulation.AddBusinessDays(date2, 2) < date1))
                    {
                        double volatilite = 0;
                        // Pour les simulations, on veut avoir une simulation cohérente
                        /*
                         * La simulation de low et high dépends de celle de open et close
                         */
                        // Initialisation des simulations utiles ***********************************************
                        List<GestionSimulation.evenement> listeOpenClose = new List<GestionSimulation.evenement>(); //
                        List<GestionSimulation.evenement> listeHigh = new List<GestionSimulation.evenement>();      //
                        List<GestionSimulation.evenement> listeLow = new List<GestionSimulation.evenement>();       //
                        List<GestionSimulation.evenement> listeVolume = new List<GestionSimulation.evenement>();    // 
                        // On effectue une simulation                                                         //
                        if (d.Columns.Contains("Open"))                                                       //
                        {                                                                                     //
                            double open1 = (double)d.Ds.Tables[0].Rows[ligne]["Open"];                        //
                            double open2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Open"];                    //
                            Data.Data dv = GestionVolatilite.donneesVolatilite(d, date2, date1, ligne);                         //
                            volatilite = GestionVolatilite.volatilite(dv, "Open");                             //
                            listeOpenClose = GestionSimulation.pontBrownien(date2, date1, open1, open2, volatilite); //
                            ouverture = true;                                                                 //
                        }                                                                                     //
                        else if (d.Columns.Contains("Close"))                                                 //
                        {                                                                                     //
                            if (!ouverture)                                                                   //
                            {                                                                                 //
                                double open1 = (double)d.Ds.Tables[0].Rows[ligne]["Close"];                   //
                                double open2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Close"];               //
                                volatilite = GestionVolatilite.volatilite(d, "Close");                        //
                                listeOpenClose = GestionSimulation.pontBrownien(date2, date1, open1,               //
                                    open2, volatilite);                                                        //
                                ouverture = true;                                                             //
                            }                                                                                 //
                        }                                                                                     //
                        if (d.Columns.Contains("High"))                                                       //
                        {                                                                                     //
                            if (ouverture)                                                                    //
                            {                                                                                 //
                                double high1 = (double)d.Ds.Tables[0].Rows[ligne]["High"];                    //
                                double high2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["High"];                //
                                listeHigh = GestionSimulation.simuleHigh(listeOpenClose, high1, high2);          //
                            }                                                                                 //
                            else                                                                              //
                            {                                                                                 //
                                double high1 = (double)d.Ds.Tables[0].Rows[ligne]["High"];                    //
                                double high2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["High"];                //
                                volatilite = GestionVolatilite.volatilite(d, "High");                         //
                                onlyhigh = true;                                                              //
                                listeHigh = GestionSimulation.pontBrownien(date2, date1, high1,                  //
                                    high2, volatilite);                                                        //
                            }                                                                                 //
                        }                                                                                     //
                        if (d.Columns.Contains("Low"))                                                        //
                        {                                                                                     //
                            if (ouverture)                                                                    //
                            {                                                                                 //
                                double low1 = (double)d.Ds.Tables[0].Rows[ligne]["Low"];                      //
                                double low2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Low"];                  //
                                listeLow = GestionSimulation.simuleLow(listeOpenClose, low1, low2);              //
                            }                                                                                 //
                            else                                                                              //
                            {                                                                                 //
                                double low1 = (double)d.Ds.Tables[0].Rows[ligne]["Low"];                      //
                                double low2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Low"];                  //
                                onlylow = true;                                                               //
                                volatilite = GestionVolatilite.volatilite(d, "Low");                          //
                                listeLow = GestionSimulation.pontBrownien(date2, date1, low1, low2, volatilite); //
                            }                                                                                 //
                        }                                                                                     //
                        if (d.Ds.Tables[0].Columns.Contains("Volume"))                                        //
                        {                                                                                     //
                            double vol1 = (double)d.Ds.Tables[0].Rows[ligne]["Volume"];                       //
                            double vol2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Volume"];                   //
                            volatilite = GestionVolatilite.volatilite(d, "Volume");                           //
                            listeVolume = GestionSimulation.pontBrownien(date2, date1, vol1, vol2, volatilite);   //
                        }                                                                                     //
                        // fin init simulation open, close, high, low, volume **********************************

                        // on réordonne high et low dans le cas où on les a simulé aléatoirement (sans open et close)
                        if (onlylow && onlyhigh)
                        {
                            for (int i = 0; i < listeLow.Count; i++)
                            {
                                double lw = listeLow[i].getCours();
                                double hg = listeHigh[i].getCours();
                                if (hg < lw)
                                {
                                    listeLow[i].setCours(hg);
                                    listeHigh[i].setCours(lw);
                                }
                            }
                        }


                        // On doit maintenant ajouter autant de row qu'il y a de date.
                        int nb_date = Math.Max(listeOpenClose.Count, listeHigh.Count);
                        nb_date = Math.Max(nb_date, listeLow.Count);
                        nb_date = Math.Max(nb_date, listeVolume.Count);

                        for (int i = nb_date - 1; i >= 0; i--)
                        {
                            // on crée la ligne
                            newRow = retour.Ds.Tables[0].NewRow();
                            DateTime dateCour = date1;
                            // on la complète
                            foreach (string colonne in d.Columns)
                            {
                                if (colonne.Equals("Open"))
                                {
                                    newRow["Open"] = listeOpenClose[i].getCours();
                                    dateCour = listeOpenClose[i].getDate();
                                }
                                else if (colonne.Equals("Close"))
                                {
                                    // si on a déjà simulé l'ouverture
                                    if (ouverture)
                                    {
                                        // on translate les valeurs
                                        // le cours de fermeture d'aujourd'hui est celui d'ouverture de demain.
                                        if (i + 1 == nb_date)
                                        {
                                            newRow["Close"] = d.Ds.Tables[0].Rows[ligne - 1]["Open"];
                                            dateCour = GestionSimulation.AddBusinessDays(date1, -1);
                                        }
                                        else
                                        {
                                            newRow["Close"] = listeOpenClose[i + 1].getCours();
                                            dateCour = listeOpenClose[i].getDate();
                                        }
                                    }
                                    // sinon on a simulé la fermeture
                                    else
                                    {
                                        newRow["Close"] = listeOpenClose[i].getCours();
                                        dateCour = listeOpenClose[i].getDate();
                                    }
                                }
                                else if (colonne.Equals("Low"))
                                {
                                    newRow["Low"] = listeLow[i].getCours();
                                    dateCour = listeLow[i].getDate();
                                }
                                else if (colonne.Equals("High"))
                                {
                                    newRow["High"] = listeHigh[i].getCours();
                                    dateCour = listeHigh[i].getDate();
                                }
                                else if (colonne.Equals("Volume"))
                                {
                                    newRow["Volume"] = (int)listeVolume[i].getCours();
                                    dateCour = listeVolume[i].getDate();
                                }
                            }
                            //on ajoute la ligne
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = dateCour;
                            retour.Ds.Tables[0].Rows.Add(newRow);
                        }

                        // on recopie les valeurs de date2
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);

                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                            ": Simulation de " +
                            date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {

                        // sinon
                        // on recopie les valeurs
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);
                    }
                    #endregion
                }
                else
                {
                    #region si liste de titres 
                    // le test est basé uniquement sur le fait qu'il existe un valeur dans les autres titres
                    // si absence de données sur plus de 1 jour ouvré 
                    // date1 > date2 car dates décroissantes
                    DateTime lendemain = GestionSimulation.AddBusinessDays(date2, 1);
                    if ((lendemain < date1)&&(pasDeValeur(d, lendemain)))
                    {
                        double volatilite = 0;
                        // Pour les simulations, on veut avoir une simulation cohérente
                        /*
                         * La simulation de low et high dépends de celle de open et close
                         */
                        // Initialisation des simulations utiles ***********************************************
                        List<GestionSimulation.evenement> listeOpenClose = new List<GestionSimulation.evenement>(); //
                        List<GestionSimulation.evenement> listeHigh = new List<GestionSimulation.evenement>();      //
                        List<GestionSimulation.evenement> listeLow = new List<GestionSimulation.evenement>();       //
                        List<GestionSimulation.evenement> listeVolume = new List<GestionSimulation.evenement>();    // 
                        // On effectue une simulation                                                         //
                        if (d.Columns.Contains("Open"))                                                       //
                        {                                                                                     //
                            double open1 = (double)d.Ds.Tables[0].Rows[ligne]["Open"];                        //
                            double open2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Open"];                    //
                            Data.Data dv = GestionVolatilite.donneesVolatilite(d, date2, date1, ligne);                         //
                            volatilite = GestionVolatilite.volatilite(dv, "Open");                             //
                            listeOpenClose = GestionSimulation.pontBrownien(date2, date1, open1, open2, volatilite); //
                            ouverture = true;                                                                 //
                        }                                                                                     //
                        else if (d.Columns.Contains("Close"))                                                 //
                        {                                                                                     //
                            if (!ouverture)                                                                   //
                            {                                                                                 //
                                double open1 = (double)d.Ds.Tables[0].Rows[ligne]["Close"];                   //
                                double open2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Close"];               //
                                volatilite = GestionVolatilite.volatilite(d, "Close");                        //
                                listeOpenClose = GestionSimulation.pontBrownien(date2, date1, open1,               //
                                    open2, volatilite);                                                        //
                                ouverture = true;                                                             //
                            }                                                                                 //
                        }                                                                                     //
                        if (d.Columns.Contains("High"))                                                       //
                        {                                                                                     //
                            if (ouverture)                                                                    //
                            {                                                                                 //
                                double high1 = (double)d.Ds.Tables[0].Rows[ligne]["High"];                    //
                                double high2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["High"];                //
                                listeHigh = GestionSimulation.simuleHigh(listeOpenClose, high1, high2);          //
                            }                                                                                 //
                            else                                                                              //
                            {                                                                                 //
                                double high1 = (double)d.Ds.Tables[0].Rows[ligne]["High"];                    //
                                double high2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["High"];                //
                                volatilite = GestionVolatilite.volatilite(d, "High");                         //
                                onlyhigh = true;                                                              //
                                listeHigh = GestionSimulation.pontBrownien(date2, date1, high1,                  //
                                    high2, volatilite);                                                        //
                            }                                                                                 //
                        }                                                                                     //
                        if (d.Columns.Contains("Low"))                                                        //
                        {                                                                                     //
                            if (ouverture)                                                                    //
                            {                                                                                 //
                                double low1 = (double)d.Ds.Tables[0].Rows[ligne]["Low"];                      //
                                double low2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Low"];                  //
                                listeLow = GestionSimulation.simuleLow(listeOpenClose, low1, low2);              //
                            }                                                                                 //
                            else                                                                              //
                            {                                                                                 //
                                double low1 = (double)d.Ds.Tables[0].Rows[ligne]["Low"];                      //
                                double low2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Low"];                  //
                                onlylow = true;                                                               //
                                volatilite = GestionVolatilite.volatilite(d, "Low");                          //
                                listeLow = GestionSimulation.pontBrownien(date2, date1, low1, low2, volatilite); //
                            }                                                                                 //
                        }                                                                                     //
                        if (d.Ds.Tables[0].Columns.Contains("Volume"))                                        //
                        {                                                                                     //
                            double vol1 = (double)d.Ds.Tables[0].Rows[ligne]["Volume"];                       //
                            double vol2 = (double)d.Ds.Tables[0].Rows[ligne - 1]["Volume"];                   //
                            volatilite = GestionVolatilite.volatilite(d, "Volume");                           //
                            listeVolume = GestionSimulation.pontBrownien(date2, date1, vol1, vol2, volatilite);   //
                        }                                                                                     //
                        // fin init simulation open, close, high, low, volume **********************************

                        // on réordonne high et low dans le cas où on les a simulé aléatoirement (sans open et close)
                        if (onlylow && onlyhigh)
                        {
                            for (int i = 0; i < listeLow.Count; i++)
                            {
                                double lw = listeLow[i].getCours();
                                double hg = listeHigh[i].getCours();
                                if (hg < lw)
                                {
                                    listeLow[i].setCours(hg);
                                    listeHigh[i].setCours(lw);
                                }
                            }
                        }


                        // On doit maintenant ajouter autant de row qu'il y a de date.
                        int nb_date = Math.Max(listeOpenClose.Count, listeHigh.Count);
                        nb_date = Math.Max(nb_date, listeLow.Count);
                        nb_date = Math.Max(nb_date, listeVolume.Count);

                        for (int i = nb_date - 1; i >= 0; i--)
                        {
                            // on crée la ligne
                            newRow = retour.Ds.Tables[0].NewRow();
                            DateTime dateCour = date1;
                            // on la complète
                            foreach (string colonne in d.Columns)
                            {
                                if (colonne.Equals("Open"))
                                {
                                    newRow["Open"] = listeOpenClose[i].getCours();
                                    dateCour = listeOpenClose[i].getDate();
                                }
                                else if (colonne.Equals("Close"))
                                {
                                    // si on a déjà simulé l'ouverture
                                    if (ouverture)
                                    {
                                        // on translate les valeurs
                                        // le cours de fermeture d'aujourd'hui est celui d'ouverture de demain.
                                        if (i + 1 == nb_date)
                                        {
                                            newRow["Close"] = d.Ds.Tables[0].Rows[ligne - 1]["Open"];
                                            dateCour = GestionSimulation.AddBusinessDays(date1, -1);
                                        }
                                        else
                                        {
                                            newRow["Close"] = listeOpenClose[i + 1].getCours();
                                            dateCour = listeOpenClose[i].getDate();
                                        }
                                    }
                                    // sinon on a simulé la fermeture
                                    else
                                    {
                                        newRow["Close"] = listeOpenClose[i].getCours();
                                        dateCour = listeOpenClose[i].getDate();
                                    }
                                }
                                else if (colonne.Equals("Low"))
                                {
                                    newRow["Low"] = listeLow[i].getCours();
                                    dateCour = listeLow[i].getDate();
                                }
                                else if (colonne.Equals("High"))
                                {
                                    newRow["High"] = listeHigh[i].getCours();
                                    dateCour = listeHigh[i].getDate();
                                }
                                else if (colonne.Equals("Volume"))
                                {
                                    newRow["Volume"] = (int)listeVolume[i].getCours();
                                    dateCour = listeVolume[i].getDate();
                                }
                            }
                            //on ajoute la ligne
                            newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                            newRow["Date"] = dateCour;
                            retour.Ds.Tables[0].Rows.Add(newRow);
                        }

                        // on recopie les valeurs de date2
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);

                        dretour.GetWarning().Add(d.Ds.Tables[0].Rows[ligne]["Symbol"] +
                            ": Simulation de " +
                            date2.ToString("dd/MM/yyyy") + " à " + date1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {

                        // sinon
                        // on recopie les valeurs
                        newRow = retour.Ds.Tables[0].NewRow();
                        foreach (string colonne in d.Columns)
                        {
                            newRow[colonne] = d.Ds.Tables[0].Rows[ligne][colonne];
                        }
                        newRow["Symbol"] = d.Ds.Tables[0].Rows[ligne]["Symbol"];
                        newRow["Date"] = d.Ds.Tables[0].Rows[ligne]["Date"];
                        retour.Ds.Tables[0].Rows.Add(newRow);
                    }
                    #endregion
                }
                date1 = date2;
            }
            return retour;
        }
    }
        #endregion
}
