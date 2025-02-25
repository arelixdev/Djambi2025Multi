using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class DjambiBoard : MonoBehaviour
{
    public static DjambiBoard Instance { get; set; }

    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material labyTileMaterial;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private float dragOffset = 0.5f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private GameObject[] playerImages;

    public bool oneTimeAgain = false;
    private int keepTeamWithoutChef = 0;
    public bool chefTurn;
    public List<int> chefEncercled = new List<int>();
    public PieceType movePieceAgain;

    private PieceType[,] boardPieces;
    private PieceType currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private PieceType movePieceDragging;
    private const int TILE_COUNT_X = 9;
    private const int TILE_COUNT_Y = 9;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool reporterAction = false;
    private List<PieceType> reporterTargets = new List<PieceType>();
    private PieceType reporterPiece;
    private int teamTurn;

    private GameObject chefInLab;

    private List<PieceType> allChef = new List<PieceType>();
    private List<PieceType> allNecro = new List<PieceType>();

    private List<PieceType> redUnits = new List<PieceType>();
    private List<PieceType> blueUnits = new List<PieceType>();
    private List<PieceType> greenUnits = new List<PieceType>();
    private List<PieceType> yellowUnits = new List<PieceType>();

    private List<PieceType> allUnits = new List<PieceType>();

    private bool[] looseTeam = new bool[4];

    //Multi logic
    private int playerCount = -1;
    private int currentTeam = -1;
    private int numberPieces = -1;
    private bool localGame = true;
    private bool startGame;

    public int GetPlayerCount()
    {
        return playerCount;
    }

    public GameObject GetTiles(int posX,int posY)
    {
        return tiles[posX, posY];
    }

    public int GetCurrentTeam()
    {
        return currentTeam;
    }


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        teamTurn = 0;
        TurnInterfaceManager.instance.SetTurn(teamTurn);

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        RegisterEvents();
    }

    private void InitBoardUnit()
    {
        startGame = true;
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void Update() {
        if(!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }


        if(!startGame)
        {
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if(Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log(boardPieces[hitPosition.x, hitPosition.y]);
            }

            //if we're hovering a tile after not hovering any tile
            if(currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //if we're hovering a tile, change the previous one
            if(currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //Debug.Log(boardPieces[hitPosition.x, hitPosition.y]);

            //if we press down mouse button
            if(Input.GetMouseButtonDown(0) && movePieceDragging == null)
            {
                
                if(boardPieces[hitPosition.x, hitPosition.y] != null && !boardPieces[hitPosition.x, hitPosition.y].isDead && !movePieceAgain)
                {
                    //Is it our turn
                    if(boardPieces[hitPosition.x, hitPosition.y].team == teamTurn  && currentTeam == teamTurn)
                    {
                        currentlyDragging = boardPieces[hitPosition.x, hitPosition.y];
                        //Get list of where i can go, hightlight tiles
                        availableMoves = currentlyDragging.GetAvailableMoves(ref boardPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    } 
                } else if(boardPieces[hitPosition.x, hitPosition.y] != null && !boardPieces[hitPosition.x, hitPosition.y].isDead && movePieceAgain)
                {
                    if(movePieceAgain == boardPieces[hitPosition.x, hitPosition.y])
                    {
                        currentlyDragging = boardPieces[hitPosition.x, hitPosition.y];
                        //Get list of where i can go, hightlight tiles
                        availableMoves = currentlyDragging.GetAvailableMoves(ref boardPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    }
                }
            }
            //if we release mouse button
            if(currentlyDragging != null  && Input.GetMouseButtonUp(0) && movePieceDragging == null && !reporterAction)
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                if(ContainsValidMove(ref availableMoves, new Vector2(hitPosition.x, hitPosition.y)))
                {
                    int pieceId = currentlyDragging.pieceId;
                    PieceType piece = currentlyDragging;
                    MoveTo(currentlyDragging.pieceId, hitPosition.x, hitPosition.y);


                    if(piece.form == PieceForm.Reporter)
                    {
                        List<PieceType> adjacentEnemies = GetAdjacentPieces(piece.currentX, piece.currentY, piece.team);

                        if (adjacentEnemies.Count > 0)
                        {
                            piece.GetComponent<Reporter>().ReporterAction(adjacentEnemies);
                        } 
                    }

                    //Net Implementation
                    NetMakeMove nm = new NetMakeMove();
                    nm.pieceId = pieceId;
                    nm.destinationX = hitPosition.x;
                    nm.destinationY = hitPosition.y;
                    nm.teamId = currentTeam;
                    if(movePieceDragging == null && !reporterAction )
                    {
                        nm.endTurn = 1;
                    } else 
                    {
                        nm.endTurn = 0;
                    }
                    Client.Instance.SendToServer(nm);

                } else 
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                    RemoveHighlightTiles();
                    
                }
                currentlyDragging = null;

                
            }

            if(movePieceDragging != null && !reporterAction)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    Vector2Int previousPosition = new Vector2Int(movePieceDragging.currentX, movePieceDragging.currentY);

                    if(tiles[hitPosition.x, hitPosition.y].name.Contains("LabyTile") || boardPieces[hitPosition.x, hitPosition.y] != null)
                    {
                        return;
                    }

                    MoveTo(movePieceDragging.pieceId, hitPosition.x, hitPosition.y, true);

                    //Net Implementation
                    NetMakeMove nm = new NetMakeMove();
                    nm.pieceId = movePieceDragging.pieceId;
                    nm.destinationX = hitPosition.x;
                    nm.destinationY = hitPosition.y;
                    nm.teamId = currentTeam;
                    nm.endTurn = 1;
                    Client.Instance.SendToServer(nm);

                    movePieceDragging.GetComponent<BoxCollider>().enabled = true;
                    movePieceDragging = null;
                }
            }

            if(reporterAction && Input.GetMouseButtonDown(0))
            {
                if(boardPieces[hitPosition.x, hitPosition.y] != null && !boardPieces[hitPosition.x, hitPosition.y].isDead && reporterTargets.Contains(boardPieces[hitPosition.x, hitPosition.y]))
                {
                    boardPieces[hitPosition.x, hitPosition.y].Die();
                    //Net Implementation
                    NetMakeKill nm = new NetMakeKill();
                    nm.pieceId = boardPieces[hitPosition.x, hitPosition.y].pieceId;
                    nm.teamSwap = reporterPiece.team;

                    Client.Instance.SendToServer(nm);
                    if(boardPieces[hitPosition.x, hitPosition.y].form == PieceForm.Chef && tiles[hitPosition.x, hitPosition.y].name.Contains("LabyTile"))
                    {
                        //All EnemyUnits switch to your team 
                        TurnInterfaceManager.instance.DesactivateChef(chefInLab.GetComponent<PieceType>().team);
                        chefInLab = null;
                        
                    }

                    reporterTargets.Clear();
                    reporterPiece = null;
                    //ChangeTurn();
                    reporterAction = false;

                    
                }
            }
        } else {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if(currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        if(currentlyDragging )
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }

        if(movePieceDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray, out distance))
            {
                movePieceDragging.SetPosition(ray.GetPoint(distance)+ Vector3.up * dragOffset);
            }
        }
    }

    //Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY){
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX/2) * tileSize, 0, (tileCountY/2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for(int x = 0; x < tileCountX; x++){
            for(int y = 0; y < tileCountY; y++){
                if(x == 4 && y == 4){
                    tiles[x, y] = GenerateLabyTile(tileSize, x, y);
                } else {
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y);
                }
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("Tile_X:{0}_Y:{1}", x, y));
        
        tileObject = GenerateTile(tileObject, tileSize, x, y, tileMaterial);

        return tileObject;
    }
    private GameObject GenerateLabyTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("LabyTile_X:{0}_Y:{1}", x, y));

        tileObject = GenerateTile(tileObject, tileSize, x, y, labyTileMaterial);

        return tileObject;
    }

    private GameObject GenerateTile(GameObject tileObject, float tileSize, int x, int y, Material typeOfTile)
    {
        tileObject.transform.SetParent(transform);

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = typeOfTile;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x*tileSize, yOffset, y*tileSize) - bounds;
        vertices[1] = new Vector3(x*tileSize, yOffset, (y+1)*tileSize) - bounds;
        vertices[2] = new Vector3((x+1)*tileSize, yOffset, y*tileSize) - bounds;
        vertices[3] = new Vector3((x+1)*tileSize, yOffset, (y+1)*tileSize) - bounds;

        int[] tris = new int[] {0, 1, 2, 1, 3, 2};

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Spawning pieces
    private void SpawnAllPieces(){
        boardPieces = new PieceType[TILE_COUNT_X, TILE_COUNT_Y];

        int redTeam = 0, blueTeam = 1, greenTeam = 2, yellowTeam = 3;

        //Red team
        boardPieces[0, 0] = SpawnSinglePiece(PieceForm.Chef, redTeam);
        boardPieces[1, 0] = SpawnSinglePiece(PieceForm.Assassin, redTeam);
        boardPieces[2, 0] = SpawnSinglePiece(PieceForm.Militant, redTeam);
        boardPieces[0, 1] = SpawnSinglePiece(PieceForm.Reporter, redTeam);
        boardPieces[1, 1] = SpawnSinglePiece(PieceForm.Diplomate, redTeam);
        boardPieces[2, 1] = SpawnSinglePiece(PieceForm.Militant, redTeam);
        boardPieces[0, 2] = SpawnSinglePiece(PieceForm.Militant, redTeam);
        boardPieces[1, 2] = SpawnSinglePiece(PieceForm.Militant, redTeam);
        boardPieces[2, 2] = SpawnSinglePiece(PieceForm.Necromobile, redTeam);


        //Blue team
        boardPieces[8, 0] = SpawnSinglePiece(PieceForm.Chef, blueTeam);
        boardPieces[7, 0] = SpawnSinglePiece(PieceForm.Assassin, blueTeam);
        boardPieces[6, 0] = SpawnSinglePiece(PieceForm.Militant, blueTeam);
        boardPieces[8, 1] = SpawnSinglePiece(PieceForm.Reporter, blueTeam);
        boardPieces[7, 1] = SpawnSinglePiece(PieceForm.Diplomate, blueTeam);
        boardPieces[6, 1] = SpawnSinglePiece(PieceForm.Militant, blueTeam);
        boardPieces[8, 2] = SpawnSinglePiece(PieceForm.Militant, blueTeam);
        boardPieces[7, 2] = SpawnSinglePiece(PieceForm.Militant, blueTeam);
        boardPieces[6, 2] = SpawnSinglePiece(PieceForm.Necromobile, blueTeam);

        //Green team
        boardPieces[8, 8] = SpawnSinglePiece(PieceForm.Chef, greenTeam);
        boardPieces[7, 8] = SpawnSinglePiece(PieceForm.Assassin, greenTeam);
        boardPieces[6, 8] = SpawnSinglePiece(PieceForm.Militant, greenTeam);
        boardPieces[8, 7] = SpawnSinglePiece(PieceForm.Reporter, greenTeam);
        boardPieces[7, 7] = SpawnSinglePiece(PieceForm.Diplomate, greenTeam);
        boardPieces[6, 7] = SpawnSinglePiece(PieceForm.Militant, greenTeam);
        boardPieces[8, 6] = SpawnSinglePiece(PieceForm.Militant, greenTeam);
        boardPieces[7, 6] = SpawnSinglePiece(PieceForm.Militant, greenTeam);
        boardPieces[6, 6] = SpawnSinglePiece(PieceForm.Necromobile, greenTeam);

        //Yellow team
        boardPieces[0, 8] = SpawnSinglePiece(PieceForm.Chef, yellowTeam);
        boardPieces[1, 8] = SpawnSinglePiece(PieceForm.Assassin, yellowTeam);
        boardPieces[2, 8] = SpawnSinglePiece(PieceForm.Militant, yellowTeam);
        boardPieces[0, 7] = SpawnSinglePiece(PieceForm.Reporter, yellowTeam);
        boardPieces[1, 7] = SpawnSinglePiece(PieceForm.Diplomate, yellowTeam);
        boardPieces[2, 7] = SpawnSinglePiece(PieceForm.Militant, yellowTeam);
        boardPieces[0, 6] = SpawnSinglePiece(PieceForm.Militant, yellowTeam);
        boardPieces[1, 6] = SpawnSinglePiece(PieceForm.Militant, yellowTeam);
        boardPieces[2, 6] = SpawnSinglePiece(PieceForm.Necromobile, yellowTeam);
    }
    private PieceType SpawnSinglePiece(PieceForm type, int team){
        PieceType piece = Instantiate(prefabs[(int)type-1], transform).GetComponent<PieceType>();

        piece.form = type;
        piece.team = team;
        numberPieces++;
        piece.pieceId = numberPieces;
        piece.SetupTeamMaterial(teamMaterials[team]);

        if(type == PieceForm.Chef)
        {
            allChef.Add(piece);
        }else if (type == PieceForm.Necromobile)
        {
            allNecro.Add(piece);
        }

        if(team == 0)
        {
            redUnits.Add(piece);
        } else if(team == 1)
        {
            blueUnits.Add(piece);
        } else if(team == 2)
        {
            greenUnits.Add(piece);
        } else if(team == 3)
        {
            yellowUnits.Add(piece);
        }

        allUnits.Add(piece);

        return piece;
    }

    //Positioning pieces
    private void PositionAllPieces()
    {
        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(boardPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                    
                }
            }
        }
    }

    public void PositionSinglePiece(int x, int y, bool force = false)
    {
        boardPieces[x, y].currentX = x;
        boardPieces[x, y].currentY = y;
        boardPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2);
    }

    //Highlighting tiles
    private void HighlightTiles()
    {
        for(int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    private void RemoveHighlightTiles()
    {
        for(int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }
    
    //Operations
    
    private void ChangeTurn()
    {
        TryIfChefIsEncircled();

        CheckIfGameOver();

        if (movePieceAgain != null && !oneTimeAgain)
        {
            oneTimeAgain = true;
            return;
        }

        oneTimeAgain = false;
        movePieceAgain = null;

        

        // Si un chef est dans le laboratoire, gérer son tour séparément
        if (chefInLab != null)
        {
            var chefTeam = chefInLab.GetComponent<PieceType>().team;

            // Si c'est actuellement le tour du chef
            if (chefTurn)
            {
                // Après le tour du chef, revenir au joueur prévu initialement
                teamTurn = keepTeamWithoutChef;
                
                chefTurn = false; // Le chef ne joue plus pour ce cycle
            }
            else if (teamTurn != chefTeam) // Assurez-vous que le chef ne rejoue pas immédiatement après lui-même
            {
                // Sauvegarde l'équipe actuelle pour y revenir après le chef
                keepTeamWithoutChef = teamTurn;

                // Passe le tour au chef
                teamTurn = chefTeam;
                chefTurn = true;

                TurnInterfaceManager.instance.SetTurn(teamTurn);
                if(!looseTeam[teamTurn])
                {
                    if(localGame)
                    {
                        currentTeam = teamTurn;
                    }
                    return;
                } else 
                {
                    TurnInterfaceManager.instance.DesactivateChef(teamTurn);
                    teamTurn = keepTeamWithoutChef;
                    chefTurn = false;
                }
            }
        }

        

        // Passer au prochain joueur
        teamTurn = (teamTurn + 1) % 4;

        // Sauter les équipes éliminées
        while (looseTeam[teamTurn])
        {
            teamTurn = (teamTurn + 1) % 4;
        }

        if(chefInLab != null && teamTurn == chefInLab.GetComponent<PieceType>().team)
        {
            teamTurn = (teamTurn + 1) % 4;
        }

        if(localGame)
        {
            currentTeam = teamTurn;
        }

        TurnInterfaceManager.instance.SetTurn(teamTurn);
    }

    private PieceType FindPieceById(int id)
    {
        foreach (PieceType piece in allUnits)
        {
            if (piece.pieceId == id)
            {
                return piece;
            }
        }
        return null;
    }

    private void TryIfChefIsEncircled()
    {
        foreach (PieceType chef in allChef)
        {
            
            Vector2Int chefPosition = new Vector2Int(chef.currentX, chef.currentY);
            bool isEncircled = IsUnitEncircled(chefPosition, chef);

            if(!chef.isDead)
            {
                if(isEncircled)
                {
                    looseTeam[chef.team] = true;
                    TurnInterfaceManager.instance.SetDead(chef.team);
                    chefEncercled.Add(chef.team);
                    chef.Die();
                    
                } 
            }

            
        }
    }

    private void CheckIfGameOver()
    {
        int teamAlive = 0;
        int teamWinner = -1;

        for (int i = 0; i < looseTeam.Length; i++)
        {
            if (!looseTeam[i])
            {
                teamAlive++;
                teamWinner = i;
            }
        }

        if (teamAlive == 1)
        {
            Debug.Log("Game Over, team " + teamWinner + " wins!");
            GameOverInterfaceManager.instance.ShowGameOver(teamWinner);
        }
    }

    bool IsUnitEncircled(Vector2Int chefPos, PieceType chef)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(chefPos);
        visited.Add(chefPos);

        while(queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();

            // Vérifie les 8 directions (haut, bas, gauche, droite, et diagonales)
            Vector2Int[] directions = {
                new Vector2Int(0, 1), new Vector2Int(0, -1), // Haut, bas
                new Vector2Int(1, 0), new Vector2Int(-1, 0), // Droite, gauche
                new Vector2Int(1, 1), new Vector2Int(1, -1), // Diagonales
                new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };

            foreach(Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentPos + dir;

                if (!IsValidPosition(neighborPos))
                {
                    // Si hors plateau, considéré comme une case bloquante
                    continue;
                }

                if (visited.Contains(neighborPos))
                    continue;

                if(boardPieces[neighborPos.x, neighborPos.y] == null)
                {
                    return false;
                }

                if(boardPieces[neighborPos.x, neighborPos.y].team != chef.team && !boardPieces[neighborPos.x, neighborPos.y].isDead)
                {
                    return false;
                }

                for(int i = 0; i < allNecro.Count; i++)
                {
                    if(allNecro[i] !=null && allNecro[i].team == chef.team && !allNecro[i].isDead)
                    {
                        return false;
                    }
                }

                if(boardPieces[neighborPos.x, neighborPos.y].team == chef.team && !boardPieces[neighborPos.x, neighborPos.y].isDead)
                {
                    queue.Enqueue(neighborPos);
                    visited.Add(neighborPos);
                }
            }

        }
        return true;
    }

    bool IsValidPosition(Vector2Int pos)
    {
        // Vérifie si la position est dans les limites du plateau
        return pos.x >= 0 && pos.x < TILE_COUNT_X &&
            pos.y >= 0 && pos.y < TILE_COUNT_Y;
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for(int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }
    private void MoveTo(int pieceId, int x, int y, bool moveTo = false)
    {

        PieceType cd = allUnits[pieceId];
        PieceType pieceType = boardPieces[x, y];
        
        Vector2Int previousPosition = new Vector2Int(cd.currentX, cd.currentY);
        if(boardPieces[x, y] != null)
        {
            //Piece Type = piece on To && cd = piece Start
            if(pieceType != null && movePieceDragging != null)
            {
                Debug.Log("1");
                return;
            }
            if(cd.team == pieceType.team && !pieceType.isDead)
            {
                Debug.Log("2");
                return;
            }
            if(pieceType.isDead && cd.form != PieceForm.Necromobile)
            {
                Debug.Log("3");
                return;
            }
            if(cd.form == PieceForm.Reporter)
            {
                Debug.Log("4");
                return;
            }
            if(!pieceType.isDead && cd.form == PieceForm.Necromobile)
            {
                Debug.Log("5");
                return;
            }
            
            //if its another enemy piece
            if(cd.team != pieceType.team && !pieceType.isDead && cd.form != PieceForm.Necromobile)
            {
                //Do something
                if(cd.form == PieceForm.Militant && pieceType.form == PieceForm.Chef)
                {
                    return;
                } 
                if(chefInLab != null && cd.form != PieceForm.Chef &&  pieceType == chefInLab.GetComponent<PieceType>())
                {
                    movePieceAgain = cd;
                }
                cd.Action(pieceType, previousPosition);

                if(pieceType.form == PieceForm.Chef && cd.form != PieceForm.Diplomate)
                {
                    //All EnemyUnits switch to your team 
                    TeamSwitch(pieceType.team, cd.team);
                }
            }
            else if(cd.form == PieceForm.Necromobile && teamTurn == cd.team)
            {
                Debug.Log("Necro Move");
                if(pieceType.isDead && currentTeam == teamTurn)
                {
                    
                    if(chefInLab == null && cd.form != PieceForm.Chef)
                    {
                        movePieceAgain = cd;
                        Debug.Log("MovePieceAgain" + movePieceAgain);
                    }
                    cd.Action(pieceType, previousPosition);
                }

                
            }
        } 

        if(chefInLab != null && cd == chefInLab.GetComponent<PieceType>())
        {
            chefInLab = null;
            boardPieces[previousPosition.x, previousPosition.y] = null;
            teamTurn =  keepTeamWithoutChef;
            TurnInterfaceManager.instance.DesactivateChef(cd.team);
        }

        if(tiles[x, y].name.Contains("LabyTile") && cd.form != PieceForm.Chef && chefInLab == null && cd.form != PieceForm.Necromobile && !pieceType.isDead)
        {
            return;
        }
        else if (tiles[x, y].name.Contains("LabyTile") && cd.form == PieceForm.Chef)
        {
            chefInLab = cd.gameObject;
            if(chefEncercled.Count > 0)
            {
                foreach (int team in chefEncercled)
                {
                    TeamSwitch(team, cd.team);
                }
            }
            TurnInterfaceManager.instance.ActivateChef(cd.team);
        }
        boardPieces[x, y] = cd;

        
        if(!moveTo && !cd.isDead && cd.team == teamTurn && !notResetStart)
        {
            boardPieces[previousPosition.x, previousPosition.y] = null;
        }

        if(notResetStart)
        {
            notResetStart = false;
        }

        PositionSinglePiece(x, y);

        if(currentlyDragging)
        {
            currentlyDragging = null; 
        }
        RemoveHighlightTiles();
        
        return;
    }

    public bool notResetStart; 
    

    private void TeamSwitch(int teamChange, int teamTo)
    {
        looseTeam[teamChange] = true;
        List<PieceType> teamUnits = null;

        TurnInterfaceManager.instance.SetDead(teamChange);

        // Détermine l'équipe concernée
        if (teamChange == 0)
            teamUnits = redUnits;
        else if (teamChange == 1)
            teamUnits = blueUnits;
        else if (teamChange == 2)
            teamUnits = greenUnits;
        else if (teamChange == 3)
            teamUnits = yellowUnits;

        // Vérifie si l'équipe a été trouvée
        if (teamUnits != null)
        {
            teamUnits.ForEach(unit =>
            {
                // Vérifie si l'unité n'est pas morte
                if (!unit.isDead)
                {
                    unit.team = teamTo;
                    if(teamTo == 0)
                    {
                        redUnits.Add(unit);
                    } else if(teamTo == 1)
                    {
                        blueUnits.Add(unit);
                    } else if(teamTo == 2)
                    {
                        greenUnits.Add(unit);
                    } else if(teamTo == 3)
                    {
                        yellowUnits.Add(unit);
                    }

                    // Reset le matériel de l'unité
                    unit.SetupTeamMaterial(teamMaterials[teamTo]);
                }
            });
        }
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one; //Invalid position
    }

    public void SetBoardPiece(int x, int y, PieceType piece)
    {
        boardPieces[x, y] = piece;
        if(piece != null)
        {
            PositionSinglePiece(x, y, false);
        }
        
    }

    internal void SetMovePieceDragging(PieceType pieceType)
    {
        if(currentTeam == teamTurn && movePieceDragging == null)
        {
            Debug.Log("SetMovePieceDragging");
            movePieceDragging = pieceType;
            movePieceDragging.GetComponent<BoxCollider>().enabled = false;
        }


    }

    private int reporterTeam;

    public void SetReporterAction(bool action, List<PieceType> targets, PieceType actionPiece)
    {
        reporterPiece = actionPiece;
        reporterTargets = targets;
        reporterAction = action;
    }

    private List<PieceType> GetAdjacentPieces(int x, int y, int team)
    {
        List<PieceType> adjacentEnemies = new List<PieceType>();

        // Directions: haut, bas, gauche, droite, et les diagonales
        Vector2Int[] directions = {
            new Vector2Int(0, 1),   // Haut
            new Vector2Int(0, -1),  // Bas
            new Vector2Int(-1, 0),  // Gauche
            new Vector2Int(1, 0),   // Droite
        };

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            // Vérifier si la position est dans les limites du plateau
            if (newX >= 0 && newX < TILE_COUNT_X && newY >= 0 && newY < TILE_COUNT_Y)
            {
                PieceType piece = boardPieces[newX, newY];
                // Vérifier si la case contient un ennemi
                if (piece != null && piece.team != team && !piece.isDead)
                {
                    adjacentEnemies.Add(piece);
                }
            }
        }

        return adjacentEnemies;
    }

    internal void RestartGame()
    {
        currentlyDragging = null;
        movePieceAgain = null;


        for(int x = 0; x < TILE_COUNT_X; x++)
        {
            for(int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(boardPieces[x, y] != null)
                {
                    Destroy(boardPieces[x, y].gameObject);
                }
            }
        }

        allChef.Clear();
        allNecro.Clear();
        redUnits.Clear();
        blueUnits.Clear();
        greenUnits.Clear();
        yellowUnits.Clear();

        looseTeam = new bool[4];

        TurnInterfaceManager.instance.RestartGame();

        teamTurn = 0;
        TurnInterfaceManager.instance.SetTurn(teamTurn);

        //SpawnAllPieces();
        //PositionAllPieces();
    }

    public void BackMenu()
    {
        RestartGame();
        GameUI.Instance.OnLeaveFromGameMenu();

        Client.Instance.Shutdown();
        Server.Instance.Shutdown();

        playerCount = -1;
        currentTeam = -1;
    }

    #region Events
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_MAKE_KILL += OnMakeKillServer;
        NetUtility.S_CLIENT_INFORMATION += OnClientInformationServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_MAKE_KILL += OnMakeKillClient;
        NetUtility.C_CLIENT_INFORMATION += OnClientInformationClient;
        NetUtility.C_UPDATE_LOBBY+= OnUpdateLobbyClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    

    private void UnregisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_MAKE_KILL -= OnMakeKillServer;
        NetUtility.S_CLIENT_INFORMATION -= OnClientInformationServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_MAKE_KILL -= OnMakeKillClient;
        NetUtility.C_CLIENT_INFORMATION -= OnClientInformationClient;
        NetUtility.C_UPDATE_LOBBY -= OnUpdateLobbyClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }

    //Server
    private void OnWelcomeServer(NetMessage message, NetworkConnection connection)
    {
        //Client has connect,, assign a team and return the message back to the client
        NetWelcome nw = message as NetWelcome;

        //Assign a team
        nw.AssignedTeam = ++playerCount;
        nw.numberPlayer = playerCount;

        //Return bback to the client
        Server.Instance.SendToClient(connection, nw);

        //If we have 3 players, start the game
        /*if(playerCount == 3)
        {
            //Start the game
            Server.Instance.BroadCast(new NetStartGame());
        }*/   
    }

    private void OnClientInformationServer(NetMessage message, NetworkConnection connection)
    {
        NetClientInformation nci = message as NetClientInformation;

         // Récupérer toutes les couleurs déjà utilisées
        HashSet<int> usedColors = new HashSet<int>();
        foreach (var client in PlayerManager.Instance.clients)
        {
            usedColors.Add(client.colorValue);
        }

        // Trouver la première couleur non utilisée
        int newColor = 0;
        while (usedColors.Contains(newColor))
        {
            newColor++;
        }

        Server.Instance.Clients.Add(new ClientInformation { 
            playerName = nci.playerName, 
            playerValue = nci.playerValue, 
            colorValue = newColor,
            isReady = nci.isReady
        });

        Server.Instance.BroadCast(message);
    }

    private void OnMakeMoveServer(NetMessage message, NetworkConnection connection)
    {
        NetMakeMove nm = message as NetMakeMove;

       //Receive, and just broadcast it to all clients 
       Server.Instance.BroadCast(message); 
    }

    private void OnMakeKillServer(NetMessage message, NetworkConnection connection)
    {
        NetMakeKill nm = message as NetMakeKill;

        //Receive, and just broadcast it to all clients 
        Server.Instance.BroadCast(message); 
    }

    //Client

    private void OnWelcomeClient(NetMessage message)
    {
        NetWelcome nw = message as NetWelcome;

        currentTeam = nw.AssignedTeam;
        playerCount = nw.numberPlayer;


        Debug.Log("Welcome to the game, you are team " + currentTeam);
        PlayerManager.Instance.SetPlayerValue(currentTeam);

        //Update Client
        GameUI.Instance.UpdateClientRoomInformation();
        TurnInterfaceManager.instance.SetYourColor(currentTeam);

        if(localGame && currentTeam == 0)
        {
            Server.Instance.BroadCast(new NetStartGame());
        } 
    }

    private void OnStartGameClient(NetMessage message)
    {
        InitBoardUnit();

        NetStartGame sm = message as NetStartGame;

        teamTurn = sm.whoStart;

        if(currentTeam == 0 || currentTeam == 1)
        {
            GameUI.Instance.ChangeCamera(cameraAngle.botSide);
            TurnInterfaceManager.instance.PlayerPanelPosition(false);
        } else if(currentTeam == 2 || currentTeam == 3)
        {
            GameUI.Instance.ChangeCamera(cameraAngle.topSide);
            TurnInterfaceManager.instance.PlayerPanelPosition(true);
        }

        for(int i = 0; i < PlayerManager.Instance.clients.Count; i++)
        {
            Color playerColor = PlayerManager.Instance.colorList[PlayerManager.Instance.clients[i].colorValue];

            teamMaterials[i].color = playerColor;
            playerImages[i].GetComponent<Image>().color = playerColor;

            // Vérifier que GetTurnElements() a bien la même taille que clients
            if (i < TurnInterfaceManager.instance.GetTurnElements().Count)
            {
                TurnInterfaceManager.instance.GetTurnElements()[i].SetupColor(playerColor);
                TurnInterfaceManager.instance.GetTurnElements()[i].SetupName(PlayerManager.Instance.clients[i].playerName);
            }
        }

        TurnInterfaceManager.instance.SetTurn(teamTurn);
    }

    private void OnMakeMoveClient(NetMessage message)
    {
        NetMakeMove nm = message as NetMakeMove;

        PieceType target = allUnits[nm.pieceId];
        if(target != null && !target.isDead)
        {
            
            availableMoves = target.GetAvailableMoves(ref boardPieces, TILE_COUNT_X, TILE_COUNT_Y);
        }
        MoveTo(nm.pieceId, nm.destinationX, nm.destinationY);
        

        availableMoves.Clear();
        if(nm.endTurn == 1)
        {
            ChangeTurn();
        }
    }

    public void OnMakeKillClient(NetMessage message)
    {
        NetMakeKill nm = message as NetMakeKill;

        PieceType target = FindPieceById(nm.pieceId);
        if(target != null)
        {
            target.Die();
            if(target.form == PieceForm.Chef)
            {
                //All EnemyUnits switch to your team 
                TeamSwitch(target.team, nm.teamSwap);
            }
        }
        ChangeTurn();
    }

    private void OnClientInformationClient(NetMessage message)
    {
        NetClientInformation nci = message as NetClientInformation;
    }

    private void OnUpdateLobbyClient(NetMessage message)
    {
        NetUpdateLobby msg = message as NetUpdateLobby;
        foreach (var client in msg.clients)
        {
            bool alreadyExists = PlayerManager.Instance.clients.Any(c => c.playerValue == client.playerValue);
            if (!alreadyExists)
            {
                PlayerManager.Instance.clients.Add(client);
            }
        }
        GameUI.Instance.UpdateLobbyClient();
    }

    //Not Network
    private void OnSetLocalGame(bool v)
    {
        localGame = v;
    }
    #endregion
}
