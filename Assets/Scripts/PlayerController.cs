using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour {
	public GameObject gunPrefab;
	public Transform[] hands;

    List<GunController> guns = new List<GunController>();

	// Use this for initialization
	void Start () {
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

    void StartSwallow() {
        guns[0].Detach();
        Destroy(guns[0].gameObject);
        guns[1].Detach();
        Destroy(guns[1].gameObject);
		GetComponent<Rigidbody>().isKinematic = true;
        Destroy(GetComponent<Collider>());

        FindObjectOfType<WormController>().StartSwallowSequence();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("WormMouth")) {
            StartSwallow();
        }
    }
}
