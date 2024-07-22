using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbstractGenerator), true)]
public class RandomDungeonGeneratorEditor : Editor
{
	private AbstractGenerator generator;

	private void Awake()
	{
		generator = (AbstractGenerator)target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Create Dungeon"))
		{
			generator.Generate();
		}
	}
}
