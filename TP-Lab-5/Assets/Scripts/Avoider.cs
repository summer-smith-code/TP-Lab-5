using UnityEngine;
using UnityEngine.AI;

public class Avoider : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] GameObject avoidee;
    [SerializeField] float range;
    [SerializeField] bool visualizeLines;
    private float rayDistance;
    private float size_x = 4;
    private float size_y = 4;
    private float cellSize = 1;


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
        if (range <= 0)
        {
            Debug.LogError("Needs a range for avoidance!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        PoissonDisc();
        
    }

    private void PoissonDisc()
    {
        var sampler = new PoissonDiscSampler(size_x, size_y, cellSize);
            foreach (var point in sampler.Samples())
        {
            Vector3 samplePosition = new Vector3(point.x, avoidee.transform.position.y, point.y);
            Vector3 localDirection = (avoidee.transform.position - samplePosition).normalized;
            Vector3 worldDirection = avoidee.transform.TransformDirection(localDirection);
            RaycastHit hit;

            if (Physics.Raycast(transform.position, worldDirection, out hit, rayDistance))
            {
                // if avoidee is blocked
                if (!hit.transform.CompareTag("Player"))
                {
                    // point red
                    return;
                }
                else
                {
                    Debug.LogError("I SEE YOU!");
                }
            }
            // visualize line
            // check if avoidee can see each point

        }
    }
}
