using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UIAlphaAnimation))]
[CanEditMultipleObjects]
public class UIAlphaAnimationEditor : AnimationBehaviourEditor {
	protected override void drawChildInspectorGUI() {
		var canvasGroup = serializedObject.FindProperty("canvasGroup");
		EditorGUILayout.PropertyField(canvasGroup);
		if (canvasGroup.objectReferenceValue == null && GUILayout.Button("Add Canvas Group")) {
			var newCanvasGroup = ((UIAlphaAnimation)target).gameObject.AddComponent<CanvasGroup>();
			canvasGroup.objectReferenceValue = newCanvasGroup;
			Undo.RegisterCreatedObjectUndo(newCanvasGroup, "Added canvas group");
		}
		
		drawPropertiesExcludingDefaultHiddenAnd("canvasGroup");
	}
}
#endif

public class UIAlphaAnimation : AnimationBehaviour {
	[SerializeField] CanvasGroup canvasGroup;
	
	public float currentValue => canvasGroup.alpha;
	
	public float fromValue;
	public float toValue;
	
	public void setVisible(bool visible, bool animated = true) {
		if (visible && animated) {
			gameObject.SetActive(true);
			beginAnimation();
		} else if (visible && !animated) {
			gameObject.SetActive(true);
			setImmediately(1);
		} else if (!visible && animated) {
			beginAnimationBack();
		} else {
			setImmediately(0);
			gameObject.SetActive(false);
		}
	}
	
	public void setImmediately(float value) {
		canvasGroup.alpha = value;
		canvasGroup.interactable = (canvasGroup.alpha > 0.001f);
		canvasGroup.blocksRaycasts = canvasGroup.interactable;
	}
	
	public void perform(float fromValue, float toValue) {
		this.fromValue = fromValue;
		this.toValue = toValue;
		beginAnimation();
	}
	
	public void performBack() {
		beginAnimationBack();
	}
	
	public IEnumerator performRoutine(float fromValue, float toValue) {
		this.fromValue = fromValue;
		this.toValue = toValue;
		yield return animationRoutine();
	}
	
	void OnValidate() {
		if (canvasGroup == null) { canvasGroup = GetComponent<CanvasGroup>(); }
	}
	
	protected override void onAnimationDone() {
		setImmediately(playBack ? fromValue : toValue);
	}
	
	protected override void onAnimationProgress(float progress) {
		setImmediately(Mathf.Lerp(fromValue, toValue, progress));
	}
}
