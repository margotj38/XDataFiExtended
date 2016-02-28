using System;
using System.Net;

namespace WebAPI_final.Models.ImportParse
{
    /// <summary>
    /// Classe abstraite, permettant de récupérer des données depuis le web
    /// </summary>
    public abstract class ImportParse
    {
        /// <summary> Parser </summary>
        protected Parser.Parser _Parser;
        /// <summary> Chemin du fichier </summary>
        protected string _Filepath;


        /// <summary>
        /// Télécharge le fichier désiré
        /// et la parse, en remplissant les données
        /// </summary>
        public abstract void ImportAndParse(Data.Data d, Data.DataRetour Erreur = null);

        /// <summary>
        /// Télécharge le fichier depuis l'url et l'enregistre suivant _Filepath
        /// </summary>
        /// <param name="siteUri">lien url du fichier à télécharger</param>
        protected void ImportFile(Uri siteUri)
        {
            WebAPI_final.Models.Constantes.displayDEBUG(_Filepath, 1);

            // Si le fichier existe, alors on le nomme autrement
            int i = 0;
            string nameFile = _Filepath;
            while (System.IO.File.Exists(@nameFile))
            {
                i++;
                nameFile = i + "_" + _Filepath;
            }
            _Filepath = nameFile;

            // Téléchargement
            WebAPI_final.Models.Constantes.displayDEBUG("start Download", 2);
            WebClient client = new WebClient();

            try
            {
                client.DownloadFile(siteUri, _Filepath);
            }
            catch (Exception e)
            {
                throw new DownloadException("Impossible de télécharger le fichier du lien : " + siteUri + "\n(" + e.Message + ")" );
            }

            WebAPI_final.Models.Constantes.displayDEBUG("end Download", 2);

            WebAPI_final.Models.Constantes.displayDEBUG("Le fichier a bien été créé", 1);
        }
    }
}
