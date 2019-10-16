using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    public GameManager manager;
    public Texture closeButtonIcon;
    public List<GameObject> buttonsToHide = new List<GameObject>();

    private bool popUp;
    //variable that is set from the gamemanager, telling the view controller whether the game was completed or not
    private bool showScore;
    private string popUpText;

    private Ray _ray;
    private RaycastHit2D _hit;
    private GameObject highlight;

    private GUIStyle _styleMessage, _styleScore;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            _hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),transform.forward);
            if(_hit.collider != null && _hit.collider.tag == "tile")
            {
                manager.OnTileClick(_hit.collider.GetComponent<Tile>());
            }
        }
    }

    private void OnGUI()
    {
        if(popUp)
        {
            for(int i = buttonsToHide.Count - 1; i >=0; i--)
            {
                buttonsToHide[i].SetActive(false);
                buttonsToHide.RemoveAt(i);
            }
            GUI.Window(0,new Rect(0,0,Screen.width,Screen.height),ShowGUI,"");
        }
    }
    void ShowGUI(int windowID)
    {
        GUI.Label(new Rect((Screen.width) - 375,Screen.height/2,400,60),popUpText, _styleMessage);
        
        if(showScore)
        {
            GUI.Label(new Rect((Screen.width) - 375,(Screen.height / 2) + Screen.height/10,400,60),"Highscore:" + manager.GetCurrentHighscore().ToString(),_styleScore);
            GUI.Label(new Rect((Screen.width) - 375,(Screen.height / 2) + Screen.height /6,400,60),"Current score:" + manager.GetTotalPoints().ToString(),_styleScore);
        }

        if(GUI.Button(new Rect((Screen.width/2) - 75/2,Screen.height - 40,75,30),closeButtonIcon))
        {
            popUp = false;
            if(showScore) manager.SetForShowingStar();
            manager.SwitchScene("StartScene");
        }

    }

    private void Awake()
    {
        manager = (GameManager)FindObjectOfType(typeof(GameManager));
        manager.viewController = this;
    }

    public Tile CreateTile(GameObject prefab, Transform parent, Vector3 position)
    {
        GameObject tile = Instantiate(prefab,position,Quaternion.identity,parent);
        return tile.GetComponent<Tile>();
    }

    public void AddIconToTile(SpriteRenderer renderer, Sprite sprite)
    {
        renderer.sprite = sprite;
    }

    public void HighlightTile(GameObject tile)
    {
        GetHighlightedObject(tile.transform).SetActive(true);
    }

    public void DeselectTile(GameObject tile)
    {
        GetHighlightedObject(tile.transform).SetActive(false);
    }

    private GameObject GetHighlightedObject(Transform tile)
    {
        foreach(Transform child in tile)
        {
            if(child.name == "Highlight")
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void UpdateText (Text textScript, string text)
    {
        textScript.text = text;
    }

    public void AddSpriteToObject(Image image, Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void CreatePopUp(string text, bool hasWon)
    {
        _styleScore = new GUIStyle();
        _styleScore.fontSize = 30;
        _styleScore.alignment = TextAnchor.MiddleCenter;

        _styleMessage = new GUIStyle();
        _styleMessage.fontSize = 50;
        _styleMessage.alignment = TextAnchor.MiddleCenter;


        popUpText = text;
        popUp = true;
        showScore = hasWon;
    }

    public void PlaceButton(GameObject button, Vector3 pos)
    {
        button.transform.position = pos;
    }
}
