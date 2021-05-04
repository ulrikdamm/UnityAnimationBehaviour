using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ShaderKeywordAnimation))]
public class ShaderKeywordAnimationEditor : AnimationBehaviourEditor {
	ShaderKeywordAnimation obj => (ShaderKeywordAnimation)target;
	SerializedProperty materialIndex => serializedObject.FindProperty("materialIndex");
	SerializedProperty keyword => serializedObject.FindProperty("keyword");
	
	SerializedProperty animateFloat => serializedObject.FindProperty("animateFloat");
	SerializedProperty floatFromValue => serializedObject.FindProperty("floatFromValue");
	SerializedProperty floatToValue => serializedObject.FindProperty("floatToValue");
	
	SerializedProperty animateColor => serializedObject.FindProperty("animateColor");
	SerializedProperty colorFromValue => serializedObject.FindProperty("colorFromValue");
	SerializedProperty colorToValue => serializedObject.FindProperty("colorToValue");
	
	SerializedProperty animateVector => serializedObject.FindProperty("animateVector");
	SerializedProperty vectorFromValue => serializedObject.FindProperty("vectorFromValue");
	SerializedProperty vectorToValue => serializedObject.FindProperty("vectorToValue");
	
	protected override void drawChildInspectorGUI() {
		if (obj.TryGetComponent<Graphic>(out var graphic)) {
			
		} else if (obj.TryGetComponent<MeshRenderer>(out var meshRenderer)) {
			drawMeshMaterials(meshRenderer);
		} else {
			EditorGUILayout.HelpBox("Add a rendering component to animate it", MessageType.Info);
			obj.instantiatedMaterial = null;
		}
		
		GUILayout.Label("Original material: " + (obj._originalMaterial == null ? "null" : obj._originalMaterial.name));
		GUILayout.Label("Instanciated material: " + (obj._instantiatedMaterial == null ? "null" : obj._instantiatedMaterial.name));
		// drawMaterialSection();
		drawKeywordSection();
	}
	
	void drawKeywordSection() {
		if (obj._originalMaterial == null) { return; }
		var shader = obj._originalMaterial.shader;
		
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Keyword", new GUIStyle(EditorStyles.boldLabel));
		
		var propertyCount = ShaderUtil.GetPropertyCount(shader);

		if (propertyCount == 0) {
			EditorGUILayout.HelpBox("This material has no shader properties defined", MessageType.Warning);
			return;
		}
		
		var propertyNames = new string[propertyCount + 1];
		
		var selectedIndex = -1;
		for (var i = 0; i < propertyCount; i++) {
			propertyNames[i] = ShaderUtil.GetPropertyName(shader, i);
			if (propertyNames[i] == keyword.stringValue) { selectedIndex = i; }
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Material keyword");
		var newSelectionIndex = EditorGUILayout.Popup(selectedIndex == -1 ? 0 : selectedIndex, propertyNames);
		EditorGUILayout.EndHorizontal();
		
		if (newSelectionIndex != selectedIndex) {
			selectedIndex = newSelectionIndex;
			keyword.stringValue = propertyNames[newSelectionIndex];
		}
		
		drawKeywordValueSection(selectedIndex);
	}
	
	void drawKeywordValueSection(int propertyIndex) {
		var shader = obj._originalMaterial.shader;
		var propertyType = ShaderUtil.GetPropertyType(shader, propertyIndex);
		
		switch (propertyType) {
			case ShaderUtil.ShaderPropertyType.Float:
				setAnimationTypes(animateFloat: true);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Float");
				EditorGUILayout.PropertyField(floatFromValue, label: GUIContent.none);
				EditorGUILayout.PropertyField(floatToValue, label: GUIContent.none);
				EditorGUILayout.EndHorizontal();
				break;
			case ShaderUtil.ShaderPropertyType.Range:
				setAnimationTypes(animateFloat: true);
				var min = ShaderUtil.GetRangeLimits(shader, propertyIndex, 1);
				var max = ShaderUtil.GetRangeLimits(shader, propertyIndex, 2);
				drawRangeProperty("From value", floatFromValue, min, max);
				drawRangeProperty("To value", floatToValue, min, max);
				break;
			case ShaderUtil.ShaderPropertyType.Color:
				setAnimationTypes(animateColor: true);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Color");
				EditorGUILayout.PropertyField(colorFromValue, label: GUIContent.none);
				EditorGUILayout.PropertyField(colorToValue, label: GUIContent.none);
				EditorGUILayout.EndHorizontal();
				break;
			case ShaderUtil.ShaderPropertyType.Vector:
				setAnimationTypes(animateVector: true);
				drawVectorProperty("From value", vectorFromValue);
				drawVectorProperty("To value", vectorToValue);
				break;
			default:
				EditorGUILayout.HelpBox("This property type can't be animated", MessageType.Warning);
				break;
		}
	}
	
	void setAnimationTypes(bool animateFloat = false, bool animateColor = false, bool animateVector = false) {
		this.animateFloat.boolValue = animateFloat;
		this.animateColor.boolValue = animateColor;
		this.animateVector.boolValue = animateVector;
	}
	
	void drawRangeProperty(string prefix, SerializedProperty property, float min, float max) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(prefix);
		property.floatValue = EditorGUILayout.Slider(property.floatValue, min, max);
		EditorGUILayout.EndHorizontal();
	}
	
