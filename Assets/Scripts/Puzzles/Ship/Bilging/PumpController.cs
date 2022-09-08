using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpController : MonoBehaviour
{

    Match3Game gameController;
    [SerializeField] bool isColorChanging = false;
    [SerializeField] Material colorChangeMaterial;

    [SerializeField] Color[] pumpSpeedsColors = new Color[7];

    Animator pumpAnimator = new Animator();

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("Match3Manager").GetComponent<Match3Game>();
        pumpAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.pumpSpeed >= 0)
        {
            pumpAnimator.speed = gameController.pumpSpeed;
        }

        if (isColorChanging)
        {
            colorChangeMaterial.DisableKeyword("_EMISSION");
            if(gameObject.GetComponent<ParticleSystem>().isPlaying && gameController.pumpSpeed < 6)
            {
                gameObject.GetComponent<ParticleSystem>().Stop();
            }

            if (gameController.pumpSpeed >= 0 && gameController.pumpSpeed < 0.5)
            {
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[0]);
            }
            else if (gameController.pumpSpeed >= 0.5 && gameController.pumpSpeed < 1.5)
            {
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[1]);
            }
            else if (gameController.pumpSpeed >= 1.5 && gameController.pumpSpeed < 2.5)
            {
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[2]);
            }
            else if (gameController.pumpSpeed >= 2.5 && gameController.pumpSpeed < 3.5)
            {
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[3]);
            }
            else if (gameController.pumpSpeed >= 3.5 && gameController.pumpSpeed < 4.5)
            {
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[4]);
            }
            else if (gameController.pumpSpeed >= 4.5 && gameController.pumpSpeed < 6)
            {
                colorChangeMaterial.EnableKeyword("_EMISSION");
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[5]);
                colorChangeMaterial.SetColor("_EmissionColor", pumpSpeedsColors[5]);
            }
            else if (gameController.pumpSpeed >= 6)
            {
                colorChangeMaterial.EnableKeyword("_EMISSION");
                colorChangeMaterial.SetColor("_BaseColor", pumpSpeedsColors[6]);
                colorChangeMaterial.SetColor("_EmissionColor", pumpSpeedsColors[6]);
                if (!gameObject.GetComponent<ParticleSystem>().isPlaying)
                {
                    gameObject.GetComponent<ParticleSystem>().Play();
                }
            }
        }
    }
}
