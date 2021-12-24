using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tetromino : MonoBehaviour
{
    float fall = 0;

    public bool allowRotation = true;
    public bool limitRotation = false;

    public AudioClip moveSound;
    public AudioClip rotateSound;
    public AudioClip landSound;

    public int individualScore = 10;
    private float individualScoreTime;

    private AudioSource audioSource;

    private float fallSpeed;
    private float continousVerticalSpeed = 0.05f; // interval between vertical moves(speed at which tetromino falls when down arrow key is held)
    private float continousHorizontalSpeed = 0.1f; //interval between horizontal moves(speed at which tetromino will move sideway when key is held)
    private float buttonDownWaitMax = 0.2f; //how long to wait before the tetromino recognizes a button is being held down.

    private float verticalTimer = 0;
    private float horizontalTimer = 0;

    private float buttonDownWaitTimerHorizontal = 0;
    private float buttonDownWaitTimerVertical = 0;

    private bool moveImmediateHorizontal = false;
    private bool moveImmediateVertical = false;

    private int touchSensitivityHorizontal = 8;
    private int touchSensitivityVertical = 4;

    Vector2 previousUnitPosition = Vector2.zero;
    Vector2 direction = Vector2.zero;

    bool moved = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        CheckUserInput();

        UpdateIndividualScore();

        UpdateFallSpeed();
    }
    void UpdateFallSpeed()
    {
        fallSpeed = Game.fallSpeed;
    }
    void UpdateIndividualScore()
    {
        if(individualScoreTime < 1)
        {
            individualScoreTime += Time.deltaTime;
        }
        else
        {
            //deduct the score each individual piece gives by 1 every second up to minimum of 1 point
            individualScoreTime = 0;

            individualScore = Mathf.Max(individualScore - 1, 1);
        }
    }
    void CheckUserInput()
    {
        //compiling the code depending on the platform we use
#if UNITY_IOS || UNITY_ANDROID
        //if the screen is touched at least once
        if(Input.touchCount > 0)
        {
            //we store the first touch
            Touch t = Input.GetTouch(0);

            if(t.phase == TouchPhase.Began)
            {
                previousUnitPosition = new Vector2(t.position.x, t.position.y);
            }
            else if(t.phase == TouchPhase.Moved)
            {
                Vector2 touchDeltaPosition = t.deltaPosition;
                direction = touchDeltaPosition.normalized;//will return 1 or -1 based on whether we are moving left or right

                //moving left by checking if the touch has been dragged over a distance greater than the sensitivity, then checking the direction from the normalized Vector2, and making sure it is not being dragged vertically(indicating a downward movement)
                if(Mathf.Abs(t.position.x - previousUnitPosition.x) >= touchSensitivityHorizontal && direction.x < 0 && t.deltaPosition.y > -10 && t.deltaPosition.y < 10)
                {
                    MoveLeft();
                    previousUnitPosition = t.position;
                    moved = true;
                }
                else if(Mathf.Abs(t.position.x - previousUnitPosition.x) >= touchSensitivityHorizontal && direction.x > 0 && t.deltaPosition.y > -10 && t.deltaPosition.y < 10)
                {
                    MoveRight();
                    previousUnitPosition = t.position;
                    moved = true;
                }
                //same but checking on the vertical axis
                else if(Mathf.Abs(t.position.y - previousUnitPosition.y) >= touchSensitivityVertical && direction.y < 0 && t.deltaPosition.x > -10 && t.deltaPosition.x < 10)
                {
                    MoveDown();
                    previousUnitPosition = t.position;
                    moved = true;
                }
            }
            else if(t.phase == TouchPhase.Ended)
            {
                //checking if the touch was a simple press and not a hold for movement
                if(!moved && t.position.x > Screen.width / 4)
                {
                    Rotate();
                }
                moved = false;
            }
        }
        if(Time.time - fall >= fallSpeed)
        {
            MoveDown();
        }
#else
        //reseting the timer when the player releases the buttons.
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            
            buttonDownWaitTimerHorizontal = 0;
            moveImmediateHorizontal = false;
            horizontalTimer = 0;

        }
        if(Input.GetKeyUp(KeyCode.DownArrow))
        {
            moveImmediateVertical = false;
            verticalTimer = 0;
            buttonDownWaitTimerVertical = 0;
        }

        //checks the keys pressed by user in order to move the piece to left, right or down by 1 or to rotate it 90 degrees
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate();
        }

        if (Input.GetKey(KeyCode.DownArrow) || Time.time - fall >= fallSpeed)//if the player presses down, or 1 second has passed in real time then the piece will go down by one.
        {
            MoveDown();
        }
