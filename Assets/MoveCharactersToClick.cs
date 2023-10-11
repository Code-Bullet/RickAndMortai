using UnityEngine;
using UnityEngine.AI;

public class MoveCharactersToClick : MonoBehaviour
{
    public NavMeshAgent character1;
    public NavMeshAgent character2;
    private NavMeshAgent activeCharacter; // The character currently being controlled

    public float rotationSpeed = 120f;
    public float movementSpeed = 0.2f;

    void Update()
    {
        // Left click to start controlling character 1
        if (Input.GetMouseButtonDown(0))
        {
            activeCharacter = character1;
        }

        // Right click to start controlling character 2
        if (Input.GetMouseButtonDown(1))
        {
            activeCharacter = character2;
        }

        // WASD Movement
        if (activeCharacter)
        {
            Vector3 forwardMovement = activeCharacter.transform.forward * Input.GetAxis("Vertical");
            // Vector3 rightMovement = activeCharacter.transform.right * Input.GetAxis("Horizontal");
            Vector3 movement = (forwardMovement).normalized;

            if (movement != Vector3.zero)
            {
                Vector3 targetPosition = activeCharacter.transform.position + movement * movementSpeed;
                activeCharacter.SetDestination(targetPosition);
            }

            // Q and E for rotation
            float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime; // Assumes you've set up a "Rotate" axis in Input settings
            activeCharacter.transform.Rotate(0, rotation, 0);
        }
    }
}
