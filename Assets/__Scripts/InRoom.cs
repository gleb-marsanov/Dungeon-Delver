﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoom : MonoBehaviour
{
    public static float ROOM_W = 16;
    public static float ROOM_H = 11;
    public static float WALL_T = 2;

    public static int MAX_RM_X = 9; //a
    public static int MAX_RM_Y = 9;

    public static Vector2[] DOORS = new Vector2[]   //b
    {
        new Vector2(14,5),
        new Vector2(7.5f,9),
        new Vector2(1,5),
        new Vector2(7.5f,1)
    };

    [Header("Set in Inspector")]
    public bool keepInRoom = true;
    public float gridMult = 1;

    private void LateUpdate()
    {
        if (keepInRoom) //b
        {
            Vector2 rPos = roomPos;
            rPos.x = Mathf.Clamp(rPos.x, WALL_T, ROOM_W - 1 - WALL_T);
            rPos.y = Mathf.Clamp(rPos.y, WALL_T, ROOM_H - 1 - WALL_T);
            roomPos = rPos; //d
        }
    }

    //Местоположение этого персонажа в локальных координатах комнаты
    public Vector2 roomPos
    {
        get
        {
            Vector2 tPos = transform.position;
            tPos.x %= ROOM_W;
            tPos.y %= ROOM_H;
            return tPos;
        }
        set
        {
            Vector2 rm = roomNum;
            rm.x *= ROOM_W;
            rm.y *= ROOM_H;
            rm += value;
            transform.position = rm;
        }
    }

    //В какой комнате находится этот персонаж?
    public Vector2 roomNum
    {
        get
        {
            Vector2 tPos = transform.position;
            tPos.x = Mathf.Floor(tPos.x / ROOM_W);
            tPos.y = Mathf.Floor(tPos.y / ROOM_H);
            return tPos;
        }
        set
        {
            Vector2 rPos = roomPos;
            Vector2 rm = value;
            rm.x *= ROOM_W; // --------------- Здесь должно быть rPos.x *= ROOM_W;
            rm.y *= ROOM_H; // --------------- Здесь должно быть rPos.x *= ROOM_W;
            transform.position = rm + rPos;
        }
    }

    //Вычисляет координаты узла сетки, ближайшего к данному персонажу
    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        if (mult == -1)
        {
            mult = gridMult;
        }

        Vector2 rPos = roomPos;
        rPos /= mult;
        rPos.x = Mathf.Round(rPos.x);
        rPos.y = Mathf.Round(rPos.y);
        rPos *= mult;
        return rPos;
    }
}
