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
        if (rb.velocity.magnitude > 0.5f && other.gameObject.GetComponent<AstriodController>()) {
            GameObject newParticles = Instantiate(TerrainGenerator.instance.asteroidExplosion, transform.position, Quaternion.identity);
            newParticles.GetComponent<Rigidbody>().velocity = other.relativeVelocity;
            if (size > 0) { //0 is a fragment
                CreateDebris();
            }

            Destroy(gameObject);
        }
    }

    void CreateDebris() {
        int numDebris = size * debrisFactor;
        for (int i = 0; i < numDebris; i++) {
            Vector3 spawnOffset = Random.insideUnitSphere * size;

            GameObject newDebrisPiece = Instantiate(TerrainGenerator.instance.asteroidPrefab, transform.position + spawnOffset, Quaternion.identity);
            newDebrisPiece.GetComponent<Rigidbody>().velocity = spawnOffset * Random.Range(6f, 10f);
            newDebrisPiece.GetComponent<AstriodController>().Init(0);
        }
    }
}
