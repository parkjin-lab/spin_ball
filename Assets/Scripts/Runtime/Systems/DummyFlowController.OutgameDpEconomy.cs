using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string DpGainBurstId = "UI_DP_GainBurst";
		private const string ProgressionLockedSfxId = "SFX_Progression_Locked";
		private const string ProgressionConfirmSfxId = "SFX_Progression_Confirm";
		private const string ResultDpGainBurstName = "ResultDpGainBurst";
		private const string LobbyDpGainBurstName = "LobbyDpGainBurst";

		private enum OutgameDpSignal
		{
			None,
			SmallGain,
			BigGain,
			Spend,
			Insufficient
		}

		private Sprite dpGainBurstSprite;
		private AudioClip progressionLockedClip;
		private AudioClip progressionConfirmClip;
		private AudioSource outgameDpAudioSource;
		private bool outgameDpEconomyCached;
		private OutgameDpSignal lastOutgameDpSignal;

		private void EnsureOutgameDpEconomy()
		{
			CacheOutgameDpEconomyAssets();
			RefreshOutgameDpEconomy();
			EnsureOutgameProgressionVisuals();
		}

		private void CacheOutgameDpEconomyAssets()
		{
			if (outgameDpEconomyCached)
			{
				return;
			}

			dpGainBurstSprite = LoadOutgameRewardSprite(DpGainBurstId);
			progressionLockedClip = Resources.Load<AudioClip>("Audio/SFX/UI/" + ProgressionLockedSfxId);
			progressionConfirmClip = Resources.Load<AudioClip>("Audio/SFX/UI/" + ProgressionConfirmSfxId);
			outgameDpEconomyCached = true;
		}

		private static Sprite LoadOutgameRewardSprite(string assetId)
		{
			Sprite sprite = Resources.Load<Sprite>("UI/Rewards/" + assetId);
			if ((Object)(object)sprite != (Object)null)
			{
				return sprite;
			}

			Texture2D texture = Resources.Load<Texture2D>("UI/Rewards/" + assetId);
			if ((Object)(object)texture == (Object)null)
			{
				return null;
			}

			sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
			sprite.name = assetId;
			return sprite;
		}

		private void SignalOutgameDpInsufficient()
		{
			lastOutgameDpSignal = OutgameDpSignal.Insufficient;
			PlayProgressionLockedCue();
			RefreshOutgameDpEconomy();
		}

		private void SignalOutgameDpSpend()
		{
			lastOutgameDpSignal = OutgameDpSignal.Spend;
			PlayProgressionConfirmCue();
			RefreshOutgameDpEconomy();
			SignalOutgameProgressionSaved();
		}

		private void RefreshOutgameDpEconomy()
		{
			CacheOutgameDpEconomyAssets();
			if (currentUiViewState == UiViewState.Result)
			{
				lastOutgameDpSignal = ResolveResultDpGainSignal();
			}

			OutgameDpSignal resultSignal = currentUiViewState == UiViewState.Result ? ResolveResultDpGainSignal() : OutgameDpSignal.None;
			PlaceDpEconomyBurst(resultSummaryText, ResultDpGainBurstName, resultSignal, new Vector2(56f, 18f), 44f);
			PlaceDpEconomyBurst(lobbyDpText, LobbyDpGainBurstName, currentUiViewState == UiViewState.Lobby ? ResolveLobbyDpSignal() : OutgameDpSignal.None, new Vector2(56f, -8f), 40f);
		}

		private OutgameDpSignal ResolveResultDpGainSignal()
		{
			if (stageEndResult == StageEndResult.None || !DidStageEndInSuccess() || lastEarnedDp <= 0)
			{
				return OutgameDpSignal.None;
			}

			return lastEarnedDp >= 100 ? OutgameDpSignal.BigGain : OutgameDpSignal.SmallGain;
		}

		private OutgameDpSignal ResolveLobbyDpSignal()
		{
			if (lastOutgameDpSignal == OutgameDpSignal.Insufficient || lastOutgameDpSignal == OutgameDpSignal.Spend)
			{
				return lastOutgameDpSignal;
			}

			if (lastEarnedDp > 0)
			{
				return lastEarnedDp >= 100 ? OutgameDpSignal.BigGain : OutgameDpSignal.SmallGain;
			}

			return OutgameDpSignal.None;
		}

		private void PlaceDpEconomyBurst(Text text, string childName, OutgameDpSignal signal, Vector2 anchoredOffset, float size)
		{
			if ((Object)(object)text == (Object)null)
			{
				return;
			}

			Transform parent = text.transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(parent, childName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = text.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMin;
			rect.pivot = new Vector2(0f, 1f);
			rect.sizeDelta = new Vector2(size, size);
			rect.anchoredPosition = source.anchoredPosition + anchoredOffset;
			image.sprite = dpGainBurstSprite;
			image.color = ResolveDpBurstColor(signal);
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = signal != OutgameDpSignal.None && (Object)(object)dpGainBurstSprite != (Object)null;
		}

		private static Color ResolveDpBurstColor(OutgameDpSignal signal)
		{
			return signal switch
			{
				OutgameDpSignal.SmallGain => new Color(0.22f, 0.86f, 0.78f, 1f),
				OutgameDpSignal.BigGain => new Color(0.42f, 0.98f, 0.72f, 1f),
				OutgameDpSignal.Spend => new Color(0.36f, 0.52f, 0.62f, 1f),
				OutgameDpSignal.Insufficient => new Color(0.86f, 0.38f, 0.28f, 1f),
				_ => Color.white
			};
		}

		private void PlayProgressionLockedCue()
		{
			CacheOutgameDpEconomyAssets();
			if ((Object)(object)progressionLockedClip == (Object)null)
			{
				return;
			}

			if ((Object)(object)outgameDpAudioSource == (Object)null)
			{
				outgameDpAudioSource = GetComponent<AudioSource>();
				if ((Object)(object)outgameDpAudioSource == (Object)null)
				{
					outgameDpAudioSource = gameObject.AddComponent<AudioSource>();
					outgameDpAudioSource.playOnAwake = false;
					outgameDpAudioSource.spatialBlend = 0f;
				}
			}

			outgameDpAudioSource.pitch = 0.92f;
			outgameDpAudioSource.PlayOneShot(progressionLockedClip, 0.78f);
		}

		private void PlayProgressionConfirmCue()
		{
			CacheOutgameDpEconomyAssets();
			if ((Object)(object)progressionConfirmClip == (Object)null)
			{
				return;
			}

			if ((Object)(object)outgameDpAudioSource == (Object)null)
			{
				outgameDpAudioSource = GetComponent<AudioSource>();
				if ((Object)(object)outgameDpAudioSource == (Object)null)
				{
					outgameDpAudioSource = gameObject.AddComponent<AudioSource>();
					outgameDpAudioSource.playOnAwake = false;
					outgameDpAudioSource.spatialBlend = 0f;
				}
			}

			outgameDpAudioSource.pitch = 1.08f;
			outgameDpAudioSource.PlayOneShot(progressionConfirmClip, 0.74f);
		}
	}
}
