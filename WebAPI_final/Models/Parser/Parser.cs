
namespace WebAPI_final.Models.Parser
{
    /// <summary>
    /// Classe abstraite, Parser servant à récupérer des données depuis un fichier
    /// </summary>
    public abstract class Parser
    {
        /// <summary> Nom du fichier à parser  </summary>
        protected string _Filepath;
        /// <summary> Symbol actuel </summary>
        protected string _CurrentSymbol;


        /// <summary> Constructeur </summary>
        public Parser()
        {
            _Filepath = "";
            _CurrentSymbol = "";
        }

        /// <summary>
        /// Modifie le filepath et le currentSymbol
        /// </summary>
        public void set(string filepath, string symbol)
        {
            _Filepath = filepath;
            _CurrentSymbol = symbol;
        }

        /// <summary>
        /// Parse le fichier désiré
        /// Insère les données dans d
        /// </summary>
        public abstract void ParseFile(Data.Data d);
    }
}
