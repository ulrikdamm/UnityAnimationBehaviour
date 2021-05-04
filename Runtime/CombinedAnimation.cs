using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CombinedAnimation))]
public class CombinedAnimationEditor : AnimationBehaviourEditor {
	protected override void drawChildInspectorGUI() {
		var obj = ((CombinedAnimation)target);
		var animations = serializedObject.FindProperty("animations");
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Components");
		if (EditorGUILayout.DropdownButton(new GUIContent($"{animations.arraySize} components"), FocusType.Keyboard)) {
			var menu = new GenericMenu();
			buildAnimationsMenu(obj.gameObject, menu);
			menu.ShowAsContext();
		}
		EditorGUILayout.EndHorizontal();
		
		drawPropertiesExcludingDefaultHiddenAnd("animations");
		
		if (animations.arraySize == 0) {
			EditorGUILayout.HelpBox("Add some animations to get started", MessageType.Info);
		} else {
			for (var i = 0; i < animations.arraySize; i++) {
				drawAnimationInspector(i, out var removed);
				if (removed) { i -= 1; }
			}
		}
	}
	
	void buildAnimationsMenu(GameObject obj, GenericMenu menu, string path = "") {
		var components = obj.GetComponents<AnimationBehaviour>();
		var didAddComponents = false;
		
		for (var i = 0; i < components.Length; i++) {
			if (components[i] == target) { continue; }
			menu.AddItem(new GUIContent(path + components[i].GetType().Name), on: containsAnimation(components[i], out _), itemSelected, components[i]);
			didAddComponents = true;
		}
		
		if (didAddComponents || obj.transform.childCount > 0) {
			menu.AddSeparator(path);
		}
		
		for (var i = 0; i < obj.transform.childCount; i++) {
			var child = obj.transform.GetChild(i);
			buildAnimationsMenu(child.gameObject, menu, $"{path}{child.name}/");
		}
	}
	
	void itemSelected(object obj) {
		if (!(obj is AnimationBehaviour)) { return; }
		var animation = (AnimationBehaviour)obj;
		var animations = serializedObject.FindProperty("animations");
		
		if (containsAnimation(animation, out var index)) {
			animations.DeleteArrayElementAtIndex(index);
		} else {
			var length = animations.arraySize;
			animations.InsertArrayElementAtIndex(length);
			var entry = animations.GetArrayElementAtIndex(length);
			entry.FindPropertyRelative("animation").objectReferenceValue = animation;
			entry.FindPropertyRelative("startTime").floatValue = 0;
			entry.FindPropertyRelative("endTime").floatValue = 1;
			entry.FindPropertyRelative("enabled").boolValue = true;
		}
		
		serializedObject.ApplyModifiedProperties();
	}
	
	bool containsAnimation(AnimationBehaviour animation, out int index) {
		var obj = ((CombinedAnimation)target);
		if (obj.animations == null) { index = default(int); return false; }
		
		for (var i = 0; i < obj.animations.Length; i++) {
			if (obj.animations[i].animation == animation) { index = i; return true; }
		}
		
		index = default(int);
		return false;
	}
	
	void drawAnimationInspector(int index, out bool removed) {
		var animations = serializedObject.FindProperty("animations");
		var animation = animations.GetArrayElementAtIndex(index);
		var animationObject = animation.FindPropertyRelative("animation").objectReferenceValue;
		if (animationObject == null || !(animationObject is AnimationBehaviour) || !((AnimationBehaviour)animationObject).transform.IsChildOf(((Component)target).transform)) {
			animations.DeleteArrayElementAtIndex(index);
			removed = true;
			return;
		}
		
		removed = false;
		var name = objectChildPath((Component)animationObject) + animationObject.GetType().Name;
		animation.FindPropertyRelative("enabled").boolValue = EditorGUILayout.BeginToggleGroup(name, animation.FindPropertyRelative("enabled").boolValue);
		
		if (animation.FindPropertyRelative("enabled").boolValue) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(8);
			EditorGUILayout.BeginVertical();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(animation.FindPropertyRelative("startTime"), label: GUIContent.none, GUILayout.Width(50));
			GUILayout.Label("to", GUILayout.ExpandWidth(false));
			EditorGUILayout.PropertyField(animation.FindPropertyRelative("endTime"), label: GUIContent.none, GUILayout.Width(50));
			EditorGUILayout.Space();
			
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
	
	string objectChildPath(Component obj, string suffix = "") {
		if (obj.gameObject == ((Component)target).gameObject) { return suffix; }
		if (suffix != null) { return objectChildPath(obj.transform.parent, $"{obj.name} / {suffix}"); }
		return obj.name;
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
	
	public AnimationReference[] animations;
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		if (playBack) { progress = 1 - progress; }
		
		for (var i = 0; i < animations.Length; i++) {
			var entry = animations[i];
			if (entry.animation == null || !entry.enabled) { continue; }
			
			var at = Mathf.InverseLerp(entry.startTime, entry.endTime, progress);
			entry.animation.invokeProgress(at, state, backwards: playBack);
		}
	}
}
