using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using WebAPI_final.Models.Data;
using WebAPI_final.Models.Service;
using WebAPI_final.Models.GestionErreurs;
using WebAPI_final.Models;
using System.Diagnostics;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private string toString(DataActif d)
        {
            return d.Ds.Tables[0].ToString();
        }

        #region Test Actif
        /// <summary>
        /// Si pas de modif 
        /// </summary>
        [TestMethod]
        public void TestOK()
        {
            List<Data.HistoricalColumn> columns = new List<Data.HistoricalColumn>();
            columns.Add(Data.HistoricalColumn.Open);
            columns.Add(Data.HistoricalColumn.High);
            columns.Add(Data.HistoricalColumn.Low);
            columns.Add(Data.HistoricalColumn.Close);
            columns.Add(Data.HistoricalColumn.Volume);



            List<String> l = new List<string>();
            l.Add("GOOG".ToUpper());


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            Data donnees = s.getActifHistorique(l, columns, debut, fin);
            DataRetour dretour = new DataRetour();
            Data d = GestionErreurs.actifErreur((DataActif)donnees,dretour);

            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                foreach (string c in d.Columns)
                {
                    Assert.AreEqual<double>((double)d.Ds.Tables[0].Rows[i][c], (double)donnees.Ds.Tables[0].Rows[i][c]);
                }
            }

        }

        /// <summary>
        /// Test s'il manque moins de deux valeurs
        /// 
        /// --> ne doit pas faire de simulation
        /// </summary>
        [TestMethod]
        public void TestValeurIsolee()
        {
            List<Data.HistoricalColumn> columns = new List<Data.HistoricalColumn>();
            columns.Add(Data.HistoricalColumn.Open);
            columns.Add(Data.HistoricalColumn.High);
            columns.Add(Data.HistoricalColumn.Low);
            columns.Add(Data.HistoricalColumn.Close);
            columns.Add(Data.HistoricalColumn.Volume);



            List<String> l = new List<string>();
            l.Add("GOOG".ToUpper());


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            Data donnees = s.getActifHistorique(l, columns, debut, fin);
            // on supprime une ligne
            // attention à ne pas prendre un vendredi ou lundi
            DataRow row = donnees.Ds.Tables[0].Rows[2];
            if (((DateTime)row["Date"]).DayOfWeek == DayOfWeek.Friday)
            {
                donnees.Ds.Tables[0].Rows.Remove(donnees.Ds.Tables[0].Rows[5]);
            }
            else if (((DateTime)row["Date"]).DayOfWeek == DayOfWeek.Monday)
            {
                donnees.Ds.Tables[0].Rows.Remove(donnees.Ds.Tables[0].Rows[3]);
            }
            else
            {
                donnees.Ds.Tables[0].Rows.Remove(row);
            }


            DataRetour dretour = new DataRetour();
            Data d = GestionErreurs.actifErreur((DataActif)donnees,dretour);

            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                foreach (string c in d.Columns)
                {
                    Assert.AreEqual<double>((double)d.Ds.Tables[0].Rows[i][c], (double)donnees.Ds.Tables[0].Rows[i][c]);
                    // Trace.Write((double)donnees.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "\n");
                }
            }
        }

        /// <summary>
        /// test s'il manque un certain nombre de valeurs
        /// avec un seul champs demandé (OPEN, CLOSE, ... )
        /// 
        /// --> doit simuler des valeurs
        /// </summary>
        [TestMethod]
        public void Test1requete()
        {
            List<Data.HistoricalColumn> columns = new List<Data.HistoricalColumn>();
            columns.Add(Data.HistoricalColumn.Open);

            List<String> l = new List<string>();
            l.Add("GOOG".ToUpper());


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            Data donnees = s.getActifHistorique(l, columns, debut, fin);
            // on supprime une ligne
            // attention à ne pas prendre un vendredi ou lundi
            for (int k = 0; k < 15; k++)
            {
                donnees.Ds.Tables[0].Rows.Remove(donnees.Ds.Tables[0].Rows[2]);
            }
            DataRetour dretour = new DataRetour();
            Data d = GestionErreurs.actifErreur((DataActif)donnees,dretour);
            Assert.AreNotEqual(d.Ds.Tables[0].Rows.Count, donnees.Ds.Tables[0].Rows.Count);
            for (int j = 0; j < donnees.Ds.Tables[0].Rows.Count; j++)
            {
                Trace.Write(((DateTime)donnees.Ds.Tables[0].Rows[j]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)donnees.Ds.Tables[0].Rows[j]["Open"] + "\n");
            }
            Trace.Write("new tab \n\n");
            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "\n");
            }
        }

        /// <summary>
        /// test s'il manque un certain nombre de valeurs
        /// en demandant plusieurs champs...
        /// 
        /// --> doit simuler des valeurs mais avec des valeurs cohérentes
        ///     cad ouvertureAujoud'hui = fermetureHier
        ///     et les valeur de high et low sont "cohérentes"
        /// </summary>
        [TestMethod]
        public void TestRequeteComplete()
        {
            List<Data.HistoricalColumn> columns = new List<Data.HistoricalColumn>();
            columns.Add(Data.HistoricalColumn.Open);
            columns.Add(Data.HistoricalColumn.High);
            columns.Add(Data.HistoricalColumn.Low);
            columns.Add(Data.HistoricalColumn.Close);
            columns.Add(Data.HistoricalColumn.Volume);

            List<String> l = new List<string>();
            l.Add("GOOG".ToUpper());


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            Data donnees = s.getActifHistorique(l, columns, debut, fin);
            // on supprime une ligne
            // attention à ne pas prendre un vendredi ou lundi
            for (int k = 0; k < 15; k++)
            {
                donnees.Ds.Tables[0].Rows.Remove(donnees.Ds.Tables[0].Rows[2]);
            }
            DataRetour dretour = new DataRetour();
            Data d = GestionErreurs.actifErreur((DataActif)donnees,dretour);
            Assert.AreEqual<double>((double)d.Ds.Tables[0].Rows[1]["Open"], (double)donnees.Ds.Tables[0].Rows[1]["Open"]);


            /////////////////////////////// A decommenter pour avoir les donnees de test en .csv \\\\\\\\\\\\\\\\\\\\\\\\\\
            /////////////////////////////// Penser à changer le path pour ecrire dans les fichiers \\\\\\\\\\\\\\\\\\\\\\\\
            /*
            // Pour extraire les données en format .csv
            string text = "Date;Open;Close;Low;High;Volume\n";
            string text2 = "Date;Open;Close;Low;High;Volume\n";
            //Trace.Write("  date        open         close        low         high     volume\n");
            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                text += ((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["Open"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["Close"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["Low"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["High"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["Volume"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            for (int i = 0; i < donnees.Ds.Tables[0].Rows.Count; i++)
            {
                text2 += ((DateTime)donnees.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["Open"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["Close"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["Low"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["High"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["Volume"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            // On écrit dans le fichier
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\SimuleValeurComplet_Actif.csv", text);
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\Temoin_Actif.csv", text2);
             * */
        }

        /// <summary>
        /// Dans le cas où un des titres n'existe pas
        /// </summary>
        [TestMethod]
        public void TestMauvaiseEntree()
        {
            List<Data.HistoricalColumn> columns = new List<Data.HistoricalColumn>();
            columns.Add(Data.HistoricalColumn.Open);
            columns.Add(Data.HistoricalColumn.High);
            columns.Add(Data.HistoricalColumn.Low);
            columns.Add(Data.HistoricalColumn.Close);
            columns.Add(Data.HistoricalColumn.Volume);



            List<String> l = new List<string>();
            l.Add("GOOG".ToUpper());
            //mauvaise entrée
            l.Add("lkdfsopf".ToUpper());
            l.Add("CA.PA".ToUpper());


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            DataRetour dretour = new DataRetour();
            Data donnees = s.getActifHistorique(l, columns, debut, fin, dretour);
            Data d = GestionErreurs.actifErreur((DataActif)donnees,dretour);
            dretour.SetData(d);
            Assert.AreEqual<string>(dretour.GetListeErreur()[0],"lkdfsopf".ToUpper());
        }
        #endregion

        #region Test Taux Interet
        [TestMethod]
        public void TestRequeteComplete_Interest_bis()
        {
            Data.InterestRate taux = Data.InterestRate.EURIBOR;


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            DataRetour dretour = new DataRetour();
            Data donnees = s.getInterestRate(taux, debut, fin, dretour);
            // Pour extraire les données en format .csv
            string text3 = "Date;1w;2w;1m;2m;3m;6m;9m;12m;\n";


            //Trace.Write("  date        1w         2w        1m         2m         3m         6m        9m     12m \n");
            for (int i = 0; i < donnees.Ds.Tables[0].Rows.Count; i++)
            {
                text3 += ((DateTime)donnees.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["3m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["6m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["9m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["12m"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            // on supprime une ligne
            // attention à ne pas prendre un vendredi ou lundi
            for (int k = 0; k < 15; k++)
            {
                donnees.Ds.Tables[0].Rows.Remove(donnees.Ds.Tables[0].Rows[15]);
            }
            Data d = GestionErreurs.interestErreur_bis((DataInterestRate)donnees,dretour);
            Assert.AreEqual<double>((double)d.Ds.Tables[0].Rows[0]["2w"], (double)donnees.Ds.Tables[0].Rows[0]["2w"]);

             /////////////////////////////// A decommenter pour avoir les donnees de test en .csv \\\\\\\\\\\\\\\\\\\\\\\\\\
            /////////////////////////////// Penser à changer le path pour ecrire dans les fichiers \\\\\\\\\\\\\\\\\\\\\\\\
            /*
            // Pour extraire les données en format .csv
            string text = "Date;1w;2w;1m;2m;3m;6m;9m;12m;\n";
            string text2 = "Date;1w;2w;1m;2m;3m;6m;9m;12m;\n";

            //Trace.Write("  date        1w         2w        1m         2m         3m         6m        9m     12m \n");
            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                text += ((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["1w"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["2w"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["1m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["2m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["3m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["6m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["9m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["12m"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            for (int i = 0; i < donnees.Ds.Tables[0].Rows.Count; i++)
            {
                text2 += ((DateTime)donnees.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["3m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["6m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["9m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["12m"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            // On écrit dans le fichier
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\SimuleValeurComplet_interest_bis.csv", text);
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\Temoin_interest_bis.csv", text2);
            //System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\Complet_interest_bis.csv", text3);
             * */
        }
        [TestMethod]
        public void TestRequeteComplete_Interest()
        {
            Data.InterestRate taux = Data.InterestRate.EURIBOR;


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            DataRetour dretour = new DataRetour();
            Data donnees = s.getInterestRate(taux, debut, fin, dretour);
            // Pour extraire les données en format .csv
            string text3 = "Date;1w;2w;1m;2m;3m;6m;9m;12m;\n";
            

            //Trace.Write("  date        1w         2w        1m         2m         3m         6m        9m     12m \n");
            for (int i = 0; i < donnees.Ds.Tables[0].Rows.Count; i++)
            {
                text3 += ((DateTime)donnees.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["3m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["6m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["9m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["12m"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            // on supprime une ligne
            // attention à ne pas prendre un vendredi ou lundi
            for (int k = 0; k < 15; k++)
            {
                donnees.Ds.Tables[0].Rows.Remove(donnees.Ds.Tables[0].Rows[15]);
            }
            Data d = GestionErreurs.interestErreur((DataInterestRate)donnees,dretour);
            Assert.AreEqual<double>((double)d.Ds.Tables[0].Rows[0]["2w"], (double)donnees.Ds.Tables[0].Rows[0]["2w"]);

             /////////////////////////////// A decommenter pour avoir les donnees de test en .csv \\\\\\\\\\\\\\\\\\\\\\\\\\
            /////////////////////////////// Penser à changer le path pour ecrire dans les fichiers \\\\\\\\\\\\\\\\\\\\\\\\
            /*
            // Pour extraire les données en format .csv
            string text = "Date;1w;2w;1m;2m;3m;6m;9m;12m;\n";
            string text2 = "Date;1w;2w;1m;2m;3m;6m;9m;12m;\n";
            
            //Trace.Write("  date        1w         2w        1m         2m         3m         6m        9m     12m \n");
            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                text += ((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["1w"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["2w"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["1m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["2m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["3m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["6m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["9m"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["12m"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            for (int i = 0; i < donnees.Ds.Tables[0].Rows.Count; i++)
            {
                text2 += ((DateTime)donnees.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2w"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["1m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["2m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["3m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["6m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["9m"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["12m"]).ToString() + "\n";
                //Trace.Write(((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd") + "   " + (double)d.Ds.Tables[0].Rows[i]["Open"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Close"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Low"] + "   " + (double)d.Ds.Tables[0].Rows[i]["High"] + "   " + (double)d.Ds.Tables[0].Rows[i]["Volume"] + "\n");
            }
            // On écrit dans le fichier
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\SimuleValeurComplet_interest.csv", text);
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\Temoin_interest.csv", text2);
            //System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\Complet_interest.csv", text3);
            */
        }
        # endregion

        #region Test Taux de change
        /// <summary>
        /// Si on a un trou sur toute les valeur 
        /// </summary>
        [TestMethod]
        public void TestRequeteComplete_Exchange()
        {
            List<Data.Currency> columns = new List<Data.Currency>();
            columns.Add(Data.Currency.USD);
            columns.Add(Data.Currency.JPY);

            Data.Currency l = Data.Currency.EUR;


            DateTime debut = new DateTime(2015, 01, 01);
            DateTime fin = new DateTime(2015, 04, 01);


            Services s = new Services();
            DataRetour dretour = new DataRetour();
            Data donnees = s.getExchangeRate(l, columns, debut, fin, Data.Frequency.Daily, dretour);
            // on supprime une ligne
            // attention à ne pas prendre un vendredi ou lundi
            for (int k = 0; k < 15; k++)
            {
                donnees.Ds.Tables[0].Rows[2].Delete();
            }
            Data d = GestionErreurs.exchangeErreur((DataExchangeRate) donnees,dretour);
            Assert.AreEqual<double>((double)d.Ds.Tables[0].Rows[1]["EUR/USD"], (double)donnees.Ds.Tables[0].Rows[1]["EUR/USD"]);
            
             /////////////////////////////// A decommenter pour avoir les donnees de test en .csv \\\\\\\\\\\\\\\\\\\\\\\\\\
            /////////////////////////////// Penser à changer le path pour ecrire dans les fichiers \\\\\\\\\\\\\\\\\\\\\\\\
            /*
            // Pour extraire les données en format .csv
            string text = "Date;EUR/USD;EUR/JPY\n";
            string text2 = "Date;EUR/USD;EUR/JPY\n";
            for (int i = 0; i < d.Ds.Tables[0].Rows.Count; i++)
            {
                text += ((DateTime)d.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["EUR/USD"]).ToString()
                    + ";" + ((double)d.Ds.Tables[0].Rows[i]["EUR/JPY"]).ToString()+ "\n";
            }
            for (int i = 0; i < donnees.Ds.Tables[0].Rows.Count; i++)
            {
                text2 += ((DateTime)donnees.Ds.Tables[0].Rows[i]["Date"]).ToString("yyyy/MM/dd")
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["EUR/USD"]).ToString()
                    + ";" + ((double)donnees.Ds.Tables[0].Rows[i]["EUR/JPY"]).ToString() + "\n";
            }
            // On écrit dans le fichier
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\SimuleValeurComplet_Exchange.csv", text);
            System.IO.File.WriteAllText(@"C:\Users\BVE\Desktop\Projet_Spe\TestResults\Temoin_Exchange.csv", text2);
             */
        }
        #endregion 
    }
    
}
