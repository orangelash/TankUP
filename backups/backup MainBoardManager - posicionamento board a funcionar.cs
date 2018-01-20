using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBoardManager : MonoBehaviour
{

    public TankMover[,] TankMovers { set; get; }
    private TankMover selectedTankMover;
    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = TILE_SIZE / 2;
    //private Vector2 arty, jeep1, jeep2, tank1, tank2, tank3;
    private int selectionX = -1;
    private int selectionY = -1;
    private int spawncounter = 0;
    //private List<int> tankPositions = new List<int>();
    public List<GameObject> tanksPreFabs;
    private List<GameObject> activeTanks = new List<GameObject>();

    public bool isMyTurn = true;



    private void Start()
    {
        TankMovers = new TankMover[4, 4];
    }

    private void Update()
    {
        UpdateSelection();
        DrawBoard();

        if (Input.GetMouseButtonDown(0))
        {
            if (spawncounter <= 6)
            {
                if (selectionX >= 0 && selectionY >= 0)
                {
                    int x = selectionX, y = selectionY, full = 0;
                    full=SpawnTanks(spawncounter, x, y);
					if (full == 1)
						spawncounter--;
                    spawncounter++;
                }
            }
            else
            {
                Debug.Log("Não pode meter mais tanques");
            }
        }
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("Boards")))

        {
            //Debug.Log(hit.point);
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.y;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SelectTank(int x, int y)
    {
        if (TankMovers[x, y] == null)
            return;
        if (true != isMyTurn)
            return;

        selectedTankMover = TankMovers[x, y];
    }

    private void MoveTank(int x, int y)
    {
        //About to be moved
        if (TankMovers[x, y] != null)
            return;
        TankMovers[selectedTankMover.CurrentX, selectedTankMover.CurrentY] = null;
        selectedTankMover.transform.position = GetTileCenter(x, y);
        TankMovers[x, y] = selectedTankMover;

        selectedTankMover = null;
    }

    private int SpawnTanks(int index, int x, int y)
    {
        if (index > 5)
			return 0;

		if(TankMovers[x,y] != null){
			Debug.Log("Já existe um veículo ai");
			return 1;
		}
            GameObject go = Instantiate(
                tanksPreFabs[index],
                GetTileCenter(x, y),
                Quaternion.identity
                ) as GameObject;

            go.transform.SetParent(transform);
            TankMovers[x, y] = go.GetComponent<TankMover>();
            TankMovers[x, y].setPosition(x, y);
            TankMovers[x, y].transform.position = GetTileCenter(x, y);

            activeTanks.Add(go);
			return 0;
        
    }

    private void DrawBoard()
    {
        Vector2 widthLine = Vector2.up * 4;
        Vector2 heightLine = Vector2.right * 4;

        // draws the board squares
        for (int i = 0; i < 5; i++)
        {
            Vector2 start = Vector2.right * i; //alterar para ir para o início
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j < 5; j++)
            {
                start = Vector2.up * j; //alterar para ir para o início
                Debug.DrawLine(start, start + heightLine);
            }
        }

        // draws the selected square
        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector2.up * selectionY + Vector2.right * selectionX,
                Vector2.up * (selectionY + 1) + Vector2.right * (selectionX + 1)
            );

            Debug.DrawLine(
                Vector2.up * (selectionY + 1) + Vector2.right * selectionX,
                Vector2.up * selectionY + Vector2.right * (selectionX + 1)
            );
        }

    }

    private Vector2 GetTileCenter(int x, int y)
    {
        Vector2 origin = Vector2.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.y += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;

    }
}
