using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UISizeAnimation))]
public class UISizeAnimationEditor : AnimationBehaviourEditor {
	UISizeAnimation obj => (UISizeAnimation)target;
	SerializedProperty fromSize => serializedObject.FindProperty("fromSize");
	SerializedProperty toSize => serializedObject.FindProperty("toSize");
	
	protected override void drawChildInspectorGUI() {
		drawPropertiesExcludingDefaultHiddenAnd(fromSize, toSize);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("From size");
		var size = fromSize.vector2Value;
		size.x = EditorGUILayout.FloatField(size.x);
		size.y = EditorGUILayout.FloatField(size.y);
		fromSize.vector2Value = size;
		
		GUI.enabled = Vector2.Distance(fromSize.vector2Value, obj.currentSize) > 0.001f;
		if (GUILayout.Button("Set", GUILayout.Width(50))) { fromSize.vector2Value = obj.currentSize; }
		if (GUILayout.Button("Restore", GUILayout.Width(70))) { obj.currentSize = fromSize.vector2Value; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("To size");
		
		size = toSize.vector2Value;
		size.x = EditorGUILayout.FloatField(size.x);
		size.y = EditorGUILayout.FloatField(size.y);
		toSize.vector2Value = size;
		
		GUI.enabled = Vector2.Distance(toSize.vector2Value, obj.currentSize) > 0.001f;
		if (GUILayout.Button("Set", GUILayout.Width(50))) { toSize.vector2Value = obj.currentSize; }
		if (GUILayout.Button("Restore", GUILayout.Width(70))) { obj.currentSize = toSize.vector2Value; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
	}
}
#endif

public class UISizeAnimation : AnimationBehaviour {
	public Vector2 fromSize;
	public Vector2 toSize;
	
	Vector2 from, to;
	
	RectTransform _rect;
	RectTransform rect => _rect != null ? _rect : _rect = (RectTransform)transform;
	
	public Vector2 currentSize {
		get => rect.sizeDelta;
		set => rect.sizeDelta = value;
	}
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? rect.sizeDelta : fromSize);
		to = (playBack && beginFromCurrent ? rect.sizeDelta : toSize);
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		currentSize = Vector2.LerpUnclamped(fromSize, toSize, progress);
	}
}
