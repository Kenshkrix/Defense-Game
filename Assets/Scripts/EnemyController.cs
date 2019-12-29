using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public float attackRange;
    public float attackCooldown;
    public float stupidity = 0.75f;
    public float trackingDistance = 15f;
    public int attackDamage = 1;
    public dmgType damageType;
    public SpecialEffect onAttackEffect;
    public NavMeshAgent agent;
    public int navFrameMod;

    float lastAttackTime;

    GameObject player;
    BlackBoard bb;
    NavMeshPath path;



    Health health;


    private void Awake() {
        bb = FindObjectOfType<BlackBoard>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
    }
    private void Start() {
        player = bb.player;
        playerTransform = player.transform;
        myTransform = this.transform;
        if (!agent.isOnNavMesh) {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(this.transform.position, out hit, 20f, NavMesh.AllAreas)) {
                this.transform.position = hit.position;
            }
        }
        StartCoroutine(TrackPlayer(stupidity + Random.Range(-0.02f, 0.02f)));
    }

    Vector3[] thePath;

    private Vector3 FindFirstPathCorner(Vector3 to) {
        path = new NavMeshPath();
        thePath = new Vector3[1];
        if (!agent.isOnNavMesh) {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(this.transform.position, out hit, 20f, NavMesh.AllAreas)) {
                this.transform.position = hit.position;
            }
        }
        if (!agent.isOnNavMesh) {
            //Still? WAY out of bounds
            Destroy(this.gameObject);
            return Vector3.zero;
        }
        agent.CalculatePath(to, path);
        if (path.corners != null) {
            var num = path.GetCornersNonAlloc(thePath);
            if (num > 0) {
                return thePath[0];
            }
        }
        return playerTransform.position;
    }

    Transform playerTransform;
    Transform myTransform;
    Vector3 targetPoint;

    IEnumerator TrackPlayer(float stupidityDelay) {
        targetPoint = FindFirstPathCorner(playerTransform.position);
        agent.destination = targetPoint;

        float difference = Mathf.Abs((playerTransform.position - myTransform.position).sqrMagnitude);

        float actualDelay = stupidityDelay * (difference / trackingDistance);

        //Shouldn't be TOO stupid, or they won't be able to tell when they're near the player
        if (actualDelay > 2.52f) { actualDelay = 2.5f + Random.Range(-0.02f, 0.02f); }

        while (difference > trackingDistance) {
            yield return new WaitForSeconds(actualDelay);

            //It's either busy dying or resurrecting, either way don't bother telling it to go somewhere
            if (health.hp > 0 && !V3Equal(agent.destination, targetPoint)) {
                if (bb.frameCountHundred % navFrameMod == 0) {
                    agent.destination = (targetPoint);
                }
            }

            //This is the escape value to switch from tracking to attacking
            difference = Mathf.Abs((playerTransform.position - myTransform.position).sqrMagnitude);
            actualDelay = stupidityDelay * (difference / trackingDistance);

            targetPoint = FindFirstPathCorner(playerTransform.position);
            if (actualDelay > 2.52f) { actualDelay = 2.5f + Random.Range(-0.02f, 0.02f); }
        }
        StartCoroutine(AttackPlayer(0.02f));
    }
    IEnumerator AttackPlayer(float stupidityDelay) {
        float difference = Mathf.Abs((playerTransform.position - myTransform.position).sqrMagnitude);

        Health h = player.GetComponent<Health>();

        while (difference < trackingDistance) {
            //Units should be much less stupid when in attack mode, but I use the same variable name anyways
            yield return new WaitForSeconds(stupidityDelay);

            //It's either busy dying or resurrecting, either way don't bother telling it to go somewhere
            if (!player.transform) { Destroy(this.gameObject); }
            if (health.hp > 0 && !V3Equal(agent.destination, playerTransform.position)) {
                agent.destination = (playerTransform.position);
            }

            difference = Mathf.Abs((playerTransform.position - myTransform.position).sqrMagnitude);
            if (difference < (attackRange) && 
                (Time.time - lastAttackTime) > attackCooldown) {

                h.Damage(this.gameObject, attackDamage);
                lastAttackTime = Time.time;
                Instantiate(onAttackEffect).transform.position = playerTransform.position + (Vector3.up * difference);
            }
        }
        StartCoroutine(TrackPlayer(stupidity));
    }

    public bool V3Equal(Vector3 a, Vector3 b) {
        return Vector3.SqrMagnitude(a - b) < 0.000001;
    }
}
