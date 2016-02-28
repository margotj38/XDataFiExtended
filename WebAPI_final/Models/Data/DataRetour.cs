using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Linq;
using System.Web;

namespace WebAPI_final.Models.Data
{
    [DataContract]
    public class DataRetour
    {
        #region Attributs
        [DataMember]
        private Data data;
        [DataMember]
        private List<string> listeErreur;
        [DataMember]
        private List<string> warning;
        #endregion

        #region Constructeurs
        public DataRetour(Data d, List<string> lE, List<string> w)
        {
            data = d;
            listeErreur = lE;
            warning = w;
        }
        public DataRetour()
        {
            listeErreur = new List<string>();
            warning = new List<string>();
        }
        #endregion

        #region Getter et Setter
        public Data GetData(){
            return data;
        }
        public List<string> GetListeErreur()
        {
            return listeErreur;
        }
        public List<string> GetWarning()
        {
            return warning;
        }

        public void SetData(Data d)
        {
            data = d;
        }
        public void SetListeErreur(List<string> lE)
        {
            listeErreur = lE;
        }
        public void SetWarning(List<string> w)
        {
            warning = w;
        }
        #endregion
    }
}