using System.ServiceModel;

namespace WebAPI_final.Models.Service
{
    [ServiceContract]
    /// <summary> Service d'acquisition XML </summary>
    public interface IXMLService
    {
        [OperationContract]
        /// <summary>
        /// Exécute la fonction demandée dans le fichier XML
        /// </summary>
        /// <param name="content">contenu du fichier XML</param>
        WebAPI_final.Models.Data.Data getXML(string content);
    }
}
