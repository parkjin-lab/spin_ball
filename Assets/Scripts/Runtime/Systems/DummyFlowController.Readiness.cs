using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void EmitReadinessReport()
		{
			List<string> list = new List<string>(24);
			if ((Object)(object)playerTransform == (Object)null)
			{
				list.Add($"Player Transform '{playerBallName}'");
			}
			if ((Object)(object)playerSpawn == (Object)null)
			{
				list.Add($"Player Spawn '{playerSpawnName}'");
			}
			if ((Object)(object)gameFlowSystem == (Object)null)
			{
				list.Add("GameFlowSystem");
			}
			if ((Object)(object)scoreSystem == (Object)null)
			{
				list.Add("ScoreSystem");
			}
			if ((Object)(object)ballGrowthSystem == (Object)null)
			{
				list.Add("BallGrowthSystem");
			}
			if ((Object)(object)cameraFollowSystem == (Object)null)
			{
				list.Add("CameraFollowSystem");
			}
			if ((Object)(object)feedbackSystem == (Object)null)
			{
				list.Add("FeedbackSystem");
			}
			if ((Object)(object)damageNumberSystem == (Object)null)
			{
				list.Add("DamageNumberSystem");
			}
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				list.Add("FormUnlockSystem");
			}
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				list.Add($"Canvas '{canvasName}'");
			}
			if ((Object)(object)hudPanel == (Object)null)
			{
				list.Add($"HUD Panel '{hudPanelName}'");
			}
			if ((Object)(object)resultPanel == (Object)null)
			{
				list.Add($"Result Panel '{resultPanelName}'");
			}
			if ((Object)(object)levelUpPanel == (Object)null)
			{
				list.Add($"LevelUp Panel '{levelUpPanelName}'");
			}
			if ((Object)(object)lobbyPanel == (Object)null)
			{
				list.Add($"Lobby Panel '{lobbyPanelName}'");
			}
			if ((Object)(object)pausePanel == (Object)null)
			{
				list.Add($"Pause Panel '{pausePanelName}'");
			}
			if ((Object)(object)hudInfoText == (Object)null)
			{
				list.Add("HUD Text 'InfoText'");
			}
			if ((Object)(object)hudObjectiveText == (Object)null)
			{
				list.Add("HUD Text 'ObjectiveText'");
			}
			if ((Object)(object)hudHintText == (Object)null)
			{
				list.Add("HUD Text 'HintText'");
			}
			if ((Object)(object)hudProgressText == (Object)null)
			{
				list.Add("HUD Text 'DestructionProgressText'");
			}
			if ((Object)(object)hudChainText == (Object)null)
			{
				list.Add("HUD Text 'ChainText'");
			}
			if ((Object)(object)hudUpgradeListText == (Object)null)
			{
				list.Add("HUD Text 'UpgradeListText'");
			}
			if ((Object)(object)levelUpTimerText == (Object)null)
			{
				list.Add("LevelUp Text 'Timer'");
			}
			if ((Object)(object)resultSummaryText == (Object)null)
			{
				list.Add("Result Text 'Summary'");
			}
			if (list.Count <= 0)
			{
				Debug.Log((object)"[AlienCrusher][Readiness] Core systems and UI bindings look ready.");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.AppendLine("[AlienCrusher][Readiness] Missing or unresolved references:");
			for (int i = 0; i < list.Count; i++)
			{
				stringBuilder.Append(" - ");
				stringBuilder.AppendLine(list[i]);
			}
			stringBuilder.AppendLine("Hint: re-run AlienCrusherSceneScaffolder and check renamed UI objects.");
			Debug.LogWarning((object)stringBuilder.ToString());
		}
	}
}
