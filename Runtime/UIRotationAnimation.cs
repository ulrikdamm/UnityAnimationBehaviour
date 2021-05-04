using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

using static UIRotationAnimation.Axis;

[CustomEditor(typeof(UIRotationAnimation))]
public class UIRotationAnimationEditor : AnimationBehaviourEditor {
	UIRotationAnimation obj => (UIRotationAnimation)target;
	SerializedProperty axis => serializedObject.FindProperty("axis");
	SerializedProperty space => serializedObject.FindProperty("space");
	SerializedProperty fromAngle => serializedObject.FindProperty("fromAngle");
	SerializedProperty toAngle => serializedObject.FindProperty("toAngle");
	
	protected override void drawChildInspectorGUI() {
		drawPropertiesExcludingDefaultHiddenAnd(axis, space, fromAngle, toAngle);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Axis and space");
		EditorGUILayout.PropertyField(axis, label: GUIContent.none);
		EditorGUILayout.PropertyField(space, label: GUIContent.none);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(fromAngle);
		GUI.enabled = !Mathf.Approximately(fromAngle.floatValue, obj.currentAngle);
		if (GUILayout.Button("Set", GUILayout.Width(50))) { fromAngle.floatValue = obj.currentAngle; }
		if (GUILayout.Button("Restore", GUILayout.Width(70))) { obj.currentAngle = fromAngle.floatValue; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(toAngle);
		GUI.enabled = !Mathf.Approximately(toAngle.floatValue, obj.currentAngle);
		if (GUILayout.Button("Set", GUILayout.Width(50))) { toAngle.floatValue = obj.currentAngle; }
		if (GUILayout.Button("Restore", GUILayout.Width(70))) { obj.currentAngle = toAngle.floatValue; }
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
	}
}
#endif

public class UIRotationAnimation : AnimationBehaviour {
	[System.Serializable] public enum Axis { x, y, z };
	public Axis axis = Axis.z;
	
	public Space space = Space.World;
	public float fromAngle = 0;
	public float toAngle = 180;
	
	Quaternion from, to;
	
	Transform _transform;
	Transform cachedTransform => _transform != null ? _transform : _transform = transform;
    
	public Quaternion currentRotation {
		get => space switch { Space.Self => cachedTransform.localRotation, Space.World => cachedTransform.rotation, _ => throw new System.Exception() };
		set {
			switch (space) {
				case Space.Self: cachedTransform.localRotation = value; break;
				case Space.World: cachedTransform.rotation = value; break;
			}
		}
	}
	
	public float currentAngle {
		get => axis switch {
			Axis.x => currentRotation.eulerAngles.x,
			Axis.y => currentRotation.eulerAngles.y,
			Axis.z => currentRotation.eulerAngles.z,
			_ => throw new System.Exception()
		};
		set => currentRotation = Quaternion.AngleAxis(value, angleAxis);
	}
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? currentRotation : quaternionFrom);
		to = (playBack && beginFromCurrent ? currentRotation : quaternionTo);
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		currentAngle = Mathf.LerpAngle(fromAngle, toAngle, progress);
	}
	
	Vector3 singleAxis(Vector3 euler) => axis switch {
		Axis.x => new Vector3(euler.x, 0, 0),
		Axis.y => new Vector3(0, euler.y, 0),
		Axis.z => new Vector3(0, 0, euler.z),
		_ => throw new System.Exception()
	};
	
	Vector3 singleAxis(float angle) => axis switch {
		Axis.x => new Vector3(angle, 0, 0),
		Axis.y => new Vector3(0, angle, 0),
		Axis.z => new Vector3(0, 0, angle),
		_ => throw new System.Exception()
	};
	
	Quaternion quaternionFrom => Quaternion.Euler(singleAxis(fromAngle));
	Quaternion quaternionTo => Quaternion.Euler(singleAxis(toAngle));
	Vector3 angleAxis => singleAxis(1);
}
