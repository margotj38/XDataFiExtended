using System.IO;

namespace WebAPI_final.Models.ImportParse
{
    public class XML : ImportParse
    {
        /// <summary>
        /// Contenu du fichier XML à parser
        /// </summary>
        string contentXML;

        #region Constructeur
        /// <summary>
        /// Méthode de connexion
        /// </summary>
        public XML(string s)
        {
            this.contentXML = s;
            _Parser = new Parser.ParserXML();
        }
        #endregion

        #region Méthodes
        /// <summary>
        /// Crée les fichiers
        /// et le parse, en remplissant les données
        /// </summary>
        /// <param name="d">Base de donnée, DataXML </param>
        public override void ImportAndParse(Data.Data d, Data.DataRetour Erreur = null)
        {
            // Création des fichiers
            string filePath = "xml.xml";
            string filePathSchema = "Schema/xmlSchema.xsd";

            // Si les fichiers existe,nt alors on les nomme autrement
            int i = 0;
            string nameFile = filePath;
            while (System.IO.File.Exists(@nameFile))
            {
                i++;
                nameFile = i + "_" + filePath;
            }
            filePath = nameFile;

            // Fichier XML
            StreamWriter stw = new StreamWriter(filePath);
            stw.Write(contentXML);
            stw.Close();
            WebAPI_final.Models.Constantes.displayDEBUG("Fichier XML créé", 1);

            // Parse
            _Parser = new Parser.ParserXML(filePath, filePathSchema);
            _Parser.ParseFile(d);

            // On supprime les fichiers
            System.IO.File.Delete(@filePath);
        }
        #endregion

    }
}
