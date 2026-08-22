using UnityEngine;

namespace AlienCrusher.Systems
{
	public static class RouteMarkerTintSet
	{
		public const string Id = "PAL_RouteMarker_Tints";

		// Bright orchid discs. Unused by district oranges, park greens, skyline whites, or boss kits
		// (cool-blue body, amber pylons, red-orange drones).
		public static readonly Color Marker = new Color(1f, 0.58f, 0.94f, 1f);

		// Deeper magenta ground paint for routeColor / landmark route stripes.
		public static readonly Color Paint = new Color(0.94f, 0.18f, 0.7f, 0.92f);

		// Brighter pink-white HOLD trail pips in the same nav family.
		public static readonly Color Trail = new Color(1f, 0.76f, 0.98f, 0.94f);
	}
}
