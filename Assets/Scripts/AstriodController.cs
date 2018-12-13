using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstriodController : MonoBehaviour {
	public Mesh[] meshes;
	public float scaleVariance;

    Rigidbody rb;

	void Start () {
		transform.rotation = Random.rotation;
		transform.localScale = new Vector3(1 + Random.Range(-scaleVariance, scaleVariance), 1 + Random.Range(-scaleVariance, scaleVariance), 1 + Random.Range(-scaleVariance, scaleVariance));

		Mesh mesh = meshes[Random.Range(0, meshes.Length)];
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;

        rb = GetComponent<Rigidbody>();
	}

    //temporary behavior
    void OnCollisionEnter(Collision other) {
        if (rb.velocity.magnitude > 0.5f && other.gameObject.GetComponent<AstriodController>()) {
            GameObject newParticles = Instantiate(TerrainGenerator.instance.asteroidExplosion, transform.position, Quaternion.identity);
            newParticles.GetComponent<Rigidbody>().velocity = other.relativeVelocity;
        }
    }
}