	void drawVectorProperty(string prefix, SerializedProperty property) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(prefix);
		var fromVector = property.vector4Value;
		var x = EditorGUILayout.FloatField(fromVector.x);
		var y = EditorGUILayout.FloatField(fromVector.y);
		var z = EditorGUILayout.FloatField(fromVector.z);
		var w = EditorGUILayout.FloatField(fromVector.w);
		property.vector4Value = new Vector4(x, y, z, w);
		EditorGUILayout.EndHorizontal();
	}
	
	// void drawMaterialSection() {
	// 	var meshRendererProp = serializedObject.FindProperty("meshRenderer");
	// 	var imageProp = serializedObject.FindProperty("image");
		
	// 	GUI.enabled = (imageProp.objectReferenceValue == null);
	// 	EditorGUILayout.PropertyField(meshRendererProp);
		
	// 	GUI.enabled = (meshRendererProp.objectReferenceValue == null);
	// 	EditorGUILayout.PropertyField(imageProp);
		
	// 	GUI.enabled = true;
		
	// 	drawMeshMaterials();
	// 	drawImageMaterials();
		
	// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("instantiateMaterial"));
	// }
	
	void drawMeshMaterials(MeshRenderer meshRenderer) {
		// var meshRendererProp = serializedObject.FindProperty("meshRenderer");
		
		// var meshRenderer = (MeshRenderer)meshRendererProp.objectReferenceValue;
		// if (meshRenderer == null) { return; }
		
		var selectedIndex = materialIndex.intValue;
		
		var materials = new string[meshRenderer.sharedMaterials.Length];
		for (var i = 0; i < meshRenderer.sharedMaterials.Length; i++) {
			materials[i] = $"[{i}] {(meshRenderer.sharedMaterials[i] != null ? meshRenderer.sharedMaterials[i].name : "null")}";
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Material");
		selectedIndex = EditorGUILayout.Popup(selectedIndex, materials);
		EditorGUILayout.EndHorizontal();
		
		materialIndex.intValue = selectedIndex;
		
		// if (meshRenderer.sharedMaterials[selectedIndex] != null) {
		// 	obj.originalMaterial = meshRenderer.sharedMaterials[selectedIndex];
		// }
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("instantiateMaterial"));
	}
	
	// void drawImageMaterials() {
	// 	var imageProp = serializedObject.FindProperty("image");
	// 	var image = (Image)imageProp.objectReferenceValue;
	// 	if (image == null) { return; }
		
	// 	EditorGUILayout.BeginHorizontal();
	// 	EditorGUILayout.PrefixLabel("Material");
	// 	EditorGUILayout.Popup(0, new string[] { image.material.name });
	// 	EditorGUILayout.EndHorizontal();
	// }
}
#endif

[ExecuteInEditMode]
public class ShaderKeywordAnimation : AnimationBehaviour, IMaterialModifier {
	[Header("Renderer")]
	public MeshRenderer meshRenderer;
	public Image image;
	public int materialIndex;
	public bool instantiateMaterial = true;
	
	[Header("Keyword")]
	public string keyword = "_Progress";
	
	public bool animateFloat = false;
	public float floatFromValue = 0;
	public float floatToValue = 1;
	
	public bool animateColor = false;
	[ColorUsage(showAlpha: true, hdr: true)] public Color colorFromValue = Color.white;
	[ColorUsage(showAlpha: true, hdr: true)] public Color colorToValue = Color.black;
	
	public bool animateVector = false;
	public Vector4 vectorFromValue = Vector4.zero;
	public Vector4 vectorToValue = Vector4.one;
	
