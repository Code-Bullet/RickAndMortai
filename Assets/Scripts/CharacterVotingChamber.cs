using UnityEngine;
using System.Collections;
using TMPro;

public class CharacterVotingChamber : MonoBehaviour
{
    // The list of characters options for the vote.
    public GameObject[] voteOptions;

    // The list of tallies for the vote.
    public TMP_Text[] voteTallies;

    // The status text.
    public TMP_Text statusText;

    // The camera that shows the stuff.
    public Camera camera;

    // Use this for initialization
    void Start()
    {
        foreach(TMP_Text t in voteTallies)
        {
            t.text = "VOTES:0";
        }

        foreach (GameObject o in voteOptions)
        {
            AIHeadRigger rigger = o.GetComponent<AIHeadRigger>();
            
        }

    }

    public void Setup(string characterName, string[] generationIds)
    {
        // Set the current camera to focus on this scene.
        // Load the 3d objects into all the character controllers.
        // Set the voting counters to 0.
        // Await a timeout.
        // When we're done, return the selected option.
    }

    // Update is called once per frame
    void Update()
    {

    }
}
