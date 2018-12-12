using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public float terrainStep;
    public float beltRadius;
    public float initialLoadAmount;

    [Space(15)]
    public GameObject asteroidPrefab;

    float curMaxTerrainDistance;

    void Start() {
        GenerateTerrain(initialLoadAmount);
    }

    void GenerateTerrain(float distance) {
        float curSpawnDist = curMaxTerrainDistance;
        curMaxTerrainDistance += distance;

        while (curSpawnDist < curMaxTerrainDistance) {
            Vector2 circlePos = Random.insideUnitCircle * beltRadius;
            Vector3 spawnPos = new Vector3(circlePos.x, circlePos.y, curSpawnDist);

            Instantiate(asteroidPrefab, spawnPos, Random.rotation);

            curSpawnDist += terrainStep;
        }
    }
}
