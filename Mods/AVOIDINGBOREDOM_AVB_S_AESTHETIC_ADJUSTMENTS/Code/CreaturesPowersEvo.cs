using UnityEngine;
using NeoModLoader.api;

namespace aaaMod{
    internal static class CreaturesPowersEvo{
        public static void init(){
//Cat
            ActorAsset editCatActor = AssetManager.actor_library.get("civ_cat");
            editCatActor.icon = "civ_catNew";

            GodPower editCatPower = AssetManager.powers.get("civ_cat");
            editCatPower.path_icon  = "civ_catNew";
//Dog
            ActorAsset editDogActor = AssetManager.actor_library.get("civ_dog");
            editDogActor.icon = "civ_dogNew";

            GodPower editDogPower = AssetManager.powers.get("civ_dog");
            editDogPower.path_icon  = "civ_dogNew";
//Chicken
            ActorAsset editChickenActor = AssetManager.actor_library.get("civ_chicken");
            editChickenActor.icon = "civ_chickenNew";

            GodPower editChickenPower = AssetManager.powers.get("civ_chicken");
            editChickenPower.path_icon  = "civ_chickenNew";
//Rabbit
            ActorAsset editRabbitActor = AssetManager.actor_library.get("civ_rabbit");
            editRabbitActor.icon = "civ_rabbitNew";

            GodPower editRabbitPower = AssetManager.powers.get("civ_rabbit");
            editRabbitPower.path_icon  = "civ_rabbitNew";
//Monkey
            ActorAsset editMonkeyActor = AssetManager.actor_library.get("civ_monkey");
            editMonkeyActor.icon = "civ_monkeyNew";

            GodPower editMonkeyPower = AssetManager.powers.get("civ_monkey");
            editMonkeyPower.path_icon  = "civ_monkeyNew";
//Fox
            ActorAsset editFoxActor = AssetManager.actor_library.get("civ_fox");
            editFoxActor.icon = "civ_foxNew";

            GodPower editFoxPower = AssetManager.powers.get("civ_fox");
            editFoxPower.path_icon  = "civ_foxNew";
//Sheep
            ActorAsset editSheepActor = AssetManager.actor_library.get("civ_sheep");
            editSheepActor.icon = "civ_sheepNew";

            GodPower editSheepPower = AssetManager.powers.get("civ_sheep");
            editSheepPower.path_icon  = "civ_sheepNew";
//Cow
            ActorAsset editCowActor = AssetManager.actor_library.get("civ_cow");
            editCowActor.icon = "civ_cowNew";

            GodPower editCowPower = AssetManager.powers.get("civ_cow");
            editCowPower.path_icon  = "civ_cowNew";
//Armadillo (skipped)
//Raccoon
            ActorAsset editRaccoonActor = AssetManager.actor_library.get("bandit");
            editRaccoonActor.icon = "iconBanditNew";

            GodPower editRaccoonPower = AssetManager.powers.get("bandit");
            editRaccoonPower.path_icon  = "iconBanditNew";
//-------------------------------------------------------------------------------------------------------
//Wolf
            ActorAsset editWolfActor = AssetManager.actor_library.get("civ_wolf");
            editWolfActor.icon = "civ_wolfNew";

            GodPower editWolfPower = AssetManager.powers.get("civ_wolf");
            editWolfPower.path_icon  = "civ_wolfNew";
//Bear
            ActorAsset editBearActor = AssetManager.actor_library.get("civ_bear");
            editBearActor.icon = "civ_bearNew";

            GodPower editBearPower = AssetManager.powers.get("civ_bear");
            editBearPower.path_icon  = "civ_bearNew";
//Rhino
            ActorAsset editRhinoActor = AssetManager.actor_library.get("civ_rhino");
            editRhinoActor.icon = "civ_rhinoNew";

            GodPower editRhinoPower = AssetManager.powers.get("civ_rhino");
            editRhinoPower.path_icon  = "civ_rhinoNew";
//Buffalo
            ActorAsset editBuffaloActor = AssetManager.actor_library.get("civ_buffalo");
            editBuffaloActor.icon = "civ_buffaloNew";

            GodPower editBuffaloPower = AssetManager.powers.get("civ_buffalo");
            editBuffaloPower.path_icon  = "civ_buffaloNew";
//Hyena
            ActorAsset editHyenaActor = AssetManager.actor_library.get("civ_hyena");
            editHyenaActor.icon = "civ_hyenaNew";

            GodPower editHyenaPower = AssetManager.powers.get("civ_hyena");
            editHyenaPower.path_icon  = "civ_hyenaNew";
//Rat
            ActorAsset editRatActor = AssetManager.actor_library.get("civ_rat");
            editRatActor.icon = "civ_ratNew";

            GodPower editRatPower = AssetManager.powers.get("civ_rat");
            editRatPower.path_icon  = "civ_ratNew";
//Alpaca
            ActorAsset editAlpacaActor = AssetManager.actor_library.get("civ_alpaca");
            editAlpacaActor.icon = "civ_alpacaNew";

            GodPower editAlpacaPower = AssetManager.powers.get("civ_alpaca");
            editAlpacaPower.path_icon  = "civ_alpacaNew";
//Capybara
            ActorAsset editCapybaraActor = AssetManager.actor_library.get("civ_capybara");
            editCapybaraActor.icon = "civ_capybaraNew";

            GodPower editCapybaraPower = AssetManager.powers.get("civ_capybara");
            editCapybaraPower.path_icon  = "civ_capybaraNew";
//Penguin
            ActorAsset editPenguinActor = AssetManager.actor_library.get("civ_penguin");
            editPenguinActor.icon = "civ_penguinNew";

            GodPower editPenguinPower = AssetManager.powers.get("civ_penguin");
            editPenguinPower.path_icon  = "civ_penguinNew";
//-------------------------------------------------------------------------------------------------------
//Scorpion
            ActorAsset editScorpionActor = AssetManager.actor_library.get("civ_scorpion");
            editScorpionActor.icon = "civ_scorpionNew";

            GodPower editScorpionPower = AssetManager.powers.get("civ_scorpion");
            editScorpionPower.path_icon  = "civ_scorpionNew";
//Turtle
            ActorAsset editTurtleActor = AssetManager.actor_library.get("civ_turtle");
            editTurtleActor.icon = "civ_turtleNew";

            GodPower editTurtlePower = AssetManager.powers.get("civ_turtle");
            editTurtlePower.path_icon  = "civ_turtleNew";
//Crocodile
            ActorAsset editCrocodileActor = AssetManager.actor_library.get("civ_crocodile");
            editCrocodileActor.icon = "civ_crocodileNew";

            GodPower editCrocodilePower = AssetManager.powers.get("civ_crocodile");
            editCrocodilePower.path_icon  = "civ_crocodileNew";
//Snake
            ActorAsset editSnakeActor = AssetManager.actor_library.get("civ_snake");
            editSnakeActor.icon = "civ_snakeNew";

            GodPower editSnakePower = AssetManager.powers.get("civ_snake");
            editSnakePower.path_icon  = "civ_snakeNew";
//Frog
            ActorAsset editFrogActor = AssetManager.actor_library.get("civ_frog");
            editFrogActor.icon = "civ_frogNew";

            GodPower editFrogPower = AssetManager.powers.get("civ_frog");
            editFrogPower.path_icon  = "civ_frogNew";
//Piranha
            ActorAsset editPiranhaActor = AssetManager.actor_library.get("civ_piranha");
            editPiranhaActor.icon = "civ_piranhaNew";

            GodPower editPiranhaPower = AssetManager.powers.get("civ_piranha");
            editPiranhaPower.path_icon  = "civ_piranhaNew";
        }
    }
}




