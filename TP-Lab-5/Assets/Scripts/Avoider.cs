using System.Collections.Generic;
using System.Drawing;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

public class Avoider : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] GameObject avoidee;
    [SerializeField] bool visualizeLines;
    public float size_x = 4;
    public float size_y = 4;
    public float cellSize = 1;
    public LayerMask enemyLayer;

    PoissonDiscSampler sampler;
    bool updatedPoisson = false;
    List<Vector2> samplePositions = new List<Vector2>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GetComponent<NavMeshAgent>() == false)
        {
            Debug.LogError("Needs NavMeshAgent component!");
            return;
        }
        else
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (avoidee == null)
        {
            Debug.LogError("Needs avoidee game object!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!updatedPoisson)
            {
                Debug.Log("start");
                PoissonDisc();
                updatedPoisson = true;
                return;
            }

            Vector3 positionToMove = Vector3.zero;
            bool shouldMove = false;
            List<Vector3> unseenPoints = new List<Vector3>();
            foreach (Vector2 point in samplePositions)
            {
                Vector3 samplePosition = new Vector3(transform.position.x + point.x - size_x / 2f, transform.position.y, transform.position.z + point.y - size_y / 2f);
                Vector3 localDirection = (avoidee.transform.position - samplePosition).normalized;
                // Visualize line
                Debug.DrawLine(transform.position, samplePosition, UnityEngine.Color.white, .01f);

                Vector3 enemyToSample = (samplePosition - transform.position).normalized;
                RaycastHit sampleHit;

                // Makes sure point is not on the other side of a wall
                if (!Physics.Raycast(transform.position, enemyToSample, out sampleHit, (samplePosition - transform.position).magnitude))
                {
                    RaycastHit hit;

                    if (Physics.Raycast(samplePosition, localDirection, out hit, Mathf.Infinity, ~enemyLayer))
                    {
                        // if avoidee is blocked
                        if (!hit.transform.CompareTag("Player"))
                        {
                            // point green
                            unseenPoints.Add(samplePosition);
                        }
                    }
                }

                float furthestFromPlayer = 0;
                float distanceFromPlayer = Vector3.Distance(avoidee.transform.position, samplePosition);
                if (furthestFromPlayer < distanceFromPlayer)
                {
                    furthestFromPlayer = distanceFromPlayer;
                    positionToMove = samplePosition;
                }
                // visualize line
                // check if avoidee can see each point
            }

            Vector3 direction = (avoidee.transform.position - transform.position).normalized;
            RaycastHit player;
            if(Physics.Raycast(transform.position, direction, out player))
            {
                if (player.transform.CompareTag("Player"))
                {
                    Debug.Log("I SEE YOU!");
                    shouldMove = true;
                }
            }

            if (shouldMove)
            {
                float distance = float.MinValue;
                if (unseenPoints.Count <= 0)
                {
                    agent.SetDestination(positionToMove);
                    PoissonDisc();
                    return;
                }

                foreach (Vector3 unseen in unseenPoints)
                {
                    float distToPlayer = Vector3.Distance(avoidee.transform.position, unseen);
                    if (distance < distToPlayer)
                    {
                        distance = distToPlayer;
                        positionToMove = unseen;
                    }
                }
                agent.SetDestination(positionToMove);
                updatedPoisson = false;
            }
        }
    }

    private void PoissonDisc()
    {
        sampler = new PoissonDiscSampler(size_x, size_y, cellSize);
        samplePositions.Clear();

        foreach (Vector2 point in sampler.Samples())
        {
            samplePositions.Add(point);
        }
    }
}
