using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(AnimationBehaviour), true)]
public class AnimationBehaviourEditor : Editor {
	float sliderValue = 0;
	bool backwards = false;
	static bool showPreview = false;
	object animationRoutine;
	
	AnimationBehaviour selectedObject => (AnimationBehaviour)target;
	SerializedProperty curve => serializedObject.FindProperty("curve");
	SerializedProperty backCurve => serializedObject.FindProperty("backCurve");
	SerializedProperty script => serializedObject.FindProperty("m_Script");
	SerializedProperty ignoreTimeScale => serializedObject.FindProperty("ignoreTimeScale");
	SerializedProperty beginFromCurrent => serializedObject.FindProperty("beginFromCurrent");
	SerializedProperty duration => serializedObject.FindProperty("duration");
	SerializedProperty durationBack => serializedObject.FindProperty("durationBack");
	SerializedProperty durationBackSameAsForward => serializedObject.FindProperty("durationBackSameAsForward");
	SerializedProperty startAnimation => serializedObject.FindProperty("startAnimation");
	SerializedProperty onCompletion => serializedObject.FindProperty("onCompletion");
	SerializedProperty onBackCompletion => serializedObject.FindProperty("onBackCompletion");
	
	System.Type editorCoroutines => System.Type.GetType("Unity.EditorCoroutines.Editor.EditorCoroutineUtility, Unity.EditorCoroutines.Editor");
	MethodInfo editorCoroutinesStart => editorCoroutines.GetMethod("StartCoroutine");
	MethodInfo editorCoroutinesStop => editorCoroutines.GetMethod("StopCoroutine");
	
	void forEachTarget(System.Action<AnimationBehaviour> callback) {
		for (var i = 0; i < targets.Length; i++) {
			callback((AnimationBehaviour)targets[i]);
		}
	}
	
	protected virtual void drawChildInspectorGUI() {
		DrawPropertiesExcluding(serializedObject, excludedProperties);
		GUI.enabled = true;
	}
	
	protected string[] excludedProperties = new string[] { "m_Script", "duration", "durationBack", "ignoreTimeScale", "startAnimation", "curve", "onCompletion", "backCurve", "onBackCompletion", "beginFromCurrent" };
	
	protected void drawPropertiesExcludingDefaultHidden() {
		DrawPropertiesExcluding(serializedObject, excludedProperties);
		GUI.enabled = true;
	}
	
	protected void drawPropertiesExcludingDefaultHiddenAnd(params SerializedProperty[] properties) {
		var allProperties = new string[excludedProperties.Length + properties.Length];
		excludedProperties.CopyTo(allProperties, 0);
		properties.Select(p => p.name).ToArray().CopyTo(allProperties, excludedProperties.Length);
		DrawPropertiesExcluding(serializedObject, allProperties);
	}
	
	protected void drawPropertiesExcludingDefaultHiddenAnd(params string[] properties) {
		var allProperties = new string[excludedProperties.Length + properties.Length];
		excludedProperties.CopyTo(allProperties, 0);
		properties.CopyTo(allProperties, excludedProperties.Length);
		DrawPropertiesExcluding(serializedObject, allProperties);
	}
	
	public override void OnInspectorGUI() {
		EditorGUILayout.PropertyField(script);
		EditorGUILayout.PropertyField(ignoreTimeScale);
		EditorGUILayout.PropertyField(beginFromCurrent);
		
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
		showPreview = EditorGUILayout.Foldout(showPreview, "Preview");
		if (showPreview) { showTestEditor(); }
	}
	
