using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// this is the script that actually acts out the scene. so itll move characters around and play tts audio files and shit.
public class SceneDirector : MonoBehaviour
{
    public List<CharacterInfo> characterList;
    public CharacterInfo defaultGuy;
    public AiArtCharacterController defaultGuyAIArtController;


    public MyCharacterController rick;
    public MyCharacterController morty;

    public DimensionGameObjects Garage;
    public DimensionGameObjects FrontYard;
    public DimensionGameObjects BikiniBottom;
    public DimensionGameObjects SimpsonsHouse;
    public DimensionGameObjects ShreksSwamp;
    public DimensionGameObjects StarWars;
    public DimensionGameObjects BackAlley;
    public DimensionGameObjects CodeBulletsVoid;
    public DimensionGameObjects DefaultDimension;

    public TMP_Text DimensionText;
    public GameObject reflectionProbe;

    public DimensionGameObjects currentDimension;
    public DimensionGameObjects previousDimension;

    public FakeYouAPIManager fakeYouAPIManager;

    public CameraShotManager cameraShotManager;

    private AudioSource audioSource;
    private AudioClip[] audioClips;

    public GameObject RickRenderer;
    public GameObject MortyRenderer;


    public bool useVoiceActing = false;

    public TMP_Text textField;
    public TMP_Text titleText;

    public Burpifier burpifier;

    public CinemachineTargetGroup targetGroup;



    // Start is called before the first frame update
    void Start()
    {

        audioSource = GetComponent<AudioSource>();
        currentDimension = Garage;
        previousDimension = FrontYard;

        ResetTargetGroup();
        AddCharacterTargetGroup(characterList[0]);
        AddCharacterTargetGroup(characterList[1]);

    }



    // this function actually plas the scene
    public async Task PlayScene(string[] outputLines, List<AudioClip> voiceActingClips)
    {

        int audioClipIndex = 0;


        foreach (string line in outputLines)
        {
            Debug.Log("Running " + line);
            string lowerLine = line.ToLower();
            int numWords = line.Split(' ').Length;


            // if its an action instruction
            if (line.Contains('[') && !line.Contains(":"))
            {

                Debug.Log("Stage Direction instruction: " + line);

                CharacterInfo character1 = null;
                CharacterInfo character2 = null;

                GetCharacters(lowerLine, ref character1, ref character2);
                Debug.Log("action found " + line);
                if (character1 == null)
                {
                    Debug.Log("Fuck there needs to be a character with an action");
                    continue;
                }

                GameObject location = null;
                GameObject lookAt = null;

                GetLocation(lowerLine, ref location, ref lookAt);

                //line is a stage direction like [rick walks to morty] or [spongebob walks to center stage]
                if (lowerLine.Contains("walks to "))
                {
                    if (character2 != null)
                    {
                        await CharacterWalksToCharacter(character1.characterController, character2.characterController);
                    }
                    else
                    {
                        await CharacterWalksToLocation(character1.characterController, location, lookAt);
                    }
                }
                else if (lowerLine.Contains("enter the portal") ||
                lowerLine.Contains("enter a portal") ||
                lowerLine.Contains("enter portal"))
                {
                    await RickAndMortyEnterPortal(lowerLine);
                }
                else
                {
                    Debug.Log("Invalid action");
                    continue;
                }

            }
            else if (line.Contains('{') && !line.Contains(":"))
            {
                Debug.Log("camera instruction: " + line);
                // if camera instruction 
                cameraShotManager.ChangeCameraShot(line);
            }
            else
            {
                Debug.Log("Dialog instruction: " + line);
                Debug.Log("clip number: " + audioClipIndex + "/" + voiceActingClips.Count);


                if (voiceActingClips != null && audioClipIndex >= voiceActingClips.Count)
                {
                    continue;
                }

                CharacterInfo talkingCharacter = GetWhosTalking(lowerLine);
                if (talkingCharacter != null)
                {

                    Debug.Log(line);
                    // if the character is the narrator then theres no character to animate and shit. so just play the audio
                    if (talkingCharacter.name == "narrator")
                    {

                        // display title
                        if (voiceActingClips != null && (audioClipIndex == 0 || audioClipIndex == 1))
                        {
                            textField.text = "";
                            // await ProcessDialogFromLines(outputLines);
                            titleText.text = line.Substring(talkingCharacter.name.Length + 2);
                            titleText.gameObject.SetActive(true);
                            if (WholeThingManager.Singleton.usingVoiceActing) audioClipIndex = await PlayAudioClipAtIndex(voiceActingClips, audioClipIndex);
                            else await Task.Delay(Mathf.FloorToInt(numWords / WholeThingManager.Singleton.wordsPerMinute * 60000) + 500);
                            titleText.gameObject.SetActive(false);

                        }
                        else
                        {
                            textField.text = line;
                            if (WholeThingManager.Singleton.usingVoiceActing) audioClipIndex = await PlayAudioClipAtIndex(voiceActingClips, audioClipIndex);
                            else await Task.Delay(Mathf.FloorToInt(numWords / WholeThingManager.Singleton.wordsPerMinute * 60000) + 500);
                        }

                        continue;
                    }

                    // if the character hasnt yet talked add them to the target group
                    if (!talkingCharacter.hasTalkedThisDimension)
                    {
                        // if the character is not in this dimension currently and its not rick or morty teleport them to this dimension
                        if (talkingCharacter.dimension != currentDimension && talkingCharacter != characterList[0] && talkingCharacter != characterList[1])
                        {
                            // teleport them in
                            talkingCharacter.characterController.TeleportTo(currentDimension.portalLocation.transform.position);
                            talkingCharacter.dimension = currentDimension;
                            // have them walk to rick
                            await CharacterWalksToCharacter(talkingCharacter.characterController, rick);
                        }
                        AddCharacterTargetGroup(talkingCharacter);
                        talkingCharacter.hasTalkedThisDimension = true;
                    }




                    talkingCharacter.characterController.StartTalking();

                    //every other character turns towards talking character
                    foreach (CharacterInfo characterInfo in characterList)
                    {
                        if (characterInfo != talkingCharacter)
                        {
                            characterInfo.characterController.LookAtTarget(talkingCharacter.gameObject);
                        }
                    }

                    // if rick it talking then burpify the audio
                    if (voiceActingClips != null && talkingCharacter.name == "rick")
                    {
                        voiceActingClips[audioClipIndex] = burpifier.Burpify(voiceActingClips[audioClipIndex]);
                    }


                    //actually play the audio
                    textField.text = line;
                    if (WholeThingManager.Singleton.usingVoiceActing) audioClipIndex = await PlayAudioClipAtIndex(voiceActingClips, audioClipIndex);
                    else await Task.Delay(Mathf.FloorToInt(numWords / WholeThingManager.Singleton.wordsPerMinute * 60000) + 500);

                    //we done
                    talkingCharacter.characterController.StopTalking();

                }

            }
        }


        // if rick and morty arent in the garage have them return to the garage.
        if (currentDimension != Garage)
        {
            await RickAndMortyEnterPortal("[rick and morty enter the portal to the garage]");
        }

    }