	public Material _originalMaterial;
	public Material originalMaterial {
		get {
			if (_originalMaterial == null) {
				if (TryGetComponent<Graphic>(out var graphic)) {
					_originalMaterial = graphic.material;
				} else if (TryGetComponent<MeshRenderer>(out var meshRenderer)) {
					_originalMaterial = meshRenderer.sharedMaterials[materialIndex];
				} else {
					return null;
				}
			}
			
			return _originalMaterial;
		}
	}
	
	[ContextMenu("Clear material")] void clearMaterial() => instantiatedMaterial = null;
	[ContextMenu("Make material")] void makeMaterial() => instantiatedMaterial = originalMaterial;
	
	public Material _instantiatedMaterial;
	public Material instantiatedMaterial {
		get => _instantiatedMaterial;
		set {
			if (_instantiatedMaterial == value) { return; }
			
			if (_instantiatedMaterial != null) {
				if (Application.isPlaying) {
					Object.Destroy(_instantiatedMaterial);
				} else {
					Object.DestroyImmediate(_instantiatedMaterial);
				}
			}
			
			_instantiatedMaterial = null;
			
			if (value != null) {
				_instantiatedMaterial = Object.Instantiate(value);
				_instantiatedMaterial.name = value.name + "(ShaderKeywordAnimation)";
			}
			
			if (TryGetComponent<Graphic>(out var graphic)) {
				graphic.SetMaterialDirty();
			} else if (TryGetComponent<MeshRenderer>(out var meshRenderer)) {
				var materials = meshRenderer.sharedMaterials;
				materials[materialIndex] = (_instantiatedMaterial != null ? _instantiatedMaterial : originalMaterial);
				meshRenderer.sharedMaterials = materials;
			}
		}
	}
	
	float floatFrom, floatTo;
	Color colorFrom, colorTo;
	Vector4 vectorFrom, vectorTo;
	
	float currentFloat {
		get => instantiatedMaterial.GetFloat(keyword);
		set => instantiatedMaterial.SetFloat(keyword, value);
	}
	
	Color currentColor {
		get => instantiatedMaterial.GetColor(keyword);
		set => instantiatedMaterial.SetColor(keyword, value);
	}
	
	Vector4 currentVector {
		get => instantiatedMaterial.GetVector(keyword);
		set => instantiatedMaterial.SetVector(keyword, value);
	}
	
	protected override void prepareAnimation() {
		if (originalMaterial == null) { Debug.LogError("Running ShaderKeywordAnimation, but there's nothing to animate", this); return; }
		if (instantiatedMaterial == null) { instantiatedMaterial = originalMaterial; }
		
		if (animateFloat) {
			floatFrom = (!playBack && beginFromCurrent ? currentFloat : floatFromValue);
			floatTo = (playBack && beginFromCurrent ? currentFloat : floatToValue);
		}
		
		if (animateColor) {
			colorFrom = (!playBack && beginFromCurrent ? currentColor : colorFromValue);
			colorTo = (playBack && beginFromCurrent ? currentColor : colorToValue);
		}
		
		if (animateVector) {
			vectorFrom = (!playBack && beginFromCurrent ? currentVector : vectorFromValue);
			vectorTo = (playBack && beginFromCurrent ? currentVector : vectorToValue);
		}
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
		if (originalMaterial == null) { Debug.LogError("Running ShaderKeywordAnimation, but there's nothing to animate", this); return; }
		if (instantiatedMaterial == null) { instantiatedMaterial = originalMaterial; }
		
		if (animateFloat) { instantiatedMaterial.SetFloat(keyword, Mathf.LerpUnclamped(floatFrom, floatTo, progress)); }
		if (animateColor) { instantiatedMaterial.SetColor(keyword, Color.LerpUnclamped(colorFrom, colorTo, progress)); }
		if (animateVector) { instantiatedMaterial.SetVector(keyword, Vector4.LerpUnclamped(vectorFrom, vectorTo, progress)); }
	}
	
	void OnValidate() {
		if (meshRenderer == null || meshRenderer.gameObject != gameObject) { meshRenderer = GetComponent<MeshRenderer>(); }
	}
	
	Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial) {
		// Debug.Log($"GetModifiedMaterial: {baseMaterial.name}");
		
		_originalMaterial = baseMaterial;
		if (instantiatedMaterial != null) { return instantiatedMaterial; }
		return originalMaterial;
		
		// if (baseMaterial == originalMaterial) { return instantiatedMaterial; }
		// originalMaterial = baseMaterial;
		// instantiatedMaterial = baseMaterial;
		// instantiatedMaterial.name = baseMaterial.name + " (+ShaderKeywordAnimation)";
		// // Debug.Log($"Instantiated to: {material.name}");
		// return instantiatedMaterial;
	}
}
