using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFacingMover
{
    int GetFacing();                             //a
    bool moving { get; }                         //b
    float GetSpeed();                            //c
    float gridMult { get; }
    Vector2 roomPos { get; set; }                //d
    Vector2 roomNum { get; set; }                //e
    Vector2 GetRoomPosOnGrid(float mult = -1);   //f
}