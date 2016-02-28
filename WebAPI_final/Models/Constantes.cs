using System;

namespace WebAPI_final.Models
{
    static public class Constantes
    {
        static public int DEBUG = 0;

        static public void displayDEBUG(string msg, int lvl)
        {
            if (DEBUG > lvl)
            {
                string s = "";
                for (int i = 0; i < lvl; i++)
                {
                    s += "      ";
                }
                s += msg;

                Console.WriteLine(s);
            }
        }
    }

}
