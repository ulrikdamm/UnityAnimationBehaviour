// #define EditorCoroutine

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if EditorCoroutine
using Unity.EditorCoroutines.Editor;
#endif

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(AnimationBehaviour), true)]
[CanEditMultipleObjects]
public class AnimationBehaviourEditor : Editor {
	float sliderValue = 0;
	bool backwards = false;
	public bool showTest = false;
	
	protected virtual void drawChildInspectorGUI() {
		DrawPropertiesExcluding(serializedObject, excludedProperties);
	}
	
	protected string[] excludedProperties = new string[] { "m_Script", "duration", "durationBack", "ignoreTimeScale", "startAnimation", "curve", "onCompletion", "backCurve", "onBackCompletion" };
	
	protected void drawPropertiesExcludingDefaultHiddenAnd(params string[] properties) {
		var allProperties = new string[excludedProperties.Length + properties.Length];
		excludedProperties.CopyTo(allProperties, 0);
		properties.CopyTo(allProperties, excludedProperties.Length);
		DrawPropertiesExcluding(serializedObject, allProperties);
	}
	
	public override void OnInspectorGUI() {
		var obj = (AnimationBehaviour)target;
		
		var curve = serializedObject.FindProperty("curve");
		var backCurve = serializedObject.FindProperty("backCurve");
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("ignoreTimeScale"));
		
		if (curve.isExpanded) { EditorGUILayout.Space(); }
		curve.isExpanded = EditorGUILayout.Foldout(curve.isExpanded, "Forwards animation");
		if (curve.isExpanded) { showForwardAnimationEditor(); }
		
		if (backCurve.isExpanded) { EditorGUILayout.Space(); }
		backCurve.isExpanded = EditorGUILayout.Foldout(backCurve.isExpanded, "Backwards animation");
		if (backCurve.isExpanded) { showBackwardAnimationEditor(); }
		
		EditorGUILayout.Space();
		
		drawChildInspectorGUI();
		
		serializedObject.ApplyModifiedProperties();
		
