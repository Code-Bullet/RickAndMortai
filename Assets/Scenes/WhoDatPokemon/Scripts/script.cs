using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class script : MonoBehaviour
{
    public GameObject character;
    public TMP_Text text;

    enum State {
        WHOS_THAT_GUY,
        REVEAL
    }

    private State state;

    // Start is called before the first frame update
    async void Awake()
    {
        var silhouette = character.GetComponent<SilhouetteController>();

        // 1. Who's that character?
        text.text = "Who's that character??";
        silhouette.SetHiding(true);
        character.GetComponent<YouSpinMeRightRound>().isRotating = true;

        // 2. Character moves and stuff, wait 500ms
        await Task.Delay(2000);

        // 3. Reveal - it's <xyz>
        var aiHeadRigger = character.GetComponent<AIHeadRigger>();
        text.text = $"It's\n{aiHeadRigger.characterKey}";
        silhouette.SetHiding(false);
        character.GetComponent<MyCharacterController>().StartTalking();
        character.GetComponent<YouSpinMeRightRound>().isRotating = false;
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
