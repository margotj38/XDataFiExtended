using System;

namespace WebAPI_final.Models
{
    /// <summary>
    /// Classe de nos exceptions
    /// </summary>
    public class MyExceptions : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public MyExceptions(string msg) : base(msg) { }
    }

    /// <summary>
    /// Test de connectivité
    /// </summary>
    public class ConnectivityException : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public ConnectivityException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Téléchargement du fichier
    /// </summary>
    public class DownloadException : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public DownloadException(string msg) : base(msg) { }
    }

    /// <summary>
    /// XML
    /// </summary>
    public class XMLException : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public XMLException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Test le type utilisé
    /// </summary>
    public class Mauvaistype : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public Mauvaistype(string msg) : base(msg) { }
    }

    /// <summary>
    /// Vérifie les valeurs négatives des prix
    /// </summary>
    public class NegativeValueImpossible : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public NegativeValueImpossible(string msg) : base(msg) { }
    }


    /// <summary>
    /// Test temporel
    /// </summary>
    public class WrongDates : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message personnalisé</param>
        public WrongDates(string msg) : base(msg) { }
    }

    /// <summary>
    /// Classe des exceptions internes au programme
    /// </summary>
    public class ErreurInterne : MyExceptions
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message explicitant l'erreur </param>
        /// <param name="creation">booléen présent uniquement pour indiquer que l'exception est lancée sans avoir été préalablement rattrapée (peut valoir indifféremment true ou false)</param>
        public ErreurInterne(string msg, bool creation) : base("Erreur interne : " + msg + "\n") { }

        /// <summary>
        /// Constructeur d'une erreur relancée
        /// </summary>
        /// <param name="msg"> nom de la fonction courante (en général dans la variable chaineException)</param>
        public ErreurInterne(string msg) : base(msg) { }
    }

    /// <summary>
    /// Classe des exceptions concernant la source d'extraction
    /// </summary>
    public class ErreurSource : MyExceptions
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">message explicitant l'erreur </param>
        /// <param name="creation">booléen présent uniquement pour indiquer que l'exception est lancée sans avoir été préalablement rattrapée (peut valoir indifféremment true ou false)</param>
        public ErreurSource(string msg, bool creation) : base("Erreur interne : " + msg + "\n") { }

        /// <summary>
        /// Constructeur d'une erreur relancée
        /// </summary>
        /// <param name="msg"> nom de la fonction courante (en général dans la variable chaineException)</param>
        public ErreurSource(string msg) : base(msg) { }
    }

    /// <summary>
    /// Classe des erreurs commises par l'utilisateur
    /// </summary>
    public class ErreurUtilisateur : MyExceptions
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="msg">indication explicite de l'erreur</param>
        public ErreurUtilisateur(string msg) : base("Erreur utilisateur : " + msg) { }
    }
}