		EditorGUILayout.Space();
		showTest = EditorGUILayout.Foldout(showTest, "Test editor");
		if (showTest) { showTestEditor(); }
	}
	
	void showForwardAnimationEditor() {
		var duration = serializedObject.FindProperty("duration");
		var durationBack = serializedObject.FindProperty("durationBack");
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		EditorGUILayout.BeginVertical();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("startAnimation"));
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("curve"));
		showCurvePresetDropdown(setForwardCurve, width: 55);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(duration);
		if (((AnimationBehaviour)target).durationBackSameAsForward) { durationBack.floatValue = duration.floatValue; }
		EditorGUILayout.PropertyField(serializedObject.FindProperty("onCompletion"));
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
	
	void showBackwardAnimationEditor() {
		var duration = serializedObject.FindProperty("duration");
		var durationBack = serializedObject.FindProperty("durationBack");
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		EditorGUILayout.BeginVertical();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("backCurve"));
		showCurvePresetDropdown(setBackwardCurve, width: 55);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		if (((AnimationBehaviour)target).durationBackSameAsForward) {
			GUI.enabled = false;
			EditorGUILayout.PropertyField(durationBack);
			GUI.enabled = true;
		} else {
			EditorGUILayout.PropertyField(durationBack);
		}
		
		serializedObject.FindProperty("durationBackSameAsForward").boolValue = !EditorGUILayout.Toggle(!serializedObject.FindProperty("durationBackSameAsForward").boolValue, GUILayout.Width(14));
		EditorGUILayout.LabelField("Custom", GUILayout.Width(45));
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("onBackCompletion"));
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
	
	void showTestEditor() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		EditorGUILayout.BeginVertical();
		
		EditorGUILayout.BeginHorizontal();
		var newSliderValue = GUILayout.HorizontalSlider(sliderValue, 0, 1);
		var backwardsStyle = new GUIStyle(EditorStyles.toggle);
		backwardsStyle.fixedWidth = 80;
		var newBackwards = GUILayout.Toggle(backwards, "Backwards", backwardsStyle);
		EditorGUILayout.EndHorizontal();
		
		if (!Mathf.Approximately(newSliderValue, sliderValue) || backwards != newBackwards) {
			backwards = newBackwards;
			sliderValue = newSliderValue;
			
			for (var i = 0; i < targets.Length; i++) {
				((AnimationBehaviour)targets[i]).invokeProgress(done: false, progress: (backwards ? 1 - sliderValue : sliderValue), backwards: backwards);
			}
		}
		
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Begin")) {
			for (var i = 0; i < targets.Length; i++) {
				var target = (AnimationBehaviour)targets[i];
				
				if (Application.isPlaying) {
					target.beginAnimation();
				} else {
					#if EditorCoroutine
					target.resetAnimation(playBack: false);
					EditorCoroutineUtility.StartCoroutine(target.animationRoutine(), this);
					#else
					Debug.LogError("Install the Editor Coroutines to preview animations in the editor");
					#endif
				}
			}
		}
		
		if (GUILayout.Button("Begin back")) {
			for (var i = 0; i < targets.Length; i++) {
				var target = (AnimationBehaviour)targets[i];
				
				if (Application.isPlaying) {
					target.beginAnimationBack();
				} else {
					#if EditorCoroutine
					target.resetAnimation(playBack: true);
					EditorCoroutineUtility.StartCoroutine(target.animationRoutine(), this);
					#else
					Debug.LogError("Install the Editor Coroutines to preview animations in the editor");
					#endif
				}
			}
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
	
	void showCurvePresetDropdown(GenericMenu.MenuFunction2 function, float width) {
		var presetStyle = new GUIStyle(EditorStyles.popup);
		presetStyle.fixedWidth = width;
		if (!EditorGUILayout.DropdownButton(new GUIContent("Presets"), FocusType.Passive, presetStyle)) { return; }
		
		var menu = new GenericMenu();
		menu.AddItem(new GUIContent("Linear"), false, function, new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1)));
		menu.AddItem(new GUIContent("Ease in"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1, 2, 2)));
		menu.AddItem(new GUIContent("Ease out"), false, function, new AnimationCurve(new Keyframe(0, 0, 2, 2), new Keyframe(1, 1)));
		menu.AddItem(new GUIContent("Ease in-out"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		menu.AddItem(new GUIContent("Spring in"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1, 1, 1, 0.33f, 0.33f), new Keyframe(1, 1)));
		menu.AddItem(new GUIContent("Spring out"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0, 1, 1, 0.33f, 0.33f), new Keyframe(1, 1)));
		menu.AddItem(new GUIContent("Turn back again"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0)));
		menu.ShowAsContext();
	}
	
	void setForwardCurve(object curve) {
		serializedObject.FindProperty("curve").animationCurveValue = (AnimationCurve)curve;
		serializedObject.ApplyModifiedProperties();
	}
	
	void setBackwardCurve(object curve) {
		serializedObject.FindProperty("backCurve").animationCurveValue = (AnimationCurve)curve;
		serializedObject.ApplyModifiedProperties();
	}
}
#endif

public abstract class AnimationBehaviour : MonoBehaviour, IgnoreTimeScale {
	public AnimationCurve curve;
	public AnimationCurve backCurve;
	[HideInInspector] public bool durationBackSameAsForward = true;
	public float duration = 0.3f;
	public float durationBack = 0.3f;
	public bool ignoreTimeScale = false;
	
	public enum AnimationStart { no, onStart, onEnable };
	public AnimationStart startAnimation;
	
	public enum CompletionAction { none, disable, destroy, loop, playReverse };
	public CompletionAction onCompletion;
	public CompletionAction onBackCompletion;
	
	protected bool playBack;
	public bool isPlayBack => playBack;
	public bool isPlaying => startTime.HasValue;
	public float? startTime;
	[System.NonSerialized] public bool paused = false;
	
	protected virtual void onAnimationBegin() {}
	protected abstract void onAnimationDone();
	protected abstract void onAnimationProgress(float progress);
	
	public delegate void AnimationEvent();
	public event AnimationEvent animationBeginEvent;
	public event AnimationEvent animationEndEvent;
	public delegate void AnimationProgressEvent(float progress);
	public event AnimationProgressEvent animationProgressEvent;
	
	void Reset() {
		curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		backCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	}
	
	public void resetAnimation(bool playBack = false) {
		this.playBack = playBack;
		onAnimationBegin();
		for (var i = 0; i < callbacks.Count; i++) { callbacks[i].animationBegin(this); }
	}
	
