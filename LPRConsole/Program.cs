using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPRConsole
{
    class Program
    {


        static void Main(string[] args)
        {

            LPR lpr = new LPR();            

            foreach (string p in args)
            {
                lpr.RecognizeNumber(p);
            }            
        }
    }
}
