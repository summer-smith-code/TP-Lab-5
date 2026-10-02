using System.Collections.Generic;
using System.Drawing;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

public class Avoider : MonoBehaviour
{
    private NavMeshAgent agent;
    public GameObject avoidee;
    public bool visualizeLines;
    public float speed;
    [Header("Poisson Disc Values")]
    public float size_x = 4;
    public float size_y = 4;
    public float cellSize = 1;
    [Tooltip("Layer the enemy is on so raycasts can see through it")]
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

        agent.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= 1) // Only makes a new Poisson disc if close to its destination instead of creating one every frame
        {
            if (!updatedPoisson)
            {
                PoissonDisc(size_x, size_y, cellSize);
                updatedPoisson = true;
                return;
            }
            Vector3 positionToMove = Vector3.zero;
            bool shouldMove = false;
            List<Vector3> unseenPoints = new List<Vector3>();
            foreach (Vector2 point in samplePositions) // Goes through all points in the poisson disc
            {
                Vector3 samplePosition = new Vector3(transform.position.x + point.x - size_x / 2f, transform.position.y, transform.position.z + point.y - size_y / 2f);
                Vector3 localDirection = (avoidee.transform.position - samplePosition).normalized;

                Vector3 enemyToSample = (samplePosition - transform.position).normalized;
                RaycastHit hit;

                // Checks if point can be seen by player
                if (Physics.Raycast(samplePosition, localDirection, out hit, Mathf.Infinity, ~enemyLayer))
                {
                    // if avoidee is blocked
                    if (!hit.transform.CompareTag("Player"))
                    {
                        // Draw point green if want to draw lines and if unseen by player & add to potential list of move points
                        unseenPoints.Add(samplePosition);
                        if (visualizeLines)
                        {
                            Debug.DrawLine(transform.position, samplePosition, UnityEngine.Color.green);
                        }
                    }
                    else
                    {
                        // Draw point red if want to draw lines & remove if was previously unseen by player
                        if (unseenPoints.Contains(samplePosition))
                        {
                            unseenPoints.Remove(samplePosition);
                        }
                        if (visualizeLines)
                        {
                            Debug.DrawLine(transform.position, samplePosition, UnityEngine.Color.red);
                        }
                    }
                }
            }

            // Checks if player can see this object
            Vector3 direction = (avoidee.transform.position - transform.position).normalized;
            RaycastHit player;
            if (Physics.Raycast(transform.position, direction, out player))
            {
                if (player.transform.CompareTag("Player"))
                {
                    shouldMove = true;
                }
            }

            if (shouldMove)
            {
                float distance = float.MaxValue;
                if (unseenPoints.Count <= 0) // If there's no unseen points, make a new poisson disc
                {
                    PoissonDisc(size_x, size_y, cellSize);
                    return;
                }

                // Get the point closest to this agent to move to
                foreach (Vector3 unseen in unseenPoints)
                {
                    float distFromEnemy = Vector3.Distance(transform.position, unseen);
                    if (distance > distFromEnemy)
                    {
                        distance = distFromEnemy;
                        positionToMove = unseen;
                    }
                }
                // Move to that point and create a new poisson disc once nearby
                agent.SetDestination(positionToMove);
                updatedPoisson = false;
            }
        }
    }

    private void PoissonDisc(float sizeX, float sizeY, float cell)
    {
        sampler = new PoissonDiscSampler(sizeX, sizeY, cell);

        samplePositions.Clear();

        foreach (Vector2 point in sampler.Samples())
        {
            samplePositions.Add(point);
        }
    }
}
