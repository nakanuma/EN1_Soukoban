using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    // �Q�[���I�u�W�F�N�g
    public GameObject playerPrefab;
    // �z��̐錾
    int[,] map;
    // �Q�[���Ǘ��p�̔z��
    GameObject[,] field;

    void PrintArray()
    {
        string debugText = "";
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                debugText += map[y, x].ToString() + ",";
            }
            debugText += "\n"; // ���s
        }
        Debug.Log(debugText);
    }

    Vector2Int GetPlayerIndex()
    {
        for (int y = 0; y < field.GetLength(0); y++)
        {
            for (int x = 0; x < field.GetLength(1); x++)
            {
                if (field[y, x] == null)
                {
                    continue;
                }
                if (field[y, x].tag == "Player")
                {
                    return new Vector2Int(y, x);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool MoveNumber(string tag, Vector2Int moveFrom, Vector2Int moveTo)
    {
        //  �ړ��悪�͈͊O�Ȃ�ړ��s��
        if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        field[moveTo.y, moveTo.x] = field[moveFrom.y, moveFrom.x];
        field[moveFrom.y, moveFrom.x] = null;

            // GameObject�̍��W���ړ������Ă���C���f�b�N�X�̓���ւ�
        field[moveFrom.y, moveFrom.x].transform.position = new Vector3(moveTo.x, map.GetLength(0) - moveTo.y, 0);

        return true;
    }


    // Start is called before the first frame update
    void Start()
    {
        map = new int[,]
            {
            {0,0,0,0,0 },
            {0,0,1,0,0 },
            {0,0,0,0,0 }
        };
        field = new GameObject
        [
            map.GetLength(0),
            map.GetLength(1)
        ];

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if (map[y, x] == 1)
                {
                    field[y, x] = Instantiate(
                        playerPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                        );
                }
            }
        };
        //PrintArray();
    }

    // Update is called once per frame
    void Update()
    {
        // �E�ړ�
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // ���\�b�h�������������g�p
            Vector2Int playerIndex = GetPlayerIndex();

            // �ړ��������֐���
            MoveNumber("Player", playerIndex, playerIndex + new Vector2Int(1, 0));
            //PrintArray();
        }

        // ���ړ�
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector2Int playerIndex = GetPlayerIndex();

            // �ړ��������֐���
            MoveNumber("Player", playerIndex, playerIndex + new Vector2Int(-1, 0));
            //PrintArray();
        }
    }
}
