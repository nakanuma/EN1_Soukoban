using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    // 配列の宣言
    int[] map;

    void PrintArray()
    {
        string debugText = "";
        for (int i = 0; i < map.Length; i++)
        {
            debugText += map[i].ToString() + ",";
        }
        Debug.Log(debugText);
    }

    int GetPlayerIndex()
    {
        for (int i = 0; i < map.Length; i++)
        {
            if (map[i] == 1)
            {
                return i;
            }
        }
        return -1;
    }

    bool MoveNumber(int number, int moveFrom, int moveTo)
    {
        //  移動先が範囲外なら移動不可
        if (moveTo < 0 || moveTo >= map.Length) { return false; }
        // 移動先に2(箱)が居たら
        if (map[moveTo] == 2)
        {
            // どの方向へ移動するかを算出
            int velocity = moveTo - moveFrom;
            // プレイヤーの移動先から、さらに先へ2(箱)を移動させる
            // 箱の移動処理、MoveNumberメソッド内でMoveNumberメソッドを呼び、
            // 処理が再帰している。移動可不可をboolで記録
            bool success = MoveNumber(2, moveTo, moveTo + velocity);
            if (!success) { return false; }
        }
        // プレイヤー・箱関わらずの移動処理
        map[moveTo] = number;
        map[moveFrom] = 0;
        return true;
    }


    // Start is called before the first frame update
    void Start()
    {
        map = new int[] { 0, 2, 0, 1, 0, 2, 0, 2, 0 };
        PrintArray();
    }

    // Update is called once per frame
    void Update()
    {
        // 右移動
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // メソッド化した処理を使用
            int playerIndex = GetPlayerIndex();

            // 移動処理を関数化
            MoveNumber(1, playerIndex, playerIndex + 1);
            PrintArray();
        }

        // 左移動
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int playerIndex = GetPlayerIndex();

            // 移動処理を関数化
            MoveNumber(1, playerIndex, playerIndex - 1);
            PrintArray();
        }
    }
}
