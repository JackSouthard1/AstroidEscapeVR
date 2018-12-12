using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public float terrainStep;
    public float beltRadius;
    public float chunkSize;
    public int maxNumChunks;

    [Space(15)]
    public GameObject asteroidPrefab;

    int curChunkId;
    List<GameObject> chunks = new List<GameObject>();

    Transform playerObj;

    void Start() {
        playerObj = FindObjectOfType<PlayerController>().transform;
        UpdateTerrain();
    }

    void UpdateTerrain() {
        int nextChunkId = curChunkId + 1; // currently chunks only load forwards
        float chunkStart = GetPosForChunkId(curChunkId);
        float chunkEnd = GetPosForChunkId(nextChunkId);
        float curSpawnPos = chunkStart;

        GameObject newChunk = new GameObject("Chunk" + curChunkId);

        while (curSpawnPos < chunkEnd) {
            Vector2 circlePos = Random.insideUnitCircle * beltRadius;
            Vector3 spawnPos = new Vector3(circlePos.x, circlePos.y, curSpawnPos);

            Instantiate(asteroidPrefab, spawnPos, Quaternion.identity, newChunk.transform);

            curSpawnPos += terrainStep;
        }

        // delete farthest back chunk
        if (chunks.Count == maxNumChunks) {
            Destroy(chunks[0]);
            chunks.RemoveAt(0);
        }
        chunks.Add(newChunk);

        curChunkId = nextChunkId;
    }

    void LateUpdate() {
        if (playerObj.position.z + chunkSize > GetPosForChunkId(curChunkId)) { // if we are close to the "next chunk" we should make a new one
            UpdateTerrain();
        }
    }

    float GetPosForChunkId(int id) {
        return id * chunkSize + transform.position.z;
    }
}
