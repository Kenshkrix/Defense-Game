using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Cinemachine;

public class BlackBoard : MonoBehaviour
{
    public GameObject player;
    PlayerController playerController;
    Vector3 playerSpawn;
    NavMeshSurface surface;

    public CinemachineVirtualCamera cam;

    public int currentEnemies;
    public Text tCurrentEnemies;
    public int deathsThisFrame;

    public int currentWave;
    public Text tCurrentWave;

    public int playerPoints;
    public Text tAmountPoints;
    public Text tChangePoints;

    public int playerKills;
    public Text tAmountKills;

    public int specialAmmo;
    public Text tAmmoCount;
    public Text tAmmoTimer;

    public Text scoreText;
    public Text txtBestScore;
    public InputField scoreNameInput;
    public GameObject ESCMenuPanel;

    int pointsThisTimer;
    int playerPointChain;
    float playerPointChainMult = 1.0f;
    public bool mouseOverPoints = false;

    public int WaveEnd = 0;
    public int frameCountHundred;
    public bool undamaged;
    public int bestScore = 0;
    private void Awake() {
        cam = FindObjectOfType<CinemachineVirtualCamera>();
        StartGame();
    }
    public void buttonCloseESCMenu()
    {
        playerController.buttonCloseESCMenu();
    }
    public void buttonSaveExit()
    {
        playerController.buttonSaveAndExit();
    }
    public void buttonSaveReturn()
    {
        playerController.buttonSaveAndReturn();
    }
    public void StartGame() {
        if (!surface) { surface = FindObjectOfType<NavMeshSurface>(); }
        playerSpawn = new Vector3(20, 1, 35);
        
        player = Instantiate(player, playerSpawn, new Quaternion(0, 0, 0, 0));
        playerController = player.GetComponent<PlayerController>();

        cam.Follow = player.transform;
        cam.LookAt = player.transform;
        tAmmoCount.text = "" + playerController.W.chargeCount;
        undamaged = true;
        StartCoroutine(updateUIText());
        StartCoroutine(EveryFrame());
    }

    public void changeEnemyCount(int enemies) {
        currentEnemies += enemies;
        tCurrentEnemies.text = "" + currentEnemies;
        if (WaveEnd <= 0 && currentEnemies <= 0) {
            EndWave();
        }
    }
    public void EndWave()
    {
        currentWave += 1;
        //TODO: Add grace period and timer before this part
        //TODO: Also this is where I'd implement a shop/skill system and such
        TerrainGenerator TG = FindObjectOfType<TerrainGenerator>();
        EnemySpawner spawner1 = TG.spawners[0].GetComponent<EnemySpawner>();
        EnemySpawner spawner2 = TG.spawners[1].GetComponent<EnemySpawner>();
        EnemySpawner spawner3 = TG.spawners[2].GetComponent<EnemySpawner>();
        EnemySpawner spawner4 = TG.spawners[3].GetComponent<EnemySpawner>();
        EnemySpawner spawner5 = TG.spawners[0].GetComponentInChildren<EnemySpawner>();
        EnemySpawner spawner6 = TG.spawners[1].GetComponentInChildren<EnemySpawner>();
        EnemySpawner spawner7 = TG.spawners[2].GetComponentInChildren<EnemySpawner>();
        EnemySpawner spawner8 = TG.spawners[3].GetComponentInChildren<EnemySpawner>();

        spawner1.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner2.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner3.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner4.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner5.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner6.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner7.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;
        spawner8.spawnCount = Random.Range(20, 30 + currentWave) + currentWave;

        if (currentWave < 100)
        {
            spawner1.spawnDelay = 1f - (currentWave / 100f);
            spawner2.spawnDelay = 1f - (currentWave / 100f);
            spawner3.spawnDelay = 1f - (currentWave / 100f);
            spawner4.spawnDelay = 1f - (currentWave / 100f);
            spawner5.spawnDelay = 1f - (currentWave / 100f);
            spawner6.spawnDelay = 1f - (currentWave / 100f);
            spawner7.spawnDelay = 1f - (currentWave / 100f);
            spawner8.spawnDelay = 1f - (currentWave / 100f);
        }
        else
        {
            spawner1.spawnDelay = 0.01f;
            spawner2.spawnDelay = 0.01f;
            spawner3.spawnDelay = 0.01f;
            spawner4.spawnDelay = 0.01f;
            spawner5.spawnDelay = 0.01f;
            spawner6.spawnDelay = 0.01f;
            spawner7.spawnDelay = 0.01f;
            spawner8.spawnDelay = 0.01f;
        }
        tCurrentWave.text = "" + currentWave;
        WaveEnd = 8;

        spawner1.startSpawning();
        spawner2.startSpawning();
        spawner3.startSpawning();
        spawner4.startSpawning();
        spawner5.startSpawning();
        spawner6.startSpawning();
        spawner7.startSpawning();
        spawner8.startSpawning();
    }

    public void changeKills(int kills) {
        playerKills += kills;
        tAmountKills.text = "" + playerKills;
        changeEnemyCount(-kills);
    }

