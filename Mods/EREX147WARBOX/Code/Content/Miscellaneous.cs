using System.Collections.Generic;
using ai.behaviours;
using WarBox.Behaviour;

namespace WarBox.Content;

internal static class WarBoxMisc
{
    public static void Init()
    {
        AddNameGenerators();
        AddTasks();
        AddDecisions();
    }

    private static void AddNameGenerators()
    {
        NameGeneratorAsset gun_namegenerator = new NameGeneratorAsset
        {
            id = "gun_name"
        };
        gun_namegenerator.addPartGroup("M,MG,SG,G,MP,AK,XM,P,HK,RPG,FIM,AEK,AN,FAMAS,UMP,AR,LR,SR,SMG,LMG,SCAR,MAC,TEC,PM,PMM,USP,PP,PPK,AWP,AWM,TAR,RPK,PPSh,DP");
        gun_namegenerator.addPartGroup("-");
        gun_namegenerator.addPartGroup("1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100");
        gun_namegenerator.addPartGroup("A1,A2,A3,A4,MK1,MK2,MK3,MK4,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
        gun_namegenerator.addTemplate("Part_group");
        AssetManager.name_generator.add(gun_namegenerator);

        CreateNameSet(new NameGeneratorAsset
        {
            id = "tank_name",
            onomastics_templates = new List<string> { "`0_###u|0:tank`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "apc_name",
            onomastics_templates = new List<string> { "`0_###u|0:apc`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "ifv_name",
            onomastics_templates = new List<string> { "`0_###u|0:ifv`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "spg_name",
            onomastics_templates = new List<string> { "`0_###u|0:spg`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "heli_name",
            onomastics_templates = new List<string> { "`0_###u|0:heli`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "bomber_name",
            onomastics_templates = new List<string> { "`0_###u|0:bomber`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "fighter_name",
            onomastics_templates = new List<string> { "`0_###u|0:fighter`" }
        });

        CreateNameSet(new NameGeneratorAsset
        {
            id = "boat_name",
            onomastics_templates = new List<string> { "`0_###u|0:ns`" }
        });
    }

    private static void AddTasks()
    {
        BehaviourTaskActor artillery_task = new BehaviourTaskActor
        {
            id = "artillery_strike"
        };
        artillery_task.setIcon("ui/icons/actors/spg");
        artillery_task.addBeh(new BehArtilleryAttack());
        artillery_task.addBeh(new BehEndJob());
        AssetManager.tasks_actor.add(artillery_task);

        BehaviourTaskActor ship_attack = new BehaviourTaskActor
        {
            id = "ship_attack"
        };
        ship_attack.setIcon("ui/icons/actors/spg");
        ship_attack.addBeh(new BehWarBoatFindTarget());
        ship_attack.addBeh(new BehGoToTileTarget());
        ship_attack.addBeh(new BehWarBoatAttack());
        ship_attack.addBeh(new BehEndJob());
        AssetManager.tasks_actor.add(ship_attack);
    }

    private static void AddDecisions()
    {
        DecisionAsset artillery_strike = new DecisionAsset
        {
            id = "artillery_strike",
            priority = NeuroLayer.Layer_4_Critical,
            unique = true,
            cooldown = 10,
            weight = 1f,
            path_icon = "ui/icons/actors/spg",
            task_id = "artillery_strike",
            action_check_launch = delegate (Actor pActor)
            {
                if (pActor == null) return false;
                if (!pActor.isAlive() || !pActor.kingdom.hasEnemies()) return false;
                if (pActor.getStamina() < 100) return false;
                return true;
            }
        };
        AssetManager.decisions_library.add(artillery_strike);

        DecisionAsset ship_attack_decision = new DecisionAsset
        {
            id = "ship_attack",
            priority = NeuroLayer.Layer_4_Critical,
            path_icon = "ui/icons/actors/spg",
            cooldown = 1,
            unique = true,
            weight = 1f,
        };
        AssetManager.decisions_library.add(ship_attack_decision);
    }

    private static void CreateNameSet(NameGeneratorAsset name_generator)
    {
        AssetManager.name_generator.add(name_generator);
        string id = name_generator.id;
        AssetManager.name_sets.add(new NameSetAsset
        {
            id = id,
            city = id,
            clan = id,
            culture = id,
            family = id,
            kingdom = id,
            language = id,
            unit = id,
            religion = id
        });
    }
}