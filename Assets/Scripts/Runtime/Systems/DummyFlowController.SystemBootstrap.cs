using System;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void EnsureGameFlowSystem()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)gameFlowSystem != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "GameFlowSystem");
			if ((Object)(object)val == (Object)null)
			{
				Transform val2 = FindChildByName(null, "_Systems");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = new GameObject("_Systems").transform;
				}
				GameObject val3 = new GameObject("GameFlowSystem");
				val3.transform.SetParent(val2, false);
				val = val3.transform;
			}
			gameFlowSystem = ((Component)val).GetComponent<GameFlowSystem>();
			if ((Object)(object)gameFlowSystem == (Object)null)
			{
				gameFlowSystem = ((Component)val).gameObject.AddComponent<GameFlowSystem>();
			}
		}

		private void EnsureScoreSystem()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)scoreSystem != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "ScoreSystem");
			if ((Object)(object)val == (Object)null)
			{
				Transform val2 = FindChildByName(null, "_Systems");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = new GameObject("_Systems").transform;
				}
				GameObject val3 = new GameObject("ScoreSystem");
				val3.transform.SetParent(val2, false);
				val = val3.transform;
			}
			scoreSystem = ((Component)val).GetComponent<ScoreSystem>();
			if ((Object)(object)scoreSystem == (Object)null)
			{
				scoreSystem = ((Component)val).gameObject.AddComponent<ScoreSystem>();
			}
		}

		private void EnsureBallGrowthSystem()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)ballGrowthSystem != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "BallGrowthSystem");
			if ((Object)(object)val == (Object)null)
			{
				Transform val2 = FindChildByName(null, "_Systems");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = new GameObject("_Systems").transform;
				}
				GameObject val3 = new GameObject("BallGrowthSystem");
				val3.transform.SetParent(val2, false);
				val = val3.transform;
			}
			ballGrowthSystem = ((Component)val).GetComponent<BallGrowthSystem>();
			if ((Object)(object)ballGrowthSystem == (Object)null)
			{
				ballGrowthSystem = ((Component)val).gameObject.AddComponent<BallGrowthSystem>();
			}
		}

		private void EnsureCameraFollowSystem()
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)cameraFollowSystem == (Object)null)
			{
				Transform val = FindChildByName(null, "CameraFollowSystem");
				if ((Object)(object)val == (Object)null)
				{
					Transform val2 = FindChildByName(null, "_Systems");
					if ((Object)(object)val2 == (Object)null)
					{
						val2 = new GameObject("_Systems").transform;
					}
					GameObject val3 = new GameObject("CameraFollowSystem");
					val3.transform.SetParent(val2, false);
					val = val3.transform;
				}
				cameraFollowSystem = ((Component)val).GetComponent<CameraFollowSystem>();
				if ((Object)(object)cameraFollowSystem == (Object)null)
				{
					cameraFollowSystem = ((Component)val).gameObject.AddComponent<CameraFollowSystem>();
				}
			}
			cameraFollowSystem.ApplyComfortPreset();
		}

		private void EnsureFeedbackSystem()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "FeedbackSystem");
			if ((Object)(object)val == (Object)null)
			{
				Transform val2 = FindChildByName(null, "_Systems");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = new GameObject("_Systems").transform;
				}
				GameObject val3 = new GameObject("FeedbackSystem");
				val3.transform.SetParent(val2, false);
				val = val3.transform;
			}
			feedbackSystem = ((Component)val).GetComponent<FeedbackSystem>();
			if ((Object)(object)feedbackSystem == (Object)null)
			{
				feedbackSystem = ((Component)val).gameObject.AddComponent<FeedbackSystem>();
			}
		}

		private void EnsureDamageNumberSystem()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)damageNumberSystem != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "DamageNumberSystem");
			if ((Object)(object)val == (Object)null)
			{
				Transform val2 = FindChildByName(null, "_Systems");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = new GameObject("_Systems").transform;
				}
				GameObject val3 = new GameObject("DamageNumberSystem");
				val3.transform.SetParent(val2, false);
				val = val3.transform;
			}
			damageNumberSystem = ((Component)val).GetComponent<DamageNumberSystem>();
			if ((Object)(object)damageNumberSystem == (Object)null)
			{
				damageNumberSystem = ((Component)val).gameObject.AddComponent<DamageNumberSystem>();
			}
		}

		private void EnsureFormUnlockSystem()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)formUnlockSystem != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "FormUnlockSystem");
			if ((Object)(object)val == (Object)null)
			{
				Transform val2 = FindChildByName(null, "_Systems");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = new GameObject("_Systems").transform;
				}
				GameObject val3 = new GameObject("FormUnlockSystem");
				val3.transform.SetParent(val2, false);
				val = val3.transform;
			}
			formUnlockSystem = ((Component)val).GetComponent<FormUnlockSystem>();
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				formUnlockSystem = ((Component)val).gameObject.AddComponent<FormUnlockSystem>();
			}
		}

		private void EnsureGameplayLighting()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			Light val = null;
			Light[] array = Object.FindObjectsByType<Light>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			foreach (Light val2 in array)
			{
				if ((Object)(object)val2 != (Object)null && (int)val2.type == 1)
				{
					val = val2;
					break;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				Transform val3 = FindChildByName(null, "_Gameplay");
				if ((Object)(object)val3 == (Object)null)
				{
					val3 = new GameObject("_Gameplay").transform;
				}
				Transform val4 = FindChildByName(val3, "GameplayKeyLight");
				if ((Object)(object)val4 == (Object)null)
				{
					GameObject val5 = new GameObject("GameplayKeyLight", new Type[1] { typeof(Light) });
					val5.transform.SetParent(val3, false);
					val4 = val5.transform;
				}
				val = ((Component)val4).GetComponent<Light>();
				if ((Object)(object)val == (Object)null)
				{
					val = ((Component)val4).gameObject.AddComponent<Light>();
				}
			}
			val.type = (LightType)1;
			val.intensity = 1.28f;
			val.color = new Color(1f, 0.95f, 0.88f, 1f);
			val.shadows = (LightShadows)2;
			((Component)val).transform.rotation = Quaternion.Euler(48f, -32f, 0f);
			((Component)val).transform.position = new Vector3(0f, 18f, 0f);
			RenderSettings.ambientLight = new Color(0.63f, 0.67f, 0.74f, 1f);
		}

		private void ApplyStylizedLookToScene()
		{
			Material val = GetRuntimeStylizedMaterial();
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Renderer[] array = Object.FindObjectsByType<Renderer>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			foreach (Renderer val2 in array)
			{
				if (!((Object)(object)val2 == (Object)null) && !(val2 is ParticleSystemRenderer) && !((Object)(object)((Component)val2).GetComponent<TextMesh>() != (Object)null))
				{
					val2.sharedMaterial = val;
				}
			}
		}

		private void ApplyMapContrastPalette()
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			Transform val = FindChildByName(null, "MapRoot");
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "Ground");
			if ((Object)(object)val2 != (Object)null)
			{
				TintObject(((Component)val2).gameObject, new Color(0.06f, 0.09f, 0.14f));
			}
			string[] array = new string[4] { "Wall_North", "Wall_South", "Wall_East", "Wall_West" };
			for (int i = 0; i < array.Length; i++)
			{
				Transform val3 = FindChildByName(val, array[i]);
				if ((Object)(object)val3 != (Object)null)
				{
					TintObject(((Component)val3).gameObject, new Color(0.14f, 0.19f, 0.28f));
				}
			}
			Transform val4 = FindChildByName(val, "CityBlocks");
			if ((Object)(object)val4 != (Object)null)
			{
				int childCount = val4.childCount;
				for (int j = 0; j < childCount; j++)
				{
					float num = ((childCount > 1) ? ((float)j / (float)(childCount - 1)) : 0f);
					Color color = Color.Lerp(new Color(0.72f, 0.76f, 0.82f), new Color(0.93f, 0.76f, 0.47f), num);
					TintObject(((Component)val4.GetChild(j)).gameObject, color);
				}
			}
			Transform val5 = FindChildByName(val, "MicroProps");
			if ((Object)(object)val5 != (Object)null)
			{
				int childCount2 = val5.childCount;
				for (int k = 0; k < childCount2; k++)
				{
					float num2 = ((childCount2 > 1) ? ((float)k / (float)(childCount2 - 1)) : 0f);
					Color color2 = Color.Lerp(new Color(0.84f, 0.88f, 0.95f), new Color(1f, 0.62f, 0.29f), num2);
					TintObject(((Component)val5.GetChild(k)).gameObject, color2);
				}
			}
		}

		private void CacheSceneReferences()
		{
			playerTransform = FindChildByName(null, playerBallName);
			playerBody = (((Object)(object)playerTransform != (Object)null) ? ((Component)playerTransform).GetComponent<Rigidbody>() : null);
			playerSpawn = FindChildByName(null, playerSpawnName);
		}

		private void ResolvePlayerController()
		{
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			PlayerBallDummyController playerBallDummyController = (((Object)(object)playerTransform != (Object)null) ? ((Component)playerTransform).GetComponent<PlayerBallDummyController>() : null);
			if ((Object)(object)playerBallDummyController == (Object)(object)cachedPlayerController)
			{
				BindLandingShockwaveEvent(playerBallDummyController);
				return;
			}
			cachedPlayerController = playerBallDummyController;
			BindLandingShockwaveEvent(cachedPlayerController);
		}
	}
}
