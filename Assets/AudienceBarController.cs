using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        SetVal(50);
    }

    public void SetVal(float x)
    {
        Debug.Log($"SetVal {x}");
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

    private void LateUpdate()
    {
        setFill(val / 100);

        // emojii position
        float ymin = 22.5f;
        float ymax = 156f;
        float y = ymin + (val / 100) * (ymax - ymin);
        var t = emojiiContainer.GetComponent<RectTransform>();
        t.localPosition = new Vector3(402.1f, y, t.position.z);

        int i = 0;

        if (val <= 100) i = 4;
        if (val <= 80) i = 3;
        if (val <= 60) i = 2;
        if (val <= 40) i = 1;
        if (val <= 20) i = 0;

        reactImg.sprite = reacts[i];
    }
}
