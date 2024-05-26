using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManagerScript : MonoBehaviour
{
    // ゲームオブジェクト
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public GameObject clearText;
    public GameObject particlePrefab;
    public GameObject wallPrefab;
    public GameObject nextText;
    // 配列の宣言
    int[,] map;
    int[,] stage0;
    int[,] stage1;
    int[,] stage2;
    // 現在のステージ
    int currentStage = 0;
    // ゲーム管理用の配列
    GameObject[,] field;

    public AudioClip sound1;
    public AudioClip sound2;

    AudioSource audioSource;
    bool hasPlayedClearSound = false; // クリア時の効果音が再生されたかどうかのフラグ

    void PrintArray()
    {
        string debugText = "";
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                debugText += map[y, x].ToString() + ",";
            }
            debugText += "\n"; // 改行
        }
        Debug.Log(debugText);
    }

    Vector2Int GetPlayerIndex()
    {
        for (int y = 0; y < field.GetLength(0); y++)
        {
            for (int x = 0; x < field.GetLength(1); x++)
            {
                if (field[y, x] == null) { continue; }

                if (field[y, x].tag == "Player")
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool MoveNumber(Vector2Int moveFrom, Vector2Int moveTo)
    {
        //  移動先が範囲外なら移動不可
        if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        // Wallタグを持っていたら動かないようにする
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Wall")
        {
            return false;
        }

        // Boxタグを持っていたら再帰処理
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Box")
        {
            Vector2Int velocity = moveTo - moveFrom;
            bool success = MoveNumber(moveTo, moveTo + velocity);
            if (!success) { return false; }
        }
        // 移動処理
        //field[moveFrom.y, moveFrom.x].transform.position = new Vector3(moveTo.x, map.GetLength(0) - moveTo.y, 0);

        Vector3 moveToPosition = new Vector3(moveTo.x, map.GetLength(0) - moveTo.y, 0);
        field[moveFrom.y, moveFrom.x].GetComponent<Move>().MoveTo(moveToPosition);

        field[moveTo.y, moveTo.x] = field[moveFrom.y, moveFrom.x];
        field[moveFrom.y, moveFrom.x] = null;

        // パーティクルの生成を行う
        if (field[moveTo.y, moveTo.x].tag == "Player")
        {
            const float kParticleNum = 4;
            for (int i = 0; i < kParticleNum; i++)
            {
                Particle.Instantiate(
                    particlePrefab,
                    new Vector3(moveFrom.x, map.GetLength(0) - moveFrom.y, 0),
                    Quaternion.identity
                    );
            }
        }

        return true;
    }

    bool IsCleard()
    {
        // 可変長配列の作成
        List<Vector2Int> goals = new List<Vector2Int>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // 格納場所か否かを判断
                if (map[y, x] == 3)
                {
                    // 格納場所のインデックスを控えておく
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // 要素数はgoals.Countで取得
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = field[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Box")
            {
                // 一つでも箱がなかったら条件未達成
                return false;
            }
        }
        // 条件未達成でなければ条件達成
        return true;
    }

    void ResetGame()
    {
        // すべてのオブジェクトを削除する
        foreach (GameObject obj in field)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // 新しいマップを作成し、各オブジェクトをその位置に配置する
        if (currentStage == 0)
        {
            map = stage0;
        }
        else if (currentStage == 1)
        {
            map = stage1;
        }
        else if (currentStage == 2)
        {
            map = stage2;
        }

        field = new GameObject[map.GetLength(0), map.GetLength(1)];

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // 各オブジェクトを配置する
                if (map[y, x] == 1)
                {
                    field[y, x] = Instantiate(playerPrefab, new Vector3(x, map.GetLength(0) - y, 0), Quaternion.identity);
                }
                else if (map[y, x] == 2)
                {
                    field[y, x] = Instantiate(boxPrefab, new Vector3(x, map.GetLength(0) - y, 0), Quaternion.identity);
                }
                else if (map[y, x] == 3)
                {
                    field[y, x] = Instantiate(goalPrefab, new Vector3(x, map.GetLength(0) - y, 0.01f), Quaternion.identity);
                }
                else if (map[y, x] == 6)
                {
                    field[y, x] = Instantiate(wallPrefab, new Vector3(x, map.GetLength(0) - y, 0), Quaternion.identity);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1280, 720, false);
        audioSource = GetComponent<AudioSource>();
        // ステージ0
        stage0 = new int[,]
        {
            {6,6,6,6,6,6,6,6,6},
            {6,6,0,0,0,0,0,6,6},
            {6,0,3,0,0,0,3,0,6},
            {6,0,0,0,2,0,0,0,6},
            {6,0,0,2,1,2,0,0,6},
            {6,0,0,0,0,0,0,0,6},
            {6,0,0,0,0,0,0,0,6},
            {6,6,0,0,3,0,0,6,6},
            {6,6,6,6,6,6,6,6,6},
        };
        // ステージ1
        stage1 = new int[,]
        {
            {6,6,6,6,6,6,6,6,6},
            {6,3,0,0,0,0,0,3,6},
            {6,6,6,0,0,0,6,6,6},
            {6,6,0,0,2,0,0,6,6},
            {6,6,0,2,1,2,0,6,6},
            {6,0,0,0,0,0,6,6,6},
            {6,0,0,0,0,6,6,6,6},
            {6,3,6,6,6,6,6,6,6},
            {6,6,6,6,6,6,6,6,6},
        };
        // ステージ2
        stage2 = new int[,]
        {
            {6,6,6,6,6,6,6,6,6},
            {6,6,6,6,6,6,6,6,6},
            {6,6,6,6,6,6,6,6,6},
            {6,6,6,0,0,0,0,0,6},
            {6,6,0,0,1,0,2,0,6},
            {6,6,0,0,0,0,2,0,6},
            {6,6,0,0,6,0,2,3,6},
            {6,6,6,0,3,0,3,6,6},
            {6,6,6,6,6,6,6,6,6},
        };

        // 現在のステージを設定
        if (currentStage == 0)
        {
            map = stage0;
        }
        else if (currentStage == 1)
        {
            map = stage1;
        }
        else if (currentStage == 2)
        {
            map = stage2;
        }

        field = new GameObject
        [
            map.GetLength(0),
            map.GetLength(1)
        ];

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // playerPrefabの実体化
                if (map[y, x] == 1)
                {
                    field[y, x] = Instantiate(
                        playerPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                        );
                }
                // boxPrefabの実体化
                if (map[y, x] == 2)
                {
                    field[y, x] = Instantiate(
                        boxPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                        );
                }
                // goalPrefabの実体化
                if (map[y, x] == 3)
                {
                    field[y, x] = Instantiate(
                        goalPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.01f), // プレイヤーと重ならないように少しだけZ座標を奥に設定
                        Quaternion.identity
                        );
                }
                // wallPrefabの実体化
                if (map[y, x] == 6)
                {
                    field[y, x] = Instantiate(
                        wallPrefab,
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
        // クリア画面では移動とリセットが行えないようにする
        if (!IsCleard())
        {
            // 右移動
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // 移動時の効果音を再生
                audioSource.PlayOneShot(sound1);
                // メソッド化した処理を使用
                Vector2Int playerIndex = GetPlayerIndex();
                // 移動処理を関数化
                MoveNumber(playerIndex, playerIndex + new Vector2Int(1, 0));
                //PrintArray();
            }

            // 左移動
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // 移動時の効果音を再生
                audioSource.PlayOneShot(sound1);
                Vector2Int playerIndex = GetPlayerIndex();

                // 移動処理を関数化
                MoveNumber(playerIndex, playerIndex + new Vector2Int(-1, 0));
                //PrintArray();
            }

            // 上移動
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 移動時の効果音を再生
                audioSource.PlayOneShot(sound1);
                Vector2Int playerIndex = GetPlayerIndex();

                // 移動処理を関数化
                MoveNumber(playerIndex, playerIndex + new Vector2Int(0, -1));
                //PrintArray();
            }

            // 下移動
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // 移動時の効果音を再生
                audioSource.PlayOneShot(sound1);
                Vector2Int playerIndex = GetPlayerIndex();

                // 移動処理を関数化
                MoveNumber(playerIndex, playerIndex + new Vector2Int(0, 1));
                //PrintArray();
            }

            // ステージリセット
            if (Input.GetKeyDown(KeyCode.R))
            {
                audioSource.PlayOneShot(sound2); // 効果音を再生
                ResetGame();
            }
        }

        //もしクリアしていたら
        if (IsCleard())
        {
            if (!hasPlayedClearSound) // クリア時の効果音がまだ再生されていない場合
            {
                audioSource.PlayOneShot(audioSource.clip); // 効果音を再生
                hasPlayedClearSound = true; // フラグを設定して再生されたことを記録する
            }
            // ゲームオブジェクトのSetActiveメソッドを使い有効化
            clearText.SetActive(true);
            nextText.SetActive(true);

            // キー入力で次のステージへ
            if (Input.GetKeyDown(KeyCode.Return))
            {
                clearText.SetActive(false); // クリアテキストを消す
                nextText.SetActive(false);
                if (currentStage <= 3)
                {
                    currentStage += 1; // 次のステージへ
                }
                ResetGame(); // ステージのリセット
                hasPlayedClearSound = false; // リセット後にフラグをリセットする
            }
        }
    }
}
