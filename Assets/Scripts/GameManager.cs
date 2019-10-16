using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ViewController viewController;
    public char[][] currentLevelLayout;
    public TextAsset highScore;

    public int horizontalRatio, verticalRatio;
    public float horizontalOffset;

    public GameObject levelButton, canvas, tile;
    public List<Sprite> tileSetSprites, buttonSprites;
    public List<TextAsset> levelLayouts;

    private List<Tile> _selectedTiles = new List<Tile>();
    private List<Tile> _tiles;
    private List<Tile> _actualPlayableTiles;
    private bool[] showStar;

    private int totalPoints = 0;
    private int currentHighscore;
    private int currentLayoutIndex = -1;
    

    private void Start()
    {
        currentHighscore = int.Parse(highScore.text);
    }
    private void Awake()
    {
        DontDestroyOnLoad(this);

        if(instance == null)
        {
            instance = this;
            showStar = new bool[levelLayouts.Count];
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene aScene,LoadSceneMode aMode)
    {
        if(aScene.name == "StartScene")
        {
            canvas = ((Canvas)FindObjectOfType(typeof(Canvas))).gameObject;
            currentLayoutIndex = -1;
            CreateLevelButtons();
        }
        else if(aScene.name == "GameplayScene")
        {
            totalPoints = 0;
            CreateGameplayScene();
        }
    }

    void CreateLevelButtons()
    {
        Vector3 prevButtonPos = new Vector3(0,200,0);
        for(int i = 0;i < levelLayouts.Count;i++)
        {
            GameObject button = Instantiate(levelButton,canvas.transform);
            button.transform.localPosition = new Vector3(prevButtonPos.x,prevButtonPos.y - 70,prevButtonPos.z);
            button.GetComponent<LevelButton>().levelLayout = GetLevelLayout(levelLayouts[i].ToString());
            button.GetComponent<LevelButton>().layoutIndex = i;
            prevButtonPos = button.transform.localPosition;
            viewController.UpdateText(button.GetComponentInChildren<Text>(),"Level " + (i + 1).ToString());
            viewController.AddSpriteToObject(button.GetComponent<Image>(),buttonSprites[i]);

            if(showStar[i]) AddStar(button);
        }
    }

    private char[][] GetLevelLayout(string layout)
    {
        string[] tempRows = layout.Split(new[] { Environment.NewLine },StringSplitOptions.None);
        char[][] levelLayout = new char[tempRows.Length][];
        for(int i = 0;i < levelLayout.Length;i++)
        {
            tempRows[i] = tempRows[i].Replace("\r","");
            tempRows[i] = tempRows[i].Replace("\t","");
            levelLayout[i] = tempRows[i].ToCharArray();
        }
        return levelLayout;
    }

    public void StartThisLevel(char[][] levelLayout, int index)
    {
        currentLevelLayout = levelLayout;
        currentLayoutIndex = index;
        SwitchScene("GameplayScene");
    }

    public void SwitchScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    private void CreateGameplayScene()
    {
        GameObject tilesParent = new GameObject();
        tilesParent.name = "TilesParent";
        int rows = currentLevelLayout.Length;
        int colums = currentLevelLayout[0].Length;
        
        horizontalOffset = (Camera.main.orthographicSize * horizontalRatio) / verticalRatio;
        tilesParent.transform.position = new Vector3(Camera.main.transform.position.x - (horizontalOffset * 0.6f), Camera.main.transform.position.y + (Camera.main.orthographicSize * 0.6f),-1f);
        Vector3 tempPos = tilesParent.transform.position;
        Tile[][] tileMap = new Tile[currentLevelLayout.Length][];
        _tiles = new List<Tile>();
        _actualPlayableTiles = new List<Tile>();
        int tileID = 0;
        for(int i = 0;i < currentLevelLayout.Length;i++)
        {
            tempPos.x = 0;
            tileMap[i] = new Tile[currentLevelLayout[i].Length];
            for(int j = 0;j < currentLevelLayout[i].Length;j++)
            {
                Tile currentTile = viewController.CreateTile(tile,tilesParent.transform,tempPos);
                currentTile.id = tileID;
                if(currentLevelLayout[i][j] == 'X')
                {
                    int random = UnityEngine.Random.Range(0,tileSetSprites.Count);
                    viewController.AddIconToTile(currentTile.GetComponentInChildren<SpriteRenderer>(),tileSetSprites[random]);
                    currentTile.tileName = tileSetSprites[random].name;
                    currentTile.isActualTile = true;
                    _actualPlayableTiles.Add(currentTile);
                }
                else
                {
                    viewController.AddIconToTile(currentTile.GetComponent<SpriteRenderer>(),null);
                }
                tileMap[i][j] = currentTile;
                _tiles.Add(currentTile);
                tempPos.x += Screen.width / colums;
                tileID++;
            }
            tempPos.y -= Screen.height / rows;
        }
        CreateTileMap(tileMap);
        tilesParent.transform.parent = viewController.transform;
        tilesParent.transform.localScale = new Vector3(0.011f,0.011f,0.011f);
    }

    private void CreateTileMap(Tile[][] map)
    {
        for(int i = 0;i < map.Length;i++)
        {
            for(int j = 0;j < map[i].Length;j++)
            {
                if(j != 0)
                {
                    map[i][j].directions.Add(map[i][j - 1],Direction.tileLeft);
                }
                if(j != map[i].Length - 1)
                {
                    map[i][j].directions.Add(map[i][j + 1],Direction.tileRight);
                }
                if(i != 0)
                {
                    map[i][j].directions.Add(map[i - 1][j],Direction.tileUp);
                }
                if(i != map.Length - 1)
                {
                    map[i][j].directions.Add(map[i + 1][j],Direction.tileDown);
                }
            }
        }
    }

    public void OnTileClick(Tile tile)
    {
        if(!_selectedTiles.Contains(tile) && tile.isActualTile)
        {
            AddTilesToSelected(tile);
        }
        else if(_selectedTiles.Contains(tile) && tile.isActualTile)
            RemoveTileFromSelected(tile);

        if(_selectedTiles.Count == 2) CheckForPathBetween(_selectedTiles[0],_selectedTiles[1],false);
    }

    private void AddTilesToSelected(Tile tempTile)
    {
        _selectedTiles.Add(tempTile);
        viewController.HighlightTile(tempTile.gameObject);
    }

    private void RemoveTileFromSelected(Tile tempTile)
    {
        viewController.DeselectTile(tempTile.gameObject);
        _selectedTiles.Remove(tempTile);
    }

    private bool CheckForPathBetween(Tile tile1,Tile tile2,bool isHint)
    {
        Direction first = Direction.tileDown;
        foreach(var pair in tile1.directions)
        {
            first = pair.Value;
            break;
        }
        if(PathFound(tile1,tile2,tile1,0,first))
        {
            //add points and deselect icons if this was not hint search
            if(!isHint)
            {
                IncreasePoints();
                viewController.DeselectTile(tile1.gameObject);
                viewController.DeselectTile(tile2.gameObject);
                RemoveTilesFromPlay(tile1,tile2);
            }
            return true;
        }
        else
        {
            //decrease points if this was not hint search
            if(!isHint)
            {
                DecreasePoints();

                foreach(var tile in _selectedTiles)
                {
                    viewController.DeselectTile(tile.gameObject);
                }
                _selectedTiles.Clear();
            }
            return false;
        }
    }

    private bool PathFound(Tile start,Tile end,Tile current,int curves,Direction currentDir)
    {
        if(curves > 2) return false;
        else if(start.tileName != end.tileName) return false;

        //first check if we found the end tile
        else if(current.id == end.id) return true;

        //continue searching if the actual tile is empty space or we are at the start
        else if(!current.isActualTile || current == start)
        {
            foreach(var dir in current.directions)
            {
                bool value = false;
                if(dir.Value == currentDir || current == start)
                {
                    value = PathFound(start,end,dir.Key,curves,dir.Value);
                }
                else
                {
                    //increase number of curves only if direction changes
                    value = PathFound(start,end,dir.Key,curves + 1,dir.Value);
                }
                if(value) return true;
            }
            return false;
        }
        else return false;
    }

    private void IncreasePoints()
    {
        totalPoints += 15;
    }
    
    private void DecreasePoints()
    {
        if(totalPoints - 10 > 0) totalPoints -= 10;
    }

    void RemoveTilesFromPlay(Tile tile1,Tile tile2)
    {
        Remove(tile1);
        Remove(tile2);
        _actualPlayableTiles.Remove(tile1);
        _actualPlayableTiles.Remove(tile2);

        foreach(var tile in _selectedTiles)
        {
            viewController.DeselectTile(tile.gameObject);
        }
        _selectedTiles.Clear();

        //check whether there is no more tiles to play, and the number of tiles is less than 2
        if(!HintExists(_tiles, false))
        {
            if(_actualPlayableTiles.Count > 1)
            {
                viewController.CreatePopUp("You've lost!", false);
            }
            else
            {
                if(totalPoints > currentHighscore) UpdateHighscore(totalPoints);
                viewController.CreatePopUp("You've won!", true);
            }
        }

    }

    void Remove(Tile tile)
    {
        tile.GetComponentInChildren<SpriteRenderer>().sprite = null;
        tile.isActualTile = false;
    }

    public void AskForHint()
    {
        HintExists(_tiles, true);
    }
    
    bool HintExists(List<Tile> _tiles, bool highlight)
    {
        for(int i = 0;i < _tiles.Count - 1;i++)
        {
            if(!_tiles[i].isActualTile)
                continue;
            for(int j = i + 1;j < _tiles.Count;j++)
            {
                if(!_tiles[j].isActualTile)
                    continue;
                if(CheckForPathBetween(_tiles[i],_tiles[j],true))
                {
                    //highlight only when we actually press the button to find hint
                    if(highlight)
                    {
                        viewController.HighlightTile(_tiles[i].gameObject);
                        viewController.HighlightTile(_tiles[j].gameObject);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public int GetTotalPoints()
    {
        return totalPoints;
    }

    public int GetCurrentHighscore()
    {
        return currentHighscore;
    }

    int GetHighscoreFromText()
    {
        string text = highScore.text;
        return int.Parse(text);
    }

    public void UpdateHighscore(int score)
    {
        currentHighscore = score;
        SaveDocument(Application.dataPath + "/Resources/highscore.txt",currentHighscore.ToString());
    }

    private void SaveDocument(string path, string text)
    {
        StreamWriter writer = new StreamWriter(path,false);
        writer.WriteLine(text);
        writer.Close();
    }

    public void SetForShowingStar()
    {
        for(int i=0; i<showStar.Length; i++)
        {
            if(i==currentLayoutIndex)
            {
                showStar[i] = true;
                break;
            }
        }
    }

    public void AddStar(GameObject button)
    {
        foreach(Transform child in button.transform)
        {
            if(child.name == "Star")
            {
                child.gameObject.SetActive(true);
                break;
            }
        }
    }

}