    void AddCharacterTargetGroup(CharacterInfo characterInfo)
    {
        // Create a new Target with the GameObject, weight, and radius
        CinemachineTargetGroup.Target newTarget = new CinemachineTargetGroup.Target
        {
            target = characterInfo.cameraTarget.transform,
            weight = 1,
            radius = 1
        };
        CinemachineTargetGroup.Target newTarget2 = new CinemachineTargetGroup.Target
        {
            target = characterInfo.gameObject.transform,
            weight = 1,
            radius = 1
        };

        // Get the existing targets from the group
        CinemachineTargetGroup.Target[] oldTargets = targetGroup.m_Targets;

        // Create a new array to store all the targets (old + new)
        CinemachineTargetGroup.Target[] newTargets = new CinemachineTargetGroup.Target[oldTargets.Length + 2];

        // Copy old targets to the new array
        for (int i = 0; i < oldTargets.Length; i++)
        {
            newTargets[i] = oldTargets[i];
        }

        // Add the new target to the new array
        newTargets[oldTargets.Length] = newTarget;
        newTargets[oldTargets.Length + 1] = newTarget2;

        // Update the target group
        targetGroup.m_Targets = newTargets;
    }


    void ResetTargetGroup()
    {
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
    }



    private void GetLocation(string lowerLine, ref GameObject location, ref GameObject lookAt)
    {

        if (lowerLine.Contains("work bench") || lowerLine.Contains("workbench")) // if the character is in the instruction
        {
            location = currentDimension.deskLocation1;
        }
        else if (lowerLine.Contains("center stage"))
        {
            location = currentDimension.centerStage1;
            lookAt = currentDimension.actualCamera;
        }

    }

