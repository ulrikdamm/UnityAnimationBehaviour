using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ShaderKeywordAnimation))]
[CanEditMultipleObjects]
public class ShaderKeywordAnimationEditor : AnimationBehaviourEditor {
	protected override void drawChildInspectorGUI() {
		// drawPropertiesExcludingDefaultHiddenAnd("materialIndex");
		
		drawMaterialSection();
		drawKeywordSection();
	}
	
	void drawKeywordSection() {
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Keyword", new GUIStyle(EditorStyles.boldLabel));
		
		var meshRenderer = (MeshRenderer)serializedObject.FindProperty("meshRenderer").objectReferenceValue;
		var image = (Image)serializedObject.FindProperty("image").objectReferenceValue;
		
		Shader shader;
		
		if (meshRenderer != null) {
			var materialIndex = serializedObject.FindProperty("materialIndex").intValue;
			if (materialIndex < 0 || materialIndex >= meshRenderer.sharedMaterials.Length) { return; }
			
			shader = meshRenderer.sharedMaterials[materialIndex].shader;
		} else if (image != null) {
			shader = image.material.shader;
		} else {
			return;
		}
		
		var propertyCount = ShaderUtil.GetPropertyCount(shader);

		if (propertyCount == 0) {
			EditorGUILayout.HelpBox("This material has no shader properties defined", MessageType.Warning);
			return;
		}
		
		var propertyNames = new string[propertyCount];
		var selectedPropertyName = serializedObject.FindProperty("keyword").stringValue;
		
		var selectedIndex = -1;
		for (var i = 0; i < propertyCount; i++) {
			propertyNames[i] = ShaderUtil.GetPropertyName(shader, i);
			if (propertyNames[i] == selectedPropertyName) { selectedIndex = i; }
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Material keyword");
		var newSelectionIndex = EditorGUILayout.Popup(selectedIndex == -1 ? 0 : selectedIndex, propertyNames);
		EditorGUILayout.EndHorizontal();
		
		if (newSelectionIndex != selectedIndex) {
			selectedIndex = newSelectionIndex;
			serializedObject.FindProperty("keyword").stringValue = propertyNames[newSelectionIndex];
		}
		
		var propertyType = ShaderUtil.GetPropertyType(shader, selectedIndex);
		switch (propertyType) {
			case ShaderUtil.ShaderPropertyType.Float:
				setAnimationTypes(animateFloat: true);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("floatFromValue"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("floatToValue"));
				break;
			case ShaderUtil.ShaderPropertyType.Range:
				setAnimationTypes(animateFloat: true);
				var min = ShaderUtil.GetRangeLimits(shader, selectedIndex, 1);
				var max = ShaderUtil.GetRangeLimits(shader, selectedIndex, 2);
				drawRangeProperty("From value", "floatFromValue", min, max);
				drawRangeProperty("To value", "floatToValue", min, max);
				break;
			case ShaderUtil.ShaderPropertyType.Color:
				setAnimationTypes(animateColor: true);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("colorFromValue"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("colorToValue"));
				break;
			case ShaderUtil.ShaderPropertyType.Vector:
				setAnimationTypes(animateVector: true);
				drawVectorProperty("From value", "vectorFromValue");
				drawVectorProperty("To value", "vectorToValue");
				break;
			default:
				EditorGUILayout.HelpBox("This property type can't be animated", MessageType.Warning);
				break;
		}
	}
	
	void setAnimationTypes(bool animateFloat = false, bool animateColor = false, bool animateVector = false) {
		serializedObject.FindProperty("animateFloat").boolValue = animateFloat;
		serializedObject.FindProperty("animateColor").boolValue = animateColor;
		serializedObject.FindProperty("animateVector").boolValue = animateVector;
	}
	
	void drawRangeProperty(string prefix, string propertyName, float min, float max) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(prefix);
		serializedObject.FindProperty(propertyName).floatValue = EditorGUILayout.Slider(serializedObject.FindProperty(propertyName).floatValue, min, max);
		EditorGUILayout.EndHorizontal();
	}
	
