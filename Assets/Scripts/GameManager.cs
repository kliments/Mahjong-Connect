using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ViewController viewController;
    public char[][] currentLevelLayout;
    public bool test = false;

    public GameObject levelButton, canvas, tile, highlight;
    public List<Sprite> tileSetSprites;
    public List<TextAsset> levelLayouts;

    private List<GameObject> highlighted = new List<GameObject>();
    private List<Tile> selectedTiles = new List<Tile>();
    public enum Direction
    {
        [field: Description("tileUp")]
        tileUp,

        [field: Description("tileDown")]
        tileDown,

        [field: Description("tileLeft")]
        tileLeft,

        [field: Description("tileRight")]
        tileRight
    }

    private Direction currentDirection = Direction.tileRight;
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CreateLevelButtons();
    }

    // Update is called once per frame
    void Update()
    {
        if(test)
        {
            test = false;

        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
    {
        if(aScene.name == "StartScene")
        {
            CreateLevelButtons();
        }
        else if(aScene.name == "GameplayScene")
        {
            CreateGameplayScene();
        }
    }

    public void SelectLevel(char[][] layout)
    {
        currentLevelLayout = layout;
    }

    void CreateLevelButtons()
    {
        Vector3 prevButtonPos = new Vector3(0, 330, 0);
        for(int i=0; i<levelLayouts.Count; i++)
        {
            GameObject button = Instantiate(levelButton, canvas.transform);
            button.transform.localPosition = new Vector3(prevButtonPos.x, prevButtonPos.y - 100, prevButtonPos.z);
            button.GetComponent<LevelButton>().levelLayout = GetLevelLayout(levelLayouts[i].ToString());
            prevButtonPos = button.transform.localPosition;
            button.GetComponent<LevelButton>().SetNameOfText((i + 1).ToString());
        }
    }

    private char[][] GetLevelLayout(string layout)
    {
        string[] tempRows = layout.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        char[][] levelLayout = new char[tempRows.Length][];
        for (int i=0; i<levelLayout.Length; i++)
        {
            tempRows[i] = tempRows[i].Replace("\r", "");
            tempRows[i] = tempRows[i].Replace("\t", "");
            levelLayout[i] = tempRows[i].ToCharArray();
        }

        return levelLayout;
    }

    public void StartThisLevel(char[][] levelLayout)
    {
        currentLevelLayout = levelLayout;
        SceneManager.LoadScene("GameplayScene");
    }

    private void CreateGameplayScene()
    {
        GameObject tilesParent = new GameObject();
        int rows = currentLevelLayout.Length;
        int colums = currentLevelLayout[0].Length;
        tilesParent.transform.position = new Vector2(Screen.width / colums, Screen.height / rows);
        Vector3 tempPos = new Vector3();
        Tile[][] tileMap = new Tile[currentLevelLayout.Length][];
        int tileID= 0;
        for(int i=0; i<currentLevelLayout.Length; i++)
        {
            tempPos.x = 0;
            tileMap[i] = new Tile[currentLevelLayout[i].Length];
            for(int j=0; j<currentLevelLayout[i].Length; j++)
            {
                GameObject obj = Instantiate(tile, tempPos, Quaternion.identity, tilesParent.transform);
                Tile currentTile = obj.GetComponent<Tile>();
                currentTile.id = tileID;
                if (currentLevelLayout[i][j] == 'X')
                {
                    int random = UnityEngine.Random.Range(0, tileSetSprites.Count);
                    obj.GetComponentInChildren<SpriteRenderer>().sprite = tileSetSprites[random];
                    currentTile.tileName = tileSetSprites[random].name;
                    currentTile.isActualTile = true;
                }
                else
                {
                    obj.GetComponent<SpriteRenderer>().sprite = null;
                }
                tileMap[i][j] = currentTile;
                tempPos.x += Screen.width / colums;
                tileID++;
            }
            tempPos.y -= Screen.height / rows;
        }
        CreateTileMap(tileMap);
    }

    private void CreateTileMap(Tile[][] map)
    {
        for(int i=0; i<map.Length; i++)
        {
            for(int j=0; j<map[i].Length; j++)
            {
                if(j!=0)
                {
                    map[i][j].directions.Add(map[i][j - 1], Direction.tileLeft);
                }
                if(j!=map[i].Length-1)
                {
                    map[i][j].directions.Add(map[i][j + 1], Direction.tileRight);
                }
                if(i!=0)
                {
                    map[i][j].directions.Add(map[i - 1][j], Direction.tileUp);
                }
                if(i!=map.Length-1)
                {
                    map[i][j].directions.Add(map[i + 1][j], Direction.tileDown);
                }
            }
        }
    }

    public void OnTileClick(Tile tile)
    {
        if (!selectedTiles.Contains(tile))
        {
            AddTile(tile);
            //viewController.SelectTile(tile.gameObject);

        }
        else RemoveTileFromSelected(tile);

        if (selectedTiles.Count == 2) CheckForPathBetween(selectedTiles[0], selectedTiles[1]);
    }

    private void AddTile(Tile tempTile)
    {
        selectedTiles.Add(tempTile);
    }

    private void RemoveTileFromSelected(Tile tempTile)
    {
        selectedTiles.Remove(tempTile);
    }

    private void CheckForPathBetween(Tile tile1, Tile tile2)
    {
        Direction first = Direction.tileDown;
        foreach(var pair in tile1.directions)
        {
            first = pair.Value;
            break;
        }
        if (PathFound(tile1, tile2, tile1, 0, first))
        {
            IncreasePoints();
            RemoveTilesFromPlay(tile1, tile2);
        }
        else DecreasePoints();
    }

    private bool PathFound(Tile start, Tile end, Tile current,int curves, Direction currentDir)
    {
        if (curves > 2) return false;
        else if (start.tileName != end.tileName) return false;

        //first check if we found the end tile
        else if (current.id == end.id) return true;

        //continue searching if the actual tile is empty space or we are at the start
        else if (!current.isActualTile || current == start)
        {
            foreach (var dir in current.directions)
            {
                bool value = false;
                if (dir.Value == currentDir || current == start)
                {
                    value = PathFound(start, end, dir.Key, curves, dir.Value);
                }
                else
                {
                    //increase number of curves only if direction changes
                    value = PathFound(start, end, dir.Key, curves + 1, dir.Value);
                }
                if (value) return true;
            }
            return false;
        }

        else return false;
    }

    private void IncreasePoints()
    {
        Debug.Log("icrease points");
    }
    private void DecreasePoints()
    {
        Debug.Log("decrease points");
        foreach(var go in highlighted)
        {
            go.SetActive(false);
        }
        selectedTiles.Clear();
    }

    void RemoveTilesFromPlay(Tile tile1, Tile tile2)
    {
        Remove(tile1);
        Remove(tile2);

        foreach (var go in highlighted)
        {
            go.SetActive(false);
        }
        selectedTiles.Clear();

    }

    void Remove(Tile tile)
    {
        tile.GetComponentInChildren<SpriteRenderer>().sprite = null;
        tile.isActualTile = false;
    }

    public GameObject Highlight(Transform parent,Vector3 pos)
    {
        GameObject go = Instantiate(highlight, pos, Quaternion.identity, parent);
        highlighted.Add(go);
        return go;
    }

}
