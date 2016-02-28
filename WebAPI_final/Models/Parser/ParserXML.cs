using System;
using System.Collections.Generic;
using System.Data;

namespace WebAPI_final.Models.Parser
{
    /// <summary>
    /// Parser pour les ficheirs XML
    /// </summary>
    class ParserXML : Parser
    {
        #region Attributs
        /// <summary> Nom du fichier à parser  </summary>
        protected string _FilepathSchema;
        #endregion

        #region Constructeur
        public ParserXML()
        {
        }

        public ParserXML(string filepath, string filepathSchema)
        {
            _Filepath = filepath;
            _FilepathSchema = filepathSchema;
        }
        #endregion

        #region Méthodes
        /// <summary>
        /// Parse le fichier désiré
        /// Insère les données dans d
        /// </summary>
        /// <param name="d">base de donnée</param>
        public override void ParseFile(Data.Data d)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start parseXML", 2);

            DataSet ds = new DataSet();
            try
            {
                ds.ReadXmlSchema(_FilepathSchema);
                WebAPI_final.Models.Constantes.displayDEBUG("ReadXmlSchema Done", 3);

                ds.ReadXml(_Filepath, XmlReadMode.ReadSchema);
                WebAPI_final.Models.Constantes.displayDEBUG("ReadXml Done", 3);
            }
            catch(Exception e)
            {
                throw new XMLException("Problème lecture fichier XML : " + e.Message);
            }

            WebAPI_final.Models.Constantes.displayDEBUG("end parseXML", 2);
            
           
            string nameFunction = ds.Tables[0].Rows[0][0].ToString();

            if (nameFunction.Equals("historical"))
            {
                // Dates
                DateTime debut = DateTime.Parse(ds.Tables[2].Rows[0][0].ToString());
                DateTime fin = DateTime.Parse(ds.Tables[2].Rows[0][1].ToString());

                // Liste Symbol
                List<string> symbol = new List<string>();
                for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                {
                    symbol.Add(ds.Tables[3].Rows[i][0].ToString());
                }

                // Liste Colonnes
                List<Data.Data.HistoricalColumn> columns = new List<Data.Data.HistoricalColumn>();
                for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
                {
                    columns.Add((Data.Data.HistoricalColumn)
                            Enum.Parse(typeof(Data.Data.HistoricalColumn), ds.Tables[4].Rows[i][0].ToString()));
                }


                // Création du Data
                Data.DataActif data = new Data.DataActif(symbol, columns, debut, fin);

                // Import des données désirées
                ImportParse.Yahoo import = new ImportParse.Yahoo();
                import.ImportAndParse(data);

                // Transfert des données
                d.set(data.Ds, data.Symbol, data.Columns, debut, fin);
            }
            else if (nameFunction.Equals("exchange"))
            {
                // Dates
                DateTime debut = DateTime.Parse(ds.Tables[5].Rows[0][0].ToString());
                DateTime fin = DateTime.Parse(ds.Tables[5].Rows[0][1].ToString());
                
                // Liste Symbol
                Data.Data.Currency symbol = (Data.Data.Currency)Enum.Parse(typeof(Data.Data.Currency), ds.Tables[5].Rows[0][2].ToString());
                
                // Fréquence
                Data.Data.Frequency freq = (Data.Data.Frequency) Enum.Parse(typeof(Data.Data.Frequency), ds.Tables[5].Rows[0][3].ToString());
               
                // Liste Colonnes
                List<Data.Data.Currency> columns = new List<Data.Data.Currency>();
                for (int i = 0; i < ds.Tables[6].Rows.Count; i++)
                {
                    columns.Add((Data.Data.Currency)
                            Enum.Parse(typeof(Data.Data.Currency), ds.Tables[6].Rows[i][0].ToString()));
                }

                // Création du Data
                Data.DataExchangeRate data = new Data.DataExchangeRate(symbol, columns, debut, fin, freq);

                // Import des données désirées
                ImportParse.FXTop import = new ImportParse.FXTop();
                import.ImportAndParse(data);

                // Transfert des données
                d.set(data.Ds, data.Symbol, data.Columns, debut, fin);
            }
            else if (nameFunction.Equals("interest"))
            {
                // Dates
                DateTime debut = DateTime.Parse(ds.Tables[7].Rows[0][0].ToString());
                DateTime fin = DateTime.Parse(ds.Tables[7].Rows[0][1].ToString());

                // Symbol
                Data.Data.InterestRate symbol = (Data.Data.InterestRate)
                            Enum.Parse(typeof(Data.Data.InterestRate), ds.Tables[7].Rows[0][2].ToString());

                // Création du Data
                Data.DataInterestRate data = new Data.DataInterestRate(symbol, debut, fin);

                // Import des données désirées
                ImportParse.EBF import = new ImportParse.EBF();
                import.ImportAndParse(data);

                // Transfert des données
                d.set(data.Ds, data.Symbol, data.Columns, debut, fin);
            }
            else
            {
                throw new XMLException("Nom de fonction inconnu : " + nameFunction);
            }
        }

        #endregion

    }
}
