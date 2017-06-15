﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

[Serializable]
public class TooltipsContainer{
	public ComponentTooltip[] tooltips;
}

[Serializable]
public class ComponentTooltip{
	public string typeName;
	public string tooltip;
}

public class ComponentWindow : EditorWindow {
	private Dictionary<string, string> tooltipMapping;
	private Vector2 scrollPosition;
	private string searchInput = "";

	[MenuItem("Window/Component Window")]
	public static void ShowWindow(){
		EditorWindow.GetWindow (typeof(ComponentWindow));
	}

	void OnEnable(){
		TextAsset textAsset = Resources.Load ("ProceduralGeneration/Tooltips") as TextAsset;
		TooltipsContainer tooltips = JsonUtility.FromJson<TooltipsContainer> (textAsset.text);
		tooltipMapping = new Dictionary<string, string> ();
		foreach(ComponentTooltip ct in tooltips.tooltips){
			tooltipMapping.Add (ct.typeName, ct.tooltip);
		}
	}

	void OnGUI(){

		scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);
		List<Type> types = typeof(AbstractProperty).Assembly.GetTypes ()
			.Where (type => type.IsSubclassOf (typeof(AbstractProperty)))
			.Where(type => type.BaseType != typeof(AbstractProperty))
			.Where(type => type != typeof(MultiplyingProperty))
			.OrderBy(type => type.BaseType.Name)
			.ToList();

		EditorGUILayout.BeginHorizontal ();
		searchInput = EditorGUILayout.TextField (searchInput);
		if (GUILayout.Button ("x", GUILayout.Width (20), GUILayout.Height(16))){
			searchInput = "";
		}
		EditorGUILayout.EndHorizontal ();

		types.RemoveAll (t => !t.Name.ToLower().Contains (searchInput.ToLower()));

		foreach (Type type in types) {
			GUI.skin = Resources.Load ("ProceduralGeneration/ComponentMenuSkin") as GUISkin;
			Texture icon = Resources.Load ("ProceduralGeneration/Icons/" + type.BaseType.Name) as Texture;
			string name = " " + ObjectNames.NicifyVariableName (type.Name);
			string tooltip = tooltipMapping.ContainsKey (type.Name) ? tooltipMapping [type.Name] : "No tooltip";
			GUIContent content = new GUIContent (name, icon, tooltip);
			if (GUILayout.Button (content)) {
				//AbstractProperty prop = (AbstractProperty)Activator.CreateInstance (type);
				Transform selectedTransform = Selection.activeTransform;
				if (selectedTransform != null) {					
					selectedTransform.gameObject.AddComponent (type);
				} else {
					Debug.Log ("No selected Object found");
				}
			}

			//DynamicTag dynTag = (DynamicTag)Activator.CreateInstance (type);
			//autoGenerated.Add (dynTag.ObtainTag ());
		}

		EditorGUILayout.EndScrollView ();
	}

	public void GenerateJSON(List<Type> types){
		TextAsset textAsset = Resources.Load ("ProceduralGeneration/Tooltips") as TextAsset;
		TooltipsContainer container = new TooltipsContainer ();
		List<ComponentTooltip> attributes = new List<ComponentTooltip> ();
		foreach (Type t in types) {
			ComponentTooltip newAttr = new ComponentTooltip ();
			newAttr.typeName = t.Name;
			newAttr.tooltip = "empty";
			attributes.Add (newAttr);
		}
		container.tooltips = attributes.ToArray ();
		string serialized = JsonUtility.ToJson (container, true);
		string path = Application.dataPath + "/Resources/ProceduralGeneration/Tooltips.json";
		File.WriteAllText (path, serialized);
	}
}
