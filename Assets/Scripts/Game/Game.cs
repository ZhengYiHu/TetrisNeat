
using UnityEngine;

public class Game : MonoBehaviour
{
   
    //game size
    private static int width;
    private static int height;

    //Prefab models to instantiate
    public GameObject[] tetrominos;

    //memory matrices to keep track of which blocks are occupied by squares
    private Transform[,] occupiedBlocks = new Transform[width, height];
    private bool[,] binaryMatrix = new bool[width, height];

    private GameObject currentTetromino;
    private Vector2Int gameOffset; 
    private float previousTime;
    private float clockTime = 0.05f;
    private int score;
    private bool gameOver = false;

    //private System.Random rng = new System.Random(1);

    // Start is called before the first frame update
    void Start()
    {
        GenerateNewTetromino();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            if (!CheckGameOver())
            {
                CleanEmptyObjects();
                if (currentTetromino.GetComponent<Tetromino>().GetGrounded())
                {
                    GenerateNewTetromino();
                }
                UpdateBinaryMatrix();
                score++;
            }
            else
            {
                gameOver = true;
            }
        }
    }



    void GenerateNewTetromino()
    {
        int i = Random.Range(0,tetrominos.Length);

        currentTetromino = Instantiate(tetrominos[i], transform);
        currentTetromino.GetComponent<Tetromino>().SetRotationPoint(i);
        currentTetromino.GetComponent<Tetromino>().InitializeGameData(gameOffset, width, height, occupiedBlocks);
        
    }

    public void ClockTickFall()
    {
        Tetromino tetromino = currentTetromino.GetComponent<Tetromino>();
        //move down every clock tic
        if (tetromino.Fall())
        {
            DestroyLines();
        }
    }

    public void ExecuteCommand(int commandId)
    {
        Tetromino tetromino = currentTetromino.GetComponent<Tetromino>();

       

        switch (commandId)
        {
            case 0:
                tetromino.InstantFall();
                DestroyLines();

                break;
            case 1:
                if (tetromino.ValidRotation())
                {
                    tetromino.RotateTetromino();
                }
                break;
            case 2:
                
                if (tetromino.ValidMove(Vector3.right))
                {
                    tetromino.MoveRight();
                }
                break;
            case 3:
               
                if (tetromino.ValidMove(Vector3.left))
                {
                    tetromino.MoveLeft();
                }
                break;
            default:
                break;
        }
    }


    //For the game to be played manually.
    //For testing the game only
    public void GetManualCommand()
    {
        Tetromino tetromino = currentTetromino.GetComponent<Tetromino>();
        if (!tetromino.GetGrounded())
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (tetromino.ValidMove(Vector3.left))
                {
                    tetromino.MoveLeft();
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (tetromino.ValidMove(Vector3.right))
                {
                    tetromino.MoveRight();
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (tetromino.ValidRotation())
                {
                    tetromino.RotateTetromino();
                }
            }

            else if (Input.GetKeyDown(KeyCode.Space))
            {
                tetromino.InstantFall();
                DestroyLines();
            }

            if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? clockTime / 10 : clockTime))
            {
                if (tetromino.Fall())
                {
                    DestroyLines();
                }
                previousTime = Time.time;
            }
        }

    }

    public void DestroyLines()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            if (FindLine(y))
            {
                for (int x = 0; x < width; x++)
                {
                    Destroy(occupiedBlocks[x, y].gameObject);
                }
                MoveDown(y);
                score += 200;
                Debug.Log(gameObject.name+ " Destroyed a line");
            }

        }
    }

    public bool FindLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (occupiedBlocks[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }

    public void MoveDown(int i)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = i; y < height - 1; y++)
            {
                occupiedBlocks[x, y] = occupiedBlocks[x, y + 1];
                if (occupiedBlocks[x, y] != null)
                {
                    occupiedBlocks[x, y].position += Vector3.down;
                }
            }
        }
    }

   

    public void CleanEmptyObjects()
    {
        foreach (Transform tetromino in transform)
        {
            int count = 0;
            foreach (Transform square in tetromino.transform)
            {
                count++;
            }
            if (count == 0 && tetromino.tag == "Tetromino")
            {
                Destroy(tetromino.gameObject);
            }
        }
    }

    public bool CheckGameOver()
    {
        for (int x = 0; x < width; x++)
        {
            if (occupiedBlocks[x, (int)transform.position.y - gameOffset.y] != null)
            {
                return true;
            }
        }
        if(score > 10000)
        {
            return true;
        }
        return false;
    }

    public void UpdateBinaryMatrix()
    {
        Tetromino tetromino = currentTetromino.GetComponent<Tetromino>();
        Vector2Int[] tetrominoSquaresCoordinates = tetromino.GetSquaresCoordinates();


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
                if(occupiedBlocks[x,y] != null)
                {
                    binaryMatrix[x, y] = true;
                }
                else
                {
                    binaryMatrix[x, y] = false;
                }
            }
        }

        for (int i = 0; i < tetrominoSquaresCoordinates.Length; i++)
        {
            binaryMatrix[tetrominoSquaresCoordinates[i].x, tetrominoSquaresCoordinates[i].y] = true;
        }
    }

    public void ResetGame()
    {
        Destroy(currentTetromino);
        foreach (Transform block in occupiedBlocks)
        {
            if(block != null) Destroy(block.gameObject);
        }
        occupiedBlocks = new Transform[width, height];
        binaryMatrix = new bool[width, height];
        score = 0;
        gameOver = false;
        //rng = new System.Random(1);
        GenerateNewTetromino();
    }

    

    /*
    ------------------------------------------------------------------------------
    Getters and setters
    */


    //Get game score
    public int GetScore()
    {
        return score;
    }

    //Get game offset
    public Vector2Int GetOffset()
    {
        return gameOffset;
    }

    //Change game offset
    public void SetOffset(Vector2Int newOffset)
    {
        gameOffset = newOffset;
    }


    public bool[,] GetBinaryMatrix()
    {
        return binaryMatrix;
    }

    public bool GetGameOver()
    {
        return gameOver;
    }

    public void SetClockTime(float newClockTime)
    {
        clockTime = newClockTime;
    }

    public void SetWidthHeight(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;
    }

}
