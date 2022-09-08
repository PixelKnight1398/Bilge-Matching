using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabbyScript : MonoBehaviour
{

    public bool isSubmerged = true;
    public Animator crabAnimator;
    public GameObject waterObject;

    // Start is called before the first frame update
    void Start()
    {
        crabAnimator = GetComponent<Animator>();
        waterObject = GameObject.Find("Water");
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.transform.position.y > waterObject.transform.position.y)
        {
            isSubmerged = false;
        }
        else
        {
            isSubmerged = true;
        }

        crabAnimator.SetBool("Submerged", isSubmerged);
    }
}