	void showForwardAnimationEditor() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		EditorGUILayout.BeginVertical();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("startAnimation"));
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(curve);
		showCurvePresetDropdown(setForwardCurve, width: 55);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(duration);
		if (selectedObject.durationBackSameAsForward) { durationBack.floatValue = duration.floatValue; }
		EditorGUILayout.PropertyField(onCompletion);
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
	
	void showBackwardAnimationEditor() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		EditorGUILayout.BeginVertical();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(backCurve);
		showCurvePresetDropdown(setBackwardCurve, width: 55);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		if (selectedObject.durationBackSameAsForward) {
			GUI.enabled = false;
			EditorGUILayout.PropertyField(durationBack);
			GUI.enabled = true;
		} else {
			EditorGUILayout.PropertyField(durationBack);
		}
		
		durationBackSameAsForward.boolValue = !EditorGUILayout.Toggle(!durationBackSameAsForward.boolValue, GUILayout.Width(14));
		EditorGUILayout.LabelField("Custom", GUILayout.Width(45));
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(onBackCompletion);
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
	
	void showTestEditor() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		EditorGUILayout.BeginVertical();
		
		var backwardsStyle = new GUIStyle(EditorStyles.toggle);
		backwardsStyle.fixedWidth = 80;
		
		EditorGUILayout.BeginHorizontal();
		if (animationRoutine != null && selectedObject.startTime.HasValue) {
			GUILayout.HorizontalSlider(selectedObject.currentProgress, 0, 1);
			GUILayout.Toggle(selectedObject.playBack, "Backwards", backwardsStyle);
		} else {
			var newSliderValue = GUILayout.HorizontalSlider(sliderValue, 0, 1);
			var newBackwards = GUILayout.Toggle(backwards, "Backwards", backwardsStyle);
			
			if (!Mathf.Approximately(newSliderValue, sliderValue) || backwards != newBackwards) {
				backwards = newBackwards;
				sliderValue = newSliderValue;
				
				forEachTarget((target) => target.invokeProgress(sliderValue, backwards: backwards));
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space(1);
		
		var shouldStopAnimation = false;
		
		if (animationRoutine != null) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Play animation");
			if (GUILayout.Button("Stop")) { shouldStopAnimation = true; }
			EditorGUILayout.EndHorizontal();
		} else {
			if (!Application.isPlaying && editorCoroutines == null) {
				EditorGUILayout.HelpBox("Install the editor coroutines to preview animations", MessageType.Warning);
				GUI.enabled = false;
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Play animation");
			if (GUILayout.Button("Forwards")) { tryPlayAnimation(backwards: false); }
			if (GUILayout.Button("Backwards")) { tryPlayAnimation(backwards: true); }
			EditorGUILayout.EndHorizontal();
			
			GUI.enabled = true;
		}
		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		
		// Must be called outside of begin/end UI calls because of Undo stuff
		if (shouldStopAnimation) { stopAnimation(); }
	}
	
	void tryPlayAnimation(bool backwards) {
		if (Application.isPlaying) {
			selectedObject.beginAnimation(backwards);
		} else {
			if (animationRoutine != null) { stopAnimation(); }
			if (editorCoroutines == null) { Debug.LogError("Install the Editor Coroutines to preview animations in the editor"); return; }
			
			Undo.RegisterFullObjectHierarchyUndo(selectedObject.gameObject, "Play animation");
			animationRoutine = editorCoroutinesStart.Invoke(null, new object[] { playAnimationRoutine(selectedObject, backwards), this });
		}
	}
	
	void stopAnimation() {
		if (animationRoutine == null) { return; }
		if (editorCoroutines == null) { Debug.LogError("Install the Editor Coroutines to preview animations in the editor"); return; }
		editorCoroutinesStop.Invoke(null, new object[] { animationRoutine });
		animationRoutine = null;
		
		if (Undo.GetCurrentGroupName() == "Play animation") { Undo.PerformUndo(); }
	}
	
	IEnumerator playAnimationRoutine(AnimationBehaviour animation, bool backwards) {
		yield return animation.animationRoutine(backwards);
		animationRoutine = null;
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
		menu.AddItem(new GUIContent("Bounce"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.8f, 1, 0.95f, -0.85f, 0.33f, 0.5f), new Keyframe(1, 1)));
		menu.AddItem(new GUIContent("Turn back again"), false, function, new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0)));
		menu.ShowAsContext();
	}
	
	void setForwardCurve(object newCurve) {
		curve.animationCurveValue = (AnimationCurve)newCurve;
		serializedObject.ApplyModifiedProperties();
	}
	
	void setBackwardCurve(object newCurve) {
		backCurve.animationCurveValue = (AnimationCurve)newCurve;
		serializedObject.ApplyModifiedProperties();
	}
	
	void OnDisable() {
		stopAnimation();
	}
	
	void OnDestroy() {
		stopAnimation();
	}

	public override bool RequiresConstantRepaint() => animationRoutine != null;
}
#endif

public abstract class AnimationBehaviour : MonoBehaviour, IgnoreTimeScale {
	/// The animation curve to use for forwards animations.
	public AnimationCurve curve;
	
