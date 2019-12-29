using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnDelay = 0.75f;
    public int spawnCount = 100;

    BlackBoard BB;
    GameObject enemy;
    EnemyController enemyController;

    private void Awake() {
        if (BB == null) { BB = FindObjectOfType<BlackBoard>(); }
        BB.WaveEnd++;
    }
    private void Start() {
        NavMeshHit hit;
        NavMesh.SamplePosition(this.transform.position, out hit, 5f, NavMesh.AllAreas);
        this.transform.position = hit.position;
        StartCoroutine(SpawnRepeatedly(spawnDelay));
    }
    public void startSpawning()
    {
        StartCoroutine(SpawnRepeatedly(spawnDelay));
    }

    IEnumerator SpawnRepeatedly(float seconds) {
        yield return new WaitForSeconds(seconds);
        enemy = Instantiate(enemyPrefab);
        enemyController = enemy.GetComponent<EnemyController>();
        enemyController.navFrameMod = (int)(Random.Range(3, 6));
        enemyController.agent.Warp(this.transform.position);
        if (enemyController.agent == null)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(this.transform.position, out hit, 5f, NavMesh.AllAreas);
            this.transform.position = hit.position;
        }
        if (!enemyController.agent.isOnNavMesh) {
            Debug.Log("Enemies spawning incorrectly, deleting them");
            Destroy(enemyController.gameObject);
        }
        else
        {
            BB.changeEnemyCount(1);
            if (spawnCount > 0)
            {
                spawnCount--;
                StartCoroutine(SpawnRepeatedly(seconds));
            }
        }
        if (spawnCount <= 0) {
            //No more enemies, kill the wave
            BB.WaveEnd--;
        }
    }
}
