using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

// used to move and animate the characters using nav mesh agents.
public class MyCharacterController : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    private NavMeshAgent agent;
    private Animator animator;



    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object");
        }
        else
        {
            agent.speed = movementSpeed;
        }
    }

    void Update()
    {
        // Check if the agent is moving
        if (agent.velocity.magnitude > 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    public void MoveTowardsPosition(Vector3 targetPosition, float stoppingDistance)
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object");
            return;
        }
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetPosition);


    }

    public void TeleportTo(Vector3 targetPosition)
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object");
            return;
        }

        bool successfulWarp = agent.Warp(targetPosition);

        if (!successfulWarp)
        {
            Debug.LogWarning("Failed to teleport NavMeshAgent");
        }

    }

    public async Task MoveTowardsPositionAsync(Vector3 targetPosition, float stoppingDistance)
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object");
            return;
        }
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetPosition);

        // check every o.1 seconds if the character has stopped running if so then end async function
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(0.1f));
            if (!animator.GetBool("isRunning"))
            {
                break;
            }
        }

    }

    public void StartTalking()
    {
        animator.SetBool("isTalking", true);
    }

    public void StopTalking()
    {
        animator.SetBool("isTalking", false);
    }

    private float rotationSpeed = 10.0f;

    public void LookAtTarget(GameObject target)
    {
        StartCoroutine(LookAtTargetOverTime(target));
    }

    private IEnumerator LookAtTargetOverTime(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;

        Quaternion lookRotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : transform.rotation;

        for (int i = 0; i < 100; i++)
        {
            if (Quaternion.Angle(transform.rotation, lookRotation) > 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                // Debug.Log("still spinning angle:  " + Quaternion.Angle(transform.rotation, lookRotation));
                yield return null; // wait until next frame
            }
            else
            {
                // Debug.Log("look at target took " + i + " frames");
                break;
            }
        }
        // Ensure the rotation is exact at the end
        transform.rotation = lookRotation;
    }
}