	/// The animation curve to use for backwards animations.
	public AnimationCurve backCurve;
	
	/// Wether to always keeo the forward and backwards animation the same duration.
	/// If true, backwards animations will use the `duration` property instead of the `durationBack` property.
	[HideInInspector] public bool durationBackSameAsForward = true;
	
	/// Duration in seconds of the forward animiation.
	public float duration = 0.3f;
	
	/// Duration in seconds of the backwards animation.
	public float durationBack = 0.3f;
	
	public bool ignoreTimeScale = false;
	
	/// Wether to begin an animation from the current state of the object, or to begin from the selected start value.
	public bool beginFromCurrent = false;
	
	public enum AnimationStart { no, onStart, onEnable };
	
	/// When to automatically start a forwards animation.
	public AnimationStart startAnimation;
	
	public enum CompletionAction { none, disable, destroy, loop, playReverse };
	
	/// Which action to perform when a forwards animation completes playing.
	public CompletionAction onCompletion;
	
	/// Which action to perform when a backwards animation completes playing.
	public CompletionAction onBackCompletion;
	
	/// Wether the current animation (or, if no animation is playing, the most recently played animatino) is playing forwards or backwards.
	public bool playBack { get; private set; }
	
	/// Wether or not the animation is currently playing.
	public bool isPlaying => startTime.HasValue;
	
	/// The start time of the animiation, or null if no animation is playing.
	public float? startTime;
	
	/// Wether the current animation is paused or not.
	public bool paused { get; set; } = false;
	
	public enum State { begin, progress, end }
	
	/// Override for a callback when the animation begins.
	/// Usually you want this if you want to support the beginFromCurrent property, which you can implement like this:
	/// `from = (!playBack && beginFromCurrent ? currentValue : fromValue);`
	/// `to = (playBack && beginFromCurrent ? currentValue : toValue);`
	protected virtual void prepareAnimation() {}
	
	/// Called every frame of the animation. Override to apply the animation.
	/// Simple example implementation: `currentPosition = Vector3.LerpUnclamped(from, to, progress);`
	/// If you're using lerps in your implementation, you want to use unclamped lerps if you want to support bouncing animations.
	/// progress: The time (0 to 1) into the animation.
	/// state: Wether the frame is the start frame, a middle frame or the end frame of the animation.
	protected abstract void updateAnimation(float progress, State state);
	
	public event System.Action onBegin;
	public event System.Action onEnd;
	public event System.Action<float> onProgress;
	
	protected virtual void Reset() {
		curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		backCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	}
	
	protected virtual void Start() {
		if (!startTime.HasValue) {
			if (startAnimation == AnimationStart.onStart) { beginAnimation(); }
		}
	}
	
	protected virtual void OnEnable() {
		if (!startTime.HasValue) {
			if (startAnimation == AnimationStart.onEnable) { beginAnimation(); }
		}
	}
	
	protected virtual void OnDisable() {
		if (startTime.HasValue) {
			endAnimation();
		}
	}
	
	/// Play the animation.
	/// backwards: wether to play the forwards or backwards animation.
	public void beginAnimation(bool backwards) {
		#if UNITY_EDITOR && EditorCoroutine
		if (!Application.isPlaying) {
			EditorCoroutineUtility.StartCoroutine(animationRoutine(backwards), this);
			return;
		}
		#endif
		
		startTime = currentTime();
		playBack = backwards;
		prepareAnimation();
		updateAnimation(progressAtTime(0), State.begin);
		stepAnimation(ref startTime);
		
		onBegin?.Invoke();
	}
	
	/// Play the backwards animation.
	public void beginAnimationBack() => beginAnimation(backwards: true);
	
	/// Play the forwards animation.
	public void beginAnimation() => beginAnimation(backwards: false);
	
	/// Play the animation as a coroutine.
	/// backwards: wether to play the forwards or backwards animation.
	public IEnumerator animationRoutine(bool backwards = false) {
		playBack = backwards;
		prepareAnimation();
		updateAnimation(progressAtTime(0), State.begin);
		
		startTime = currentTime();
		stepAnimation(ref startTime);
		
		while (startTime != null) {
			yield return new WaitForEndOfFrame();
			
			if (paused) { startTime += deltaTime(); }
			else { stepAnimation(ref startTime); }
		}
	}
	
