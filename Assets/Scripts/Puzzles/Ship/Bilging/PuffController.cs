using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuffController : MonoBehaviour
{
    public bool isPuffed = false;
    Animator puffAnimator;

    public bool isLerping = false;
    float timeElapsed = 0;
    float lerpDuration = 0.5f;
    Vector3 startTransform = new Vector3(0f, 0f, 0f);
    Vector3 endTransform = new Vector3(0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        puffAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        puffAnimator.SetBool("isPuffed", isPuffed);

        if (isPuffed)
        {
            lerpToScale();
        }
    }

    public void TransformLerp(Vector3 newTransform, float zLerpDuration = 1.0f)
    {
        isLerping = true;
        timeElapsed = 0;
        lerpDuration = zLerpDuration;
        startTransform = gameObject.transform.localScale;
        endTransform = newTransform;
    }

    void lerpToScale()
    {
        if (timeElapsed > lerpDuration)
        {
            gameObject.transform.localScale = endTransform;
            isLerping = false;
        }

        gameObject.transform.localScale = Vector3.Lerp(startTransform, endTransform, timeElapsed / lerpDuration);

        timeElapsed += Time.deltaTime;
    }
}
