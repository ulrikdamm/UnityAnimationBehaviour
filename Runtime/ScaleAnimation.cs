using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ScaleAnimation))]
public class ScaleAnimationEditor : AnimationBehaviourEditor {
	ScaleAnimation obj => (ScaleAnimation)target;
	SerializedProperty fromScale => serializedObject.FindProperty("fromScale");
	SerializedProperty toScale => serializedObject.FindProperty("toScale");
	
	protected override void drawChildInspectorGUI() {
		drawPropertiesExcludingDefaultHiddenAnd(fromScale, toScale);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(fromScale);
		GUI.enabled = Vector3.Distance(fromScale.vector3Value, obj.transform.localScale) > 0.001f;
		if (GUILayout.Button("Set", GUILayout.Width(50))) { fromScale.vector3Value = obj.transform.localScale; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(toScale);
		GUI.enabled = Vector3.Distance(toScale.vector3Value, obj.transform.localScale) > 0.001f;
		if (GUILayout.Button("Set", GUILayout.Width(50))) { toScale.vector3Value = obj.transform.localScale; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Restore");
		GUI.enabled = Vector3.Distance(fromScale.vector3Value, obj.transform.localScale) > 0.001f;
		if (GUILayout.Button("From")) { obj.transform.localScale = fromScale.vector3Value; }
		GUI.enabled = Vector3.Distance(toScale.vector3Value, obj.transform.localScale) > 0.001f;
		if (GUILayout.Button("To")) { obj.transform.localScale = toScale.vector3Value; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
	}
}
#endif

public class ScaleAnimation : AnimationBehaviour {
	public Vector3 fromScale = Vector3.zero;
	public Vector3 toScale = Vector3.one;
	
	Vector3 from, to;
	
	Transform _transform;
	Transform cachedTransform => _transform != null ? _transform : _transform = transform;
	
	protected override void Reset() {
		base.Reset();
		toScale = cachedTransform.localScale;
	}
	
	public void beginAnimation(Vector3 fromValue, Vector3 toValue) {
		beginFromCurrent = false;
		this.fromScale = fromValue;
		this.toScale = toValue;
		beginAnimation();
	}
	
	public IEnumerator animationRoutine(Vector3 fromValue, Vector3 toValue) {
		beginFromCurrent = false;
		this.fromScale = fromValue;
		this.toScale = toValue;
		yield return animationRoutine();
	}
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? cachedTransform.localScale : fromScale);
		to = (playBack && beginFromCurrent ? cachedTransform.localScale : toScale);
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		var before = cachedTransform.localScale;
		var after = Vector3.LerpUnclamped(from, to, progress);
		cachedTransform.localScale = after;
	}
}
