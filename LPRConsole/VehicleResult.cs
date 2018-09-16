using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPRConsole
{
    

    class VehicleResult
    {
        public string number { get; set; }
        public Int16 listID { get; set; }
        public Int16 imageID { get; set; }

        public VehicleResult(string num,Int16 listid,Int16 imageid)
        {
            this.number = num;
            this.listID = listid;
            this.imageID = imageid;
        }
        
    }
}
