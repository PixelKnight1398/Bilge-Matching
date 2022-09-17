using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3Piece : MonoBehaviour
{
    Match3Game GameController;

    [SerializeField] Vector3 pieceRotationSpeed = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] Vector3 worldPosition = new Vector3(0.0f, 0.0f, 0.0f);

    [SerializeField] int[] boardIndices = new int[] { 0, 0 }; //y, x; row, col;
    [SerializeField] string pieceType;
    [SerializeField] public bool isClickable = true;
    [SerializeField] public bool isSwappable = true;

    public bool isMatched = false;
    public bool isMoving = false;
    [SerializeField] bool doesRotate = true;
    float timeElapsed = 0;
    float lerpDuration = 0.5f;
    Vector3 startPosition = new Vector3(0f, 0f, 0f);
    Vector3 middlePosition = new Vector3(0f, 0f, 0f);
    Vector3 endPosition = new Vector3(0f, 0f, 0f);

    bool isSelfDestruct = false;

    [SerializeField] bool isLogging = true;
    
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
        pieceType = gameObject.transform.name.Substring(0, 6);
        GameController = GameObject.Find("Match3Manager").GetComponent<Match3Game>();

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

        if (isMoving)
        {
            lerpToPosition();
        }
        else
        {
            SetPieceMatchStatus();
        }

    }

    public void SetPieceMatchStatus()
    {
        //vertical and horizontal match counts
        int[] matchCounts = new int[4]; //up, right, down, left (clockwise)

        //check in each direction 2 spots for pieces

        int checkIncrement = 1;

        matchCounts[0] += CheckCoordsMatch(0, checkIncrement) ? 1 : 0;
        matchCounts[1] += CheckCoordsMatch(1, checkIncrement) ? 1 : 0;
        matchCounts[2] += CheckCoordsMatch(0, (checkIncrement * -1)) ? 1 : 0;
        matchCounts[3] += CheckCoordsMatch(1, (checkIncrement * -1)) ? 1 : 0;

        //Debug.Log("Up 1: " + matchCounts[0]);
        //Debug.Log("Right 1: " + matchCounts[1]);
        //Debug.Log("Down 1: " + matchCounts[2]);
        //Debug.Log("Left 1: " + matchCounts[3]);

        checkIncrement = 2;

        matchCounts[0] += (CheckCoordsMatch(0, checkIncrement) && matchCounts[0] > 0) ? 1 : 0;
        matchCounts[1] += (CheckCoordsMatch(1, checkIncrement) && matchCounts[1] > 0) ? 1 : 0;
        matchCounts[2] += (CheckCoordsMatch(0, (checkIncrement * -1)) && matchCounts[2] > 0) ? 1 : 0;
        matchCounts[3] += (CheckCoordsMatch(1, (checkIncrement * -1)) && matchCounts[3] > 0) ? 1 : 0;

        //Debug.Log("Up 2: " + matchCounts[0]);
        //Debug.Log("Right 2: " + matchCounts[1]);
        //Debug.Log("Down 2: " + matchCounts[2]);
        //Debug.Log("Left 2: " + matchCounts[3]);

        //if up + down or left + right are either greater than 2 pieces then this piece is a matching piece
        if ((matchCounts[0] + matchCounts[2]) >= 2 || (matchCounts[1] + matchCounts[3]) >= 2)
        {
            isMatched = true;
        }
    }

    bool CheckCoordsMatch(int affectedIndex, int affectIncrement)
    {
        int boardAxisLimit = (affectedIndex == 0) ? GameController.boardHeight : GameController.boardWidth;

        if (isLogging)
        {
            Debug.Log("BoardIndex: " + affectedIndex);
            Debug.Log("BoardIndexValue: " + BoardIndices[affectedIndex]);
            Debug.Log("Increment: " + affectIncrement);
            Debug.Log("BoardIndex + affectIncrement: " + (BoardIndices[affectedIndex] + affectIncrement));
            Debug.Log("BoardAxisLimit: " + boardAxisLimit);
        }

        if(InBounds(BoardIndices[affectedIndex]+affectIncrement, boardAxisLimit))
        {
            int[] nextIndices = new int[] { BoardIndices[0], BoardIndices[1] };
            nextIndices[affectedIndex] += affectIncrement;
            Match3Piece checkPieceController;
            string checkPieceType = "";
            if (GameController.gameBoardArray[nextIndices[0], nextIndices[1]] != null)
            {
                checkPieceController = GameController.gameBoardArray[nextIndices[0], nextIndices[1]].GetComponent<Match3Piece>();
                checkPieceType = checkPieceController.pieceType;

                if (checkPieceType == pieceType && !checkPieceController.isMoving)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool InBounds(int boardCoord, int limit)
    {
        if (boardCoord >= 0 && boardCoord < limit)
        {
            return true;
        }
        return false;
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
