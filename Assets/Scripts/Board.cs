using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoes;
    public Tilemap tilemap{get; private set;}
    public Piece activePiece{get; private set;}
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10,20);
    public Tetromino heldTetromino = Tetromino.empty;
    public Sprite[] heldPieces;
    public SpriteRenderer currentHeldPiece;

    //scoring
    public Text scoreText;
    public Text levelText;
    public int score = 0;
    public int linesCleared = 0;
    public int level = 0;

    public RectInt bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);
        
        if(IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(activePiece);
        }else 
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        level = 0;
        linesCleared = 0;
        score = 0;
        currentHeldPiece.sprite = heldPieces[7];
        scoreText.text = score.ToString();
        levelText.text = level.ToString();
        this.tilemap.ClearAllTiles();
    }

    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile); 
        }
    }

    public void Clear(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null); 
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.bounds;
        int row = bounds.yMin, linesClearedTMP = 0;

        while(row < bounds.yMax)
        {   
            if(IsLineFull(row))
            {
                ClearLine(row);
                linesClearedTMP++;
            }else
            {
                row++;
            }
        }

        if (linesClearedTMP != 0)
        {
            linesCleared += linesClearedTMP;
            level = linesCleared / 10;
            score += calculateScore(level, linesCleared);
        }

        scoreText.text = score.ToString();
        levelText.text = level.ToString();
    }

    private bool IsLineFull(int row)
    {   
        RectInt bounds = this.bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if(!this.tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void ClearLine(int row)
    {
        RectInt bounds = this.bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.yMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row , 0);
                this.tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    private int calculateScore(int level, int linesCleared)
    {
        int newScore = 0, multiplier = 1;

        switch (linesCleared)
        {
            case 1:
                multiplier = 40;
            break;
            case 2:
                multiplier = 100;
            break;
            case 3:
                multiplier = 300;
            break;
            case 4:
                multiplier = 1200;
            break;
        }
        
        newScore = multiplier * (level + 1);
        return newScore;
    }
}
