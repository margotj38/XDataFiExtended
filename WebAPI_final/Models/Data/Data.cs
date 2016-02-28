using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace WebAPI_final.Models.Data
{
    [DataContract]
    [KnownType(typeof(DataActif))]
    [KnownType(typeof(DataExchangeRate))]
    [KnownType(typeof(DataInterestRate))]
    [KnownType(typeof(DataXML))]
    public class Data
    {
        #region Enums

        /// <summary>
        /// Choix entre historique des taux de change journalier, mensuel ou annuel
        /// </summary>
        public enum Frequency
        {
            Daily,
            Monthly,
            Yearly
        }

        /// <summary>
        /// Enumération des informations historiques que l'on peut demander
        /// Chacune prendra une colonne de données
        /// </summary>
        public enum HistoricalColumn
        {
            Open,
            High,
            Low,
            Close,
            Volume
        }

        /// <summary>
        /// énumération des devises connues par le programme
        /// </summary>
        public enum Currency
        {
            ADF, ADP,
            AED,
            AFA,
            AFN,
            AFR,
            ALL,
            AMD,
            ANG,
            AOA,
            AON,
            ARS,
            ATS,
            AUD,
            AWF,
            AWG,
            AZM,
            AZN,
            BAM,

            BBD,

            BDT,

            BEF,

            BGL,

            BGN,

            BHD,

            BIF,

            BMD,

            BND,

            BOB,

            BRL,

            BSD,

            BTN,

            BWP,

            BYR,

            BZD,

            CAD,

            CDF,

            CHF,

            CLP,

            CNY,

            COP,

            CRS,

            CUC,

            CUP,

            CVE,

            CYP,

            CZK,

            DEM,

            DIF,

            DKK,

            DOP,

            DZF,

            ECS,

            EEK,

            EGP,

            ERN,

            ESP,

            ETD,

            EUR,

            FIM,

            FJD,

            FKP,

            GBP,

            GEL,

            GGP,

            GHC,

            GHS,

            GIP,

            GMP,

            GNF,

            GRD,

            GTQ,

            GYD,

            HKD,

            HNL,

            HRK,

            HTG,

            HUF,

            IDR,

            IEP,

            ILS,

            IMP,

            INR,

            IQD,

            IRR,

            ISK,

            ITL,

            JEP,

            JMP,

            JOD,

            JPY,

            KES,

            KGS,

            KHR,

            KMF,

            KPW,

            KRW,

            KWD,

            KYD,

            KZT,

            LAK,

            LBP,

            LKR,

            LRD,

            LSL,

            LTL,

            LUF,

            LVL,

            LYD,

            MAD,

            MDL,

            MGA,

            MGF,

            MKD,

            MMK,

            MNT,

            MOP,

            MRO,

            MTL,

            MUR,

            MVR,

            MWK,

            MXN,

            MYR,

            MZM,

            MZN,

            NAD,

            NGN,

            NIO,

            NLG,

            NOK,

            NPR,

            NTD,

            NZD,
            OMR,

            PAB,

            PEN,

            PGK,

            PHP,

            PKR,

            PLN,

            PSL,

            PTE,

            PYG,

            QAR,

            ROL,
            RON,

            RSD,

            RUB,

            RWF,

            SAR,

            SBD,

            SCR,

            SDD,

            SDG,

            SDP,

            SEK,

            SGD,

            SHP,

            SIT,

            SKK,

            SLL,

            SOS,

            SPL,

            SRD,

            SRG,

            STD,

            SVC,

            SVP,

            SZL,

            THB,

            TJS,

            TMM,

            TND,

            TOP,

            TRL,

            TRY,

            TTD,

            TYD,

            TWD,

            TZS,

            UAH,

            UGX,

            USD,

            UYP,

            UYU,

            UZS,

            VAL,

            VEB,

            VEF,

            VND,

            VUV,

            WST,

            XAF,

            XAG,

            XAU,

            XCD,

            XDR,

            XEU,

            XOF,

            XPD,

            XPF,

            XPT,

            YER,

            YUN,

            ZAR,

            ZMK,

            ZWD

        }

        /// <summary>
        /// Enumération des différents taux d'intérêts disponibles
        /// </summary>
        public enum InterestRate
        {
            EURIBOR,
            EONIA,
            EUREPO,
            EONIASWAP
        }

        /// <summary>
        /// Différents types de données que l'on peut récupérer
        /// </summary>
        public enum TypeData
        {
            HistoricalData,
            ExchangeRate,
            InterestRate,
            RealTime
        }
        #endregion

        #region Attributs

        [DataMember]
        /// <summary> Base de donnée </summary>
        public DataSet Ds { get; protected set; }

        [DataMember]
        /// <summary> Symbole des données traitées </summary>
        public List<string> Symbol { get; set; }

        [DataMember]
        /// <summary> Liste des colonnes </summary>
        public List<string> Columns { get; protected set; }

        [DataMember]
        /// <summary> Date de début de l'acquisition </summary>
        public DateTime Start { get; protected set; }

        [DataMember]
        /// <summary> Date de fin de l'acquisition </summary>
        public DateTime End { get; protected set; }

        [DataMember]
        /// <summary> Type des données </summary>
        public TypeData Type { get; protected set; }

        #endregion

        #region Constructeur
        public Data()
        {
        }
        #endregion

        #region Méthodes
        public void set(DataSet ds, List<string> symbol, List<string> columns, DateTime start, DateTime end)
        {
            Ds = ds;
            Symbol = symbol;
            Columns = columns;
            Start = start;
            End = end;
        }

        public void set(DataSet ds, List<string> symbol, List<string> columns, DateTime start, DateTime end, TypeData dataType)
        {
            Ds = ds;
            Symbol = symbol;
            Columns = columns;
            Start = start;
            End = end;
            Type = dataType;
        }

        /// <summary>
        /// Initialisation du dataset
        /// </summary>
        protected void initDataSet()
        {
            // Création du DataSet
            Ds  = new DataSet();
            DataTable dt = new DataTable();

            int nbCol = 2 + Columns.Count;
            DataColumn[] dataColumns = new DataColumn[nbCol];
            
            dataColumns[0] = new DataColumn("Symbol", System.Type.GetType("System.String"));
            dt.Columns.Add(dataColumns[0]);
            dataColumns[1] = new DataColumn("Date", System.Type.GetType("System.DateTime"));
            dt.Columns.Add(dataColumns[1]);

            int i = 2;
            foreach (string s in Columns)
            {
                dataColumns[i] = new DataColumn(s, System.Type.GetType("System.Double"));
                dt.Columns.Add(dataColumns[i]);
                i++;
            }

            Ds.Tables.Add(dt);
        }
        #endregion
    }
}
