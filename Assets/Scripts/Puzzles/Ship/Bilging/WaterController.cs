using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{

    Match3Game gameController;

    Vector3 currentTransform;

    [SerializeField] public float maxWaterSpeed = 0.15f;
    [SerializeField] float minWaterSpeed = -5.0f;
    [SerializeField] public float currentWaterSpeed = 0.0f;

    public bool isLerping = false;
    float timeElapsed = 0;
    float lerpDuration = 0.5f;
    Vector3 startPosition = new Vector3(0f, 0f, 0f);
    Vector3 endPosition = new Vector3(0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("Match3Manager").GetComponent<Match3Game>();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentWaterSpeed < maxWaterSpeed)
        {
            currentWaterSpeed += maxWaterSpeed / 500;
        }
        if(currentWaterSpeed < minWaterSpeed)
        {
            currentWaterSpeed = minWaterSpeed;
        }

        currentTransform = transform.position;
        if (gameController.currentGameState == "PLAYING")
        {
            currentTransform.y += currentWaterSpeed / 1000;
        }

        transform.position = currentTransform;

        if(transform.position.y < -2.7f)
        {
            transform.position = new Vector3(transform.position.x, -2.7f, transform.position.z);
        }
        if(transform.position.y >= 6.15f)
        {
            if (gameController.currentGameState == "PLAYING")
            {
                gameController.ClearBoard("LOSE");
            }

            currentWaterSpeed = 0.0f;
            transform.position = new Vector3(transform.position.x, 6.10f, transform.position.z);
        }

        if (isLerping)
        {
            lerpToPosition();
        }
    }

    public void PumpWater()
    {
        currentWaterSpeed -= 1f;
    }

    public void SetLerp(Vector3 newPosition, float zLerpDuration = 1.0f)
    {
        isLerping = true;
        timeElapsed = 0;
        lerpDuration = zLerpDuration;
        startPosition = gameObject.transform.position;
        endPosition = newPosition;
    }

    void lerpToPosition()
    {
        if (timeElapsed > lerpDuration)
        {
            gameObject.transform.position = endPosition;
            isLerping = false;
        }

        gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / lerpDuration);

        timeElapsed += Time.deltaTime;
    }
}
