using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CombinedAnimation))]
[CanEditMultipleObjects]
public class CombinedAnimationEditor : AnimationBehaviourEditor {
	protected override void drawChildInspectorGUI() {
		((CombinedAnimation)target).updateComponents();
		
		drawPropertiesExcludingDefaultHiddenAnd("animations");
		
		var animationCount = serializedObject.FindProperty("animations").arraySize;
		
		if (animationCount == 0) {
			EditorGUILayout.HelpBox("Add Animation Components to this object and they will show up here", MessageType.Info);
		} else {
			for (var i = 0; i < animationCount; i++) {
				drawAnimationInspector(i);
			}
		}
	}
	
	void drawAnimationInspector(int index) {
		var animation = serializedObject.FindProperty("animations").GetArrayElementAtIndex(index);
		if (animation.FindPropertyRelative("animation").objectReferenceValue == null) { return; }
		
		var name = animation.FindPropertyRelative("animation").objectReferenceValue.GetType().Name;
		animation.FindPropertyRelative("enabled").boolValue = EditorGUILayout.BeginToggleGroup(name, animation.FindPropertyRelative("enabled").boolValue);
		
		if (animation.FindPropertyRelative("enabled").boolValue) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(8);
			EditorGUILayout.BeginVertical();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Interval");
			
			var min = animation.FindPropertyRelative("startTime").floatValue;
			var max = animation.FindPropertyRelative("endTime").floatValue;
			EditorGUILayout.MinMaxSlider(ref min, ref max, 0, 1);
			animation.FindPropertyRelative("startTime").floatValue = min;
			animation.FindPropertyRelative("endTime").floatValue = max;
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
		}
		
		EditorGUILayout.EndToggleGroup();
	}
}
#endif

public class CombinedAnimation : AnimationBehaviour {
	[System.Serializable] public struct AnimationReference {
		public AnimationBehaviour animation;
		public bool enabled;
		public float startTime;
		public float endTime;
	}
	
	[Header("Input")]
	public AnimationReference[] animations;
	
	protected override void onAnimationBegin() {
		for (var i = 0; i < animations.Length; i++) {
			if (!animations[i].enabled) { continue; }
			animations[i].animation.resetAnimation(playBack: playBack);
		}
	}
	
	protected override void onAnimationDone() {
		for (var i = 0; i < animations.Length; i++) {
			if (!animations[i].enabled) { continue; }
			animations[i].animation.invokeProgress(done: true, backwards: playBack);
		}
	}
	
	protected override void onAnimationProgress(float progress) {
		for (var i = 0; i < animations.Length; i++) {
			var animation = animations[i];
			if (!animation.enabled) { continue; }
			var p = mapProgress(progress, animation.startTime, animation.endTime);
			animation.animation.invokeProgress(done: false, progress: p, backwards: playBack);
		}
	}
	
	#if UNITY_EDITOR
	public void updateComponents() {
		var newAnimations = new List<AnimationReference>(animations);
		var anyChanges = false;
		
		for (var i = 0; i < newAnimations.Count; i++) {
			if (newAnimations[i].animation != null) { continue; }
			newAnimations.RemoveAt(i);
			i -= 1;
			anyChanges = true;
		}
		
		var animComponents = GetComponents<AnimationBehaviour>();
		for (var component = 0; component < animComponents.Length; component++) {
			if (animComponents[component] == this) { continue; }
			var exists = false;
			
			for (var animation = 0; animation < newAnimations.Count; animation++) {
				if (newAnimations[animation].animation == animComponents[component]) { exists = true; break; }
			}
			
			if (!exists) {
				newAnimations.Add(new AnimationReference { animation = animComponents[component], startTime = 0, endTime = 1 });
				anyChanges = true;
			}
		}
		
		if (anyChanges) {
			animations = newAnimations.ToArray();
		}
	}
	
	void Reset() {
		var animComponents = GetComponents<AnimationBehaviour>();
		
		var animations = new List<AnimationReference>();
		for (var i = 0; i < animComponents.Length; i++) {
			if (animComponents[i] == this) { continue; }
			animations.Add(new AnimationReference { animation = animComponents[i], startTime = 0, endTime = 1 });
		}
		
		this.animations = animations.ToArray();
		curve = AnimationCurve.Linear(0, 0, 1, 1);
		backCurve = AnimationCurve.Linear(0, 0, 1, 1);
	}
	#endif
}
