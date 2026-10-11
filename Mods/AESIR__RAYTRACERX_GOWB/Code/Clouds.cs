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
using Beebyte.Obfuscator;

namespace GOWB
{
	class Clouds
	{
		public static void Init()
		{
			if (AssetManager.clouds.get("ThunderCloud") != null || AssetManager.clouds.get("DraupnirCloud") != null)
			{
				return;
			}

			//Cloud details
			CloudAsset ThunderCloud = new CloudAsset();
			ThunderCloud.id = "ThunderCloud";
			ThunderCloud.color_hex = "#445366";
			ThunderCloud.cloud_action_1 = new CloudAction(GodLightning);
			ThunderCloud.path_sprites = new string[]
			{
				"effects/clouds/cloud_big_1",
				"effects/clouds/cloud_big_2"
			};
			ThunderCloud.speed_min = 0.7f;
			ThunderCloud.speed_max = 0.7f;
			ThunderCloud.considered_disaster = true;
			AssetManager.clouds.add(ThunderCloud);

			CloudAsset DraupnirCloud = new CloudAsset();
			DraupnirCloud.id = "DraupnirCloud";
			DraupnirCloud.color_hex = "#FFFFFF";
			DraupnirCloud.max_alpha = 0.01f;
			DraupnirCloud.drop_id = "Draupdrop";
			DraupnirCloud.cloud_action_1 = new CloudAction(DraupdropCloudAction);
			DraupnirCloud.path_sprites = new string[]
			{
				"effects/clouds/cloud_small_2"
			};
			DraupnirCloud.speed_min = 0.7f;
			DraupnirCloud.speed_max = 0.7f;
			DraupnirCloud.considered_disaster = true;
			AssetManager.clouds.add(DraupnirCloud);

			//Cloud Postfix
			ThunderCloud.cached_sprites = ThunderCloud.path_sprites
				.Select(SpriteTextureLoader.getSprite)
				.Where(sprite => sprite != null)
				.ToArray();

			DraupnirCloud.cached_sprites = DraupnirCloud.path_sprites
				.Select(SpriteTextureLoader.getSprite)
				.Where(sprite => sprite != null)
				.ToArray();
		}

		private static void DraupdropCloudAction(Cloud pCloud)
		{
			if (pCloud == null || pCloud.asset == null || pCloud.alive_time < 3f)
			{
				return;
			}

			DropAsset draupdrop = AssetManager.drops.get("Draupdrop");
			if (draupdrop == null)
			{
				return;
			}

			float effectTextureWidth = pCloud.effect_texture_width;
			float effectTextureHeight = pCloud.effect_texture_height;
			float shadowOffsetY = pCloud.spriteShadow != null ? pCloud.spriteShadow.offset.y : 0f;

			int x = (int)pCloud.transform.localPosition.x;
			int y = (int)pCloud.transform.localPosition.y;
			x += (int)Randy.randomFloat(-effectTextureWidth, effectTextureWidth);
			y += (int)Randy.randomFloat(-effectTextureHeight + shadowOffsetY, effectTextureHeight + shadowOffsetY);

			WorldTile tile = World.world.GetTile(x, y);
			if (tile != null)
			{
				World.world.drop_manager.spawn(tile, draupdrop, 10f, -1f, false, -1L);
			}

			// DelayedKillCloud(pCloud, 30f);
		}

		public static IEnumerator DelayedKillCloud(Cloud pCloud, float delay)
        {
			if (pCloud == null || pCloud.asset == null || pCloud.alive_time < 3f)
			{
				yield break;
			}
            yield return new WaitForSeconds(delay);
            pCloud._lifespan = 1f;
        }

		public static void GodLightning(Cloud pCloud)
		{
			if (!Randy.randomChance(0.1f))
			{
				return;
			}
			int num = (int)pCloud.transform.localPosition.x;
			int num2 = (int)pCloud.transform.localPosition.y;
			float effect_texture_width = pCloud.effect_texture_width;
			float effect_texture_height = pCloud.effect_texture_height;
			num += (int)Randy.randomFloat(-effect_texture_width, effect_texture_width);
			num2 += (int)Randy.randomFloat(-effect_texture_height + pCloud.spriteShadow.offset.y, effect_texture_height + pCloud.spriteShadow.offset.y);
			WorldTile tile = World.world.GetTile(num, num2);
			if (tile != null)
			{
				if (Randy.randomBool())
				{
					MapBox.spawnLightningMedium(tile, 0.15f);
					return;
				}
				MapBox.spawnLightningSmall(tile, 0.15f);
			}
		}
	}
}