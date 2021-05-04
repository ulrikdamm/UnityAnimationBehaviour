using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UIPositionAnimation))]
public class UIPositionAnimationEditor : AnimationBehaviourEditor {
	UIPositionAnimation obj => (UIPositionAnimation)target;
	SerializedProperty animatePivot => serializedObject.FindProperty("animatePivot");
	SerializedProperty fromPivot => serializedObject.FindProperty("fromPivot");
	SerializedProperty toPivot => serializedObject.FindProperty("toPivot");
	
	SerializedProperty animateAnchor => serializedObject.FindProperty("animateAnchor");
	SerializedProperty fromAnchorMin => serializedObject.FindProperty("fromAnchorMin");
	SerializedProperty toAnchorMin => serializedObject.FindProperty("toAnchorMin");
	SerializedProperty fromAnchorMax => serializedObject.FindProperty("fromAnchorMax");
	SerializedProperty toAnchorMax => serializedObject.FindProperty("toAnchorMax");
	
	protected override void drawChildInspectorGUI() {
		drawPropertiesExcludingDefaultHidden();
		
		EditorGUILayout.PropertyField(animatePivot);
		if (animatePivot.boolValue) {
			EditorGUILayout.PropertyField(fromPivot);
			EditorGUILayout.PropertyField(toPivot);
		}
		
		EditorGUILayout.PropertyField(animateAnchor);
		if (animateAnchor.boolValue) {
			EditorGUILayout.PropertyField(fromAnchorMin);
			EditorGUILayout.PropertyField(toAnchorMin);
			EditorGUILayout.PropertyField(fromAnchorMax);
			EditorGUILayout.PropertyField(toAnchorMax);
		}
		
		GUILayout.Space(10);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Set");
		if (GUILayout.Button("From")) { obj.setFrom(); }
		if (GUILayout.Button("To")) { obj.setTo(); }
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Restore");
		if (GUILayout.Button("From")) { obj.restoreFrom(); }
		if (GUILayout.Button("To")) { obj.restoreTo(); }
		EditorGUILayout.EndHorizontal();
	}
}
#endif

public class UIPositionAnimation : AnimationBehaviour {
	public Vector2 fromPosition;
	public Vector2 toPosition;
	
	[Space]
	[HideInInspector] public bool animatePivot;
	[HideInInspector] public Vector2 fromPivot;
	[HideInInspector] public Vector2 toPivot;
	
	[Space]
	[HideInInspector] public bool animateAnchor;
	[HideInInspector] public Vector2 fromAnchorMin;
	[HideInInspector] public Vector2 toAnchorMin;
	[HideInInspector] public Vector2 fromAnchorMax;
	[HideInInspector] public Vector2 toAnchorMax;
	
	Vector2 from, to;
	Vector2 pivotFrom, pivotTo;
	Vector2 anchorMinFrom, anchorMinTo;
	Vector2 anchorMaxFrom, anchorMaxTo;
	
	RectTransform _rect;
	RectTransform rect => _rect != null ? _rect : _rect = (RectTransform)transform;
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? rect.anchoredPosition : fromPosition);
		to = (playBack && beginFromCurrent ? rect.anchoredPosition : toPosition);
		
		if (animatePivot) {
			pivotFrom = (!playBack && beginFromCurrent ? rect.pivot : fromPivot);
			pivotTo = (playBack && beginFromCurrent ? rect.pivot : toPivot);
		}
		
		if (animateAnchor) {
			anchorMinFrom = (!playBack && beginFromCurrent ? rect.anchorMin : anchorMinFrom);
			anchorMinTo = (playBack && beginFromCurrent ? rect.anchorMin : anchorMinTo);
			anchorMaxFrom = (!playBack && beginFromCurrent ? rect.anchorMax : anchorMaxFrom);
			anchorMaxTo = (playBack && beginFromCurrent ? rect.anchorMax : anchorMaxTo);
		}
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		rect.anchoredPosition = Vector3.LerpUnclamped(from, to, progress);
		
		if (animatePivot) {
			rect.pivot = Vector2.LerpUnclamped(pivotFrom, pivotTo, progress);
		}
		
		if (animateAnchor) {
			rect.anchorMin = Vector2.LerpUnclamped(anchorMinFrom, anchorMinTo, progress);
			rect.anchorMax = Vector2.LerpUnclamped(anchorMaxFrom, anchorMaxTo, progress);
		}
	}
	
	#if UNITY_EDITOR
	[ContextMenu("Set current position as from")]
	public void setFrom() {
		Undo.RecordObject(this, "Set current position as from");
		fromPosition = rect.anchoredPosition;
		fromPivot = rect.pivot;
		fromAnchorMin = rect.anchorMin;
		fromAnchorMax = rect.anchorMax;
	}
	
	[ContextMenu("Set current position as to")]
	public void setTo() {
		Undo.RecordObject(this, "Set current position as to");
		toPosition = rect.anchoredPosition;
		toPivot = rect.pivot;
		toAnchorMin = rect.anchorMin;
		toAnchorMax = rect.anchorMax;
	}
	
	[ContextMenu("Restore from-position")]
	public void restoreFrom() {
		Undo.RecordObject(this, "Restore from-position");
		rect.anchoredPosition = fromPosition;
		if (animatePivot) { rect.pivot = fromPivot; }
		if (animateAnchor) { rect.anchorMin = fromAnchorMin; }
		if (animateAnchor) { rect.anchorMax = fromAnchorMax; }
	}
	
	[ContextMenu("Restore to-position")]
	public void restoreTo() {
		Undo.RecordObject(this, "Restore to-position");
		rect.anchoredPosition = toPosition;
		if (animatePivot) { rect.pivot = toPivot; }
		if (animateAnchor) { rect.anchorMin = toAnchorMin; }
		if (animateAnchor) { rect.anchorMax = toAnchorMax; }
	}
	#endif
}
