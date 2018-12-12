using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour {
	public GameObject gunPrefab;
	public Transform[] hands;

	// Use this for initialization
	void Start () {
		List<GunController> guns = new List<GunController>();
		for (int i = 0; i < hands.Length; i++) {
			GameObject gun = Instantiate(gunPrefab, hands[i]);
			gun.transform.localPosition = Vector3.zero;
			gun.transform.localRotation = Quaternion.identity;
			guns.Add(gun.GetComponent<GunController>());
		}

		guns[0].otherGun = guns[1];
		guns[1].otherGun = guns[0];
	}

	// Update is called once per frame
	void Update () {
		
	}
}
