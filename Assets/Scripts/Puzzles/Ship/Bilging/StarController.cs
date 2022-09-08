using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{

    public float fillLevel = 0.0f;
    public int starIndex = 0;

    public float rotationSpeed = 0.2f;

    bool levelFinished = false;
    float timeElapsed = 0;
    float lerpDuration = 0.5f;
    Vector3 startPosition = new Vector3(0f, 0f, 0f);
    Vector3 endPosition = new Vector3(0f, 0f, 0f);

    Match3Game gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("Match3Manager").GetComponent<Match3Game>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed));

        float fillMin = (gameController.currentLevelProgressCap/gameController.currentLevel)*starIndex;
        float starAmount = (gameController.currentLevelProgressCap / gameController.currentLevel);
        if(gameController.currentLevelProgress > fillMin)
        {
            fillLevel = (gameController.currentLevelProgress - fillMin) / starAmount;
        }

        gameObject.GetComponent<Renderer>().material.SetFloat("_FillLevel", fillLevel);

        if (levelFinished)
        {
            lerpToPosition();
        }
    }

    public void SetLerp(Vector3 newPosition, float zLerpDuration = 0.5f)
    {
        levelFinished = true;
        timeElapsed = 0;
        lerpDuration = zLerpDuration;
        startPosition = gameObject.transform.position;
        endPosition = newPosition;

        rotationSpeed = 1.0f;
    }

    void lerpToPosition()
    {
        if (timeElapsed > lerpDuration)
        {
            gameObject.transform.position = endPosition;
            levelFinished = false;
        }
        else
        {
            gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
        }
    }
}
