using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isActualTile;
    public string tileName;
    public int id;

    public Dictionary<Tile, Direction> directions = new Dictionary<Tile, Direction>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
