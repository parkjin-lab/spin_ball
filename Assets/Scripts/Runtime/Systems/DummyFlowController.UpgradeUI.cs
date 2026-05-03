using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void UpdateUpgradeListText()
		{
			if (!((Object)(object)hudUpgradeListText == (Object)null))
			{
				bool retailFrenzyActive = IsRetailFrenzyActive();
				if (speedUpgradeCount + impactUpgradeCount + drillUpgradeCount + comboPowerUpgradeCount + comboTriggerUpgradeCount + seismicUpgradeCount + trafficPanicUpgradeCount <= 0 && !retailFrenzyActive)
				{
					hudUpgradeListText.text = "UPGRADES: NONE";
					return;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("UPGRADES: ");
				int appendedCount = 0;
				AppendUpgradeLabel(stringBuilder, "SPEED", speedUpgradeCount, ref appendedCount);
				AppendUpgradeLabel(stringBuilder, "IMPACT", impactUpgradeCount, ref appendedCount);
				AppendUpgradeLabel(stringBuilder, "DRILL", drillUpgradeCount, ref appendedCount);
				AppendUpgradeLabel(stringBuilder, "RUSH_PWR", comboPowerUpgradeCount, ref appendedCount);
				AppendUpgradeLabel(stringBuilder, "RUSH_ENG", comboTriggerUpgradeCount, ref appendedCount);
				AppendUpgradeLabel(stringBuilder, "SEISMIC", seismicUpgradeCount, ref appendedCount);
				AppendUpgradeLabel(stringBuilder, "CHAOS", trafficPanicUpgradeCount, ref appendedCount);
				if (retailFrenzyActive)
				{
					if (appendedCount > 0)
					{
						stringBuilder.Append(" | ");
					}
					stringBuilder.Append("FRENZY ");
					stringBuilder.Append(retailFrenzyRemaining.ToString("0.0"));
					stringBuilder.Append("s");
				}
				hudUpgradeListText.text = stringBuilder.ToString();
			}
		}

		private static void AppendUpgradeLabel(StringBuilder builder, string label, int count, ref int appendedCount)
		{
			if (count > 0)
			{
				if (appendedCount > 0)
				{
					builder.Append(" | ");
				}
				builder.Append(label);
				if (count > 1)
				{
					builder.Append(" x");
					builder.Append(count);
				}
				appendedCount++;
			}
		}
	}
}
