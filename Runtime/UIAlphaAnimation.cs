using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UIAlphaAnimation))]
public class UIAlphaAnimationEditor : AnimationBehaviourEditor {
	UIAlphaAnimation obj => (UIAlphaAnimation)target;
	SerializedProperty fromAlpha => serializedObject.FindProperty("fromAlpha");
	SerializedProperty toAlpha => serializedObject.FindProperty("toAlpha");
	
	protected override void drawChildInspectorGUI() {
		drawPropertiesExcludingDefaultHiddenAnd(fromAlpha, toAlpha);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(fromAlpha);
		GUI.enabled = !Mathf.Approximately(fromAlpha.floatValue, obj.currentAlpha);
		if (GUILayout.Button("Set", GUILayout.Width(50))) { fromAlpha.floatValue = obj.currentAlpha; }
		if (GUILayout.Button("Restore", GUILayout.Width(70))) { obj.currentAlpha = fromAlpha.floatValue; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(toAlpha);
		GUI.enabled = !Mathf.Approximately(toAlpha.floatValue, obj.currentAlpha);
		if (GUILayout.Button("Set", GUILayout.Width(50))) { toAlpha.floatValue = obj.currentAlpha; }
		if (GUILayout.Button("Restore", GUILayout.Width(70))) { obj.currentAlpha = toAlpha.floatValue; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
	}
}
#endif

[RequireComponent(typeof(CanvasGroup))]
public class UIAlphaAnimation : AnimationBehaviour {
	public float fromAlpha = 0;
	public float toAlpha = 1;
	public bool changeCanvasGroupInteractable = true;
	
	float from, to;
	
	CanvasGroup _canvasGroup;
	CanvasGroup canvasGroup => _canvasGroup != null ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();
	
	public float currentAlpha {
		get => canvasGroup.alpha;
		set {
			canvasGroup.alpha = value;
			if (changeCanvasGroupInteractable) {
				canvasGroup.interactable = (canvasGroup.alpha > 0.001f);
				canvasGroup.blocksRaycasts = canvasGroup.interactable;
			}
		}
	}
	
	public void beginAnimation(float fromValue, float toValue) {
		beginFromCurrent = false;
		this.fromAlpha = fromValue;
		this.toAlpha = toValue;
		beginAnimation();
	}
	
	public IEnumerator animationRoutine(float fromValue, float toValue) {
		beginFromCurrent = false;
		this.fromAlpha = fromValue;
		this.toAlpha = toValue;
		yield return animationRoutine();
	}
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? currentAlpha : fromAlpha);
		to = (playBack && beginFromCurrent ? currentAlpha : toAlpha);
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
	}
}