	void drawVectorProperty(string prefix, string propertyName) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(prefix);
		var fromVector = serializedObject.FindProperty(propertyName).vector4Value;
		var x = EditorGUILayout.FloatField(fromVector.x);
		var y = EditorGUILayout.FloatField(fromVector.y);
		var z = EditorGUILayout.FloatField(fromVector.z);
		var w = EditorGUILayout.FloatField(fromVector.w);
		serializedObject.FindProperty(propertyName).vector4Value = new Vector4(x, y, z, w);
		EditorGUILayout.EndHorizontal();
	}
	
	void drawMaterialSection() {
		var meshRendererProp = serializedObject.FindProperty("meshRenderer");
		var imageProp = serializedObject.FindProperty("image");
		var materialIndexProp = serializedObject.FindProperty("materialIndex");
		
		GUI.enabled = (imageProp.objectReferenceValue == null);
		EditorGUILayout.PropertyField(meshRendererProp);
		
		GUI.enabled = (meshRendererProp.objectReferenceValue == null);
		EditorGUILayout.PropertyField(imageProp);
		
		GUI.enabled = true;
		
		drawMeshMaterials();
		drawImageMaterials();
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("instantiateMaterial"));
	}
	
	void drawMeshMaterials() {
		var materialIndexProp = serializedObject.FindProperty("materialIndex");
		var meshRendererProp = serializedObject.FindProperty("meshRenderer");
		
		var meshRenderer = (MeshRenderer)meshRendererProp.objectReferenceValue;
		if (meshRenderer == null) { return; }
		
		var selectedIndex = materialIndexProp.intValue;
		
		var materials = new string[meshRenderer.sharedMaterials.Length];
		for (var i = 0; i < meshRenderer.sharedMaterials.Length; i++) {
			materials[i] = meshRenderer.sharedMaterials[i].name;
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Material");
		selectedIndex = EditorGUILayout.Popup(selectedIndex, materials);
		EditorGUILayout.EndHorizontal();
		
		materialIndexProp.intValue = selectedIndex;
	}
	
	void drawImageMaterials() {
		var imageProp = serializedObject.FindProperty("image");
		var image = (Image)imageProp.objectReferenceValue;
		if (image == null) { return; }
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Material");
		EditorGUILayout.Popup(0, new string[] { image.material.name });
		EditorGUILayout.EndHorizontal();
	}
}
#endif

public class ShaderKeywordAnimation : AnimationBehaviour {
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
	public Color colorFromValue = Color.white;
	public Color colorToValue = Color.black;
	
	public bool animateVector = false;
	public Vector4 vectorFromValue = Vector4.zero;
	public Vector4 vectorToValue = Vector4.one;
	
	Material _material;
	public Material material {
		get {
			if (_material == null) {
				if (meshRenderer != null) {
					var materials = meshRenderer.sharedMaterials;
					
					if (instantiateMaterial) {
						_material = Object.Instantiate(materials[materialIndex]);
						materials[materialIndex] = _material;
						meshRenderer.sharedMaterials = materials;
					} else {
						_material = materials[materialIndex];
					}
				} else if (image != null) {
					_material = (instantiateMaterial ? Object.Instantiate(image.material) : image.material);
					image.material = _material;
				} else {
					throw new System.Exception("No MeshRenderer or Image assigned");
				}
			}
			
			return _material;
		}
	}
	
	protected override void onAnimationBegin() {
		
	}
	
	protected override void onAnimationDone() {
		if (animateFloat) { material.SetFloat(keyword, (playBack  ? floatFromValue : floatToValue)); }
		if (animateColor) { material.SetColor(keyword, (playBack ? colorFromValue : colorToValue)); }
		if (animateVector) { material.SetVector(keyword, playBack ? vectorFromValue : vectorToValue); }
	}
	
	protected override void onAnimationProgress(float progress) {
		if (animateFloat) { material.SetFloat(keyword, Mathf.LerpUnclamped(floatFromValue, floatToValue, progress)); }
		if (animateColor) { material.SetColor(keyword, Color.LerpUnclamped(colorFromValue, colorToValue, progress)); }
		if (animateVector) { material.SetVector(keyword, Vector4.LerpUnclamped(vectorFromValue, vectorToValue, progress)); }
	}
	
	void OnValidate() {
		if (meshRenderer == null || meshRenderer.gameObject != gameObject) { meshRenderer = GetComponent<MeshRenderer>(); }
	}
}
