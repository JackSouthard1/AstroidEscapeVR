using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabable : MonoBehaviour {
	Transform anchor = null;
	bool hasAnchor = false;

	public Vector3 GetAnchorPos ()
	{
		return anchor.position;
	}

	public void CreateAnchor (Vector3 anchorPos)
	{
		if (!hasAnchor) {
			anchor = Instantiate(new GameObject(), transform).transform;
			hasAnchor = true;
		}
		anchor.position = anchorPos;
	}

	public void DestoryAnchor ()
	{
		if (hasAnchor) {
			Destroy(anchor.gameObject);
			hasAnchor = false;
		}
	}
}
