using System;
using System.Collections.Generic;
using UnityEngine;

public enum PieceForm
{
    None = 0,

    Chef = 1,
    Militant = 2,
    Assassin = 3,
    Reporter = 4,
    Diplomate = 5,
    Necromobile = 6, 
}



public abstract class PieceType : MonoBehaviour
{
    public int team;

    public int pieceId;
    public int currentX;
    public int currentY;
    public PieceForm form;
    public bool isDead = false;

    [SerializeField] private GameObject[] pieceTeam;
    [SerializeField] private GameObject pieceAlive;
    [SerializeField] private GameObject pieceDead;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    public void Start()
    {
        pieceAlive.SetActive(true);
        pieceDead.SetActive(false);
        InitPieceType();
    }

    private void InitPieceType()
    {
        if(team == 0)
        {
            gameObject.name = $"Red_{form}";
        } else if(team == 1)
        {
            gameObject.name = $"Blue_{form}";
        } else if(team == 2)
        {
            gameObject.name = $"Green_{form}";
        } else if(team == 3)
        {
            gameObject.name = $"Yellow_{form}";
        }
    }

    internal void SetupTeamMaterial(Material material)
    {
        foreach (var piece in pieceTeam)
        {
            piece.GetComponent<MeshRenderer>().material = material;
        }
    }

    protected void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref PieceType[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3,3));
        r.Add(new Vector2Int(3,4));
        r.Add(new Vector2Int(4,3));
        r.Add(new Vector2Int(4,4));

        return r;
    }

    public void SwitchTeam(int teamToSwitch)
    {
        team = teamToSwitch;
        InitPieceType();
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if(force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if(force)
        {
            transform.localScale = desiredScale;
        }
    }

    public abstract void Action(PieceType target, Vector2Int initialPosition = default);

    public virtual void Die()
    {
        isDead = true;
        pieceAlive.SetActive(false);
        pieceDead.SetActive(true);
    }
}
