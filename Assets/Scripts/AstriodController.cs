using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstriodController : MonoBehaviour {
	public float scaleVariance;
    public int debrisFactor;
    int size;

    Rigidbody rb;

    public void Init(int newSize) {
        size = newSize;

        transform.rotation = Random.rotation;
        transform.localScale = new Vector3(1 + Random.Range(-scaleVariance, scaleVariance), 1 + Random.Range(-scaleVariance, scaleVariance), 1 + Random.Range(-scaleVariance, scaleVariance));

        Mesh mesh = TerrainGenerator.instance.GetMeshOfSize(size);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        rb = GetComponent<Rigidbody>();
    }

    //temporary behavior
    void OnCollisionEnter(Collision other) {
		if (size > 0) {
			AstriodController otherAstroid = other.gameObject.GetComponent<AstriodController>();
			if (other.gameObject.CompareTag("Deadly") || (otherAstroid != null && otherAstroid.size > 0 && other.relativeVelocity.magnitude > 20)) {
				GameObject newParticles = Instantiate(TerrainGenerator.instance.asteroidExplosion, transform.position, Quaternion.identity);
				newParticles.GetComponent<Rigidbody>().velocity = other.relativeVelocity;
				if (size > 0) { //0 is a fragment
					CreateDebris(other.relativeVelocity.magnitude);
				}

				Destroy(gameObject);
			}
        }
    }

    void CreateDebris(float collisionForce) {
        int numDebris = size * debrisFactor;
        for (int i = 0; i < numDebris; i++) {
            Vector3 spawnOffset = Random.onUnitSphere * size;

            GameObject newDebrisPiece = Instantiate(TerrainGenerator.instance.asteroidPrefab, transform.position + spawnOffset * 1.5f, Quaternion.identity, transform.root);
            // newDebrisPiece.GetComponent<Rigidbody>().velocity = spawnOffset * Random.Range(0.00001f, 0.0001f) * collisionForce;
            newDebrisPiece.GetComponent<AstriodController>().Init(0);
        }
    }
}
