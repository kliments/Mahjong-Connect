using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    private Ray _ray;
    private RaycastHit2D _hit;
    private GameManager manager;
    private GameObject highlight;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            _hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.forward);
            if(_hit.collider.tag == "tile" && _hit.collider!=null)
            {
                manager.OnTileClick(_hit.collider.GetComponent<Tile>());
            }
        }
    }

    private void Awake()
    {
        manager = (GameManager)FindObjectOfType(typeof(GameManager));
        manager.viewController = this;
    }

    public void SelectTile(GameObject tile)
    {
        if (highlight == null) highlight = manager.Highlight(tile.transform, tile.transform.position);
        else highlight.SetActive(true);
    }

    public void DeselectTile(GameObject tile)
    {
        if (highlight != null) highlight.SetActive(false);
    }
}