    public bool changePoints(int amount) {
        if (undamaged && playerPointChainMult < 1.0001f) {
            playerPointChainMult = 1.5f;
        }
        if (currentWave > 1)
        {
            playerPointChainMult *= 1f + (currentWave / 50f); //2% more score per wave
        }
        if (amount <= 0) {
            if (playerPoints + amount < 0) {
                //Debug.Log("Can't have less than zero points");
                playerPoints = 0;
                return false;
            }
            else {
                playerPoints += amount;
                pointsThisTimer += amount;
            }
        } else {
            //If the gained points text is still visible then increase the multiplier, else reset it
            //I could time it but I'd rather ensure that the player receives accurate feedback even in the case of bugs
            if (tChangePoints.color.a > 0.01f) {
                if (playerPointChainMult > 32.0f) {//100 more kills, double again. Should be nearly impossible.
                    playerPointChainMult += 0.32f;
                }
                else if (playerPointChainMult > 16.0f) { //100 more kills, double again
                    playerPointChainMult += 0.16f;
                }
                else if (playerPointChainMult > 8.0f) { //100 more kills, double again
                    playerPointChainMult += 0.08f;
                }
                else if (playerPointChainMult > 4.0f) { //100 more kills, double again
                    playerPointChainMult += 0.04f;
                } else if (playerPointChainMult > 2.0f) { //After 100 kills, double bonus' bonus increase
                    playerPointChainMult += 0.02f;
                } else {
                    playerPointChainMult += 0.01f;
                }
            }
            else {
                if (undamaged) {
                    playerPointChainMult = 1.5f;
                } else {
                    playerPointChainMult = 1.0f;
                }
                playerPointChain = amount;
            }


            //Truncate anything that doesn't fully increase the gained points using a straight int cast
            amount = (int)(amount * playerPointChainMult);

            //Update all the various point trackers by the new amount
            playerPointChain += amount;
            playerPoints += amount;
            pointsThisTimer += amount;
        }
        if (playerPoints > bestScore) { bestScore = playerPoints; }
        return true;
    }

    IEnumerator updateUIText() {
        //Update this five times every game second forever
        while (true) {
            yield return new WaitForSeconds(0.2f);
            if (mouseOverPoints) { continue; }

            tAmmoCount.text = "" + playerController.W.chargeCount;
            if (pointsThisTimer < 0) {
                tChangePoints.text = "-" + pointsThisTimer;
                tChangePoints.GetComponent<UITextColor>().colorRed();
            }
            else {
                if (tChangePoints.color.a <= 0.01f) {
                    if (undamaged) { playerPointChainMult = 1.5f;
                    } else {
                        playerPointChainMult = 1.0f;
                    }
                    playerPointChain = 0;
                }
                if (pointsThisTimer != 0) {
                    tChangePoints.GetComponent<UITextColor>().colorGreen();
                    if (pointsThisTimer > 1000000000) {
                        tChangePoints.text = "+" + pointsThisTimer / 1000000000 + "B";
                    } else if (pointsThisTimer > 10000000) {
                        tChangePoints.text = "+" + pointsThisTimer / 1000000 + "M";
                    } else if (pointsThisTimer > 10000) {
                        tChangePoints.text = "+" + pointsThisTimer / 1000 + "k";
                    } else {
                        tChangePoints.text = "+" + pointsThisTimer;
                    }
                }
            }
            if (playerPoints > 1000000000) {
                if (playerPointChainMult != 1.0f) {
                    tAmountPoints.text = "" + playerPoints / 1000000000 + "B *" + Truncate(playerPointChainMult, 2);
                }
                else {
                    tAmountPoints.text = "" + playerPoints / 1000000000 + "B";
                }
            }else if (playerPoints > 10000000) {
                if (playerPointChainMult != 1.0f) {
                    tAmountPoints.text = "" + playerPoints / 1000000 + "M *" + Truncate(playerPointChainMult, 2);
                }
                else {
                    tAmountPoints.text = "" + playerPoints / 1000000 + "M";
                }
            } else if (playerPoints > 10000) {
                if (playerPointChainMult != 1.0f) {
                    tAmountPoints.text = "" + playerPoints / 1000 + "k *" + Truncate(playerPointChainMult, 2);
                }
                else {
                    tAmountPoints.text = "" + playerPoints / 1000 + "k";
                }
            }
            else {
                if (playerPointChainMult != 1.0f) {
                    tAmountPoints.text = "" + playerPoints + " *" + Truncate(playerPointChainMult, 2);
                }
                else {
                    tAmountPoints.text = "" + playerPoints;
                }
            }
            pointsThisTimer = 0;
        }
    }
    IEnumerator EveryFrame() {
        bool flip = false;
        while (true) {
            yield return new WaitForEndOfFrame();
            deathsThisFrame = 0;
            if (flip) {
                if (frameCountHundred >= 100) { frameCountHundred = 0; } else { frameCountHundred++; }
                flip = false;
            } else { flip = true; }
            
            if (!player.transform) { Destroy(this.gameObject); }
            /*
            if (player.transform.position.y < 4.0f) {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(player.transform.position, out hit, 20f, NavMesh.AllAreas)) {
                    player.transform.position = hit.position + Vector3.up * 2f;
                    changePoints(-100);
                }
            }
            */
        }
    }

    public float Truncate(float f, int digits) {
        f *= 10 * digits;
        f = (int)f;
        f /= 10 * digits;
        return f;
    }
}