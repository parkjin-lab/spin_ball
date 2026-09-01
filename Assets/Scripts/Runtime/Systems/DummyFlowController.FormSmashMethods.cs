using System.Collections.Generic;
using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const float UfoRayTickSeconds = 0.16f;
		private const float UfoRayLength = 8.4f;
		private const float UfoRayHalfWidth = 1.05f;
		private const float DrillBurrowLength = 7.2f;
		private const float DrillBurrowHalfWidth = 0.92f;
		private const float DrillBurrowCooldown = 0.92f;
		private const float ChargeBurstLength = 6.4f;
		private const float ChargeBurstHalfWidth = 1.15f;
		private const float ChargeBurstCooldown = 0.78f;
		private const float ChargeBurstBuildSeconds = 0.48f;
		private const float MagnetPullRadius = 5.6f;
		private const float MagnetPullSeconds = 0.24f;
		private const float MagnetDetonateCooldown = 1.35f;
		private const int FormSmashTargetCap = 10;

		private FormSkillHoldTracker transformHoldTracker;
		private FormSkillHoldTracker supportHoldTracker;
		private bool formSkillHeldPrevious;
		private float formSmashMethodCooldown;
		private float chargeBurst01;
		private bool magnetPullActive;
		private float magnetPullRemaining;
		private readonly List<DummyStreetPropReactive> magnetProps = new List<DummyStreetPropReactive>(16);
		private readonly List<DummyDestructibleBlock> magnetBlocks = new List<DummyDestructibleBlock>(8);
		private readonly List<Vector3> magnetPropStarts = new List<Vector3>(16);
		private readonly List<Vector3> magnetBlockStarts = new List<Vector3>(8);
		private GameObject formSmashRayVisual;
		private Renderer formSmashRayRenderer;
		private float formSmashRayVisibleUntil;

		private void EnsureFormSkillHoldTrackers()
		{
			transformHoldTracker = EnsureHoldTracker(FindButton("TransformButton"), transformHoldTracker);
			supportHoldTracker = EnsureHoldTracker(FindButton("Special1Button"), supportHoldTracker);
		}

		private static FormSkillHoldTracker EnsureHoldTracker(Button button, FormSkillHoldTracker existing)
		{
			if ((Object)(object)existing != (Object)null)
			{
				return existing;
			}

			if ((Object)(object)button == (Object)null)
			{
				return null;
			}

			FormSkillHoldTracker tracker = button.GetComponent<FormSkillHoldTracker>();
			if ((Object)(object)tracker == (Object)null)
			{
				tracker = button.gameObject.AddComponent<FormSkillHoldTracker>();
			}

			return tracker;
		}

		private bool IsFormSkillHeld()
		{
			if ((Object)(object)transformHoldTracker != (Object)null && transformHoldTracker.Held)
			{
				return true;
			}

			if ((Object)(object)supportHoldTracker != (Object)null && supportHoldTracker.Held)
			{
				return true;
			}

			Keyboard keyboard = Keyboard.current;
			if (keyboard == null)
			{
				return false;
			}

			return keyboard.spaceKey.isPressed || keyboard.eKey.isPressed;
		}

		private void ResetFormSmashMethodRuntime()
		{
			formSmashMethodCooldown = 0f;
			chargeBurst01 = 0f;
			formSkillHeldPrevious = false;
			formSmashRayVisibleUntil = 0f;
			CancelMagnetPull(applyDetonate: false);
			SetFormSmashRayVisible(false);
			ResolvePlayerController();
			cachedPlayerController?.SetFormSmashCharge(0f);
			cachedPlayerController?.SetFormSmashBeam(false, Vector3.forward, 0f);
		}

		private void UpdateFormSmashMethods(float deltaTime)
		{
			if (!stageRunning || levelUpOpen || !enableFormActiveSkills)
			{
				SetFormSmashRayVisible(false);
				return;
			}

			EnsureFormSkillHoldTrackers();
			formSmashMethodCooldown = Mathf.Max(0f, formSmashMethodCooldown - Mathf.Max(0f, deltaTime));
			if (magnetPullActive)
			{
				TickMagnetPull(deltaTime);
			}

			FormSmashMethod method = FormCatalog.GetSmashMethod(GetCurrentSelectedForm());
			bool held = IsFormSkillHeld();
			switch (method)
			{
			case FormSmashMethod.UfoRay:
				if (held && formSmashMethodCooldown <= 0f)
				{
					FireUfoRaySmash(heldTick: true);
				}

				if (!held)
				{
					SetFormSmashRayVisible(false);
				}

				break;
			case FormSmashMethod.ChargeBurst:
				if (held)
				{
					chargeBurst01 = Mathf.Clamp01(chargeBurst01 + Mathf.Max(0.01f, deltaTime) / ChargeBurstBuildSeconds);
					ResolvePlayerController();
					cachedPlayerController?.SetFormSmashCharge(chargeBurst01);
				}
				else if (formSkillHeldPrevious && chargeBurst01 >= 0.18f && formSmashMethodCooldown <= 0f)
				{
					FireChargeBurstSmash(chargeBurst01);
				}
				else if (!held)
				{
					chargeBurst01 = 0f;
					ResolvePlayerController();
					cachedPlayerController?.SetFormSmashCharge(0f);
				}

				break;
			case FormSmashMethod.DrillBurrow:
				if (held && formSmashMethodCooldown <= 0f)
				{
					FireDrillBurrowSmash();
				}

				break;
			case FormSmashMethod.MagnetGrab:
				if (held && !magnetPullActive && formSmashMethodCooldown <= 0f)
				{
					BeginMagnetGrabSmash();
				}

				break;
			default:
				SetFormSmashRayVisible(false);
				break;
			}

			if (Time.unscaledTime >= formSmashRayVisibleUntil && method != FormSmashMethod.UfoRay)
			{
				SetFormSmashRayVisible(false);
			}

			formSkillHeldPrevious = held;
		}

		private bool TryTriggerFormSmashMethod(FormSmashMethod method)
		{
			switch (method)
			{
			case FormSmashMethod.UfoRay:
				return FireUfoRaySmash(heldTick: false);
			case FormSmashMethod.DrillBurrow:
				return FireDrillBurrowSmash();
			case FormSmashMethod.ChargeBurst:
				if (chargeBurst01 >= 0.18f)
				{
					return false;
				}

				return FireChargeBurstSmash(0.42f);
			case FormSmashMethod.MagnetGrab:
				return BeginMagnetGrabSmash();
			default:
				return false;
			}
		}

		private bool FireUfoRaySmash(bool heldTick)
		{
			if (formSmashMethodCooldown > 0f)
			{
				return false;
			}

			if (!TryResolveSmashOrigin(out Vector3 origin, out Vector3 facing))
			{
				return false;
			}

			int hits = ApplyLaneSmash(origin, facing, UfoRayLength, UfoRayHalfWidth, saucerDashDamageRange, FormSmashTargetCap, drillMode: false, suppressFeedback: true);
			int score = Mathf.Max(0, 18 + hits * 22);
			if (score > 0)
			{
				scoreSystem?.AddScore(score);
			}

			ShowFormSmashRay(origin, facing, UfoRayLength, new Color(0.45f, 0.95f, 1f, 0.92f));
			PlayFormSmashFeedback(origin + facing * (UfoRayLength * 0.45f), UfoRayLength * 0.28f, 0.46f + hits * 0.05f, 0.42f);
			ResolvePlayerController();
			cachedPlayerController?.PlaySaucerDashVisualCue();
			cachedPlayerController?.SetFormSmashBeam(true, facing, UfoRayLength);
			formSmashMethodCooldown = UfoRayTickSeconds;
			if (!heldTick)
			{
				PushAnnouncement(hits > 0 ? $"UFO RAY +{score}" : "UFO RAY", AnnouncementTone.Burst, 0.42f);
			}

			return true;
		}

		private bool FireDrillBurrowSmash()
		{
			if (formSmashMethodCooldown > 0f)
			{
				return false;
			}

			if (!TryResolveSmashOrigin(out Vector3 origin, out Vector3 facing))
			{
				return false;
			}

			if ((Object)(object)playerBody != (Object)null)
			{
				playerBody.AddForce(facing * Mathf.Max(4.2f, ramBreachVelocityBoost * 0.72f), ForceMode.VelocityChange);
			}

			int hits = ApplyLaneSmash(origin, facing, DrillBurrowLength, DrillBurrowHalfWidth, spikeBurstDamageRange, FormSmashTargetCap, drillMode: true, suppressFeedback: true);
			int score = Mathf.Max(0, 28 + hits * 28);
			if (score > 0)
			{
				scoreSystem?.AddScore(score);
			}

			ShowFormSmashRay(origin, facing, DrillBurrowLength, new Color(0.92f, 0.95f, 0.28f, 0.88f));
			PlayFormSmashFeedback(origin + facing * (DrillBurrowLength * 0.4f), DrillBurrowLength * 0.24f, 0.52f + hits * 0.04f, 0.58f);
			ResolvePlayerController();
			cachedPlayerController?.PlaySpikeBurstVisualCue();
			formSmashMethodCooldown = DrillBurrowCooldown;
			PushAnnouncement(hits > 0 ? $"DRILL +{score}" : "DRILL", AnnouncementTone.Burst, 0.48f);
			return true;
		}

		private bool FireChargeBurstSmash(float charge01)
		{
			if (formSmashMethodCooldown > 0f)
			{
				return false;
			}

			if (!TryResolveSmashOrigin(out Vector3 origin, out Vector3 facing))
			{
				return false;
			}

			float charge = Mathf.Clamp01(charge01);
			float length = Mathf.Lerp(ChargeBurstLength * 0.62f, ChargeBurstLength, charge);
			if ((Object)(object)playerBody != (Object)null)
			{
				playerBody.AddForce(facing * Mathf.Lerp(6.2f, Mathf.Max(8.5f, ramBreachVelocityBoost), charge), ForceMode.VelocityChange);
			}

			Vector2 damage = new Vector2(
				Mathf.Lerp(ramBreachDamageRange.x * 0.72f, ramBreachDamageRange.x, charge),
				Mathf.Lerp(ramBreachDamageRange.y * 0.72f, ramBreachDamageRange.y, charge));
			int hits = ApplyLaneSmash(origin, facing, length, ChargeBurstHalfWidth, damage, FormSmashTargetCap, drillMode: false, suppressFeedback: true);
			int score = Mathf.Max(0, 36 + hits * 32 + Mathf.RoundToInt(charge * 40f));
			if (score > 0)
			{
				scoreSystem?.AddScore(score);
			}

			ShowFormSmashRay(origin, facing, length, new Color(1f, 0.62f, 0.22f, 0.9f));
			PlayFormSmashFeedback(origin + facing * (length * 0.35f), length * 0.3f, 0.58f + charge * 0.22f, 0.74f);
			ResolvePlayerController();
			cachedPlayerController?.PlayRamBreachVisualCue();
			cachedPlayerController?.SetFormSmashCharge(0f);
			chargeBurst01 = 0f;
			formSmashMethodCooldown = ChargeBurstCooldown;
			PushAnnouncement(hits > 0 ? $"TANK BURST +{score}" : "TANK BURST", AnnouncementTone.Burst, 0.5f);
			return true;
		}

		private bool BeginMagnetGrabSmash()
		{
			if (magnetPullActive || formSmashMethodCooldown > 0f)
			{
				return false;
			}

			if (!TryResolveSmashOrigin(out Vector3 origin, out _))
			{
				return false;
			}

			CollectMagnetTargets(origin);
			if (magnetProps.Count == 0 && magnetBlocks.Count == 0)
			{
				return false;
			}

			magnetPullActive = true;
			magnetPullRemaining = MagnetPullSeconds;
			ResolvePlayerController();
			cachedPlayerController?.PlayCrusherSlamVisualCue();
			cachedPlayerController?.SetFormSmashMagnet(true);
			PushAnnouncement("MAGNET PULL", AnnouncementTone.Burst, 0.4f);
			return true;
		}

		private void TickMagnetPull(float deltaTime)
		{
			if (!magnetPullActive)
			{
				return;
			}

			if (!TryResolveSmashOrigin(out Vector3 origin, out _))
			{
				CancelMagnetPull(applyDetonate: false);
				return;
			}

			magnetPullRemaining -= Mathf.Max(0f, deltaTime);
			float t = 1f - Mathf.Clamp01(magnetPullRemaining / MagnetPullSeconds);
			Vector3 pullPoint = origin + Vector3.up * 0.15f;
			for (int i = 0; i < magnetProps.Count; i++)
			{
				DummyStreetPropReactive prop = magnetProps[i];
				if (!CollisionContactGuard.IsUnityAlive(prop) || !prop.IsAlive)
				{
					continue;
				}

				Transform propTransform = prop.transform;
				if (!CollisionContactGuard.IsUnityAlive(propTransform))
				{
					continue;
				}

				propTransform.position = Vector3.Lerp(magnetPropStarts[i], pullPoint, t);
			}

			for (int i = 0; i < magnetBlocks.Count; i++)
			{
				DummyDestructibleBlock block = magnetBlocks[i];
				if (!CollisionContactGuard.IsUnityAlive(block) || !block.IsAlive)
				{
					continue;
				}

				Transform blockTransform = block.transform;
				if (!CollisionContactGuard.IsUnityAlive(blockTransform))
				{
					continue;
				}

				blockTransform.position = Vector3.Lerp(magnetBlockStarts[i], pullPoint, t);
			}

			if (magnetPullRemaining > 0f)
			{
				return;
			}

			DetonateMagnetTargets(pullPoint);
		}

		private void CollectMagnetTargets(Vector3 origin)
		{
			magnetProps.Clear();
			magnetBlocks.Clear();
			magnetPropStarts.Clear();
			magnetBlockStarts.Clear();
			float radiusSq = MagnetPullRadius * MagnetPullRadius;
			DummyStreetPropReactive[] props = Object.FindObjectsByType<DummyStreetPropReactive>(FindObjectsInactive.Exclude);
			if (props != null)
			{
				for (int i = 0; i < props.Length && magnetProps.Count < FormSmashTargetCap; i++)
				{
					DummyStreetPropReactive prop = props[i];
					if (!CollisionContactGuard.IsUnityAlive(prop) || !prop.IsAlive)
					{
						continue;
					}

					Vector3 delta = prop.transform.position - origin;
					delta.y = 0f;
					if (delta.sqrMagnitude > radiusSq)
					{
						continue;
					}

					magnetProps.Add(prop);
					magnetPropStarts.Add(prop.transform.position);
				}
			}

			RefreshDestructibleCache();
			for (int i = 0; i < destructibleCache.Count && magnetBlocks.Count < 6; i++)
			{
				DummyDestructibleBlock block = destructibleCache[i];
				if (!CollisionContactGuard.IsUnityAlive(block) || !block.IsAlive || block.IsStageBoss || block.IsLargeBuildingTarget)
				{
					continue;
				}

				Vector3 delta = block.transform.position - origin;
				delta.y = 0f;
				if (delta.sqrMagnitude > radiusSq)
				{
					continue;
				}

				magnetBlocks.Add(block);
				magnetBlockStarts.Add(block.transform.position);
			}
		}

		private void DetonateMagnetTargets(Vector3 origin)
		{
			int hits = 0;
			for (int i = 0; i < magnetProps.Count; i++)
			{
				DummyStreetPropReactive prop = magnetProps[i];
				if (!CollisionContactGuard.IsUnityAlive(prop) || !prop.IsAlive)
				{
					continue;
				}

				prop.ApplyExternalBreak(prop.transform.position + Vector3.up * 0.15f, 0.92f, drillMode: false, suppressFeedback: true);
				hits++;
			}

			for (int i = 0; i < magnetBlocks.Count; i++)
			{
				DummyDestructibleBlock block = magnetBlocks[i];
				if (!CollisionContactGuard.IsUnityAlive(block) || !block.IsAlive)
				{
					continue;
				}

				float damage = Mathf.Lerp(crusherSlamDamageRange.x, crusherSlamDamageRange.y, 0.72f);
				block.ApplyExternalImpactDamage(damage, block.transform.position + Vector3.up * 0.35f, 0.9f, suppressFeedback: true);
				hits++;
			}

			int score = Mathf.Max(0, 48 + hits * 34);
			if (score > 0)
			{
				scoreSystem?.AddScore(score);
			}

			PlayFormSmashFeedback(origin, MagnetPullRadius * 0.45f, 0.64f + hits * 0.04f, 0.7f);
			PushAnnouncement(hits > 0 ? $"MAGNET +{score}" : "MAGNET", AnnouncementTone.Burst, 0.5f);
			CancelMagnetPull(applyDetonate: false);
			formSmashMethodCooldown = MagnetDetonateCooldown;
		}

		private void CancelMagnetPull(bool applyDetonate)
		{
			if (applyDetonate && magnetPullActive)
			{
				if (TryResolveSmashOrigin(out Vector3 origin, out _))
				{
					DetonateMagnetTargets(origin + Vector3.up * 0.15f);
					return;
				}
			}

			magnetPullActive = false;
			magnetPullRemaining = 0f;
			magnetProps.Clear();
			magnetBlocks.Clear();
			magnetPropStarts.Clear();
			magnetBlockStarts.Clear();
			ResolvePlayerController();
			cachedPlayerController?.SetFormSmashMagnet(false);
		}

		private bool TryResolveSmashOrigin(out Vector3 origin, out Vector3 facing)
		{
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}

			if ((Object)(object)playerTransform == (Object)null)
			{
				origin = Vector3.zero;
				facing = Vector3.forward;
				return false;
			}

			if ((Object)(object)playerBody == (Object)null)
			{
				playerBody = playerTransform.GetComponent<Rigidbody>();
			}

			ResolvePlayerController();
			origin = playerTransform.position;
			facing = cachedPlayerController != null
				? cachedPlayerController.GetPlanarFacing()
				: ResolveFallbackFacing();
			return true;
		}

		private Vector3 ResolveFallbackFacing()
		{
			if ((Object)(object)playerBody != (Object)null)
			{
				Vector3 velocity = new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z);
				if (velocity.sqrMagnitude > 0.04f)
				{
					return velocity.normalized;
				}
			}

			if ((Object)(object)playerTransform != (Object)null)
			{
				Vector3 forward = playerTransform.forward;
				forward.y = 0f;
				if (forward.sqrMagnitude > 0.001f)
				{
					return forward.normalized;
				}
			}

			return Vector3.forward;
		}

		private int ApplyLaneSmash(Vector3 origin, Vector3 direction, float length, float halfWidth, Vector2 damageRange, int maxTargets, bool drillMode, bool suppressFeedback)
		{
			direction.y = 0f;
			if (direction.sqrMagnitude < 0.0001f)
			{
				direction = Vector3.forward;
			}

			direction.Normalize();
			float safeLength = Mathf.Max(1.2f, length);
			float safeHalfWidth = Mathf.Max(0.35f, halfWidth);
			int cap = Mathf.Max(1, maxTargets);
			int hits = 0;
			float minDamage = Mathf.Min(damageRange.x, damageRange.y);
			float maxDamage = Mathf.Max(damageRange.x, damageRange.y);
			RefreshDestructibleCache();
			for (int i = 0; i < destructibleCache.Count && hits < cap; i++)
			{
				DummyDestructibleBlock block = destructibleCache[i];
				if (!CollisionContactGuard.IsUnityAlive(block) || !block.IsAlive)
				{
					continue;
				}

				if (!IsPointOnLane(origin, direction, safeLength, safeHalfWidth, block.transform.position, out float along01))
				{
					continue;
				}

				float damage = Mathf.Lerp(minDamage, maxDamage, along01);
				block.ApplyExternalImpactDamage(damage, block.transform.position + Vector3.up * 0.4f, Mathf.Lerp(0.55f, 0.95f, along01), suppressFeedback);
				hits++;
			}

			DummyStreetPropReactive[] props = Object.FindObjectsByType<DummyStreetPropReactive>(FindObjectsInactive.Exclude);
			if (props == null)
			{
				return hits;
			}

			for (int i = 0; i < props.Length && hits < cap; i++)
			{
				DummyStreetPropReactive prop = props[i];
				if (!CollisionContactGuard.IsUnityAlive(prop) || !prop.IsAlive)
				{
					continue;
				}

				if (!IsPointOnLane(origin, direction, safeLength, safeHalfWidth, prop.transform.position, out float along01))
				{
					continue;
				}

				prop.ApplyExternalBreak(prop.transform.position + Vector3.up * 0.18f, Mathf.Lerp(0.55f, 1f, along01), drillMode, suppressFeedback);
				hits++;
			}

			return hits;
		}

		private static bool IsPointOnLane(Vector3 origin, Vector3 direction, float length, float halfWidth, Vector3 point, out float along01)
		{
			Vector3 to = point - origin;
			to.y = 0f;
			float along = Vector3.Dot(to, direction);
			along01 = 0f;
			if (along < -0.15f || along > length)
			{
				return false;
			}

			Vector3 lateral = to - direction * along;
			if (lateral.sqrMagnitude > halfWidth * halfWidth)
			{
				return false;
			}

			along01 = Mathf.Clamp01(along / Mathf.Max(0.01f, length));
			return true;
		}

		private void PlayFormSmashFeedback(Vector3 position, float radius, float intensity, float impulse)
		{
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindAnyObjectByType<FeedbackSystem>();
			}

			if ((Object)(object)feedbackSystem != (Object)null && SmashVfxBudget.TryConsumeComboRushVisual())
			{
				feedbackSystem.PlayComboRushFeedback(position + Vector3.up * 0.16f, Mathf.Clamp01(intensity), Mathf.Max(1.2f, radius));
			}

			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindAnyObjectByType<CameraFollowSystem>();
			}

			cameraFollowSystem?.AddImpulse(Mathf.Clamp(impulse, 0.2f, 1.1f));
		}

		private void ShowFormSmashRay(Vector3 origin, Vector3 direction, float length, Color color)
		{
			if (!SmashVfxBudget.TryConsumeBurstSpawn())
			{
				return;
			}

			EnsureFormSmashRayVisual();
			if ((Object)(object)formSmashRayVisual == (Object)null)
			{
				return;
			}

			direction.y = 0f;
			if (direction.sqrMagnitude < 0.0001f)
			{
				direction = Vector3.forward;
			}

			direction.Normalize();
			float safeLength = Mathf.Max(1.2f, length);
			Vector3 mid = origin + direction * (safeLength * 0.5f) + Vector3.up * 0.22f;
			formSmashRayVisual.transform.position = mid;
			formSmashRayVisual.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
			formSmashRayVisual.transform.localScale = new Vector3(0.22f, safeLength * 0.5f, 0.22f);
			if ((Object)(object)formSmashRayRenderer != (Object)null && formSmashRayRenderer.material != null)
			{
				Material material = formSmashRayRenderer.material;
				if (material.HasProperty("_BaseColor"))
				{
					material.SetColor("_BaseColor", color);
				}

				if (material.HasProperty("_Color"))
				{
					material.SetColor("_Color", color);
				}

				if (material.HasProperty("_EmissionColor"))
				{
					material.EnableKeyword("_EMISSION");
					material.SetColor("_EmissionColor", color * 1.6f);
				}
			}

			formSmashRayVisual.SetActive(true);
			formSmashRayVisibleUntil = Time.unscaledTime + 0.14f;
		}

		private void SetFormSmashRayVisible(bool visible)
		{
			if ((Object)(object)formSmashRayVisual == (Object)null)
			{
				return;
			}

			formSmashRayVisual.SetActive(visible);
			if (!visible)
			{
				formSmashRayVisibleUntil = 0f;
				ResolvePlayerController();
				cachedPlayerController?.SetFormSmashBeam(false, Vector3.forward, 0f);
			}
		}

		private void EnsureFormSmashRayVisual()
		{
			if ((Object)(object)formSmashRayVisual != (Object)null)
			{
				return;
			}

			formSmashRayVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			formSmashRayVisual.name = "VFX_FormSmash_Ray";
			Collider collider = formSmashRayVisual.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}

			formSmashRayRenderer = formSmashRayVisual.GetComponent<Renderer>();
			if ((Object)(object)formSmashRayRenderer != (Object)null)
			{
				Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
				                ?? Shader.Find("Unlit/Color")
				                ?? Shader.Find("Universal Render Pipeline/Lit");
				if ((Object)(object)shader != (Object)null)
				{
					formSmashRayRenderer.sharedMaterial = new Material(shader)
					{
						name = "M_Runtime_FormSmashRay"
					};
				}
			}

			formSmashRayVisual.SetActive(false);
		}

		private void EnsureFormStrategyHints()
		{
			for (int i = 0; i < FormCatalog.All.Length; i++)
			{
				FormCatalog.Entry entry = FormCatalog.All[i];
				RefreshFormStrategyHint(entry);
			}
		}

		private void RefreshFormStrategyHint(FormCatalog.Entry entry)
		{
			Button button = FindButton(entry.ButtonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Transform existing = FindDirectChild(button.transform, "FormStrategyHint");
			GameObject go;
			if ((Object)(object)existing == (Object)null)
			{
				go = new GameObject("FormStrategyHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
				go.transform.SetParent(button.transform, false);
			}
			else
			{
				go = existing.gameObject;
			}

			Text text = go.GetComponent<Text>();
			if ((Object)(object)text == (Object)null)
			{
				text = go.AddComponent<Text>();
			}

			RectTransform rect = text.rectTransform;
			rect.anchorMin = new Vector2(0f, 0f);
			rect.anchorMax = new Vector2(1f, 0f);
			rect.pivot = new Vector2(0.5f, 0f);
			rect.offsetMin = new Vector2(6f, 4f);
			rect.offsetMax = new Vector2(-6f, 36f);
			bool unlocked = formUnlockSystem != null && formUnlockSystem.IsUnlocked(entry.Type);
			text.text = entry.StrategyHint;
			text.alignment = TextAnchor.LowerCenter;
			text.fontSize = 14;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
			text.verticalOverflow = VerticalWrapMode.Overflow;
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 10;
			text.resizeTextMaxSize = 15;
			text.raycastTarget = false;
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.color = unlocked ? new Color(0.86f, 0.92f, 0.96f, 0.96f) : new Color(0.58f, 0.62f, 0.68f, 0.78f);
		}
	}
}
