using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpatriatePostingLinksV1
{
    class cVehicle : cPosting
    {
        enum eVehicleTransmissionType
        {
            None =0,
            Automatic = 1,
            Manual = 2
        }

        string sMake = string.Empty;
        string sModel = string.Empty;
        int iYear = 0;
        double dKM = 0;
        string sTransmission = string.Empty;
        eVehicleTransmissionType eTransmission = eVehicleTransmissionType.None;

        public cVehicle(string rawHTML,string rawDate):base(rawHTML,rawDate)
        {
            ParseForVehicle();
        }

        public cVehicle(string rawHTML):base(rawHTML)
        {
            ParseForVehicle();
        }       
        
        private void ParseForVehicle()
        {
            if(sPostingDesc == string.Empty) { return; }

            if(sCategory != "vehicles") { return; }

            string[] saVehicle = { "" };
            bool bHasPrice = false;

            if (sPostingDesc.Contains("/") && sPostingDesc.ToLower().Contains("bhd"))
            {
                saVehicle = sPostingDesc.Split("/".ToCharArray());
                bHasPrice = true;
            }
            else
                saVehicle = sPostingDesc.Split(",".ToCharArray());

            string[] saVehicleParts = { "" };

            if (bHasPrice)            
                saVehicleParts = saVehicle[1].Split(",".ToCharArray());          
            else
                saVehicleParts = saVehicle;

            // get make and model
            string[] saMakeModel = saVehicleParts[0].Split(" ".ToCharArray());
            if (saMakeModel.Length == 2)
            {
                sMake = saMakeModel[0];
                sModel = saMakeModel[1];
            }
            else
            { sMake = saMakeModel[0]; }

            // year
            if (saVehicleParts.Length >= 2)
            {
                int.TryParse(saVehicleParts[1], out iYear);
            }

            // transmission type
            if(saVehicleParts.Length >= 3)
            {
                if(saVehicleParts[2].ToLower().Contains("automatic"))
                {
                    sTransmission = "Automatic";
                    eTransmission = eVehicleTransmissionType.Automatic;
                }
                else if (saVehicleParts[2].ToLower().Contains("manual"))
                {
                    sTransmission = "Manual";
                    eTransmission = eVehicleTransmissionType.Manual;
                }
            }

            // transmission type
            if (saVehicleParts.Length >= 4)
            {
                if (saVehicleParts[3].ToLower().Contains("km"))
                {
                    string sKM = saVehicleParts[3].ToLower().Replace("km", "");
                    sKM = sKM.Trim();
                    double.TryParse(sKM, out dKM);
                }                
            }
            
        }
        

        
    }
}
