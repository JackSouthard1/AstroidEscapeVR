using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabable : MonoBehaviour {
	Transform anchor = null;
	bool hasAnchor = false;
	public bool moving = false;
	public Vector3 lastPos;

	void LateUpdate () {
		if (moving && hasAnchor) {
			lastPos = anchor.position;
		}
	}

	public Vector3 anchorPos ()
	{
		return anchor.position;
	}

	public Vector3 anchorDiff ()
	{
		if (moving) {
			return anchor.position - lastPos;
		} else {
			return Vector3.zero;
		}
	}

	public void CreateAnchor (Vector3 anchorPos)
	{
		if (!hasAnchor) {
			anchor = Instantiate(new GameObject(), transform).transform;
			hasAnchor = true;
		}
		anchor.position = anchorPos;

		if (moving) {
			lastPos = anchor.position;
		}
	}

	public void DestoryAnchor ()
	{
		if (hasAnchor) {
			Destroy(anchor.gameObject);
			hasAnchor = false;
		}
	}
}
