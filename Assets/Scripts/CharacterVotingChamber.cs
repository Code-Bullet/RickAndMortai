using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CharacterVoteResults
{
    public string selectedGeneration = "";
    public string[] options = new string[4];
    public int[] tallies = new int[4];
}

public class CharacterVotingChamber : MonoBehaviour
{
    // The list of characters options for the vote.
    public GameObject[] voteOptions;

    // The list of tallies for the vote.
    public TMP_Text[] voteTallies;

    // The status text.
    public TMP_Text statusText;

    // The camera that shows the stuff.
    public Camera stageCamera;


    public bool isActive = false;

    private CharacterVoteResults results;

    // Use this for initialization
    void Start()
    {
        this.results = new CharacterVoteResults();
        this.results.options = new string[] { };
        this.results.tallies = new int[4];


        foreach (TMP_Text t in voteTallies)
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
        this.results = new CharacterVoteResults();
        this.results.options = generationIds;
        this.results.tallies = new int[4];

        // Set the current camera to focus on this scene.
        // Load the 3d objects into all the character controllers.
        // Set the voting counters to 0.
        // Await a timeout.
        // When we're done, return the selected option.

        if (generationIds.Length != voteOptions.Length)
        {
            Debug.LogWarning($"{generationIds.Length} character generations given, but we have {voteOptions.Length} vote options");
        }

        int i = 0;

        foreach (GameObject o in voteOptions)
        {
            AIHeadRigger rigger = o.GetComponent<AIHeadRigger>();
            int generationIndex = Math.Min(i, generationIds.Length - 1);
            rigger.RenderGeneration(characterName, generationIds[generationIndex]);
            i++;
        }


    }

    
    public async Task<CharacterVoteResults> RunVote(int timeToVoteMilliseconds)
    {
        isActive = true;

        await Task.Delay(timeToVoteMilliseconds);

        var results = this.results;
        this.results = new CharacterVoteResults();

        // Now tally votes.
        int highestVoted = 0;
        for (int j = 0; j < results.tallies.Length; j++)
        {
            if (results.tallies[j] > results.tallies[highestVoted]) highestVoted = j;
        }

        // Sometimes we only get 3 generations back, cus of some NSFW stuff.
        // So restrict highestVoted index based on how many options we actually had.
        highestVoted = highestVoted % this.results.options.Length;

        results.selectedGeneration = results.options[highestVoted];

        isActive = false;

        return results;
    }

    void Update()
    {
        if (!isActive) return;


        // Detect manual keyboard voting.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            this.results.tallies[0] += 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            this.results.tallies[1] += 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            this.results.tallies[2] += 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            this.results.tallies[3] += 1;
        }

        // Update tallies text.
        for(int i = 0; i < voteTallies.Length; i++)
        {
            TMP_Text t = voteTallies[i];
            if (results.tallies.Length < i) continue;

            t.text = $"VOTES:{results.tallies[i]}";
        }
    }
}
