using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseButton : GenericButton
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => GameManager.instance.SwitchScene("StartScene"));
        viewController.buttonsToHide.Add(gameObject);
        viewController.closeButtonIcon = GetComponent<Image>().mainTexture;

        //update position in case screen ratio has changed
        Vector3 pos = new Vector3(Screen.width * 0.85f,Screen.height * 0.90f,-1);
        viewController.PlaceButton(gameObject,pos);
    }

    public override void Awake()
    {
        base.Awake();
    }
}
