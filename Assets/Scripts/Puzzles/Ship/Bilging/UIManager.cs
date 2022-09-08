using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    [SerializeField] List<GameObject> UIPieces;
    [SerializeField] GameObject nextLevelPanel;
    [SerializeField] GameObject newPieceDescription;

    [SerializeField] GameObject levelFailedPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowLevelDialogue(int currentLevel)
    {
        switch (currentLevel)
        {
            case 2:
                //show puff
                UIPieces[0].SetActive(true);
                newPieceDescription.GetComponent<TextMeshProUGUI>().text = "Introducing Puff, destroys a 3x3 area when interacted with!";
                break;
            case 3:
                //show next piece
                UIPieces[1].SetActive(true);
                newPieceDescription.GetComponent<TextMeshProUGUI>().text = "A new piece was added to the board.  Make better combos!";
                break;
            case 4:
                //show crabby
                UIPieces[2].SetActive(true);
                newPieceDescription.GetComponent<TextMeshProUGUI>().text = "Introducing Crabby, score extra points when crabby reaches the surface!";
                break;
            case 5:
                //show jelly
                UIPieces[3].SetActive(true);
                newPieceDescription.GetComponent<TextMeshProUGUI>().text = "Introducing Jelly, swap jelly to score mega big points!";
                break;
            case 6:
                //show next piece
                UIPieces[4].SetActive(true);
                newPieceDescription.GetComponent<TextMeshProUGUI>().text = "A new piece was added to the board.  Make better combos!";
                break;
            default:
                //do nothing
                gameObject.GetComponent<Match3Game>().currentGameState = "INIT";
                return;
        }
        nextLevelPanel.SetActive(true);
    }

    public void PlayGameButton()
    {
        nextLevelPanel.SetActive(false);
        for (int i = 0; i < UIPieces.Count; i++)
        {
            UIPieces[i].SetActive(false);
        }
        newPieceDescription.GetComponent<TextMeshProUGUI>().text = "New Piece Description Text";
        gameObject.GetComponent<Match3Game>().currentGameState = "INIT";
    }

    public void ShowLevelRetryDialogue()
    {
        levelFailedPanel.SetActive(true);
    }

    public void LevelRetryButton()
    {
        levelFailedPanel.SetActive(false);
        gameObject.GetComponent<Match3Game>().currentGameState = "INIT";
    }
}
