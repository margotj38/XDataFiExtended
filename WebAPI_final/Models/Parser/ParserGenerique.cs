using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebAPI_final.Models.Parser
{
    /// <summary> 
    /// Classe de parser générique de code HTML
    /// (marche pour du XML)
    /// </summary>
    public class ParserGenerique : Parser
    {
        #region Attributs
        /// <summary> Nom du fichier à parser  </summary>
        protected string _FilepathSchema;

        /// <summary> DataSet obtenu  </summary>
        protected DataSet ds;
        #endregion

        #region Constructeur
        public ParserGenerique(string filepathSchema)
        {
            _FilepathSchema = filepathSchema;
        }
        #endregion

        #region Methodes
        /// <summary> Retourne le dataset </summary>
        public DataSet getDataSet()
        {
            return ds;
        }

        /// <summary>
        /// Parse le fichier désiré
        /// Insère les données dans d
        /// </summary>
        /// <param name="d">base de donnée</param>
        public override void ParseFile(Data.Data d)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start parseGenerique", 2);

            // Lecture du schema
            List<Balise> list = readSchema();

            // Récupération des données
            readFile(list);

            WebAPI_final.Models.Constantes.displayDEBUG("end parseGenerique", 2);
        }

        /// <summary> Lecture du fichier schema </summary>
        /// <returns> Liste des balises </returns>
        private List<Balise> readSchema()
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start readSchema", 3);

            // Liste à retourner
            List<Balise> list = new List<Balise>();

            // Lecture fichier
            
            StreamReader str = new StreamReader(_FilepathSchema);
            string line;



            while ((line = str.ReadLine()) != null)
            {
                // Split via " "
                string[] separator = new String[1];
                separator[0] = " ";
                string[] split = line.Split(separator, System.StringSplitOptions.None);

                // Ajout d'une balise avec les bons paramètres
                switch (split.Length)
                {
                    case 1:
                        list.Add(new Balise(split[0], false));
                        break;
                    case 2:
                        // "nom *" (option checkProfondeur) 
                        // ou
                        // "nom min"
                        string name2 = split[0];
                        if (split[1].Equals("*"))
                        {
                            list.Add(new Balise(name2, true));
                        }
                        else
                        {
                            int min2 = int.Parse(split[1]);
                            list.Add(new Balise(name2, min2, false));
                        }
                        break;
                    case 3:
                        // "nom min *" (option checkProfondeur) 
                        // ou
                        // "nom min max"
                        string name3 = split[0];
                        int min3 = int.Parse(split[1]);
                        if (split[2].Equals("*"))
                        {
                            list.Add(new Balise(name3, min3, true));
                        }
                        else
                        {
                            int max3 = -1;
                            if (!split[2].Equals("max") && !split[2].Equals("MAX"))
                            {
                                max3 = int.Parse(split[2]);
                            }
                            list.Add(new Balise(name3, min3, max3, false));
                        }
                        break;
                    case 4:
                        // "nom min max *" (option checkProfondeur) 
                        string name4 = split[0];
                        int min4 = int.Parse(split[1]);
                        int max4 = -1;
                        if (!split[2].Equals("max") && !split[2].Equals("MAX"))
                        {
                            max4 = int.Parse(split[2]);
                        }
                        list.Add(new Balise(name4, min4, max4, true));
                        break;
                    default:
                        throw new Exception();
                }

            }
            str.Close();
            WebAPI_final.Models.Constantes.displayDEBUG("end readSchema", 3);

            return list;
        }

        /// <summary> Lecture du fichier </summary>
        /// <param name="list"> Liste des balises à chercher </param>
        /// <returns> DataSet comprenant les données voulues </returns>
        private DataSet readFile(List<Balise> list)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start readFile", 3);

            // Lecture fichier
            StreamReader str = new StreamReader(_Filepath);
            string contenu = str.ReadToEnd();
            str.Close();

            // Création du DataSet
            ds = new DataSet();
            foreach (Balise b in list)
            {
                DataTable dt = new DataTable();

                DataColumn[] dataColumns = new DataColumn[2];

                dataColumns[0] = new DataColumn("Content", System.Type.GetType("System.String"));
                dt.Columns.Add(dataColumns[0]);
                dataColumns[1] = new DataColumn("LineParent", System.Type.GetType("System.Int32"));
                dt.Columns.Add(dataColumns[1]);

                ds.Tables.Add(dt);
            }


            // Récupération des données
            int profondeur = 0;
            seek(ref contenu, list, ref profondeur, 0, 0);

            WebAPI_final.Models.Constantes.displayDEBUG("end readFile", 3);

            return ds;
        }

        /// <summary> 
        /// Recherche des données 
        /// fonction récursive
        /// </summary>
        /// <param name="contenu"> string à parser </param>
        /// <param name="list"> Liste des balises </param>
        /// <param name="profondeur"> profondeur actuelle </param>
        /// <param name="profondeurMin"> profondeur minimal </param>
        /// <param name="nbBaliseFather"> numéro de la balise du parent </param>
        private void seek(ref string contenu, List<Balise> list, ref int profondeur, int profondeurMin, int nbBaliseFather)
        {
            WebAPI_final.Models.Constantes.displayDEBUG("start seek prof : " + profondeur, 4);

            // Si plus de Balise à rechercher, il n'y a plus rien à faire
            if (list.Count != 0)
            {
                Balise b = list.First();
                int nbBalise = 0;

                // Tant qu'on n'est pas remonté à la balise parent, on continue
                while (profondeur >= profondeurMin)
                {
                    // Si on a épuisé le contenu, Alors on arrête
                    if (contenu.IndexOf("<") == -1 && profondeur == 0)
                    {
                        return;
                    }

                    // S'il n'y plus de balise, Alors on lève une exception
                    if (contenu.IndexOf("<") == -1)
                    {
                        throw new Exception();
                    }

                    contenu = contenu.Substring(contenu.IndexOf("<") + 1);

                    // On regarde le type de Balise
                    if (contenu.IndexOf("!--") == 0)
                    {
                        // Commentaire
                        contenu = contenu.Substring(contenu.IndexOf("-->") + 3);
                    }
                    else if (contenu.IndexOf("!") == 0)
                    {
                        // En-tete
                        contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                    }
                    else if (contenu.IndexOf("/") == 0)
                    {
                        // Fermeture
                        contenu = contenu.Substring(1);

                        profondeur--;
                        contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                    }
                    else
                    {
                        // Ouverture
                        // On regarde le Nom de la balise
                        string name = getName(contenu);

                        // Type de balise (avec ou sans contenu)
                        int index1 = contenu.IndexOf("/>");
                        int index2 = contenu.IndexOf(">");

                        if (contenu.IndexOf("/>") > contenu.IndexOf(">") || contenu.IndexOf("/>") == -1)
                        {
                            // Balise avec contenu
                            profondeur++;
                            contenu = contenu.Substring(contenu.IndexOf(">") + 1);

                            // Balise recherchée
                            if (b.name == name)
                            {
                                // On regarde si on désire ce bloc ou non
                                if (b.min <= nbBalise && (nbBalise <= b.max || b.max == -1))
                                {
                                    int index = ds.Tables.Count - list.Count;
                                    DataRow dr = ds.Tables[index].NewRow();

                                    // Si on est arrivé à la dernière balise, Alors on insère le contenu
                                    if (list.Count == 1)
                                    {
                                        if (b.checkProfondeur)
                                        {
                                            string s = skipProfondeur(ref contenu, ref profondeur);
                                            dr[0] = s;
                                        }
                                        else
                                        {
                                            dr[0] = contenu.Substring(0, contenu.IndexOf("<"));
                                        }
                                    }
                                    dr[1] = nbBaliseFather; // référence à la balise parent
                                    ds.Tables[index].Rows.Add(dr);

                                    // On enlève la balise trouvée de la liste
                                    // et on lance la recherche récursive
                                    List<Balise> list2 = new List<Balise>(list);
                                    list2.Remove(b);
                                    seek(ref contenu, list2, ref profondeur, profondeur, nbBalise);
                                }
                                else
                                {
                                    // On skip le bloc non voulu
                                    skip(ref contenu, name);
                                    profondeur--;
                                }

                                nbBalise++;
                            }
                            else
                            {
                                // On skip le bloc non voulu
                                skip(ref contenu, name);
                                profondeur--;
                            }
                        }
                        else
                        {
                            // Balise sans contenu
                            contenu = contenu.Substring(contenu.IndexOf("/>") + 2);
                        }
                    }
                }
            }
            WebAPI_final.Models.Constantes.displayDEBUG("end seek prof : " + profondeur, 4);
        }

        /// <summary> 
        /// Fonction permettant de passer les balises intérieures
        /// Récupère uniquement le contenu
        /// </summary>
        /// <param name="contenu"> string à parser </param>
        /// <param name="profondeur"> profondeur actuelle </param>
        /// <returns> renvoie le contenu </returns>
        private string skipProfondeur(ref string contenu, ref int profondeur)
        {
            string res = "";
            int profLocal = 0;
            bool needSkip = true;

            // On parse jusqu'au contenu
            while (needSkip)
            {
                // S'il n'y plus de balise, Alors on lève une exception
                if (contenu.IndexOf("<") == -1)
                {
                    throw new Exception();
                }

                string s = contenu.Substring(0, contenu.IndexOf("<"));

                contenu = contenu.Substring(contenu.IndexOf("<") + 1);

                // On regarde le type de la balise
                if (contenu.IndexOf("!--") == 0)
                {
                    // Commentaire
                    contenu = contenu.Substring(contenu.IndexOf("-->") + 3);
                }
                else if (contenu.IndexOf("!") == 0)
                {
                    // En-tete
                    contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                }
                else if (contenu.IndexOf("/") == 0)
                {
                    // Fermeture
                    contenu = contenu.Substring(1);

                    // On stock le résultat
                    res = s;
                    needSkip = false;

                    profLocal--;
                    profondeur--;
                    contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                }
                else
                {
                    // Ouverture

                    // Type de balise (avec ou sans contenu)
                    int index1 = contenu.IndexOf("/>");
                    int index2 = contenu.IndexOf(">");

                    if (contenu.IndexOf("/>") > contenu.IndexOf(">") || contenu.IndexOf("/>") == -1)
                    {
                        // Balise avec contenu
                        profLocal++;
                        profondeur++;
                        contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                    }
                    else
                    {
                        // Balise sans contenu
                        contenu = contenu.Substring(contenu.IndexOf("/>") + 2);
                    }
                }
            }

            // On parse les fermetures de balise
            while (profLocal > 0)
            {
                // S'il n'y plus de balise, Alors on lève une exception
                if (contenu.IndexOf("<") == -1)
                {
                    throw new Exception();
                }
                contenu = contenu.Substring(contenu.IndexOf("<") + 1);

                // On regarde le type de la balise
                if (contenu.IndexOf("!--") == 0)
                {
                    // Commentaire
                    contenu = contenu.Substring(contenu.IndexOf("-->") + 3);
                }
                else if (contenu.IndexOf("!") == 0)
                {
                    // En-tete
                    contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                }
                else if (contenu.IndexOf("/") == 0)
                {
                    // Fermeture
                    contenu = contenu.Substring(1);

                    profLocal--;
                    profondeur--;
                    contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                }
                else
                {
                    // Ouverture

                    // Type de balise (avec ou sans contenu)
                    int index1 = contenu.IndexOf("/>");
                    int index2 = contenu.IndexOf(">");

                    if (contenu.IndexOf("/>") > contenu.IndexOf(">") || contenu.IndexOf("/>") == -1)
                    {
                        // Balise avec contenu
                        profLocal++;
                        profondeur++;
                        contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                    }
                    else
                    {
                        // Balise sans contenu
                        contenu = contenu.Substring(contenu.IndexOf("/>") + 2);
                    }
                }
            }

            return res;
        }

        /// <summary> 
        /// Fonction permettant de passer la balise en cours
        /// </summary>
        /// <param name="contenu"> string à parser </param>
        /// <param name="nameBalise"> nom de la balise </param>
        private void skip(ref string contenu, string nameBalise)
        {
            int profondeur = 0;

            while (profondeur >= 0)
            {
                // S'il n'y plus de balise, Alors on lève une exception
                if (contenu.IndexOf("<") == -1)
                {
                    throw new Exception();
                }

                contenu = contenu.Substring(contenu.IndexOf("<") + 1);

                // On regarde le type de balise
                if (contenu.IndexOf("!--") == 0)
                {
                    contenu = contenu.Substring(contenu.IndexOf("-->") + 3);
                }
                else if (contenu.IndexOf("!") == 0)
                {
                    contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                }
                else if (contenu.IndexOf("/") == 0)
                {
                    // Fermeture
                    contenu = contenu.Substring(1);

                    profondeur--;
                    contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                }
                else
                {
                    // Ouverture

                    // Type de balise (avec ou sans contenu)
                    int index1 = contenu.IndexOf("/>");
                    int index2 = contenu.IndexOf(">");

                    if (contenu.IndexOf("/>") > contenu.IndexOf(">") || contenu.IndexOf("/>") == -1)
                    {
                        // Balise avec contenu
                        profondeur++;
                        contenu = contenu.Substring(contenu.IndexOf(">") + 1);
                    }
                    else
                    {
                        // Balise sans contenu
                        contenu = contenu.Substring(contenu.IndexOf("/>") + 2);
                    }
                }
            }
        }

        /// <summary> 
        /// Retourne le premier mot du paramètre
        /// </summary>
        /// <param name="contenu"> string à parser </param>
        private String getName(string contenu)
        {
            // On regarde jusqu'où va le nom
            int index1 = contenu.IndexOf(" ");
            int index2 = contenu.IndexOf(">");

            int index = 0;
            if (index2 < index1 || index1 == -1)
            {
                index = index2;
            }
            else
            {
                index = index1;
            }

            // Récupère le nom
            string name = contenu.Substring(0, index);

            return name;
        }

        /// <summary>
        /// Fonction de Debug
        /// </summary>
        private void displayDataSet()
        {
            int j = ds.Tables.Count - 1;
            int n = ds.Tables[j].Rows.Count;
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(ds.Tables[j].Rows[i][0] + " - " + ds.Tables[j].Rows[i][1]);
            }
        }
        #endregion
    }
}
