using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class TerrainGenerator : MonoBehaviour {
    Terrain terrain;
    TerrainData terrainData;

    public int Seed = 42;
    System.Random r;

    public List<GameObject> spawners;
    public NavMeshSurface surface;
    public int mapX, mapY;
    int centerMargin = 40;

    public int birthLimit = 4;
    public int deathLimit = 3;
    public int numberOfSteps = 4;
    public float chanceAlive = 0.4f;

    int heightmapResolution;
    bool[,] cellMap;

    private void Awake() {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        r = new System.Random(Seed);
    }
    private void Start() {
        if (terrain == null) { Debug.Log("This TerrainGenerator is not on a Terrain"); return; }

        heightmapResolution = terrainData.heightmapResolution;
        terrain.heightmapMaximumLOD = 0;

        mapX = terrainData.heightmapWidth;
        mapY = terrainData.heightmapHeight;
        cellMap = new bool[mapX, mapY];

        float[,] heights = terrainData.GetHeights(0, 0, mapX, mapY);

        for (int x = 0; x < mapX - 1; x = x + 2) {
            for (int y = 0; y < mapY - 1; y = y + 2) {
                

                bool b = r.Next(0, 100) < chanceAlive * 100;
                if (b) {
                    cellMap[x, y] = true;
                    cellMap[x + 1, y] = true;
                    cellMap[x, y + 1] = true;
                    cellMap[x + 1, y + 1] = true;
                }
                if (x == 0 ||
                    y == 0 ||
                    x == mapX - 1 ||
                    y == mapY - 1) {

                    cellMap[x, y] = false;
                    cellMap[x + 1, y] = false;
                    cellMap[x, y + 1] = false;
                    cellMap[x + 1, y + 1] = false;
                }
                if ((x > (mapX / 2) - centerMargin
                   && x < (mapX / 2) + centerMargin)
                   && y > (mapY / 2) - centerMargin
                   && y < (mapY / 2) + centerMargin) {
                    cellMap[x, y] = true;
                }
            }
        }

        for (int i = 0; i < numberOfSteps; i++) {
            cellMap = doSimulationStep(cellMap);
        }

        for (int x = 0; x < mapX; x++) {
            for (int y = 0; y < mapY; y++) {
                if (cellMap[x, y]) {
                    heights[x, y] = 0.0f;
                } else {
                    heights[x, y] = 1.0f;
                }
            }
        }
        terrainData.SetHeights(0, 0, heights);
        GetComponent<AssignSplatMap>().enabled = true;

        if (!surface) { surface = FindObjectOfType<NavMeshSurface>(); }
        surface.overrideVoxelSize = true;
        surface.overrideTileSize = true;
        surface.voxelSize = 1.0f;
        surface.tileSize = 32;
        surface.BuildNavMesh();

        foreach (GameObject s in spawners) {
            s.SetActive(true);
        }

        NavMeshHit hit;

        Vector3
        pos = new Vector3(250, 0, 250);
        if (NavMesh.SamplePosition(pos, out hit, 200f, NavMesh.AllAreas)) {
            spawners[0].transform.position = hit.position;
            spawners[0].GetComponentInChildren<EnemySpawner>().transform.position = hit.position;
        }
        pos = new Vector3(0, 0, 250);
        if (NavMesh.SamplePosition(pos, out hit, 200f, NavMesh.AllAreas)) {
            spawners[1].transform.position = hit.position;
            spawners[1].GetComponentInChildren<EnemySpawner>().transform.position = hit.position;
        }
        pos = new Vector3(0, 0, 0);
        if (NavMesh.SamplePosition(pos, out hit, 200f, NavMesh.AllAreas)) {
            spawners[2].transform.position = hit.position;
            spawners[2].GetComponentInChildren<EnemySpawner>().transform.position = hit.position;
        }
        pos = new Vector3(250, 0, 0);
        if (NavMesh.SamplePosition(pos, out hit, 200f, NavMesh.AllAreas)) {
            spawners[3].transform.position = hit.position;
            spawners[3].GetComponentInChildren<EnemySpawner>().transform.position = hit.position;
        }

    }

    public bool[,] doSimulationStep(bool[,] oldMap) {
        bool[,] newMap = new bool[mapX, mapY];

        for (int x = 0; x < mapX; x++) {
            for (int y = 0; y < mapY; y++) {

                int n = GetNeighborCount(oldMap, x, y, 2);

                if (oldMap[x, y]) {
                    if (n < deathLimit) {
                        newMap[x, y] = false;
                    } else {
                        newMap[x, y] = true;

                    }
                } else {
                    if (n > birthLimit) {
                        newMap[x, y] = true;
                    } else {
                        newMap[x, y] = false;
                    }
                }
                if (x > (mapX / 2) - centerMargin
                 && x < (mapX / 2) + centerMargin
                 && y > (mapY / 2) - centerMargin
                 && y < (mapY / 2) + centerMargin) {
                    newMap[x, y] = true; //Ignore a square in the center of the map
                }
            }
        }
        return newMap;
    }
    public int GetNeighborCount(bool[,] map, int x, int y, int distance) {
        int count = 0;

        for (int i = -distance; i <= distance; i++) {
            for (int j = -distance; j <= distance; j++) {
                int nX = x + i;
                int nY = y + j;
                if (i == 0 && j == 0) {
                    continue;
                }
                if (nX < 0 ||
                    nY < 0 ||
                    nX >= map.GetLength(0) ||
                    nY >= map.GetLength(1)) {
                    //count++;
                } else if (map[nX, nY]) {
                    count++;
                }
            }
        }
        return count;
    }
}