    // returns the charcter that is talking in the dialog, so Rick: fuck you will return rick. 
    private CharacterInfo GetWhosTalking(string dialogLine)
    {

        string processedDialog = System.Text.RegularExpressions.Regex.Replace(dialogLine.ToLower(), "patrick", "pat", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        processedDialog = System.Text.RegularExpressions.Regex.Replace(processedDialog, "jar jar:", "jar jar binks:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        processedDialog = System.Text.RegularExpressions.Regex.Replace(processedDialog, "jar jar :", "jar jar binks:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        processedDialog = System.Text.RegularExpressions.Regex.Replace(processedDialog, "jarjar:", "jar jar binks:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        processedDialog = System.Text.RegularExpressions.Regex.Replace(processedDialog, "jarjar :", "jar jar binks:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (CharacterInfo character in characterList)
        {
            string characterSpeakingString = character.name + ":";
            string characterSpeakingString2 = character.name + " :";
            if (processedDialog.Contains(characterSpeakingString) || processedDialog.Contains(characterSpeakingString2))
            {
                // found the speaker. 
                return character;
            }

        }

        // if the processed dialog contains a colon but no character then just default to the default character
        if (processedDialog.Contains(":"))
        {
            defaultGuy.name = processedDialog.Split(':')[0];
            return defaultGuy;
        }

        return null;
    }

    // gets the characters that are participating in this action, character 1 is the first mention character and character 2 is the second.
    // so "[rick walks to morty]  returns chacters 1 rick, character 2 morty.
    public void GetCharacters(string lowerLine, ref CharacterInfo character1, ref CharacterInfo character2)
    {
        int character1Index = -1;
        int character2Index = -1;

        //replace all references of patrick to pat so it doesnt trigger contain rick
        lowerLine = System.Text.RegularExpressions.Regex.Replace(lowerLine, "patrick", "pat", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (lowerLine.Contains("jar jar") && !lowerLine.Contains("jar jar binks"))
        {
            lowerLine = System.Text.RegularExpressions.Regex.Replace(lowerLine, "jar jar", "jar jar binks", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        if (lowerLine.Contains("jarjar") && !lowerLine.Contains("jar jar binks"))
        {
            lowerLine = System.Text.RegularExpressions.Regex.Replace(lowerLine, "jar jar", "jar jar binks", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        foreach (CharacterInfo character in characterList)
        {


            if (lowerLine.Contains(character.name.ToLower())) // if the character is in the instruction
            {

                if (character1 != null) // if this is the second character found
                {
                    //set it to character2
                    character2Index = lowerLine.IndexOf(character.name.ToLower());
                    character2 = character;
                    if (character1Index > character2Index) // check if need to swap 
                    {
                        // swap
                        int temp = character1Index;
                        character1Index = character2Index;
                        character2Index = temp;

                        character2 = character1;
                        character1 = character;
                    }
                }
                else
                {
                    //if this is the first character found we just chuck that bitch in.
                    character1Index = lowerLine.IndexOf(character.name.ToLower());
                    character1 = character;
                }
            }
        }



    }

    // plays the audio clip and returns the new audioclip index.
    private async Task<int> PlayAudioClipAtIndex(List<AudioClip> voiceActingClips, int audioClipIndex)
    {

        if (voiceActingClips != null && audioClipIndex < voiceActingClips.Count && voiceActingClips[audioClipIndex] != null)
        {

            audioSource.clip = voiceActingClips[audioClipIndex];
            audioSource.Play();

            // have the max delay be 30 seconds
            int maxDelay = Mathf.Min((int)(1000 * voiceActingClips[audioClipIndex].length), 20 * 1000);
            await Task.Delay(maxDelay);
            audioClipIndex++;
        }
        else
        {
            await Task.Delay(2000);
        }

        return audioClipIndex;
    }

    // this bad boy looks at the script from chatgpt and creates 3 string lists which define who is talking and what they are saying.
    // it will also convert bad attempts at instructions to narrator lines, because sometimes chatgpt gets a little creative with instructions.
    // like [rick and morty start fighting shrek] thats not a thing we can do so i just get the narrator to read it out.

    // this is also where we identify that ai images we need to generate. thats the reference string inputs
    public List<string>[] ProcessDialogFromLines(ref string[] outputLines, ref string nameOfAiGeneratedCharacter, ref string nameOfAiGeneratedDimension)
    {


        var voiceModelUUIDs = new List<string>();
        var characterNames = new List<string>();
        var textsToSpeak = new List<string>();



        for (int i = 0; i < outputLines.Length; i++)
        {
            string line = outputLines[i];

            string lowerLine = line.ToLower();

            // if its a dialog line
            if (line.Contains(":"))
            {
                //get which character is talking and add their dialog to the lists
                CharacterInfo talkingCharacter = GetWhosTalking(lowerLine);
                if (talkingCharacter != null)
                {

                    // if a character is used that isnt in the roster then default guy will be chosen
                    // this is returned as a ref variable so the main thing manager knows that it needs to generate images.
                    if (talkingCharacter == defaultGuy)
                    {
                        Debug.Log("found character " + defaultGuy.name);
                        nameOfAiGeneratedCharacter = defaultGuy.name;
                    }

                    voiceModelUUIDs.Add(talkingCharacter.fakeYouUUID);
                    characterNames.Add(talkingCharacter.name);
                    // Ensures the character's name isn't the only thing on the line, prevents a potential error
                    if (talkingCharacter.name.Length + 2 <= line.Length)
                    {
                        textsToSpeak.Add(line.Substring(talkingCharacter.name.Length + 2));
                    }

                }

            }
            else if (line.Contains("["))
            {

                CharacterInfo character1 = null;
                CharacterInfo character2 = null;

                GetCharacters(lowerLine, ref character1, ref character2);

                // if there is no character in the direction its immidiately invalid
                if (character1 != null)
                {
                    GameObject location = null;
                    GameObject lookAt = null;

                    GetLocation(lowerLine, ref location, ref lookAt);

                    //line is a walks to direction, and there is a location or a character2
                    if (lowerLine.Contains("walks to ") && (location != null || character2 != null || lowerLine.Contains("workbench")))
                    {
                        // then its a valid direction
                        continue;
                    }
                    else if (lowerLine.Contains("enter the portal to") ||
                    lowerLine.Contains("enter a portal to") ||
                    lowerLine.Contains("enter portal to"))
                    {
                        // this is a mad autistic way of doing this but i cant be fucked changin it
                        if (lowerLine.Contains("yard"))
                        {
                        }
                        else if (lowerLine.Contains("garage"))
                        {
                        }
                        else if (lowerLine.Contains("bikinibottom") || lowerLine.Contains("bikini bottom"))
                        {
                        }
                        else if (lowerLine.Contains("simpsonshouse") || lowerLine.Contains("simpsons"))
                        {
                        }
                        else if (lowerLine.Contains("shreksswamp") || lowerLine.Contains("shreks"))
                        {
                        }
                        else if (lowerLine.Contains("star wars cantina") || lowerLine.Contains("star wars") ||
                        lowerLine.Contains("cantina") || lowerLine.Contains("starwars"))
                        {
                        }
                        else if (lowerLine.Contains("alley"))
                        {
                        }
                        else if (lowerLine.Contains("void"))
                        {
                        }
                        else
                        {
                            //update location text
                            // get the text after rick and morty enter the portal to _______

                            int index = lowerLine.IndexOf("portal to ");

                            // If "portal to" exists in the original string
                            if (index != -1)
                            {
                                // Extract the part of the string after "portal to"
                                string destination = lowerLine.Substring(index + "portal to ".Length);
                                // remove final ]
                                nameOfAiGeneratedDimension = destination.Substring(0, destination.Length - 1);
                            }
                            else
                            {
                                // idk how it can not have "portal to" in it but in that case just chuck the whole bitch in the ai generated thing.
                                nameOfAiGeneratedDimension = lowerLine;

                            }

                            Debug.Log("found dimension: " + nameOfAiGeneratedDimension);
                        }



                        // also a valid direction
                        continue;
                    }

                }

                // if you got to this one then this means that its an invalid action, 
                // we will convert it to a narration line.
                string newline = "Narrator: " + line.Replace("[", "").Replace("]", "");
                outputLines[i] = newline;
                // now that weve reset the new line re run it and it should be detected as dialog.
                i -= 1;
                continue;
            }
        }

        return new List<string>[] { voiceModelUUIDs, characterNames, textsToSpeak };

    }



    // instructs a character to walk to a gameobeject. and waits until they get there
    public async Task CharacterWalksToLocation(MyCharacterController character, GameObject targetPosition, GameObject lookAt)
    {
        if (targetPosition == null)
        {
            return;
        }
        await character.MoveTowardsPositionAsync(targetPosition.transform.position, 2f);
        if (lookAt != null)
        {
            character.LookAtTarget(targetPosition);
        }
    }

    // same as above but it walks to another character and they both look at each other
    public async Task CharacterWalksToCharacter(MyCharacterController character, MyCharacterController targetCharacter)
    {
        await character.MoveTowardsPositionAsync(targetCharacter.gameObject.transform.position, 2f);
        character.LookAtTarget(targetCharacter.gameObject);
        targetCharacter.LookAtTarget(character.gameObject);
    }

    // rick and morty enter a new dimension, this involves:
    // starting the portal
    // have rick and morty walk into the portal 
    // reset the cameras target group.
    // sets the current dimension to the new dimension.
    // open a portal at the new dimension.
    // teleport rick and morty to the new dimension.
    // rick and morty walk to center stage. 
    public async Task RickAndMortyEnterPortal(string lowerLine)
    {

        // if going into the void set the reflection probe to active so it looks all cool
        if (lowerLine.Contains("void"))
        {
            //turn on reflection probe
            reflectionProbe.gameObject.SetActive(true);

        }

        // currentPortalController.OpenPortal();
        currentDimension.portalController.OpenPortal();

        //wait for the portal to open
        await Task.Delay(TimeSpan.FromSeconds(0.3f));


        // reset the target group to be rick and morty
        ResetTargetGroup();
        AddCharacterTargetGroup(characterList[0]);
        AddCharacterTargetGroup(characterList[1]);

        foreach (CharacterInfo character in characterList)
        {
            character.hasTalkedThisDimension = false;
        }

        characterList[0].hasTalkedThisDimension = true;
        characterList[1].hasTalkedThisDimension = true;



        await rick.MoveTowardsPositionAsync(currentDimension.portalLocation.gameObject.transform.position, 1f);
        RickRenderer.SetActive(false);
        await morty.MoveTowardsPositionAsync(currentDimension.portalLocation.gameObject.transform.position, 1f);


        MortyRenderer.SetActive(false);

        previousDimension = currentDimension;

        RenderSettings.fog = false;

        if (lowerLine.Contains("yard"))
        {
            currentDimension = FrontYard;
        }
        else if (lowerLine.Contains("garage"))
        {
            currentDimension = Garage;
        }
        else if (lowerLine.Contains("bikinibottom") || lowerLine.Contains("bikini bottom"))
        {
            currentDimension = BikiniBottom;
            RenderSettings.fog = true;
        }
        else if (lowerLine.Contains("simpsonshouse") || lowerLine.Contains("simpsons"))
        {
            currentDimension = SimpsonsHouse;
        }
        else if (lowerLine.Contains("shreksswamp") || lowerLine.Contains("shreks"))
        {
            currentDimension = ShreksSwamp;
        }
        else if (lowerLine.Contains("star wars cantina") || lowerLine.Contains("star wars") ||
        lowerLine.Contains("cantina") || lowerLine.Contains("starwars"))
        {
            currentDimension = StarWars;
        }
        else if (lowerLine.Contains("alley"))
        {
            currentDimension = BackAlley;
        }
        else if (lowerLine.Contains("void"))
        {
            currentDimension = CodeBulletsVoid;
        }
        else
        {
            currentDimension = DefaultDimension;
            //update location text
            // get the text after rick and morty enter the portal to _______

            int index = lowerLine.IndexOf("portal to ");

            // If "portal to" exists in the original string
            if (index != -1)
            {
                // Extract the part of the string after "portal to"
                string destination = lowerLine.Substring(index + "portal to ".Length);


                DimensionText.text = "[todo: make " + destination;
            }
            else
            {
                DimensionText.text = "[todo: make Dimension]";
            }
        }



        rick.TeleportTo(currentDimension.portalLocation.transform.position);
        morty.TeleportTo(currentDimension.portalLocation.transform.position);


        previousDimension.actualCamera.SetActive(false);
        currentDimension.actualCamera.SetActive(true);

        previousDimension.virtualCamera.SetActive(false);
        currentDimension.virtualCamera.SetActive(true);

        currentDimension.portalController.ClosePortal();
        // wait for the portal to open before rick and morty step out
        await Task.Delay(TimeSpan.FromSeconds(0.7f));
        RickRenderer.SetActive(true);
        MortyRenderer.SetActive(true);

        rick.MoveTowardsPosition(currentDimension.centerStage1.gameObject.transform.position, 0f);
        rick.LookAtTarget(currentDimension.actualCamera.gameObject);
        await morty.MoveTowardsPositionAsync(currentDimension.centerStage2.gameObject.transform.position, 0f);
        morty.LookAtTarget(currentDimension.actualCamera.gameObject);
        // if going into the void set the reflection probe to active so it looks all cool
        if (!lowerLine.Contains("void"))
        {
            //turn on reflection probe
            reflectionProbe.gameObject.SetActive(false);

        }
    }


}
