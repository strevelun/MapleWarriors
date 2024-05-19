using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MultiplayMenu
{
#if UNITY_EDITOR
	[MenuItem("Run Multiplayer(Win64)/ºôµå")]
	private static void PerformWin64Build1()
	{
		PerformWin64Build(1);
	}

	private static void PerformWin64Build(int _playerCount)
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(
			BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

		for (int i = 1; i <= _playerCount; i++)
		{
			BuildPipeline.BuildPlayer(GetScenePaths(),
				"Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe",
				BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);
		}
	}

	private static string GetProjectName()
	{
		string[] s = Application.dataPath.Split('/');
		return s[s.Length - 2];
	}

	private static string[] GetScenePaths()
	{
		string[] scenes = new string[EditorBuildSettings.scenes.Length];

		for (int i = 0; i < scenes.Length; i++)
		{
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}

		return scenes;
	}
#endif
}