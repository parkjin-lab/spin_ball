using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteMarkerTintSet
	{
		public const string Id = "PAL_RouteMarker_Tints";

		// Bright orchid discs. Unused by district oranges, park greens, skyline whites, or boss kits
		// (cool-blue body, amber pylons, red-orange drones).
		private static readonly Color FallbackMarker = new Color(1f, 0.58f, 0.94f, 1f);

		// Deeper magenta ground paint for routeColor / landmark route stripes.
		private static readonly Color FallbackPaint = new Color(0.94f, 0.18f, 0.7f, 0.92f);

		// Brighter pink-white HOLD trail pips in the same nav family.
		private static readonly Color FallbackTrail = new Color(1f, 0.76f, 0.98f, 0.94f);

		private static bool resolved;
		private static Color marker = FallbackMarker;
		private static Color paint = FallbackPaint;
		private static Color trail = FallbackTrail;

		public static Color Marker
		{
			get
			{
				EnsureDraftTints();
				return marker;
			}
		}

		public static Color Paint
		{
			get
			{
				EnsureDraftTints();
				return paint;
			}
		}

		public static Color Trail
		{
			get
			{
				EnsureDraftTints();
				return trail;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetDraftTints()
		{
			resolved = false;
			marker = FallbackMarker;
			paint = FallbackPaint;
			trail = FallbackTrail;
		}

		private static void EnsureDraftTints()
		{
			if (resolved)
			{
				return;
			}

			resolved = true;
			Material draft = DistrictPaletteDrafts.LoadRoute();
			if ((Object)(object)draft == (Object)null)
			{
				return;
			}

			marker = DistrictPaletteDrafts.ReadColor(draft, "_BaseColor", FallbackMarker);
			paint = DistrictPaletteDrafts.ReadColor(draft, "_EmissionColor", FallbackPaint);
			trail = DistrictPaletteDrafts.ReadColor(draft, "_SpecColor", FallbackTrail);
		}
	}
}
