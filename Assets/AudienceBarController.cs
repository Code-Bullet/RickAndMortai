using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
//[ExecuteAlways]
public class AudienceBarController : MonoBehaviour
{
    [Range(0, 100)]
    public float val;

    public GameObject fill;
    public GameObject emojiiContainer;
    public Sprite[] reacts;
    public Image reactImg;
    public bool showText;
    public GameObject label;

    void Awake()
    {
        SetVal(50);
    }

    public void SetVal(float x)
    {
        //Debug.Log($"SetVal {x}");
        val = x;
    }

    // x = 0.5 is 50% full.
    private void setFill(float x)
    {
        // range: [244 -> 2.5]
        float dist = 244f - 2.5f;
        float v = 244 - (dist * (x));

        var t = fill.GetComponent<RectTransform>();
        t.offsetMax = new Vector2(-v, t.offsetMax.y);
    }

    private void Update()
    {
        // Fill the "meter".
        setFill(val / 100);

        // Move the emoji to match the top of the meter.
        float ymin = 22.5f;
        float ymax = 156f;
        float y = ymin + (val / 100) * (ymax - ymin);
        var t = emojiiContainer.GetComponent<RectTransform>();
        t.localPosition = new Vector3(402.1f, y, t.position.z);

        // Swap the bullet react face based on some thresholds (it's over 9000!!!!!).
        int i = 0;

        if (val <= 100) i = 4;
        if (val <= 80) i = 3;
        if (val <= 60) i = 2;
        if (val <= 40) i = 1;
        if (val <= 20) i = 0;

        reactImg.sprite = reacts[i];

        // Update the text label.
        if(label != null)
        {
            TMP_Text tm = label.GetComponentInChildren<TMP_Text>();
            
            if (tm != null)
            {
                tm.text = $"{Mathf.Floor(val)}%";
            }
        }
    }
}
