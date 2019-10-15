using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public bool isFinished;
    public char[][] levelLayout;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNameOfText(string text)
    {
        GetComponentInChildren<Text>().text += text;
    }

    public void SelectThisButton()
    {
        GameManager.instance.StartThisLevel(levelLayout);
    }
}