	public void invokeProgress(bool done, float progress = 1, bool backwards = false) {
		playBack = backwards;
		
		if (done) {
			onAnimationDone();
		} else if (backwards) {
			onAnimationProgress(backCurve.Evaluate(progress));
		} else {
			onAnimationProgress(curve.Evaluate(progress));
		}
	}
	
	public interface Callback {
		void animationBegin(AnimationBehaviour animation);
		void animationProgress(AnimationBehaviour animation, float progress);
		void animationDone(AnimationBehaviour animation);
	}
	List<Callback> callbacks = new List<Callback>();
	
	public void registerCallback(Callback callback) {
		callbacks.Add(callback);
	}
	
	void Start() {
		if (!startTime.HasValue) {
			if (startAnimation == AnimationStart.onStart) { beginAnimation(); }
		}
	}
	
	void OnEnable() {
		if (!startTime.HasValue) {
			if (startAnimation == AnimationStart.onEnable) { beginAnimation(); }
		}
	}
	
	void OnDisable() {
		if (startTime.HasValue) {
			endAnimation();
			startTime = null;
		}
	}
	
	public void beginAnimationBack() {
		startTime = currentTime();
		playBack = true;
		enabled = true;
		onAnimationBegin();
		stepAnimation(ref startTime);
		
		for (var i = 0; i < callbacks.Count; i++) { callbacks[i].animationBegin(this); }
		if (animationBeginEvent != null) { animationBeginEvent(); }
	}
	
	public void beginAnimation() {
		startTime = currentTime();
		playBack = false;
		enabled = true;
		onAnimationBegin();
		stepAnimation(ref startTime);
		
		for (var i = 0; i < callbacks.Count; i++) { callbacks[i].animationBegin(this); }
		if (animationBeginEvent != null) { animationBeginEvent(); }
	}
	
	public IEnumerator animationRoutine() {
		startTime = currentTime();
		stepAnimation(ref startTime);
		
		while (startTime != null) {
			yield return new WaitForEndOfFrame();
			
			if (paused) { startTime += deltaTime(); }
			else { stepAnimation(ref startTime); }
		}
	}
	
	public virtual void Update() {
		stepAnimation(ref startTime);
	}
	
	float currentTime() {
		if (!Application.isPlaying) { return Time.realtimeSinceStartup; }
		if (ignoreTimeScale) { return Time.unscaledTime; }
		return Time.time;
	}
	
	float deltaTime() {
		if (ignoreTimeScale) { return Time.unscaledDeltaTime; }
		return Time.deltaTime;
	}
	
	void stepAnimation(ref float? startTime) {
		if (!startTime.HasValue) { return; }
		var time = (currentTime() - startTime.Value) / (playBack ? durationBack : duration);
		
		if (time > 1) {
			startTime = null;
			endAnimation();
			return;
		}
		
		updateAnimation(time);
	}
	
	void endAnimation() {
		onAnimationDone();
		
		var completion = (playBack ? onBackCompletion  : onCompletion);
		
		for (var i = 0; i < callbacks.Count; i++) { callbacks[i].animationDone(this); }
		if (animationEndEvent != null) { animationEndEvent(); }
		
		switch (completion) {
			case CompletionAction.none: break;
			case CompletionAction.disable: gameObject.SetActive(false); break;
			case CompletionAction.destroy: Destroy(gameObject); break;
			case CompletionAction.loop: beginAnimation(); break;
			case CompletionAction.playReverse: if (isPlayBack) { beginAnimation(); } else { beginAnimationBack(); } break;
		}
	}
	
	void updateAnimation(float time) {
		float progress;
		
		if (playBack) {
			progress = 1 - backCurve.Evaluate(time);
		} else {
			progress = curve.Evaluate(time);
		}
		
		onAnimationProgress(progress);
		
		for (var i = 0; i < callbacks.Count; i++) { callbacks[i].animationProgress(this, progress); }
		if (animationProgressEvent != null) { animationProgressEvent(progress); }
	}
	
	public void setIgnoreTimeScale(bool ignore) {
		ignoreTimeScale = ignore;
	}
	
	public static float mapProgress(float progress, float start, float end, bool clamped = true) {
		if (clamped && progress < start) { return 0; }
		if (clamped && progress > end) { return 1; }
		return (progress - start) / (end - start);
	}
}
