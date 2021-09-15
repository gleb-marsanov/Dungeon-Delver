using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSwap           //a
{
    public int tileNum;
    public GameObject swapPrefab;
    public GameObject guaranteedItemDrop;
    public int overrideTileNum = -1;
}

public class TileCamera : MonoBehaviour
{
    static int W, H;
    static int[,] MAP;
    public static Sprite[] SPRITES;
    public static Transform TILE_ANCOR;
    public static Tile[,] TILES;
    public static string COLLISIONS;

    [Header("Set in Inspector")]
    public TextAsset mapData;
    public Texture2D mapTiles;
    public TextAsset mapCollisions; // Будет использоваться позже
    public Tile tilePrefab;
    public int defaultTileNum;      //b
    public List<TileSwap> tileSwaps;    //c

    private Dictionary<int, TileSwap> tileSwapDict; //c
    private Transform enemyAnchor, itemAnchor;

    private void Awake()
    {
        COLLISIONS = Utils.RemoveLineEndings(mapCollisions.text);
        PrepareTileSwapDict();      //d
        enemyAnchor = (new GameObject("Enemy Anchor")).transform;
        itemAnchor = (new GameObject("Item Anchor")).transform;
        LoadMap();
    }

    private void OnGUI()
    {
        Awake();
    }

    public void LoadMap()
    {
        // Создать TILE_ANCHOR. Он будет играть роль родителя для всех плиток Tile
        GameObject go = new GameObject("TILE_ANCHOR");
        TILE_ANCOR = go.transform;

        //Загрузить все спрайты из mapTiles
        SPRITES = Resources.LoadAll<Sprite>(mapTiles.name);

        //Прочитать информацию для карты
        string[] lines = mapData.text.Split('\n');
        H = lines.Length;
        string[] tileNums = lines[0].Split(' ');
        W = tileNums.Length;

        System.Globalization.NumberStyles hexNum;
        hexNum = System.Globalization.NumberStyles.HexNumber;
        //Сохранить информацию для карты в двумерный массив для ускорения доступа
        MAP = new int[W, H];
        for (int j = 0; j < H; j++)
        {
            tileNums = lines[j].Split(' ');
            for (int i = 0; i < W; i++)
            {
                if (tileNums[i] == "..")
                {
                    MAP[i, j] = 0;
                }
                else
                {
                    MAP[i, j] = int.Parse(tileNums[i], hexNum);
                }
                CheckTileSwaps(i, j);       //e
            }
        }

        print("Parsed " + SPRITES.Length + " sprites.");
        print("Map size: " + W + " wide by " + H + " high");

        ShowMap();
    }

    //Генерирует плитки сразу для всей карты
    void ShowMap()
    {
        TILES = new Tile[W, H];

        //Просмотреть всю карту и создать плитки, где необходимо
        for (int j = 0; j < H; j++)
        {
            for (int i = 0; i < W; i++)
            {
                if (MAP[i, j] != 0)
                {
                    Tile ti = Instantiate<Tile>(tilePrefab);
                    ti.transform.SetParent(TILE_ANCOR);
                    ti.SetTile(i, j);   //c
                    TILES[i, j] = ti;
                }
            }
        }
    }

    void PrepareTileSwapDict()  //d
    {
        tileSwapDict = new Dictionary<int, TileSwap>();
        foreach (TileSwap ts in tileSwaps)
        {
            tileSwapDict.Add(ts.tileNum, ts);
        }
    }

    void CheckTileSwaps(int i, int j)       //e
    {
        int tNum = GET_MAP(i, j);
        if (!tileSwapDict.ContainsKey(tNum)) return;
        //Мы должны заменить плитку
        TileSwap ts = tileSwapDict[tNum];
        if (ts.swapPrefab != null)
        {
            GameObject go = Instantiate(ts.swapPrefab);
            Enemy e = go.GetComponent<Enemy>();
            if (e != null)
            {
                go.transform.SetParent(enemyAnchor);
            }
            else
            {
                go.transform.SetParent(itemAnchor);
            }

            go.transform.position = new Vector3(i, j, 0);
            if (ts.guaranteedItemDrop != null)             //g
            {
                if (e != null)
                {
                    e.guaranteedItemDrop = ts.guaranteedItemDrop;
                }
            }
        }

        //Заменить другой плиткой
        if (ts.overrideTileNum == -1)        //h
        {
            SET_MAP(i, j, defaultTileNum);
        }
        else
        {
            SET_MAP(i, j, ts.overrideTileNum);
        }
    }

    public static int GET_MAP(int x, int y)
    {
        if (x < 0 || x >= W || y < 0 || y >= H)
        {
            return -1; //Предотвратить исключение IndexOutOfRangeException
        }
        return MAP[x, y];
    }

    //Перегружаемая float-версия GET_MAP()
    public static int GET_MAP(float x, float y)
    {
        int tX = Mathf.RoundToInt(x);
        int tY = Mathf.RoundToInt(y - 0.25f);
        return GET_MAP(tX, tY);
    }

    public static void SET_MAP(int x, int y, int tNum)
    {
        if (x < 0 || x >= W || y < 0 || y >= H)
        {
            return; //Предотвратить исключение IndexOutOfRangeException
        }
        MAP[x, y] = tNum;
    }
}
