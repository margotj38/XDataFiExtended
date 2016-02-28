﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI_final.Models.GestionErreurs
{
    public static class GestionSimulation
    {

        #region Attributs
        public struct evenement
        {
            private double p;
            private DateTime dateTime;

            public evenement(double p, DateTime dateTime)
            {
                this.p = p;
                this.dateTime = dateTime;
            }

            public double getCours()
            {
                return p;
            }

            public DateTime getDate()
            {
                return dateTime;
            }
            public void setCours(double c)
            {
                this.p = c;
            }

            public String toString()
            {
                //on sépare par une virgule pour pouvoir exporter les données sous format csv si l'on veut
                String s;
                s = dateTime.ToString("dd/MM/yyyy") + ";" + '"' + p + '"';
                return s;
            }
        }

        private static readonly Random rand = new Random();
        #endregion

        #region Methodes annexes
        /// <summary>
        /// Permet de retourner une valeure suivan une loi normale
        /// </summary>
        /// <param name="moyenne"></param>
        /// <param name="ecart_type"></param>
        /// <returns></returns>
        public static double normale(double moyenne, double ecart_type)
        {
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = moyenne + ecart_type * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        /// <summary>
        /// Permet de translater la date de x jours ouvrés. ( dans le passé ou le futur)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="businessDays"></param>
        /// <returns></returns>
        public static DateTime AddBusinessDays(DateTime startDate,
                                         int businessDays)
        {
            int direction = Math.Sign(businessDays);
            if (direction == 1)
            {
                if (startDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    startDate = startDate.AddDays(2);
                    businessDays = businessDays - 1;
                }
                else if (startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDate = startDate.AddDays(1);
                    businessDays = businessDays - 1;
                }
            }
            else
            {
                if (startDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    startDate = startDate.AddDays(-1);
                    businessDays = businessDays + 1;
                }
                else if (startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDate = startDate.AddDays(-2);
                    businessDays = businessDays + 1;
                }
            }

            int initialDayOfWeek = Convert.ToInt32(startDate.DayOfWeek);

            int weeksBase = Math.Abs(businessDays / 5);
            int addDays = Math.Abs(businessDays % 5);

            if ((direction == 1 && addDays + initialDayOfWeek > 5) ||
                 (direction == -1 && addDays >= initialDayOfWeek))
            {
                addDays += 2;
            }

            int totalDays = (weeksBase * 7) + addDays;
            return startDate.AddDays(totalDays * direction);
        }

        /// <summary>
        /// Génère la liste de jours ouvrés entre début et fin (exclus)
        /// </summary>
        /// <param name="debut"></param>
        /// <param name="fin"></param>
        /// <returns></returns>
        public static List<DateTime> listeDate(DateTime debut, DateTime fin)
        {
            List<DateTime> retour = new List<DateTime>();
            DateTime cour = debut;

            // on va au 1er jour ouvré suivant
            cour = AddBusinessDays(cour, 1);

            // puis on ajoute tous les jours ouvrés jusqu'à fin
            while (cour < fin)
            {
                retour.Add(cour);
                cour = AddBusinessDays(cour, 1);
            }
            return retour;
        }
        #endregion

        #region Méthodes de simulation
        /// <summary>
        /// Simule récursivement une liste de { cours , date } dans une période donnée
        /// </summary>
        /// <param name="dateDebut"></param>
        /// <param name="dateFin"></param>
        /// <param name="valeurDebut"></param>
        /// <param name="valeurFin"></param>
        /// <param name="volatilite"></param>
        /// <returns></returns>
        static public List<evenement> pontBrownien(DateTime dateDebut, DateTime dateFin, double valeurDebut, double valeurFin, double volatilite)
        {
            List<evenement> retour = new List<evenement>();
            int arrondi = 10000;

            // on génère la liste des dates ouvrées entre début et fin exclus
            List<DateTime> listedate = listeDate(dateDebut, dateFin);

            int nb_date = listedate.Count;

            // Conditions d'arret *********************************************************************
            if (nb_date == 1)                                                                        //
            {                                                                                        //
                /*evenement event1 = new evenement(Math.Round(normale((valeurDebut + valeurFin) / 2,   //
                    (valeurDebut - valeurFin) / ((dateFin - dateDebut).TotalDays / 10)) * arrondi,        //
                     MidpointRounding.AwayFromZero) / arrondi, listedate[0]);  */                         //
                evenement event1 = new evenement(Math.Round(normale((valeurDebut + valeurFin) / 2,   //
                    volatilite) * arrondi,MidpointRounding.AwayFromZero) / arrondi, listedate[0]);
                retour.Add(event1);                                                                  //
                return retour;                                                                       //
            }                                                                                        //
            if (nb_date == 0)                                                                        //
            {                                                                                        //
                return retour;                                                                       //
            }                                                                                        //
            // ****************************************************************************************

            // récursivité tant que dateDebut < dateFin
            // en effet on va utiliser la methode du pont brownien entre tO et t1:
            if (dateDebut < dateFin)
            {
                // on se place donc en (t0+t1)/2 donc au milieu.
                DateTime dateMilieu = AddBusinessDays(dateDebut, (int)nb_date / 2);
                double valeurMilieu = Math.Round(normale((valeurDebut + valeurFin) / 2, volatilite) * arrondi, MidpointRounding.AwayFromZero) / arrondi;
                evenement event1 = new evenement(valeurMilieu, dateMilieu);
                // on appelle ensuite la procédure récursivement à gauche et à droite
                retour.AddRange(pontBrownien(dateDebut, dateMilieu, valeurDebut, valeurMilieu,volatilite/2));
                retour.Add(event1);
                retour.AddRange(pontBrownien(dateMilieu, dateFin, valeurMilieu, valeurFin,volatilite/2));

            }
            return retour;

        }

        /// <summary>
        /// Permet de simuler high lorsqu'on a déjà simuler open ou close
        /// </summary>
        /// <param name="l"></param>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static public List<evenement> simuleHigh(List<evenement> l, double p, double s)
        {
            List<evenement> retour = new List<evenement>();
            double prec = p;
            for (int i = 0; i < l.Count; i++)
            {
                double cour = l[i].getCours();
                double suiv = s;
                if (i < l.Count - 1)
                {
                    suiv = l[i + 1].getCours();
                }
                double max = Math.Max(cour, prec);
                max = Math.Max(max, suiv);
                double newhigh = max + rand.NextDouble();
                prec = cour;
                retour.Add(new evenement(Math.Round(newhigh * 10000, MidpointRounding.AwayFromZero) / 10000, l[i].getDate()));
            }
            return retour;
        }

        /// <summary>
        /// Permet de simuler low lorsqu'on a déjà simuler open ou close
        /// </summary>
        /// <param name="l"></param>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        static public List<evenement> simuleLow(List<evenement> l, double p, double s)
        {
            List<evenement> retour = new List<evenement>();
            double prec = p;
            for (int i = 0; i < l.Count; i++)
            {
                double cour = l[i].getCours();
                double suiv = s;
                if (i < l.Count - 1)
                {
                    suiv = l[i + 1].getCours();
                }
                double min = Math.Min(cour, prec);
                min = Math.Min(min, suiv);
                double newlow = min - rand.NextDouble();
                prec = cour;
                retour.Add(new evenement(Math.Round(newlow * 10000, MidpointRounding.AwayFromZero) / 10000, l[i].getDate()));
            }
            return retour;
        }


        /// <summary>
        /// Simule une liste de { cours , date } à partir d'une date donnée pour une nombre de jours données
        /// En suivant le modèle d'un Mouvement brownien
        /// </summary>
        /// <param name="dateDebut"></param>
        /// <param name="nombre_date"></param>
        /// <param name="valeur_debut"></param>
        /// <param name="volatilite"></param>
        /// <returns></returns>
        static public List<evenement> mouvementBrownien(DateTime dateDebut, int nombre_date, double valeur_debut, double volatilite)
        {
            List<evenement> retour = new List<evenement>();
            //On ajoute un nombre prédéfinie de valeur à simuler
            DateTime dateFin = AddBusinessDays(dateDebut, nombre_date + 1);
            // on génère la liste des dates ouvrées entre début(exclus) et fin (non exclus)
            List<DateTime> listedate = listeDate(dateDebut, dateFin);
            int nb_date = listedate.Count;
            int cpt = 0;
            //On va simuler des lois normale,
            
            //On simule les valeurs obtenues du mouvement brownien
            double[] val = new double[nb_date + 1];
            val[0] = valeur_debut;
            for (int i = 1; i < nb_date; i++)
            {
                val[i] = val[i - 1] + normale(0, volatilite);
            }

            //On créer nos événements  
            while (cpt < nb_date)
            {
                evenement event1 = new evenement(val[cpt], listedate[cpt]);
                retour.Add(event1);
                cpt++;
            }
            return retour;
        }
        /// <summary>
        /// On cree une liste d'evenement ou les dates vont de debut à fin
        /// La valeur de tous les evenements est la valeur du cours de e
        /// </summary>
        /// <param name="debut"></param>
        /// <param name="fin"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<evenement> recopieEvenement(DateTime debut, DateTime fin, evenement e)
        {
            List<evenement> retour = new List<evenement>();
            List<DateTime> listedate = listeDate(debut, fin);
            int nb_date = listedate.Count;
            int cpt = 0;
            double val = e.getCours();
            //On créer nos événements  
            while (cpt < nb_date)
            {
                evenement event1 = new evenement(val, listedate[cpt]);
                retour.Add(event1);
                cpt++;
            }
            return retour;
        }

        /// <summary>
        /// Interpolation linéaire entre deux evenements
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns></returns>
        public static List<evenement> interpolationLineraire(evenement e1, evenement e2)
        {
            DateTime debut = e1.getDate();
            DateTime fin = e2.getDate();
            double valDebut = e1.getCours();
            double valFin = e2.getCours();

            List<evenement> retour = new List<evenement>();
            List<DateTime> listedate = listeDate(debut, fin);
            int nb_date = listedate.Count;
            int cpt = 0;
            double increment = (valFin - valDebut) / nb_date;
            //On créer nos événements  
            while (cpt < nb_date)
            {
                evenement event1 = new evenement(valDebut + increment*cpt , listedate[cpt]);
                retour.Add(event1);
                cpt++;
            }
            return retour;
        }

        public static List<evenement> calculTauxExchange(Data.DataExchangeRate exchange, DateTime debut, DateTime fin)
        {
            List<evenement> retour = new List<evenement>();
            List<DateTime> listedate = listeDate(debut, fin);
            int nb_date = listedate.Count;
            int cpt = 0;
            double val1;
            double val2;
            double val;
            String nomCol1 = exchange.Columns[0];
            String nomCol2 = exchange.Columns[1];

            while (cpt < nb_date)
            {
                val1 = (double)exchange.Ds.Tables[0].Rows[cpt][nomCol1];
                val2 = (double)exchange.Ds.Tables[0].Rows[cpt][nomCol2];
                if (val1 != 0)
                {
                    val = val2 / val1;
                }
                else
                {
                    val = val2;
                }
                evenement event1 = new evenement(val, listedate[cpt]);
                retour.Add(event1);
                cpt++;
            }
            return retour;
        }
        #endregion
    }
}