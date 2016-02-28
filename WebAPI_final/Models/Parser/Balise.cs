
namespace WebAPI_final.Models.Parser
{
    /// <summary> 
    /// Classe Balise
    /// nécessaire au parser générique
    /// </summary>
    public class Balise
    {
        #region Attributs
        /// <summary> Nom de la balise  </summary>
        public string name;

        // min et max permettent de récupérer une balise spécifique
        // Par exemple, min=2 et min=3, indique que l'on veut les balises n°2 et n°3

        /// <summary> Nombre minimum. min = -1 ~ min = 0 </summary>
        public int min;
        /// <summary> Nombre maximum. max = -1 ~ max = nombre maximum  </summary>
        public int max;
        /// <summary> Boolean indiquant si on doit ignorer les balises intérieures ou non </summary>
        public bool checkProfondeur;
        #endregion

        #region Constructeurs
        /// <summary> 
        /// Constructeur sans indication du min et max
        /// min = 0 et max = max
        /// </summary>
        /// <param name="nameBalise">Nom de la balise</param>
        /// <param name="b">boolean indiquant si on doit ignorer les balises intérieures ou non </param>
        public Balise(string nameBalise, bool b)
        {
            name = nameBalise;
            checkProfondeur = b;
            min = -1;
            max = -1;
        }

        /// <summary> 
        /// Constructeur avec indication du numéro de la balise voulu
        /// min = max = nbAppear
        /// </summary>
        /// <param name="nameBalise">Nom de la balise</param>
        /// <param name="nbAppear">Numéro de la balise</param>
        /// <param name="b">boolean indiquant si on doit ignorer les balises intérieures ou non </param>
        public Balise(string nameBalise, int nbAppear, bool b)
        {
            name = nameBalise;
            checkProfondeur = b;

            // min
            if (nbAppear >= -1) { min = max = nbAppear; }
            else {  min = max = -1; }
        }

        /// <summary> 
        /// Constructeur avec indication du min et max
        /// </summary>
        /// <param name="nameBalise">Nom de la balise</param>
        /// <param name="minAppear">Nombre minimum</param>
        /// <param name="maxAppear">Nombre maximum</param>
        /// <param name="b">boolean indiquant si on doit ignorer les balises intérieures ou non </param>
        public Balise(string nameBalise, int minAppear, int maxAppear, bool b)
        {
            name = nameBalise;
            checkProfondeur = b;
            
            // min
            if (minAppear >= -1)  { min = minAppear; }
            else {  min = -1; }
            // max
            if (maxAppear >= -1) { max = maxAppear; }
            else { max = -1; }

            if (min > max && max != -1)
            {
                min = max = -1;
            }
        }
        #endregion
    }
}
