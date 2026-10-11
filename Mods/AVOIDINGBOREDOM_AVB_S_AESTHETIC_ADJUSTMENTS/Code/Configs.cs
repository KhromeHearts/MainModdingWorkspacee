using System;
using NeoModLoader;

namespace aaaMod{
    internal static class Configs{
        public static bool UnevoColor { get; set; } = false;
        public static bool EvoColor { get; set; } = false;
        public static bool PhenoSort { get; set; } = false;
        public static bool PhenoNameplates { get; set; } = false;

        public static void TurnOffUnevoColor(bool pCurrentValue)
        {
            if (pCurrentValue){
                UnevoColor = true;
            }
            else{
                PhenoSort = false;
            }
        }
        public static void TurnOffEvoColor(bool pCurrentValue)
        {
            if (pCurrentValue){
                EvoColor = true;
            }
            else{
                PhenoSort = false;
            }
        }
        public static void SortPheno(bool pCurrentValue)
        {
            if (pCurrentValue){
                PhenoSort = true;
            }
            else{
                PhenoSort = false;
            }
        }
        public static void CheckPhenoNameplates(bool pCurrentValue)
        {
            if (pCurrentValue){
                PhenoNameplates = true;
            }
            else{
                PhenoNameplates = false;
            }
        }
    }
}




