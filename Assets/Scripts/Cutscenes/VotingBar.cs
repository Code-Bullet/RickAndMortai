using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// smoothly changes the width of a vote bar to a desired percentage
// cbf properly commenting so this is all you get.

public class BarWidthController : MonoBehaviour
{
    public RectTransform targetRectTransform;
    public float fillPercentage = 0.5f;
    private Vector3 targetScale;
    private float lerpTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize targetScale to the initial local scale
        targetScale = transform.localScale;
    }

    void Update()
    {
        // // Check for spacebar press
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     // Generate a random fill percentage between 0.05 and 1.0
        //     float randomFill = Random.Range(0.05f, 1.0f);

        //     // Update the fill percentage
        //     SetFillPercentage(randomFill);
        // }
    }
    public void ResetBar()
    {
        transform.localScale = new Vector3(0.05f, transform.localScale.y, transform.localScale.z);
    }

    public void SetFillPercentage(float decimalValue)
    {
        Transform transform = GetComponent<Transform>();
        decimalValue = Mathf.Clamp(decimalValue, 0.05f, 1.0f);

        // Create a new scale vector with the desired x scale
        targetScale = new Vector3(decimalValue, transform.localScale.y, transform.localScale.z);

        // Stop any ongoing coroutine to smoothly change the scale
        StopCoroutine("LerpScale");

        // Start a new coroutine to smoothly change the scale
        StartCoroutine("LerpScale");
    }

    IEnumerator LerpScale()
    {
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / lerpTime);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
