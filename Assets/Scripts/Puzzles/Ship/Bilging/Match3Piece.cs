using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3Piece : MonoBehaviour
{
    [SerializeField] Vector3 pieceRotationSpeed = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] Vector3 worldPosition = new Vector3(0.0f, 0.0f, 0.0f);

    [SerializeField] int[] boardIndices = new int[] { 0, 0 }; //x, y; row, col;
    [SerializeField] public bool isClickable = true;
    [SerializeField] public bool isSwappable = true;

    public bool isMoving = false;
    [SerializeField] bool doesRotate = true;
    float timeElapsed = 0;
    float lerpDuration = 0.5f;
    Vector3 startPosition = new Vector3(0f, 0f, 0f);
    Vector3 middlePosition = new Vector3(0f, 0f, 0f);
    Vector3 endPosition = new Vector3(0f, 0f, 0f);

    bool isSelfDestruct = false;
    
    public int[] BoardIndices
    {
        get { return boardIndices; }
        set {
            boardIndices = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pieceRotationSpeed = new Vector3(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f));
    }

    // Update is called once per frame
    void Update()
    {
        if (doesRotate)
        {
            //rotate the game piece
            gameObject.transform.Rotate(pieceRotationSpeed);
        }

        //slow spin
        pieceRotationSpeed.x = Mathf.Clamp(pieceRotationSpeed.x - 0.001f, 0.01f, 5);
        pieceRotationSpeed.y = Mathf.Clamp(pieceRotationSpeed.y - 0.001f, 0.01f, 5);
        pieceRotationSpeed.z = Mathf.Clamp(pieceRotationSpeed.z - 0.001f, 0.01f, 5);

        if (isMoving){
            lerpToPosition();
        }
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public void SetLerp(Vector3 newPosition, float zLerpDuration = 0.5f, string action = "")
    {
        isMoving = true;
        timeElapsed = 0;
        lerpDuration = zLerpDuration;
        startPosition = gameObject.transform.position;
        endPosition = newPosition;

        pieceRotationSpeed = new Vector3(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f));

        switch (action)
        {
            case "destroy":
                isSelfDestruct = true;
                break;
            default:
                //do nothing
                break;
        }
    }

    public void SetLerp(Vector3 newPosition, Vector3 modifierPosition, float zLerpDuration = 0.5f, string action = "")
    {
        isMoving = true;
        timeElapsed = 0;
        lerpDuration = zLerpDuration;
        startPosition = gameObject.transform.position;
        endPosition = newPosition;
        middlePosition = findVectorCenter(startPosition, endPosition) + modifierPosition;

        pieceRotationSpeed = new Vector3(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f));

        switch (action)
        {
            case "destroy":
                isSelfDestruct = true;
                break;
            default:
                //do nothing
                break;
        }
    }

    void lerpToPosition()
    {
        if(timeElapsed > lerpDuration)
        {
            gameObject.transform.position = endPosition;
            isMoving = false;

            if (isSelfDestruct)
            {
                SelfDestruct();
            }
        }

        float halfTime = lerpDuration / 2;

        if(timeElapsed <= halfTime && middlePosition != Vector3.zero)
        {
            gameObject.transform.position = Vector3.Lerp(startPosition, middlePosition, timeElapsed / halfTime);
        }
        else
        {
            if(middlePosition == Vector3.zero)
            {
                gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / lerpDuration);
            }
            else
            {
                gameObject.transform.position = Vector3.Lerp(middlePosition, endPosition, timeElapsed / lerpDuration);
            }
        }

        timeElapsed += Time.deltaTime;
    }

    Vector3 findVectorCenter(Vector3 vector1, Vector3 vector2)
    {
        float vector1X = vector1.x;
        float vector2X = vector2.x;

        float vector1Y = vector1.y;
        float vector2Y = vector2.y;

        float vector1Z = vector1.z;
        float vector2Z = vector2.z;

        return new Vector3(
            (vector1X + vector2X) / 2,
            (vector1Y + vector2Y) / 2,
            (vector1Z + vector2Z) / 2
        );
    }

}
