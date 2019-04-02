using UnityEngine;

public class RotationAnimation : AnimationBehaviour {
    public Vector3 fromRotation;
    public Vector3 toRotation;
    
    public Space space;
    
    Quaternion fromQuart;
    Quaternion toQuart;
    
    protected override void onAnimationBegin() {
		fromQuart = Quaternion.Euler(fromRotation);
		toQuart = Quaternion.Euler(toRotation);
	}
	
	protected override void onAnimationDone() {
		var finalRotation = (playBack ? fromQuart : toQuart);
        
        switch (space) {
            case Space.Self: transform.localRotation = finalRotation; break;
            case Space.World: transform.rotation = finalRotation; break;
        }
	}
	
	protected override void onAnimationProgress(float progress) {
        var newRotation = Quaternion.LerpUnclamped(fromQuart, toQuart, progress);
		
        switch (space) {
            case Space.Self: transform.localRotation = newRotation; break;
            case Space.World: transform.rotation = newRotation; break;
        }
	}
}
