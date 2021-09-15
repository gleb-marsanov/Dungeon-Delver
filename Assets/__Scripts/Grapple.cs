using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public enum eMode { none, gOut, gInMiss, gInHit }    //a

    [Header("Set in Inspector")]
    public float grappleSpd = 10;
    public float grappleLength = 7;
    public float grappleInLength = 0.5f;
    public int unsafeTileHealthPenalty = 2;
    public TextAsset mapGrappleable;

    [Header("Set Dynamically")]
    public eMode mode = eMode.none;
    //Номера плиток, на которые можно забросить крюк
    public List<int> grappleTiles;  //b
    public List<int> unsafeTiles;

    Dray dray;
    Rigidbody rigidbody;
    Animator animator;
    Collider drayCollider;

    GameObject grapHead;    //c
    LineRenderer grapLine;
    Vector3 p0, p1;
    int facing;

    Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    private void Awake()
    {
        string gTiles = mapGrappleable.text;    //d
        gTiles = Utils.RemoveLineEndings(gTiles);
        grappleTiles = new List<int>();
        unsafeTiles = new List<int>();
        for (int i = 0; i < gTiles.Length; i++)
        {
            switch (gTiles[i])
            {
                case 'S':
                    grappleTiles.Add(i);
                    break;
                case 'X':
                    unsafeTiles.Add(i);
                    break;
            }
        }

        dray = GetComponent<Dray>();
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        drayCollider = GetComponent<Collider>();

        Transform trans = transform.Find("Grappler");
        grapHead = trans.gameObject;
        grapLine = grapHead.GetComponent<LineRenderer>();
        grapHead.SetActive(false);
    }

    private void Update()
    {
        if (!dray.hasGrappler) return;

        switch (mode)
        {
            case eMode.none:
                //Если нажата клавиша применения крюка
                if (Input.GetKeyDown(KeyCode.X))
                {
                    StartGrapple();
                }
                break;
        }
    }

    void StartGrapple()  //f
    {
        facing = dray.GetFacing();
        dray.enabled = false; //g
        dray.GetComponent<Rigidbody>().velocity = Vector3.zero;
        animator.CrossFade("Dray_Attack_" + facing, 0);
        drayCollider.enabled = false;

        grapHead.SetActive(true);

        p0 = transform.position + (directions[facing] * 0.5f);
        p1 = p0;
        grapHead.transform.position = p1;
        grapHead.transform.rotation = Quaternion.Euler(0, 0, 90 * facing);

        grapLine.positionCount = 2; //h
        grapLine.SetPosition(0, p0);
        grapLine.SetPosition(1, p1);
        mode = eMode.gOut;
    }

    private void FixedUpdate()
    {
        switch (mode)
        {
            case eMode.gOut:    //Крюк брошен   //i
                p1 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                grapHead.transform.position = p1;
                grapLine.SetPosition(1, p1);

                //Проверить, попал ли крюк куда-нибудь
                int tileNum = TileCamera.GET_MAP(p1.x, p1.y);
                if (grappleTiles.IndexOf(tileNum) != -1)
                {
                    //Крюк попал на плитку, за которую можно зацепиться!
                    mode = eMode.gInHit;
                    break;
                }
                if ((p1 - p0).magnitude >= grappleLength)
                {
                    //Крюк улетел на всю длину веревки, но никуда не попал
                    mode = eMode.gInMiss;
                }
                break;

            case eMode.gInMiss: //Игрок промахнулся, вернуть крюк на удвоенной скорости //j
                p1 -= directions[facing] * 2 * grappleSpd * Time.fixedDeltaTime;
                if (Vector3.Dot((p1 - p0), directions[facing]) > 0)
                {
                    //Крюк все ещё перед Дреем
                    grapHead.transform.position = p1;
                    grapLine.SetPosition(1, p1);
                }
                else
                {
                    StopGrapple();
                }
                break;

            case eMode.gInHit:  //Крюк зацепился, поднять Дрея на стену //k
                float dist = grappleInLength + grappleSpd * Time.fixedDeltaTime;
                if (dist > (p1 - p0).magnitude)
                {
                    p0 = p1 - (directions[facing] * grappleInLength);
                    transform.position = p0;
                    StopGrapple();
                    break;
                }
                p0 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                transform.position = p0;
                grapLine.SetPosition(0, p0);
                grapHead.transform.position = p1;
                break;
        }
    }

    void StopGrapple()      //l
    {
        dray.enabled = true;
        drayCollider.enabled = true;

        //Проверить безопасность плитки
        int tileNum = TileCamera.GET_MAP(p0.x, p0.y);
        if (mode == eMode.gInHit && unsafeTiles.IndexOf(tileNum) != -1)
        {
            //Дрей попал на небезопасную плитку
            dray.ResetInRoom(unsafeTileHealthPenalty);
        }

        grapHead.SetActive(false);
        mode = eMode.none;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;
        mode = eMode.gInMiss;
    }
}
