using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour {
    enum State {
        Waiting,
        Chasing,
        Swallowing,
        Finished
    };
    State state;

	Transform player;
	public float startFollowDist;
	public float maxFollowDist;
	public float speed;
	public float speedIncreaseRate;

    public float spawnDistance; //how far does the player have to be away before the worm will appear?

    [Space(15)]
    public Transform swallowStartPos;
    public Transform swallowEndPos;
    public float swallowTime;

    Rigidbody rb;

	void Start () {
		player = FindObjectOfType<PlayerController>().transform;
        rb = GetComponent<Rigidbody>();
	}
	
	void Update () {
        switch (state) {
            case State.Chasing:
                float dist = (transform.position - player.position).magnitude;
                float curSpeed = speed;
                if (dist > maxFollowDist) {
                    curSpeed *= 10;
                }
                transform.LookAt(player);
                rb.velocity = transform.forward * curSpeed;

                speed += speedIncreaseRate * Time.deltaTime;
                break;
            case State.Waiting:
                if (player.transform.position.z < spawnDistance) {
                    return;
                }

                Activate();
                break;
            case State.Finished:
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Camera.main.transform.position = swallowEndPos.position;
                break;
        }
	}

    void Activate() {
        transform.position = player.transform.position - Vector3.forward * maxFollowDist + Vector3.down * 20; // start arbitrarily below the belt
        state = State.Chasing;
    }

    public void StartSwallowSequence() {
        state = State.Swallowing;
        StartCoroutine(SwallowSequence());
    }

    IEnumerator SwallowSequence() {

        float p = 0f;
        while (p < 1f) {
            Camera.main.transform.position = Vector3.Lerp(swallowStartPos.position, swallowEndPos.position, p);
            yield return new WaitForEndOfFrame();
            p += (Time.deltaTime / swallowTime);
        }

        OnSwallowComplete();
    }

    void OnSwallowComplete() {
        print("u ded");
        state = State.Finished;
    }
}
