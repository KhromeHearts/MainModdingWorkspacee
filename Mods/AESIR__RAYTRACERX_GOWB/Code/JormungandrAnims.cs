using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOWB;

internal static class JormOverrideSprite
{
    public static void init()
    {
        var asset = AssetManager.actor_library.get("Jormungandr");
        asset.animation_speed_based_on_walk_speed = true;
        asset.visible_on_minimap = true;
        asset.has_override_sprite = true;
        asset.get_override_sprite = get_jorm_override_sprite;
        GOWB.Units.setDefaultResourcePools(asset);
        AssetManager.actor_library.loadTexturesAndSprites(asset);
    }

    private static string[] walk_sprites = new[]
    {
        "actors/Jormungandr/walk_1", 
        "actors/Jormungandr/walk_2",
        "actors/Jormungandr/walk_3", 
        "actors/Jormungandr/walk_4",
        "actors/Jormungandr/walk_5",
        "actors/Jormungandr/walk_6",
        "actors/Jormungandr/walk_7",
        "actors/Jormungandr/walk_8"
    };
    private static string[] swim_sprites = new[]
    {
        "actors/Jormungandr/swim_2",
        "actors/Jormungandr/swim_2",
        "actors/Jormungandr/swim_2",
        "actors/Jormungandr/swim_3",
        "actors/Jormungandr/swim_3",
        "actors/Jormungandr/swim_3",
        "actors/Jormungandr/swim_4",
        "actors/Jormungandr/swim_4",
        "actors/Jormungandr/swim_4",
        "actors/Jormungandr/swim_5",
        "actors/Jormungandr/swim_5"
    };
    private static string[] idle_sprites = new[]
    {
        "actors/Jormungandr/walk_2"
    };
    private static string[] attack_sprites = new[]
    {
        "effects/Jormungandr_Attack/SBiteAttack_1",
        "effects/Jormungandr_Attack/SBiteAttack_2",
        "effects/Jormungandr_Attack/SBiteAttack_3",
        "effects/Jormungandr_Attack/SBiteAttack_4",
        "effects/Jormungandr_Attack/SBiteAttack_5",
        "effects/Jormungandr_Attack/SBiteAttack_6",
        "effects/Jormungandr_Attack/SBiteAttack_7", 
        "effects/Jormungandr_Attack/SBiteAttack_8"
    };
    private static string[] attackw_sprites = new[]
    {
        "effects/Jormungandr_Attack_Water/SBiteAttack_1",
        "effects/Jormungandr_Attack_Water/SBiteAttack_2",
        "effects/Jormungandr_Attack_Water/SBiteAttack_3",
        "effects/Jormungandr_Attack_Water/SBiteAttack_4",
        "effects/Jormungandr_Attack_Water/SBiteAttack_5",
        "effects/Jormungandr_Attack_Water/SBiteAttack_6",
        "effects/Jormungandr_Attack_Water/SBiteAttack_7", 
        "effects/Jormungandr_Attack_Water/SBiteAttack_8"
    };
    private static Sprite get_jorm_override_sprite(Actor actor)
    {
        actor.data.get("last_anim_frame_idx", out int last_frame);
        actor.data.get("last_anim_state", out string last_anim_state);
		actor.data.get("last_anim_timer", out float last_anim_timer);

        string frame_path = idle_sprites[0];
        int curr_frame = last_frame;
        string curr_anim_state = last_anim_state;

		last_anim_timer += Time.deltaTime;
		if (last_anim_timer > 0.1f)
		{
			last_anim_timer = 0;
			curr_frame++;
		}
		actor.data.set("last_anim_timer", last_anim_timer);

        //Check if Game is paused or actor is frozen then return to first frame of current anim state
        if (actor.is_ai_frozen || Config.paused || MapBox.instance.isGameplayControlsLocked())
        {
            if (actor.isAffectedByLiquid())
            {
                frame_path = swim_sprites[0];
            }
            else if (actor.is_moving)
            {
                frame_path = walk_sprites[0];
            }
            else
            {
                frame_path = idle_sprites[0];
            }
            // return get_single_sprite(active_frame);
            return get_single_sprite(frame_path);
        }
        
        // Check attack at first
        if (Mathf.Abs(actor.attack_timer) < 0.01f)
        {// Just begin attack action
            curr_frame = 0;
            curr_anim_state = "attack";
        }
        if (actor.attack_timer > 0 && curr_anim_state == "attack")
        {// Continue play the attack animation
          if (actor.isAffectedByLiquid())
          {
            if (curr_frame >= attackw_sprites.Length)
            {// attack animation finishes. Turn to idle
                // do nothing, it can be done below
            }
            else
            {
                frame_path = attackw_sprites[curr_frame];

                actor.data.set("last_anim_frame_idx", curr_frame);
                actor.data.set("last_anim_state", curr_anim_state);
                return get_single_sprite(frame_path);
            }
          }
          else
          {
            if (curr_frame >= attack_sprites.Length)
            {// attack animation finishes. Turn to idle
                // do nothing, it can be done below
            }
            else
            {
                frame_path = attack_sprites[curr_frame];

                actor.data.set("last_anim_frame_idx", curr_frame);
                actor.data.set("last_anim_state", curr_anim_state);
                return get_single_sprite(frame_path);
            }
          }
        }

        if (actor.isAffectedByLiquid())
        {
            curr_anim_state = "swim";
        }
        else if (actor.is_moving)
        {
            curr_anim_state = "walk";
        }
        else
        {
            curr_anim_state = "idle";
        }

        switch (curr_anim_state)
        {
            case "walk":
                curr_frame %= walk_sprites.Length;
                frame_path = walk_sprites[curr_frame];
                break;
            case "swim":
                curr_frame %= swim_sprites.Length;
                frame_path = swim_sprites[curr_frame];
                break;
            case "idle":
                curr_frame %= idle_sprites.Length;
                frame_path = idle_sprites[curr_frame];
                break;
        }
        
        actor.data.set("last_anim_frame_idx", curr_frame);
        actor.data.set("last_anim_state",     curr_anim_state);
        return get_single_sprite(frame_path);
    }

    private static Sprite get_single_sprite(string path)
    {
        var components = path.Split('/');

        var folder_builder = new StringBuilder();
        folder_builder.Append(components[0]);
        for (int i = 1; i < components.Length - 1; i++)
        {
            folder_builder.Append('/');
            folder_builder.Append(components[i]);
        }

        var sprite_list = SpriteTextureLoader.getSpriteList(folder_builder.ToString());
        return sprite_list.FirstOrDefault(x => x.name == components.Last());
    }
}