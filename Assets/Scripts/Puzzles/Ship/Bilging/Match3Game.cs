using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3Game : MonoBehaviour
{

    [SerializeField] Camera gameCamera;
    bool isClicking = false;
    Vector3 clickStartPosition;

    [Space(10)]
    [Header("Board Controls")]
    [SerializeField] public int boardWidth = 0; //game width in pieces
    [SerializeField] public int boardHeight = 0; //game height in pieces
    [SerializeField] public float pieceWidth = 0.0f;
    [SerializeField] public float pieceHeight = 0.0f;
    [SerializeField] public float piecePadding = 0.0f;
    [SerializeField] float spawnBuffer = 0.0f;

    [Space(5)]
    public GameObject[,] gameBoardArray;
    [SerializeField] List<GameObject> boardPieces = new List<GameObject>(); //the possible pieces to be used in the game board
    [SerializeField] List<GameObject> specialBoardPieces = new List<GameObject>(); //puff, jelly, crabby
    [SerializeField] public GameObject gameBoardObject;
    [SerializeField] GameObject piecesContainer;
    [SerializeField] GameObject pieceSelector;
    [SerializeField] GameObject popEffectObject;
    [SerializeField] GameObject particlesContainer;
    public GameObject[,] popEffectArray;
    [SerializeField] GameObject starContainer;
    [SerializeField] GameObject starObject;
    [SerializeField] List<GameObject> starList = new List<GameObject>();

    [Tooltip("Add all bilge pumps with animation which are contained within the scene.  Add the tiny bilge pump indicator to the last slot only.")]
    [SerializeField] GameObject[] bilgePumps = new GameObject[4];

    [Space(10)]
    [Header("Audio")]
    [SerializeField] AudioSource gameStartSound = new AudioSource();
    [SerializeField] AudioSource gameEndSound = new AudioSource();
    [SerializeField] AudioSource Match3Sound = new AudioSource();
    [SerializeField] AudioSource Match4Sound = new AudioSource();
    [SerializeField] AudioSource Match5Sound = new AudioSource();
    [SerializeField] AudioSource Match6Sound = new AudioSource();
    [SerializeField] AudioSource MatchCombo1Sound = new AudioSource();
    [SerializeField] AudioSource MatchCombo2Sound = new AudioSource();
    [SerializeField] AudioSource SwapPieceSound = new AudioSource();
    [SerializeField] AudioSource CrabbySound = new AudioSource();

    [Space(10)]
    [Header("Game Stats")]
    public int currentLevel = 1;
    public int currentLevelProgress = 0;
    public int currentLevelProgressCap = 200;
    public int numPossiblePieces = 5;
    public bool puffEnabled = false;
    public bool crabbyEnabled = false;
    public bool jellyEnabled = false;
    public int comboState = 0;
    public bool holdsCombo = false;
    public string currentGameState = "LOADING"; //game states; INIT, PLAYING, CLEAR

    public float pumpSpeed = 0.0f;
    [SerializeField] GameObject waterObject;

    int[] selectedPiece = new int[] { -1, -1 };

    [SerializeField] LayerMask inputRaycastLayerMask;

    [SerializeField] enum newPieceDirection
    {
        Down,
        Left,
        Up,
        Right
    }
    int[,] pieceDirectionModifier = new int[,]
    {
        { -1, 0 },
        { 0, -1 },
        { 1, 0 },
        { 0, 1 },
    };
    static newPieceDirection BoardDirection = newPieceDirection.Up;

    bool checkingPatterns = false;
    bool fixBoard = false;

    // Start is called before the first frame update
    void Start()
    {
        pieceSelector.transform.localScale = new Vector3(pieceSelector.transform.localScale.x * pieceWidth * 1.3f, pieceSelector.transform.localScale.y * pieceHeight * 1.3f, 0);

        gameBoardArray = new GameObject[boardHeight, boardWidth];
        popEffectArray = new GameObject[boardHeight, boardWidth];

        currentGameState = "INIT";
    }

    //generates new board and stars
    void InitBoard()
    {
        //clear piece container
        foreach (Transform child in piecesContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //initialize pieces
        for (int j = 0; j < boardWidth; j++)
        {
            for (int i = 0; i < boardHeight; i++)
            {
                float newXPosition = gameBoardObject.transform.position.x + (j * pieceWidth) + (j * piecePadding);
                float newYPosition = gameBoardObject.transform.position.y + (i * pieceHeight) + (i * piecePadding);

                float spawnXPosition = newXPosition - (pieceDirectionModifier[(int)BoardDirection, 1] * spawnBuffer) - (pieceDirectionModifier[(int)BoardDirection, 1] * (boardWidth * pieceWidth));
                float spawnYPosition = newYPosition - (pieceDirectionModifier[(int)BoardDirection, 0] * spawnBuffer) - (pieceDirectionModifier[(int)BoardDirection, 0] * (boardHeight * pieceHeight));

                int bilgePiece = UnityEngine.Random.Range(0, numPossiblePieces);

                gameBoardArray[i, j] = Instantiate(boardPieces[bilgePiece], new Vector3(spawnXPosition, spawnYPosition, gameBoardObject.transform.position.z), Quaternion.identity, piecesContainer.transform);
                gameBoardArray[i, j].transform.localScale = new Vector3(pieceWidth / 2, pieceHeight / 2, pieceWidth / 2);
                //gameBoardArray[i, j].transform.name = "bilge" + bilgePiece.ToString() + "_" + i.ToString() + "_" + j.ToString();
                gameBoardArray[i, j].gameObject.GetComponent<Match3Piece>().BoardIndices = new int[] { i, j };

                popEffectArray[i, j] = Instantiate(popEffectObject, new Vector3(newXPosition, newYPosition, gameBoardObject.transform.position.z), Quaternion.identity, particlesContainer.transform);
            }
        }

        //clear matching pieces at beginning
        //fixed update checks for this variable and runs replacement function before board loads
        fixBoard = true;

        currentLevelProgress = 0;

        ResetStars();
    }

    //starts level by sending pieces up, playing start sounds, enabling play variables
    IEnumerator LoadLevel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //Your Function You Want to Call

        fixBoard = false;

        gameStartSound.Play();
        waterObject.GetComponent<WaterController>().SetLerp(new Vector3(2f, -1.75f, 8.5f));

        //send up pieces
        for (int j = 0; j < boardWidth; j++)
        {
            float lerpSpeed = UnityEngine.Random.Range(1.75f, 2.25f);
            for (int i = 0; i < boardHeight; i++)
            {
                float newXPosition = gameBoardObject.transform.position.x + (j * pieceWidth) + (j * piecePadding);
                float newYPosition = gameBoardObject.transform.position.y + (i * pieceHeight) + (i * piecePadding);

                if (gameBoardArray[i, j] == null)
                {
                    continue;
                }
                gameBoardArray[i, j].GetComponent<Match3Piece>().SetLerp(new Vector3(newXPosition, newYPosition, gameBoardObject.transform.position.z), lerpSpeed - (i * 0.1f));
            }
        }

        checkingPatterns = true;
        currentGameState = "PLAYING";
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentGameState)
        {
            case "INIT":

                break;
            case "LOADING":

                break;
            case "PLAYING":
                GetInput();

                CheckEmptyPieces();
                break;
            case "CLEAR":

                break;
            default:

                break;
        }
    }

    private void FixedUpdate()
    {

        bool boardEmpty = true;

        switch (currentGameState)
        {
            case "INIT":
                InitBoard();

                StartCoroutine(LoadLevel(1.0f));
                currentGameState = "LOADING";
                break;
            case "LOADING":

                if (fixBoard)
                {
                    ReplacePieces();
                }

                break;
            case "UI":

                break;
            case "PLAYING":
                if (pumpSpeed > 0)
                {
                    pumpSpeed -= 0.0001f;
                }
                break;
            case "CLEAR":
                //check if board empty yet

                boardEmpty = true;

                for (int i = 0; i < boardHeight; i++)
                {
                    for (int j = 0; j < boardWidth; j++)
                    {
                        if (gameBoardArray[i, j] == null)
                        {
                            continue;
                        }
                        if (gameBoardArray[i, j].transform.position.y < 7.9f)
                        {
                            boardEmpty = false;
                        }
                    }
                }

                if (boardEmpty)
                {
                    currentGameState = "UI";
                    gameObject.GetComponent<UIManager>().ShowLevelDialogue(currentLevel);
                }
                break;
            case "LOSE":
                //check if board empty yet

                boardEmpty = true;

                for (int i = 0; i < boardHeight; i++)
                {
                    for (int j = 0; j < boardWidth; j++)
                    {
                        if (gameBoardArray[i, j] == null)
                        {
                            continue;
                        }
                        if (gameBoardArray[i, j].transform.position.y < 7.9f)
                        {
                            boardEmpty = false;
                        }
                    }
                }

                if (boardEmpty)
                {
                    currentGameState = "UI";
                    gameObject.GetComponent<UIManager>().ShowLevelRetryDialogue();
                }
                break;
            default:

                break;
        }
    }

    void LateUpdate()
    {
        switch (currentGameState)
        {
            case "INIT":

                break;
            case "LOADING":

                break;
            case "PLAYING":
                if (checkingPatterns)
                {
                    //check patterns is going to look for patterns
                    //it's also going to check for crabs
                    CheckPatterns();
                }
                AddNewPieces();
                break;
            case "CLEAR":

                break;
            default:

                break;
        }
    }

    GameObject GetSpecialPiece()
    {
        int specialPieceIndex = -1;
        if (currentLevel >= 2 && currentLevel < 4)
        {
            specialPieceIndex = UnityEngine.Random.Range(0, 1);
        }
        else if (currentLevel >= 4 && currentLevel < 6)
        {
            specialPieceIndex = UnityEngine.Random.Range(0, 2);
        }
        else if (currentLevel >= 6)
        {
            specialPieceIndex = UnityEngine.Random.Range(0, 3);
        }
        if (specialPieceIndex != -1)
        {
            return specialBoardPieces[specialPieceIndex];
        }
        else
        {
            return null;
        }
    }

    void AddNewPieces()
    {
        switch (BoardDirection)
        {
            case newPieceDirection.Up:
                for (int j = 0; j < boardWidth; j++) {
                    for (int i = boardHeight - 1; i > -1; i--)
                    {
                        if (gameBoardArray[i, j] == null)
                        {
                            int bilgePiece = UnityEngine.Random.Range(0, numPossiblePieces);

                            float newXPosition = gameBoardObject.transform.position.x + (j * pieceWidth) + (j * piecePadding);
                            float newYPosition = gameBoardObject.transform.position.y + (i * pieceHeight) + (i * piecePadding);

                            float spawnXPosition = newXPosition - (pieceDirectionModifier[(int)BoardDirection, 1] * (boardWidth * pieceWidth));
                            //float spawnYPosition = newYPosition - (pieceDirectionModifier[(int)BoardDirection, 0] * spawnBuffer) - (pieceDirectionModifier[(int)BoardDirection, 0] * (boardHeight * pieceHeight));
                            float spawnYPosition = gameBoardObject.transform.position.y - ((((boardHeight - i) * pieceHeight) + ((boardHeight - i) * piecePadding)) * pieceHeight);

                            GameObject pieceSpawn;
                            //check special piece
                            if (UnityEngine.Random.Range(0, 100) > (99 - (currentLevel / 2)) && currentLevel >= 2)
                            {
                                pieceSpawn = GetSpecialPiece();
                                //pieceSpawn.transform.name = pieceSpawn.transform.name + "_" + i.ToString() + "_" + j.ToString();
                            }
                            else
                            {
                                pieceSpawn = boardPieces[bilgePiece];
                                //pieceSpawn.transform.name = "bilge" + bilgePiece.ToString() + "_" + i.ToString() + "_" + j.ToString();
                            }

                            //gameBoardObject.transform.position.y - (pieceHeight + piecePadding)

                            gameBoardArray[i, j] = Instantiate(
                                pieceSpawn,
                                new Vector3(spawnXPosition, spawnYPosition, gameBoardObject.transform.position.z),
                                Quaternion.identity,
                                piecesContainer.transform
                            );
                            //gameBoardArray[i, j].transform.name = pieceSpawn.transform.name;
                            gameBoardArray[i, j].transform.localScale = new Vector3(pieceWidth / 2, pieceHeight / 2, pieceWidth / 2);
                            gameBoardArray[i, j].GetComponent<Match3Piece>().BoardIndices = new int[] { i, j };
                            gameBoardArray[i, j].GetComponent<Match3Piece>().SetLerp(new Vector3(newXPosition, newYPosition, gameBoardObject.transform.position.z));
                        }
                    }
                }
                break;
            case newPieceDirection.Right:

                break;
            case newPieceDirection.Down:

                break;
            case newPieceDirection.Left:

                break;
        }
    }

    void CheckEmptyPieces() {
        //swap loop increments i <> j
        //loop from top down
        //if hits null store indices coords in array and continue down until piece hit
        //for each piece hit, assign respective null indices coords using incremental index for null indices array
        //remove piece and store it's coords in the null indices coords array so the remaining pieces can shift up

        List<int[]> nullIndicesStore = new List<int[]>();
        int storeAccessIndex = 0; //null indices store access index.  Determines which coords need to be assigned still from the store

        for(int j = 0; j < boardWidth; j++)
        {
            storeAccessIndex = 0;
            nullIndicesStore.Clear();
            for (int i = boardHeight-1; i > -1; i--)
            {
                if(gameBoardArray[i, j] == null)
                {
                    nullIndicesStore.Add(new int[] { i, j });
                }

                if(nullIndicesStore.Count == 0)
                {
                    continue;
                }

                if(i == 0)
                {
                    //Debug.Log("Piece resetting");
                }

                if(gameBoardArray[i, j] != null)
                {
                    int[] newCoords = nullIndicesStore[storeAccessIndex];
                    storeAccessIndex++;

                    gameBoardArray[i, j].GetComponent<Match3Piece>().BoardIndices = new int[] { newCoords[0], newCoords[1] };
                    //gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]].transform.name = gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]].transform.name.Substring(0, 6) + "_" + checkLocationIndices[0].ToString() + "_" + checkLocationIndices[1].ToString();
                    float newXPosition = gameBoardObject.transform.position.x + (newCoords[1] * pieceWidth) + (newCoords[1] * piecePadding);
                    float newYPosition = gameBoardObject.transform.position.y + (newCoords[0] * pieceHeight) + (newCoords[0] * piecePadding);
                    gameBoardArray[i, j].GetComponent<Match3Piece>().SetLerp(new Vector3(newXPosition, newYPosition, gameBoardObject.transform.position.z));

                    gameBoardArray[newCoords[0], newCoords[1]] = gameBoardArray[i, j];
                    gameBoardArray[i, j] = null;
                    nullIndicesStore.Add(new int[] { i, j });
                }
            }
        }

        nullIndicesStore.Clear();
        
        //for (int i = 0; i < boardHeight; i++)
        //{
        //    for (int j = 0; j < boardWidth; j++)
        //    {
        //        int[] checkLocationIndices = new int[] { i + pieceDirectionModifier[(int)BoardDirection, 0], j + pieceDirectionModifier[(int)BoardDirection, 1] };

        //        if (checkLocationIndices[0] < 0 || checkLocationIndices[0] > boardHeight - 1)
        //        {
        //            continue;
        //        }
        //        if (checkLocationIndices[1] < 0 || checkLocationIndices[1] > boardWidth - 1)
        //        {
        //            continue;
        //        }
        //        if (gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]] != null)
        //        {
        //            continue;
        //        }

        //        gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]] = gameBoardArray[i, j];
        //        gameBoardArray[i, j] = null;
        //        gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]].GetComponent<Match3Piece>().BoardIndices = new int[] { checkLocationIndices[0], checkLocationIndices[1] };
        //        //gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]].transform.name = gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]].transform.name.Substring(0, 6) + "_" + checkLocationIndices[0].ToString() + "_" + checkLocationIndices[1].ToString();
        //        float newXPosition = gameBoardObject.transform.position.x + (checkLocationIndices[1] * pieceWidth) + (checkLocationIndices[1] * piecePadding);
        //        float newYPosition = gameBoardObject.transform.position.y + (checkLocationIndices[0] * pieceHeight) + (checkLocationIndices[0] * piecePadding);
        //        gameBoardArray[checkLocationIndices[0], checkLocationIndices[1]].GetComponent<Match3Piece>().SetLerp(new Vector3(newXPosition, newYPosition, gameBoardObject.transform.position.z));
        //    }
        //}
    }

    void ReplacePieces()
    {
        List<List<int[]>> piecesToReplaceGroups = FindMatchingPieces();
        List<int[]> piecesToReplace;

        for (int j = 0; j < piecesToReplaceGroups.Count; j++)
        {
            piecesToReplace = piecesToReplaceGroups[j];
            for (int i = 0; i < piecesToReplace.Count; i++)
            {
                int replacementPiece = UnityEngine.Random.Range(0, numPossiblePieces);

                float newXPosition = gameBoardObject.transform.position.x + (piecesToReplace[i][1] * pieceWidth) + (piecesToReplace[i][1] * piecePadding);
                float newYPosition = gameBoardObject.transform.position.y + (piecesToReplace[i][0] * pieceHeight) + (piecesToReplace[i][0] * piecePadding);

                Destroy(gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]]);

                gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]] = Instantiate(
                    boardPieces[replacementPiece],
                    new Vector3(
                        gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]].transform.position.x,
                        gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]].transform.position.y,
                        gameBoardObject.transform.position.z
                    ),
                    Quaternion.identity,
                    piecesContainer.transform
                );
                //gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]].transform.name = "bilge" + replacementPiece.ToString() + "_" + piecesToReplace[i][0].ToString() + "_" + piecesToReplace[i][1].ToString();
                gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]].transform.localScale = new Vector3(pieceWidth / 2, pieceHeight / 2, pieceWidth / 2);
                gameBoardArray[piecesToReplace[i][0], piecesToReplace[i][1]].GetComponent<Match3Piece>().BoardIndices = new int[] { piecesToReplace[i][0], piecesToReplace[i][1] };
            }
        }
    }

    List<List<int[]>> FindMatchingPieces()
    {
        List<List<int[]>> matchPieceGroups = new List<List<int[]>>();
        for(var m = 0; m < boardPieces.Count; m++)
        {
            matchPieceGroups.Add(new List<int[]>());
        }
        //matchpieces groups should look like:
        //[ <- this is the group container
        //    [ <- this array level is a type
        //        [0,1], [0,2], [0,3] <- this is all the matching pieces of a type
        //    ],
        //]

        List<int[]> matchPieces = new List<int[]>();

        holdsCombo = false;

        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                if (gameBoardArray[i, j] == null)
                {
                    holdsCombo = true;
                    continue;
                }
                if (gameBoardArray[i, j].gameObject.GetComponent<Match3Piece>().isMoving)
                    holdsCombo = true;

                string currentPieceName = gameBoardArray[i, j].transform.name.Substring(0, 6);

                int matchPieceGroupIndex = 0;
                try
                {
                    matchPieceGroupIndex = int.Parse(currentPieceName.Substring(currentPieceName.Length - 1)) - 1;
                } catch(System.FormatException er1)
                {
                    //no piece number exists, don't worry about it
                    //Debug.LogError("No piece number");
                }

                if (gameBoardArray[i, j].GetComponent<Match3Piece>().isMatched)
                {
                    matchPieceGroups[matchPieceGroupIndex].Add(new int[] { i, j });
                    //matchPieces.Add(new int[] { i, j });
                }

                //FIND CRABS
                if (currentPieceName == "Crabby")
                {
                    //is crab

                    if (!gameBoardArray[i, j].GetComponent<CrabbyScript>().isSubmerged)
                    {
                        Combo(10);
                        CrabbySound.Play();
                        gameBoardArray[i, j].GetComponent<Match3Piece>().SetLerp(
                            new Vector3(gameBoardArray[i, j].transform.position.x + 10.0f, gameBoardArray[i, j].transform.position.y, gameBoardObject.transform.position.z),
                            3.0f,
                            "Destroy"
                        );
                        gameBoardArray[i, j] = null;
                    }
                }

                ////Debug.Log("Piece were on: " + i + ", " + j);
                ////Debug.Log(gameBoardArray[i, j].transform.name);
                //if (gameBoardArray[i, j] == null || gameBoardArray[i, j].gameObject.GetComponent<Match3Piece>().isMoving)
                //{
                //    continue;
                //}

                //bool skipPiece = false;
                //for (int q = 0; q < matchPieces.Count; q++)
                //{
                //    if (i == matchPieces[q][0] && j == matchPieces[q][1])
                //    {
                //        skipPiece = true;
                //    }
                //}
                //if (skipPiece)
                //{
                //    continue;
                //}

                ////Debug.Log("Piece is not null");
                //string currentPieceName = gameBoardArray[i, j].transform.name.Substring(0, 6);

                ////MATCH PATTERNS
                ////Debug.Log("Current piece name: " + currentPieceName);
                //List<int[]> tempHorizontalMatchPieces = new List<int[]>();
                //tempHorizontalMatchPieces.Add(new int[] { i, j });
                //List<int[]> tempVerticalMatchPieces = new List<int[]>();
                //tempVerticalMatchPieces.Add(new int[] { i, j });
                ////Debug.Log("Looping row width left");
                //for (int k = 1; k < boardWidth; k++)
                //{
                //    //Debug.Log("Current k: " + k);
                //    //Debug.Log("I: " + i + "; J: " + j + "; K: " + k + "; J-K: " + (j - k));
                //    if ((j - k) < 0 || gameBoardArray[i, j - k] == null)
                //    {
                //        //Debug.Log("J-K is null or less than 0");
                //        break;
                //    }
                //    //Debug.Log("J-K is not null");
                //    //Debug.Log(currentPieceName + " =? " + gameBoardArray[i, j - k].transform.name.Substring(0, 6));
                //    if (gameBoardArray[i, j - k].transform.name.Substring(0, 6) != currentPieceName)
                //    {
                //        //Debug.Log("Piece names do not match");
                //        break;
                //    }

                //    if (gameBoardArray[i, j - k].gameObject.GetComponent<Match3Piece>().isMoving)
                //    {
                //        //Debug.Log("Next piece moving");
                //        break;
                //    }
                //    //Debug.Log("Piece names match");
                //    tempHorizontalMatchPieces.Add(new int[] { i, j - k });
                //}

                ////Debug.Log("Looping row width right");
                //for (int k = 1; k < boardWidth; k++)
                //{
                //    //Debug.Log("Current k: " + k);
                //    //Debug.Log("I: " + i + "; J: " + j + "; K: " + k + "; J+K: " + (j + k));
                //    if ((j + k) > (boardWidth - 1) || gameBoardArray[i, j + k] == null)
                //    {
                //        //Debug.Log("J+K is null or more than board width");
                //        break;
                //    }
                //    //Debug.Log("J+K is not null");
                //    //Debug.Log(currentPieceName + " =? " + gameBoardArray[i, j + k].transform.name.Substring(0, 6));
                //    if (gameBoardArray[i, j + k].transform.name.Substring(0, 6) != currentPieceName)
                //    {
                //        //Debug.Log("Piece names do not match");
                //        break;
                //    }
                //    //Debug.Log("Piece names match");

                //    if (gameBoardArray[i, j + k].gameObject.GetComponent<Match3Piece>().isMoving)
                //    {
                //        //Debug.Log("Next piece moving");
                //        break;
                //    }

                //    tempHorizontalMatchPieces.Add(new int[] { i, j + k });
                //}

                ////Debug.Log("Looping row width right");
                //for (int k = 1; k < boardHeight; k++)
                //{
                //    //Debug.Log("Current k: " + k);
                //    //Debug.Log("I: " + i + "; J: " + j + "; K: " + k + "; J+K: " + (j + k));
                //    if ((i - k) < 0 || gameBoardArray[i - k, j] == null)
                //    {
                //        //Debug.Log("J+K is null or more than board width");
                //        break;
                //    }
                //    //Debug.Log("J+K is not null");
                //    //Debug.Log(currentPieceName + " =? " + gameBoardArray[i, j + k].transform.name.Substring(0, 6));
                //    if (gameBoardArray[i - k, j].transform.name.Substring(0, 6) != currentPieceName)
                //    {
                //        //Debug.Log("Piece names do not match");
                //        break;
                //    }
                //    //Debug.Log("Piece names match");

                //    if (gameBoardArray[i - k, j].gameObject.GetComponent<Match3Piece>().isMoving)
                //    {
                //        //Debug.Log("Next piece moving");
                //        break;
                //    }

                //    tempVerticalMatchPieces.Add(new int[] { i - k, j });
                //}

                ////Debug.Log("Looping row width right");
                //for (int k = 1; k < boardHeight; k++)
                //{
                //    //Debug.Log("Current k: " + k);
                //    //Debug.Log("I: " + i + "; J: " + j + "; K: " + k + "; J+K: " + (j + k));
                //    if ((i + k) > (boardHeight - 1) || gameBoardArray[i + k, j] == null)
                //    {
                //        //Debug.Log("J+K is null or more than board width");
                //        break;
                //    }
                //    //Debug.Log("J+K is not null");
                //    //Debug.Log(currentPieceName + " =? " + gameBoardArray[i, j + k].transform.name.Substring(0, 6));
                //    if (gameBoardArray[i + k, j].transform.name.Substring(0, 6) != currentPieceName)
                //    {
                //        //Debug.Log("Piece names do not match");
                //        break;
                //    }
                //    //Debug.Log("Piece names match");

                //    if (gameBoardArray[i + k, j].gameObject.GetComponent<Match3Piece>().isMoving)
                //    {
                //        //Debug.Log("Next piece moving");
                //        break;
                //    }

                //    tempVerticalMatchPieces.Add(new int[] { i + k, j });
                //}

                ////Debug.Log("Amount of matches for J=" + j + ": " + tempMatchPieces.Count);
                //if (tempHorizontalMatchPieces.Count > 2)
                //{
                //    for (int l = 0; l < tempHorizontalMatchPieces.Count; l++)
                //    {
                //        bool containsMatchAlready = false;
                //        //Debug.Log(matchPieces.Count);
                //        for (int q = 0; q < matchPieces.Count; q++)
                //        {
                //            //Debug.Log("(" + tempMatchPieces[l][0] + " =? " + matchPieces[q][0] + ") (" + tempMatchPieces[l][1] + " =? " + matchPieces[q][1] + ")");
                //            if (tempHorizontalMatchPieces[l][0] == matchPieces[q][0] && tempHorizontalMatchPieces[l][1] == matchPieces[q][1])
                //            {
                //                containsMatchAlready = true;
                //            }
                //        }
                //        if (!containsMatchAlready)
                //        {
                //            matchPieces.Add(tempHorizontalMatchPieces[l]);
                //        }
                //    }
                //}

                ////Debug.Log("Amount of matches for J=" + j + ": " + tempMatchPieces.Count);
                //if (tempVerticalMatchPieces.Count > 2)
                //{
                //    for (int l = 0; l < tempVerticalMatchPieces.Count; l++)
                //    {
                //        bool containsMatchAlready = false;
                //        //Debug.Log(matchPieces.Count);
                //        for (int q = 0; q < matchPieces.Count; q++)
                //        {
                //            //Debug.Log("(" + tempMatchPieces[l][0] + " =? " + matchPieces[q][0] + ") (" + tempMatchPieces[l][1] + " =? " + matchPieces[q][1] + ")");
                //            if (tempVerticalMatchPieces[l][0] == matchPieces[q][0] && tempVerticalMatchPieces[l][1] == matchPieces[q][1])
                //            {
                //                containsMatchAlready = true;
                //            }
                //        }
                //        if (!containsMatchAlready)
                //        {
                //            matchPieces.Add(tempVerticalMatchPieces[l]);
                //        }
                //    }
                //}
            }
        }

        if (holdsCombo == false)
        {
            comboState = 0;
        }

        return matchPieceGroups;
    }

    void CheckPatterns()
    {
        List<List<int[]>> matchPieceGroups = FindMatchingPieces();

        //Debug.Log("Match Pieces Count: " + matchPieces.Count);
        for (int q = 0; q < matchPieceGroups.Count; q++)
        {
            List<int[]> matchPieces = matchPieceGroups[q];
            if (matchPieces.Count > 2)
            {
                bool skipGroupMoving = false;
                for(int y = 0; y < matchPieces.Count; y++)
                {
                    if(gameBoardArray[matchPieces[y][0], matchPieces[y][1]].GetComponent<Match3Piece>().isMoving)
                    {
                        skipGroupMoving = true;
                    }
                }
                if (skipGroupMoving)
                    continue;
                for (int r = 0; r < matchPieces.Count; r++)
                {
                    //Debug.Log("I: " + matchPieces[r][0] + "; J: " + matchPieces[r][1] + ";");
                    GameObject tempMatchPiece = gameBoardArray[matchPieces[r][0], matchPieces[r][1]].gameObject;
                    //Debug.Log(tempMatchPiece.transform.name);
                    Match3Piece tempMatchPieceScript = tempMatchPiece.GetComponent<Match3Piece>();
                    //Debug.Log("Vertical Matches: " + (tempMatchPieceScript.matchCounts[0] + tempMatchPieceScript.matchCounts[2]));
                    //Debug.Log("Horizontal Matches: " + (tempMatchPieceScript.matchCounts[1] + tempMatchPieceScript.matchCounts[3]));

                    int[] deleteIndices = matchPieces[r];
                    if (gameBoardArray[deleteIndices[0], deleteIndices[1]] == null)
                    {
                        continue;
                    }
                    Destroy(gameBoardArray[deleteIndices[0], deleteIndices[1]].transform.gameObject);
                    gameBoardArray[deleteIndices[0], deleteIndices[1]] = null;

                    popEffectArray[deleteIndices[0], deleteIndices[1]].GetComponent<ParticleSystem>().Play();

                }

                switch (comboState)
                {
                    case 0:
                        if (matchPieces.Count == 3)
                        {
                            Match3Sound.Play();
                        }
                        else if (matchPieces.Count == 4)
                        {
                            Match4Sound.Play();
                        }
                        else if (matchPieces.Count == 5)
                        {
                            Match5Sound.Play();
                        }
                        else if (matchPieces.Count >= 6)
                        {
                            Match6Sound.Play();
                        }
                        comboState = 1;
                        break;
                    case 1:
                        MatchCombo1Sound.Play();
                        comboState = 2;
                        break;
                    case 2:
                        MatchCombo1Sound.Play();
                        break;
                }
                Combo(Mathf.RoundToInt(matchPieces.Count / 2));
            }
        }
    }

    void Combo(int increment)
    {
        currentLevelProgress += increment;
        if (pumpSpeed < 6.5)
        {
            pumpSpeed += 0.03f*increment;
        }
        waterObject.GetComponent<WaterController>().PumpWater();

        if (currentLevelProgress >= currentLevelProgressCap)
        {
            ProgressLevel();
        }
    }

    void ProgressLevel()
    {
        currentLevel += 1;
        currentLevelProgress = 0;
        currentLevelProgressCap = currentLevel * 200;
        waterObject.GetComponent<WaterController>().maxWaterSpeed += 0.1f;

        switch (currentLevel)
        {
            case 2:
                //puff
                break;
            case 3:
                numPossiblePieces = 6;
                break;
            case 4:
                //crabby
                break;
            case 5:
                //jelly
                break;
            case 6:
                numPossiblePieces = 7;
                break;
        }

        if (currentLevel == 2)
        {
            //show puff
        }
        else if (currentLevel == 3)
        {
            //6th piece
        }
        else if (currentLevel == 4)
        {
            //crabby
        }
        else if (currentLevel == 5)
        {
            //jelly
        }
        else if (currentLevel == 6)
        {
            //7th piece
        }

        ClearBoard();
    }

    public void ClearBoard(string nextGameState = "CLEAR")
    {
        checkingPatterns = false;
        currentGameState = nextGameState;

        gameEndSound.Play();

        for (int j = 0; j < boardWidth; j++)
        {
            float lerpSpeed = UnityEngine.Random.Range(1.75f, 2.25f);
            for (int i = 0; i < boardHeight; i++)
            {
                float newXPosition = gameBoardObject.transform.position.x + (j * pieceWidth) + (j * piecePadding);
                //float newYPosition = gameCamera.ScreenToWorldPoint(new Vector3(0.0f, gameCamera.pixelHeight, gameBoardObject.transform.position.z)).y;
                float newYPosition = 10.0f;

                if (gameBoardArray[i, j] == null)
                {
                    continue;
                }

                gameBoardArray[i, j].GetComponent<Match3Piece>().SetLerp(
                    new Vector3(newXPosition, newYPosition, gameBoardObject.transform.position.z),
                    lerpSpeed - (i * 0.1f)
                );
            }
        }

        for (int k = 0; k < starList.Count; k++)
        {
            Vector3 newStarPosition = new Vector3(starList[k].transform.position.x, 10.0f, gameBoardObject.transform.position.z);
            starList[k].gameObject.GetComponent<StarController>().SetLerp(newStarPosition, 1.0f);
        }
    }

    void ResetStars()
    {
        for (int i = 0; i < starList.Count; i++)
        {
            Destroy(starList[i]);
        }
        starList.Clear();
        for (int j = 0; j < currentLevel; j++)
        {
            starList.Add(
                Instantiate(
                    starObject,
                    new Vector3(starContainer.transform.position.x, starContainer.transform.position.y + ((starObject.transform.lossyScale.y + 0.2f) * j), starContainer.transform.position.z),
                    Quaternion.Euler(90, 0, 0),
                    starContainer.transform
                )
            );
            starList[j].GetComponent<StarController>().starIndex = j;
        }
    }

    IEnumerator PuffPop(int[] popCoords, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        for (int yIncrement = -1; yIncrement < 2; yIncrement++)
        {
            for (int xIncrement = -1; xIncrement < 2; xIncrement++)
            {
                if(popCoords[0] + yIncrement > boardHeight-1 || popCoords[0] + yIncrement < 0 || popCoords[1] + xIncrement > boardWidth-1 || popCoords[1] + xIncrement < 0)
                {
                    continue;
                }
                Destroy(gameBoardArray[popCoords[0] + yIncrement, popCoords[1] + xIncrement].transform.gameObject);
                gameBoardArray[popCoords[0] + yIncrement, popCoords[1] + xIncrement] = null;
                popEffectArray[popCoords[0] + yIncrement, popCoords[1] + xIncrement].GetComponent<ParticleSystem>().Play();
            }
        }
        Combo(3);
    }

    IEnumerator JellySwap(string pieceType, int[] jellyCoords, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        for (int j = 0; j < boardWidth; j++)
        {
            for (int i = 0; i < boardHeight; i++)
            {
                if (gameBoardArray[i, j].transform.name.Substring(0, 6) == pieceType)
                {
                    Combo(1);
                    Destroy(gameBoardArray[i, j].transform.gameObject);
                    gameBoardArray[i, j] = null;
                    popEffectArray[i, j].GetComponent<ParticleSystem>().Play();
                }
            }
        }

        Destroy(gameBoardArray[jellyCoords[0], jellyCoords[1]].transform.gameObject);
        gameBoardArray[jellyCoords[0], jellyCoords[1]] = null;
        popEffectArray[jellyCoords[0], jellyCoords[1]].GetComponent<ParticleSystem>().Play();
    }

    void HandlePieceClick(GameObject hitPiece)
    {
        Match3Piece hitPieceScript = hitPiece.transform.GetComponent<Match3Piece>();

        //check if piece is clickable
        if (!hitPieceScript.isClickable)
        {
            return;
        }

        int[] hitPieceCoords = hitPieceScript.BoardIndices;

        if(hitPiece.transform.name.Substring(0, 4) == "Puff")
        {
            //animate puff
            //create coroutine or function for deleting surrounding pieces on time with animation
            hitPiece.transform.gameObject.GetComponent<PuffController>().isPuffed = true;
            hitPiece.transform.gameObject.GetComponent<PuffController>().TransformLerp(new Vector3(0.45f, 0.9f, 0.45f), 0.6f);
            StartCoroutine(PuffPop(hitPieceCoords, 0.6f));
        }

        //no piece is clicked
        if (selectedPiece[0] == -1 && selectedPiece[1] == -1)
        {
            selectedPiece = hitPieceScript.BoardIndices;
            pieceSelector.transform.position = new Vector3(hitPiece.transform.position.x, hitPiece.transform.position.y, hitPiece.transform.position.z);
            return;
        }

        //piece is already clicked, clicked piece again
        if (selectedPiece[0] == hitPieceCoords[0] && selectedPiece[1] == hitPieceCoords[1])
        {
            selectedPiece = new int[] { -1, -1 };
            pieceSelector.transform.position = new Vector3(0, 10, 0);
            return;
        }

        //piece is clicked, adjacent piece was clicked
        if (selectedPiece[0] == hitPieceCoords[0])
        {
            if(hitPieceCoords[1] == selectedPiece[1]-1 || hitPieceCoords[1] == selectedPiece[1] + 1)
            {
                //swap pieces
                //GameObject tempPiece = gameBoardArray[selectedPiece[0], selectedPiece[1]];

                //float hitXPosition = gameBoardObject.transform.position.x + (hitPieceCoords[1] * pieceWidth) + (hitPieceCoords[1] * piecePadding);
                //float hitYPosition = gameBoardObject.transform.position.y + (hitPieceCoords[0] * pieceHeight) + (hitPieceCoords[0] * piecePadding);
                //gameBoardArray[selectedPiece[0], selectedPiece[1]].GetComponent<Match3Piece>().BoardIndices = new int[] { hitPieceCoords[0], hitPieceCoords[1] };
                //gameBoardArray[selectedPiece[0], selectedPiece[1]].GetComponent<Match3Piece>().SetLerp(new Vector3(hitXPosition, hitYPosition, gameBoardObject.transform.position.z), 0.25f);
                //gameBoardArray[selectedPiece[0], selectedPiece[1]] = gameBoardArray[hitPieceCoords[0], hitPieceCoords[1]];

                //float selectedXPosition = gameBoardObject.transform.position.x + (selectedPiece[1] * pieceWidth) + (selectedPiece[1] * piecePadding);
                //float selectedYPosition = gameBoardObject.transform.position.y + (selectedPiece[0] * pieceHeight) + (selectedPiece[0] * piecePadding);
                //gameBoardArray[hitPieceCoords[0], hitPieceCoords[1]].GetComponent<Match3Piece>().BoardIndices = new int[] { selectedPiece[0], selectedPiece[1] };
                //gameBoardArray[hitPieceCoords[0], hitPieceCoords[1]].GetComponent<Match3Piece>().SetLerp(new Vector3(selectedXPosition, selectedYPosition, gameBoardObject.transform.position.z), 0.25f);
                //gameBoardArray[hitPieceCoords[0], hitPieceCoords[1]] = tempPiece;

                StartCoroutine(SwapPieces(selectedPiece, hitPieceCoords));

                selectedPiece = new int[] { -1, -1 };
                pieceSelector.transform.position = new Vector3(0, 10, 0);

                return;
            }
        }

        //piece is clicked, non-adjacent piece was clicked
        selectedPiece = hitPieceScript.BoardIndices;
        pieceSelector.transform.position = new Vector3(hitPiece.transform.position.x, hitPiece.transform.position.y, hitPiece.transform.position.z);
        return;
    }

    void HandlePieceDrag(GameObject hitPiece, int direction)
    {
        int[] hitPieceCoords = hitPiece.transform.GetComponent<Match3Piece>().BoardIndices;

        if(hitPieceCoords[1] + direction < 0 || hitPieceCoords[1] + direction > (boardWidth - 1)){
            return;
        }

        StartCoroutine(SwapPieces(hitPieceCoords, new int[] { hitPieceCoords[0], hitPieceCoords[1] + direction }));

        selectedPiece = new int[] { -1, -1 };
        pieceSelector.transform.position = new Vector3(0, 10, 0);
    }

    IEnumerator SwapPieces(int[] piece1Coords, int[] piece2Coords)
    {
        pumpSpeed -= 0.025f;
        if(pumpSpeed < 0)
        {
            pumpSpeed = 0;
        }
        waterObject.GetComponent<WaterController>().currentWaterSpeed -= 0.1f;

        //store objects to swap
        GameObject piece1Object = gameBoardArray[piece1Coords[0], piece1Coords[1]];
        GameObject piece2Object = gameBoardArray[piece2Coords[0], piece2Coords[1]];

        if(!piece1Object.GetComponent<Match3Piece>().isSwappable || !piece2Object.GetComponent<Match3Piece>().isSwappable)
        {
            yield break;
        }

        while(piece1Object.GetComponent<Match3Piece>().isMoving || piece2Object.GetComponent<Match3Piece>().isMoving)
        {
            yield return new WaitForSeconds(0.05f);
        }

        //store object positions
        float[] piece1Position = new float[]
        {
            gameBoardObject.transform.position.x + (piece1Coords[1] * pieceWidth) + (piece1Coords[1] * piecePadding),
            gameBoardObject.transform.position.y + (piece1Coords[0] * pieceHeight) + (piece1Coords[0] * piecePadding)
        };
        float[] piece2Position = new float[]
        {
            gameBoardObject.transform.position.x + (piece2Coords[1] * pieceWidth) + (piece2Coords[1] * piecePadding),
            gameBoardObject.transform.position.y + (piece2Coords[0] * pieceHeight) + (piece2Coords[0] * piecePadding)
        };

        //update BoardIndices in object script
        gameBoardArray[piece1Coords[0], piece1Coords[1]].GetComponent<Match3Piece>().BoardIndices = new int[] { piece2Coords[0], piece2Coords[1] };
        gameBoardArray[piece2Coords[0], piece2Coords[1]].GetComponent<Match3Piece>().BoardIndices = new int[] { piece1Coords[0], piece1Coords[1] };

        //start moving pieces to new position
        float swapSpeed = 0.4f;
        if (gameBoardArray[piece1Coords[0], piece1Coords[1]].transform.position.y < waterObject.transform.position.y)
        {
            swapSpeed = 0.75f;
        }
        gameBoardArray[piece1Coords[0], piece1Coords[1]].GetComponent<Match3Piece>().SetLerp(
            new Vector3(piece2Position[0], piece2Position[1], gameBoardObject.transform.position.z),
            new Vector3(0f, 0f, gameBoardObject.transform.position.z+0.25f),
            swapSpeed
        );
        gameBoardArray[piece2Coords[0], piece2Coords[1]].GetComponent<Match3Piece>().SetLerp(
            new Vector3(piece1Position[0], piece1Position[1], gameBoardObject.transform.position.z),
            new Vector3(0f, 0f, gameBoardObject.transform.position.z-0.25f),
            swapSpeed
        );

        //change object names
        //piece1Object.transform.name = piece1Object.transform.name + "_" + piece2Coords[0].ToString() + "_" + piece2Coords[1].ToString();
        //piece2Object.transform.name = piece2Object.transform.name + "_" + piece1Coords[0].ToString() + "_" + piece1Coords[1].ToString();

        //update gameBoardArray to swap objects
        gameBoardArray[piece1Coords[0], piece1Coords[1]] = piece2Object;
        gameBoardArray[piece2Coords[0], piece2Coords[1]] = piece1Object;

        SwapPieceSound.Play();

        //Check for jelly swap
        if(piece1Object.tag == "Special" && piece1Object.transform.name.Substring(0, 5) == "Jelly")
        {
            StartCoroutine(JellySwap(piece2Object.transform.name.Substring(0, 6), piece2Coords, swapSpeed));
        }
        else if(piece2Object.tag == "Special" && piece2Object.transform.name.Substring(0, 5) == "Jelly")
        {
            StartCoroutine(JellySwap(piece1Object.transform.name.Substring(0, 6), piece1Coords, swapSpeed));
        }
    }

    void GetInput()
    {
        if (Input.GetMouseButtonDown(0)) //check for mouse click left button
        {
            isClicking = true;
            clickStartPosition = Input.mousePosition;
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //send ray by mouse position
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, inputRaycastLayerMask))
            {
                GameObject pieceClicked = hit.transform.gameObject;
                HandlePieceClick(pieceClicked);
                //int[] pieceClickedCoords = hit.transform.GetComponent<Match3Piece>().BoardIndices;

                //Destroy(GameObject.Find(pieceClicked));
                //gameBoardArray[pieceClickedCoords[0], pieceClickedCoords[1]] = null;

                //simplePopSound.Play();
            }

            //Vector3 screenOrigin = gameCamera.WorldToScreenPoint(new Vector3(0, 0, 0));
            //Vector3 worldOrigin = gameCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenOrigin.z));

            //Vector3 camPosition = gameCamera.transform.localPosition;
            //Vector3 direction = new Vector3(0, 0, camPosition.z);
            //if(Physics.Raycast(worldOrigin, direction, out hit, 2))
            //{
            //    Debug.Log(hit.transform.name);
            //    Destroy(GameObject.Find(hit.transform.name));
            //}

            ////Debug
            //Debug.DrawRay(worldOrigin, direction, Color.blue, 2f);
        }
        if (Input.GetMouseButtonUp(0))
        {
            isClicking = false;
        }
        if (isClicking)
        {
            if (selectedPiece[0] == -1 || selectedPiece[1] == -1)
            {
                //nothing to drag
            }
            else
            {
                if (Input.mousePosition.x < clickStartPosition.x - 100)
                {
                    HandlePieceDrag(gameBoardArray[selectedPiece[0], selectedPiece[1]], -1);
                    isClicking = false;
                }
                else if (Input.mousePosition.x > clickStartPosition.x + 100)
                {
                    HandlePieceDrag(gameBoardArray[selectedPiece[0], selectedPiece[1]], 1);
                    isClicking = false;
                }
            }
        }
        
    }
}