#endif
    }
    void MoveLeft()
    {
        if (moveImmediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }
            if (horizontalTimer <= continousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }

        if (!moveImmediateHorizontal)
            moveImmediateHorizontal = true;

        horizontalTimer = 0;

        transform.position += new Vector3(-1, 0, 0);

        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
            PlayMoveAudio();
        }
        else
        {
            transform.position += new Vector3(1, 0, 0);
        }
    }
    void MoveRight()
    {
        if (moveImmediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }
            if (horizontalTimer <= continousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }
        if (!moveImmediateHorizontal)
            moveImmediateHorizontal = true;

        horizontalTimer = 0;

        transform.position += new Vector3(1, 0, 0);

        //if the position is valid we will update the grid to contain the new tetromino
        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
            PlayMoveAudio();
        }
        else
        {
            //if not we will cancel the move by moving it back in the same frame
            transform.position += new Vector3(-1, 0, 0);
        }
    }
    void MoveDown()
    {
        //we check if the key press is simple or continous(held)
        if (moveImmediateVertical)
        {
            //the input delay to verify that a button is pressed.
            if (buttonDownWaitTimerVertical < buttonDownWaitMax)
            {
                buttonDownWaitTimerVertical += Time.deltaTime;
                return;
            }
            //the piece will only move in steps and not continously
            if (verticalTimer < continousVerticalSpeed)
            {
                verticalTimer += Time.deltaTime;
                return;
            }
        }

        if (!moveImmediateVertical)
            moveImmediateVertical = true;

        verticalTimer = 0;

        transform.position += new Vector3(0, -1, 0);

        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);

            if (Input.GetKey(KeyCode.DownArrow))
            {
                PlayMoveAudio();
            }
        }
        else
        {
            //if the piece can't go down anymore it means it has reached the bottom
            transform.position += new Vector3(0, 1, 0);

            //so we check if we need to clear any rows
            FindObjectOfType<Game>().DeleteRow();

            //we check if the piece landed above the grid thus ending the game
            if (FindObjectOfType<Game>().CheckIsAboveGrid(this))
            {
                FindObjectOfType<Game>().GameOver();
            }

            //then we disable the piece, play land audio and spawn the next one
            PlayLandAudio();

            enabled = false;

            FindObjectOfType<Game>().SpawnNextTetromino();

            Game.currentScore += individualScore;
        }

        fall = Time.time;
    }
    void Rotate()
    {
        //rotation is not necessary to certain pieces (square, cross)
        if (allowRotation)
        {
            //rotation can have only two positions on certain pieces(S, Z)
            if (limitRotation)
            {
                if (transform.rotation.eulerAngles.z >= 90)
                {
                    transform.Rotate(0, 0, -90);
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                }
            }
            else
            {
                transform.Rotate(0, 0, 90);
            }
            if (CheckIsValidPosition())
            {
                FindObjectOfType<Game>().UpdateGrid(this);
                PlayRotateAudio();
            }
            else
            {
                if (limitRotation)
                {
                    if (transform.rotation.eulerAngles.z >= 90)
                    {
                        transform.Rotate(0, 0, -90);
                    }
                    else
                    {
                        transform.Rotate(0, 0, 90);
                    }
                }
                else
                {
                    transform.Rotate(0, 0, -90);
                }
            }
        }
    }
    bool CheckIsValidPosition()
    {
        //we check if the tetramino is in a valid position by checking the coordinates of each individual mino from the transform
        foreach (Transform mino in transform)
        {
            Vector2 pos = FindObjectOfType<Game>().Round(mino.position);

            //if a mino is outside the grid then it is an invalid position
            if(FindObjectOfType<Game>().CheckIsInsideGrid(pos) == false)
            {
                return false;
            }

            //if a position on the grid is occupied by another mino (either from a differen tetromino of from the parent one) then it's also an invalid position
            if(FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent != transform)
            {
                return false;
            }
        }

        return true;
    }
    void PlayMoveAudio()
    {
        //plays audio clip once upon movement
        audioSource.PlayOneShot(moveSound);
    }
    void PlayRotateAudio()
    {
        audioSource.PlayOneShot(rotateSound);
    }
    void PlayLandAudio()
    {
        audioSource.PlayOneShot(landSound);
    }
}