	public virtual void Update() {
		#if UNITY_EDITOR
		if (!Application.isPlaying) { return; }
		#endif
		
		if (paused) { startTime += deltaTime(); }
		else { stepAnimation(ref startTime); }
	}
	
	/// The current progress (0 to 1) through the animation.
	/// Will be 0 if it's not currently playing an animation.
	public float currentProgress => startTime == null ? 0 : Mathf.InverseLerp(startTime.Value, startTime.Value + duration, currentTime());
	
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
			endAnimation();
			return;
		}
		
		updateAnimation(time);
	}
	
	void endAnimation() {
		startTime = null;
		updateAnimation(progress: progressAtTime(1), State.end);
		onEnd?.Invoke();
		
		switch (playBack ? onBackCompletion : onCompletion) {
			case CompletionAction.none: break;
			case CompletionAction.disable: gameObject.SetActive(false); break;
			case CompletionAction.destroy: Object.Destroy(gameObject); break;
			case CompletionAction.loop: beginAnimation(playBack); break;
			case CompletionAction.playReverse: beginAnimation(!playBack); break;
		}
	}
	
	void updateAnimation(float time) {
		var progress = progressAtTime(time);
		updateAnimation(progress, State.progress);
		onProgress?.Invoke(progress);
	}
	
	float progressAtTime(float time) {
		if (playBack) {
			return 1 - backCurve.Evaluate(time);
		} else {
			return curve.Evaluate(time);
		}
	}
	
	/// Invoke the animation at a specific time. This will not play any animation, only put it in the specified state.
	/// Useful for example when you want to jump to a specific frame of an animation, like calling invokeProgress(0) to jump to the first frame of the animation
	/// or invokeProgress(1, AnimationBehaviour.State.end) to jump to the finished state of the animation.
	/// Can also be used to custom play the animation in a coroutine, like: for (var i = 0f; i <= 1; i += Time.deltaTime) { invokeProgress(i); }
	/// progress: The time (0 to 1) into the animation to invoke at.
	/// state: Wether the invokation is the beginning, middle or end of the animation (default beginning).
	/// backwards: Wether to invoke the forward to backwards animation.
	public void invokeProgress(float progress, State state = State.begin, bool backwards = false) {
		playBack = backwards;
		prepareAnimation();
		updateAnimation(progressAtTime(progress), state);
	}
	
	public void setIgnoreTimeScale(bool ignore) {
		ignoreTimeScale = ignore;
	}
	
	[ContextMenu("Print current curve")]
	void printCurve() {
		for (var i = 0; i < curve.keys.Length; i++) {
			Debug.Log($"[{i}] time: {curve.keys[i].time}, value: {curve.keys[i].value}, inTangent: {curve.keys[i].inTangent}, outTangent: {curve.keys[i].outTangent}, inWeight: {curve.keys[i].inWeight}, outWeight: {curve.keys[i].outWeight}");
		}
	}
	
	/// Accessor for `isObjectActive()` and `setObjectActive`. Will always animate.
	public bool objectActive {
		get => isObjectActive();
		set => setObjectActive(value);
	}
	
	/// Is the gameobject active, and not in progress of an animation that will disable it on end.
	/// Used as an animated alternative to gameObject.activeSelf.
	public bool isObjectActive() => gameObject.activeSelf && !(isPlaying && playBack && onBackCompletion == CompletionAction.disable);
	
	/// Used as an animated alternative to gameObject.SetActive().
	/// If active is true, it will enable animation-on-OnEnable, and activate the gameobject.
	/// If active if false, it will enable disable-on-animation-back-completion, and start the back animation.
	/// If animated is false, it works just like gameObject.SetActive().
	/// If the correct animation is already in progress when this method is called, it will do nothing.
	public void setObjectActive(bool active, bool animated = true) {
		if (active) {
			startAnimation = AnimationStart.onEnable;
			
			if (!gameObject.activeSelf || (isPlaying && playBack)) {
				gameObject.SetActive(true);
				
				if (animated) {
					beginAnimation();
				} else {
					invokeProgress(1, State.end);
				}
			}
		} else {
			onBackCompletion = CompletionAction.disable;
			
			if (!(isPlaying && playBack)) {
				if (animated) {
					beginAnimationBack();
				} else {
					invokeProgress(1, State.end, backwards: true);
				}
			}
		}
	}
}
