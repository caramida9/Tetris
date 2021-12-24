using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static int gridwidth = 10;
    public static int gridheight = 20;

    public static Transform[,] grid = new Transform[gridwidth, gridheight];

    public static int currentScore = 0;

    public int currentLevel = 0;

    public static bool startingAtLevelZero;
    public static int startingLevel = 0;

    public int scoreOneLine = 10;
    public int scoreTwoLine = 20;
    public int scoreThreeLine = 50;
    public int scoreFourLine = 100;

    public static float fallSpeed = 1.0f;

    public Text hud_score;
    public Text hud_level;
    public Text hud_line;

    public AudioClip clearRow;

    private int numberOfRowsThisTurn = 0;

    private int numLinesCleared = 0;

    private AudioSource audioSource;

    private GameObject previewTetromino;
    private GameObject nextTetromino;

    private bool gameStarted = false;

    private Vector2 previewTetrominoPosition = new Vector2(-6.5f, 16f);
    void Start()
    {
        currentLevel = startingLevel;

        currentScore = 0;

        SpawnNextTetromino();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdateScore();

        UpdateUI();

        UpdateLevel();

        UpdateSpeed();
    }
    void UpdateLevel()
    {
        //only updating the levels if the player starts at level 0 (if not, then only if the numoflines is enough to increase the level)
        //this is required because the current level is updated dynamically at every frame
        if (startingAtLevelZero == true || startingAtLevelZero == false && numLinesCleared / 10 > startingLevel)
        {
            //updating the level for every 10 lines cleared
            currentLevel = numLinesCleared / 10;
        }
    }
    void UpdateSpeed()
    {
        //updating the fall speed based on the current level (up to a minimum update interval of 0.1 seconds)
        fallSpeed = Mathf.Max(1.0f - (float)currentLevel * 0.1f, 0.1f);
    }
    public void UpdateUI()
    {
        hud_score.text = currentScore.ToString();
        hud_level.text = currentLevel.ToString();
        hud_line.text = numLinesCleared.ToString();
    }
    public void UpdateScore()
    {
        //updating the score based on the number of rows cleared in one go
        if(numberOfRowsThisTurn > 0)
        {
            if(numberOfRowsThisTurn == 1)
            {
                ClearedOneLine();
            }
            else if(numberOfRowsThisTurn == 2)
            {
                ClearedTwoLines();
            }
            else if(numberOfRowsThisTurn == 3)
            {
                ClearedThreeLines();
            }
            else if(numberOfRowsThisTurn == 4)
            {
                ClearedFourLines();
            }

            //increamenting the total number of line cleared for level difficulty purposes
            numLinesCleared += numberOfRowsThisTurn;
            numberOfRowsThisTurn = 0;

            PlayLineClearedSound();
        }
    }
    public void ClearedOneLine()
    {
        currentScore += scoreOneLine + (currentLevel * 10);
    }
    public void ClearedTwoLines()
    {
        currentScore += scoreTwoLine + (currentLevel * 15);
    }
    public void ClearedThreeLines()
    {
        currentScore += scoreThreeLine + (currentLevel * 20);
    }
    public void ClearedFourLines()
    {
        currentScore += scoreFourLine + (currentLevel * 25);
    }

    public void PlayLineClearedSound()
    {
        audioSource.PlayOneShot(clearRow);
    }
    public bool CheckIsAboveGrid(Tetromino tetromino)
    {
        for(int x = 0; x < gridwidth; ++x)
        {
            foreach(Transform mino in tetromino.transform)
            {
                Vector2 pos = Round(mino.position);

                if(pos.y > gridheight -1)
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool CheckIsInsideGrid(Vector2 pos)
    {
        //check if the specified position is within the boundaries
        return ((int)pos.x >= 0 && (int)pos.x < gridwidth && (int)pos.y >= 0);
    }
    public bool IsFullRowAt (int y)
    {
        //The parameter y, is the row we will iterate over in the grid array to check each x position of a transform
        for (int x = 0; x < gridwidth; ++x)
        {
            //if we find a position that returns NULL instead of a transform, we know the row is not full 
            if(grid[x, y] == null)
            {
                return false;
            }
        }

        //how many rows were cleared this turn
        numberOfRowsThisTurn++;

        //otherwise the row is full so we return true
        return true;
    }
    public void DeleteMinoAt(int y)
    {
        //y is the row we iterate over the grid array
        for (int x = 0; x < gridwidth; ++x)
        {
            //we destroy the gameobject of each transform at the current iteration of x,y
            Destroy(grid[x, y].gameObject);

            //then we set the x,y location in the grid to null
            grid[x, y] = null;
        }
    }
    public void MoveRowDown(int y)
    {
        //we iterate over each mino in the row of the y coordinate
        for (int x = 0; x < gridwidth; ++x)
        {
            //we check if the current x and y in the grid is not null
            if(grid[x, y] != null)
            {
                //if it doesn't the we set the current transform to one position bellow in the grid
                grid[x, y - 1] = grid[x, y];

                //then we set it to null
                grid[x, y] = null;

                //and we adjust the position of the sprite to move down by 1
                grid[x, y - 1].position += new Vector3(0, -1, 0);
            }
        }
    }
    public void MoveAllRowsDown(int y)
    {
        //the y coordinate was the row that was just deleted
        for (int i=y; i < gridheight; ++i)
        {
            //so we call move row down for each row up to the gridheight
            MoveRowDown(i);
        }
    }
    public void DeleteRow()
    {
        //after calling it from the Tetromino class we iterate over all the rows to gridheight
        for (int y=0; y<gridheight;++y)
        {
            //we check if the row is full
            if(IsFullRowAt(y))
            {
                //if it is we delete all the minos at that row
                DeleteMinoAt(y);

                //the move all the rows above it down by one
                MoveAllRowsDown(y + 1);

                //and reset the current height that we are checking by one
                --y;
            }
        }
    }
    public void UpdateGrid(Tetromino tetromino)
    {
        for(int y=0; y<gridheight; ++y)
        {
            for (int x=0; x<gridwidth; ++x)
            {
                //we go through all the position of the grid where a mino could be
                if(grid[x,y] != null)
                {
                    if(grid[x,y].parent == tetromino.transform)
                    {
                        //if there isn't a mino there we set that transform in the grid to null
                        grid[x, y] = null;
                    }
                }
            }
        }

        foreach(Transform mino in tetromino.transform)
        {
            Vector2 pos = Round(mino.position);

            //for each mino below the gridheight we  set the grid of the position to the mino transform
            if(pos.y < gridheight)
            {
                grid[(int)pos.x, (int)pos.y] = mino;
            }
        }
    }
    public Transform GetTransformAtGridPosition(Vector2 pos)
    {
        if(pos.y > gridheight -1)
        {
            return null;
        }
        else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }
    public void SpawnNextTetromino()
    {
        if(!gameStarted)
        {
            gameStarted = true;
            //instantiate the next tetromino from the resources folder at a fixed position and rotation above the grid
            nextTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), new Vector2(5.0f, 20.0f), Quaternion.identity);

            //also instantiate the preview tetromino and disable it so it won't move
            previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), previewTetrominoPosition, Quaternion.identity);
            previewTetromino.GetComponent<Tetromino>().enabled = false;
        }
        else
        {
            //setting the next tetromino to the one that has been previewed and activating it
            previewTetromino.transform.localPosition = new Vector2(5.0f, 20.0f);
            nextTetromino = previewTetromino;
            nextTetromino.GetComponent<Tetromino>().enabled = true;

            //get a new preview
            previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), previewTetrominoPosition, Quaternion.identity);
            previewTetromino.GetComponent<Tetromino>().enabled = false;
        }
        
    }
    public Vector2 Round (Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }
    string GetRandomTetromino()
    {
        int randomTetromino = Random.Range(1, 9);

        string randomTetrominoName = "Tetromino_T";

        switch (randomTetromino)
        {
            case 1:
                randomTetrominoName = "Tetromino_T";
                break;
            case 2:
                randomTetrominoName = "Tetromino_Long";
                break;
            case 3:
                randomTetrominoName = "Tetromino_Square";
                break;
            case 4:
                randomTetrominoName = "Tetromino_j";
                break;
            case 5:
                randomTetrominoName = "Tetromino_L";
                break;
            case 6:
                randomTetrominoName = "Tetromino_S";
                break;
            case 7:
                randomTetrominoName = "Tetromino_Z";
                break;
            case 8:
                randomTetrominoName = "Tetromino_plus";
                break;
        }

        return "Prefabs/" + randomTetrominoName;
    }
    public void GameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
}
