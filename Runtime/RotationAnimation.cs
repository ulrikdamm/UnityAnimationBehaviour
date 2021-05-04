using UnityEngine;

public class RotationAnimation : AnimationBehaviour {
    public Space space;
    public Vector3 fromRotation;
    public Vector3 toRotation;
    
    Quaternion from, to;
	
	Transform _transform;
	Transform cachedTransform => _transform != null ? _transform : _transform = transform;
    
	Quaternion currentRotation {
		get => space switch { Space.Self => cachedTransform.localRotation, Space.World => cachedTransform.rotation, _ => throw new System.Exception() };
		set {
			switch (space) {
				case Space.Self: cachedTransform.localRotation = value; break;
				case Space.World: cachedTransform.rotation = value; break;
			}
		}
	}
	
	protected override void prepareAnimation() {
		from = (!playBack && beginFromCurrent ? currentRotation : Quaternion.Euler(fromRotation));
		to = (playBack && beginFromCurrent ? currentRotation : Quaternion.Euler(toRotation));
	}
	
	protected override void updateAnimation(float progress, AnimationBehaviour.State state) {
        currentRotation = Quaternion.LerpUnclamped(from, to, progress);
	}
}
