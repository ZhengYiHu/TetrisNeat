using System.Collections.Generic;
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    private bool grounded;
    private Vector2 rotationPoint;
    private GameData gameData;

    private struct GameData{

        public Vector2Int gameOffset;
        public int width;
        public int height;
        public Transform[,] occupiedBlocks;
    }

    public void InitializeGameData(Vector2Int gameOffset, int width,int height, Transform[,] occupiedBlocks)
    {
        gameData.gameOffset = gameOffset;
        gameData.width = width;
        gameData.height = height;
        gameData.occupiedBlocks = occupiedBlocks;
    }


    public void RotateTetromino()
    {
        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
    }

    public void MoveLeft()
    {
        transform.position += Vector3.left;
    }

    public void MoveRight()
    {
        transform.position += Vector3.right;
    }
    
    public void InstantFall()
    {
        while (ValidMove(Vector3.down))
        {
            transform.position += Vector3.down;
        }
        PlaceBlock();
    }

    public bool Fall()
    {
        if (ValidMove(Vector3.down))
        {
            transform.position += Vector3.down;
            return false;
        }
        else
        {
            PlaceBlock();
            return true;
        }

    }

    public void PlaceBlock()
    {
        grounded = true;
        AddToOccupiedBlocks();
        
    }

    public bool ValidMove(Vector3 direction)
    {
        foreach (Transform square in transform)
        {
            
            int x = Mathf.RoundToInt(square.position.x + direction.x);
            int y = Mathf.RoundToInt(square.position.y + direction.y);

            if (x < gameData.gameOffset.x || x >= (gameData.width + gameData.gameOffset.x) || y < gameData.gameOffset.y || gameData.occupiedBlocks[x - gameData.gameOffset.x ,y - gameData.gameOffset.y] != null)
            {
                return false;
            }
        }
        return true;
    }

    public bool ValidRotation()
    {
        
        foreach (Transform square in transform)
        {
            Vector3 newPos = RotatePointAroundPivot(square.position,new Vector3(0,0,90));
            int x = Mathf.RoundToInt(newPos.x);
            int y = Mathf.RoundToInt(newPos.y);

            if (x < gameData.gameOffset.x || x >= (gameData.width + gameData.gameOffset.x) || y < gameData.gameOffset.y || gameData.occupiedBlocks[x - gameData.gameOffset.x, y - gameData.gameOffset.y] != null)
            {
                return false;
            }
        }
        return true;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 angle) {
        Vector3 pivot = transform.TransformPoint(rotationPoint);
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angle) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }


public void AddToOccupiedBlocks()
    {
        foreach (Transform square in transform)
        {

            int x = Mathf.RoundToInt(square.position.x - gameData.gameOffset.x);
            int y = Mathf.RoundToInt(square.position.y - gameData.gameOffset.y);

            gameData.occupiedBlocks[x, y] = square;
        }
    }

    public Vector2Int[] GetSquaresCoordinates()
    {
        List<Vector2Int> squaresCoordinates = new List<Vector2Int>();
        foreach (Transform square in transform)
        {

            int x = Mathf.RoundToInt(square.position.x - gameData.gameOffset.x);
            int y = Mathf.RoundToInt(square.position.y - gameData.gameOffset.y);
            squaresCoordinates.Add(new Vector2Int(x, y));
        }
        return squaresCoordinates.ToArray();
    }

    public void SetRotationPoint(int i)
    {
        switch (i)
        {
            case 0:
                rotationPoint = new Vector2(0, 0);
                break;
            case 1:
                rotationPoint = new Vector2(0.5f, 0.5f);
                break;
            case 2:
                rotationPoint = new Vector2(0, 0);
                break;
            case 3:
                rotationPoint = new Vector2(1, 0);
                break;
            case 4:
                rotationPoint = new Vector2(0.5f, 0.5f);
                break;
            case 5:
                rotationPoint = new Vector2(-1, 0);
                break;
            case 6:
                rotationPoint = new Vector2(1, 0);
                break;
            default:
                rotationPoint = new Vector2(0, 0);
                break;
        }
    }

    /*
    ------------------------------------------------------------------------------
    Getters and setters
    */

    public bool GetGrounded()
    {
        return grounded;
    }

    public void SetGrounded(bool newGrounded)
    {
        grounded = newGrounded;
    }

   
}
