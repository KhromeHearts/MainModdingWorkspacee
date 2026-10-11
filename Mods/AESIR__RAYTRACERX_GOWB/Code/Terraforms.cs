using System;
using System.Threading;
using NCMS;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngine;
using ReflectionUtility;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Beebyte.Obfuscator;

namespace GOWB
{
    class Terraforms
    {
        public static void Init()
        {
            if (AssetManager.terraform.get("Drifa curse") != null && AssetManager.terraform.get("Flameball impact") != null)
            {
                return;
            }

            //TerraformOptions details
            if (AssetManager.terraform.get("Drifa curse") == null)
            {
                TerraformOptions Drifacurse = new TerraformOptions();
                Drifacurse.id = "Drifa curse";
                Drifacurse.add_trait = "cursed";
                Drifacurse.damage = 50;
                AssetManager.terraform.add(Drifacurse);
            }

            if (AssetManager.terraform.get("Flameball impact") == null)
            {
                TerraformOptions Flameballimpact = AssetManager.terraform.clone("Flameball impact", "grenade");
                Flameballimpact.id = "Flameball impact";
                Flameballimpact.damage_buildings = true;
                Flameballimpact.damage = 200;
                Flameballimpact.shake = false;
                AssetManager.terraform.add(Flameballimpact);
            }


        }
    }
}