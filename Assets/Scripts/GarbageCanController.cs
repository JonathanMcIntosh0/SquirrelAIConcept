using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GarbageCanController : MonoBehaviour
{
    // public Renderer meshRenderer;
    
    public Color emptyColor = Color.grey;
    public Color fullColor = Color.black;

    public bool isEmpty;
    public bool hasSquirrel;

    private float _timeSinceLastChange = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(DateTime.Now.Millisecond);
        
        //Choose if the can starts full or empty
        SetState(Random.value > 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceLastChange += Time.deltaTime;
        // At least 10 sec have passed and not hasSquirrel (although the latter should not happen since we reset _timeSinceLastChange
        // when we trap/release a squirrel)
        if (_timeSinceLastChange >= 10f && !hasSquirrel) 
        {
            ChangeState();
        }

    }

    public void ChangeState()
    {
        SetState(!isEmpty);
    }

    // If state == true then we set state to empty otherwise we set to full 
    public void SetState(bool state)
    {
        GetComponentInChildren<Renderer>().material.color = (state) ? emptyColor : fullColor;
        isEmpty = state;

        _timeSinceLastChange = 0f;
    }


    public void ChangeSquirrelState()
    {
        SetSquirrelState(!hasSquirrel);
    }
    
    public void SetSquirrelState(bool trapSquirrel)
    {
        hasSquirrel = trapSquirrel;
        
        _timeSinceLastChange = 0f;
    }
}
