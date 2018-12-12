using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour {
	Transform player;
	public float startFollowDist;
	public float maxFollowDist;
	public float speed;
	public float speedIncreaseRate;

	void Start () {
		player = FindObjectOfType<PlayerController>().transform;
	}
	
	void Update () {
		float dist = (transform.position - player.position).magnitude;
		float curSpeed = speed;
		if (dist > maxFollowDist) {
			curSpeed *= 10;
		}
		transform.LookAt(player);
		transform.position += transform.forward * curSpeed * Time.deltaTime;

		speed += speedIncreaseRate * Time.deltaTime;
	}
}
