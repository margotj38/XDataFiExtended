using System.Data;

namespace WebAPI_final.Models.Data
{
    /// <summary>
    /// Implémentation de Data à partir d'un fichier XML de configuration
    /// </summary>
    public class DataXML : Data
    {
        
        /// <summary>
        /// Construction à partir d'un fichier XML de configuration
        /// </summary>
        public DataXML()
            : base()
        {
            Ds = new DataSet();
        }
    }
}
