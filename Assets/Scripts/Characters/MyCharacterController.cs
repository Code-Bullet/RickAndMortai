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


    public RuntimeAnimatorController dancingAnimatorController;
    private RuntimeAnimatorController normalAnimatorController;

    public GameObject lipSyncManager;

    private bool isDancing = false;



    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent component missing from this game object: {this.name}");
        }
        else
        {
            agent.speed = movementSpeed;
        }



        if (lipSyncManager != null)
        {
            lipSyncManager.SetActive(false);
        }


        normalAnimatorController = animator.runtimeAnimatorController;

    }






    void Update()
    {

        if (!isDancing)
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
    }



    public void StartDancing(int danceNumber)
    {

        if (dancingAnimatorController != null)
        {

            Debug.Log("start dancing baby");
            isDancing = true;
            animator.runtimeAnimatorController = dancingAnimatorController;
            animator.SetInteger("dance number", danceNumber);
        }

    }

    public void StopDancing()
    {
        Debug.Log("stop dancing baby");

        isDancing = false;
        animator.runtimeAnimatorController = normalAnimatorController;
    }


    public void MoveTowardsPosition(Vector3 targetPosition, float stoppingDistance)
    {
        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent component missing from this game object: {this.name}");
            return;
        }
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetPosition);


    }

    public void TeleportTo(Vector3 targetPosition)
    {
        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent component missing from this game object: {this.name}");
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
            Debug.LogError($"NavMeshAgent component missing from this game object: {this.name}");
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
        if (lipSyncManager != null)
        {
            lipSyncManager.SetActive(true);
        }
        animator.SetBool("isTalking", true);

    }

    public void StopTalking()
    {
        if (lipSyncManager != null)
        {
            lipSyncManager.SetActive(false);
        }
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
