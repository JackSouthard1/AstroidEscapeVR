using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public float terrainStep;
    public float beltRadius;
    public float chunkSize;
    public int maxNumChunks;

    [Space(15)]
    public float launchRadius;
    public float launchBufferOffset;
    public float minLaunchWait;
    public float maxLaunchWait;
    public float launchConeAngle;
    public float minLaunchSpeed;
    public float maxLaunchSpeed;
    float nextLaunch;

    [Space(15)]
    public GameObject asteroidPrefab;

    int curChunkId;
    List<GameObject> chunks = new List<GameObject>();

    Rigidbody playerRb;

    void Start() {
        playerRb = FindObjectOfType<PlayerController>().GetComponent<Rigidbody>();
        nextLaunch = Time.time + Random.Range(minLaunchWait, maxLaunchWait);
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

    void Update() {
        if (Time.time > nextLaunch) {
            nextLaunch = Time.time + Random.Range(minLaunchWait, maxLaunchWait);
            LaunchAsteroid();
        }
    }

    void LaunchAsteroid() {
        float launchSpeed = Random.Range(minLaunchSpeed, maxLaunchSpeed);
        float buffer = Mathf.Max(launchRadius * playerRb.velocity.z / launchSpeed, 50f) + Random.Range(-launchBufferOffset, launchBufferOffset); //z position to spawn at, include this arbitrary minimum distance so that it can't be too close or spawn behind us

        float randomAngle = Random.Range(0, Mathf.PI); // a random angle on the top of the unit circle
        Vector3 spawnPos = new Vector3(Mathf.Cos(randomAngle) * launchRadius, Mathf.Sin(randomAngle) * launchRadius, playerRb.position.z + buffer);

        GameObject newMovingAsteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity, chunks[chunks.Count - 1].transform); //include it in the farthest forward chunk so that it unloads last
        Rigidbody newAsteroidRb = newMovingAsteroid.GetComponent<Rigidbody>();
        newAsteroidRb.drag = 0;

        Quaternion launchAngle = Quaternion.Euler(0, Random.Range(-launchConeAngle, launchConeAngle), (randomAngle * Mathf.Rad2Deg) - 180f + Random.Range(-launchConeAngle, launchConeAngle));
        newAsteroidRb.velocity = launchAngle * Vector3.right * launchSpeed;
    }

    void LateUpdate() {
        if (playerRb.transform.position.z + chunkSize > GetPosForChunkId(curChunkId - 1)) { // if we are close to the "next chunk" we should make a new one
            UpdateTerrain();
        }
    }

    float GetPosForChunkId(int id) {
        return id * chunkSize + transform.position.z;
    }
}
