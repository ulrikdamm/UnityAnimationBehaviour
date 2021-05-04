using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PositionAnimation))]
public class PositionAnimationEditor : AnimationBehaviourEditor {
	PositionAnimation obj => (PositionAnimation)target;
	
	protected override void drawChildInspectorGUI() {
		drawPropertiesExcludingDefaultHidden();
		
		GUILayout.Space(10);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Set");
		
		GUI.enabled = (Vector3.Distance(obj.currentPosition, obj.fromPosition) > 0.001f);
		if (GUILayout.Button("From")) { obj.setFrom(); }
		
		GUI.enabled = (Vector3.Distance(obj.currentPosition, obj.toPosition) > 0.001f);
		if (GUILayout.Button("To")) { obj.setTo(); }
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Restore");
		
		GUI.enabled = (Vector3.Distance(obj.currentPosition, obj.fromPosition) > 0.001f);
		if (GUILayout.Button("From")) { obj.restoreFrom(); }
		
		GUI.enabled = (Vector3.Distance(obj.currentPosition, obj.toPosition) > 0.001f);
		if (GUILayout.Button("To")) { obj.restoreTo(); }
		
		EditorGUILayout.EndHorizontal();
	}
}
#endif

public class PositionAnimation : AnimationBehaviour {
	public Space space;
	public Vector3 fromPosition;
	public Vector3 toPosition;
	
	Vector3 from, to;
	
	Transform _transform;
	Transform cachedTransform => _transform != null ? _transform : _transform = transform;
	
	public Vector3 currentPosition {
		get => space switch { Space.Self => cachedTransform.localPosition, Space.World => cachedTransform.position, _ => throw new System.Exception() };
		set {
			switch (space) {
				case Space.Self: cachedTransform.localPosition = value; break;
				case Space.World: cachedTransform.position = value; break;
			}
		}
	}
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? currentPosition : fromPosition);
		to = (playBack && beginFromCurrent ? currentPosition : toPosition);
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		currentPosition = Vector3.LerpUnclamped(from, to, progress);
	}
	
	#if UNITY_EDITOR
	[ContextMenu("Set current position as from")]
	public void setFrom() {
		Undo.RecordObject(this, "Set current position as from");
		fromPosition = currentPosition;
	}
	
	[ContextMenu("Set current position as to")]
	public void setTo() {
		Undo.RecordObject(this, "Set current position as to");
		toPosition = currentPosition;
	}
	
	[ContextMenu("Restore from-position")]
	public void restoreFrom() {
		Undo.RecordObject(this, "Restore from-position");
		currentPosition = fromPosition;
	}
	
	[ContextMenu("Restore to-position")]
	public void restoreTo() {
		Undo.RecordObject(this, "Restore to-position");
		currentPosition = toPosition;
	}
	#endif
}
