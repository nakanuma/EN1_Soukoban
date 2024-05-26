using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManagerScript : MonoBehaviour
{
    // �Q�[���I�u�W�F�N�g
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public GameObject clearText;
    public GameObject particlePrefab;
    public GameObject wallPrefab;
    public GameObject nextText;
    // �z��̐錾
    int[,] map;
    int[,] stage0;
    int[,] stage1;
    int[,] stage2;
    // ���݂̃X�e�[�W
    int currentStage = 0;
    // �Q�[���Ǘ��p�̔z��
    GameObject[,] field;

    public AudioClip sound1;
    public AudioClip sound2;

    AudioSource audioSource;
    bool hasPlayedClearSound = false; // �N���A���̌��ʉ����Đ����ꂽ���ǂ����̃t���O

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
        //  �ړ��悪�͈͊O�Ȃ�ړ��s��
        if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        // Wall�^�O�������Ă����瓮���Ȃ��悤�ɂ���
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Wall")
        {
            return false;
        }

        // Box�^�O�������Ă�����ċA����
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Box")
        {
            Vector2Int velocity = moveTo - moveFrom;
            bool success = MoveNumber(moveTo, moveTo + velocity);
            if (!success) { return false; }
        }
        // �ړ�����
        //field[moveFrom.y, moveFrom.x].transform.position = new Vector3(moveTo.x, map.GetLength(0) - moveTo.y, 0);

        Vector3 moveToPosition = new Vector3(moveTo.x, map.GetLength(0) - moveTo.y, 0);
        field[moveFrom.y, moveFrom.x].GetComponent<Move>().MoveTo(moveToPosition);

        field[moveTo.y, moveTo.x] = field[moveFrom.y, moveFrom.x];
        field[moveFrom.y, moveFrom.x] = null;

        // �p�[�e�B�N���̐������s��
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
        // �ϒ��z��̍쐬
        List<Vector2Int> goals = new List<Vector2Int>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // �i�[�ꏊ���ۂ��𔻒f
                if (map[y, x] == 3)
                {
                    // �i�[�ꏊ�̃C���f�b�N�X���T���Ă���
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // �v�f����goals.Count�Ŏ擾
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = field[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Box")
            {
                // ��ł������Ȃ�������������B��
                return false;
            }
        }
        // �������B���łȂ���Ώ����B��
        return true;
    }

    void ResetGame()
    {
        // ���ׂẴI�u�W�F�N�g���폜����
        foreach (GameObject obj in field)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // �V�����}�b�v���쐬���A�e�I�u�W�F�N�g�����̈ʒu�ɔz�u����
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
                // �e�I�u�W�F�N�g��z�u����
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
        // �X�e�[�W0
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
        // �X�e�[�W1
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
        // �X�e�[�W2
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

        // ���݂̃X�e�[�W��ݒ�
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
                // playerPrefab�̎��̉�
                if (map[y, x] == 1)
                {
                    field[y, x] = Instantiate(
                        playerPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                        );
                }
                // boxPrefab�̎��̉�
                if (map[y, x] == 2)
                {
                    field[y, x] = Instantiate(
                        boxPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                        );
                }
                // goalPrefab�̎��̉�
                if (map[y, x] == 3)
                {
                    field[y, x] = Instantiate(
                        goalPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.01f), // �v���C���[�Əd�Ȃ�Ȃ��悤�ɏ�������Z���W�����ɐݒ�
                        Quaternion.identity
                        );
                }
                // wallPrefab�̎��̉�
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
        // �N���A��ʂł͈ړ��ƃ��Z�b�g���s���Ȃ��悤�ɂ���
        if (!IsCleard())
        {
            // �E�ړ�
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // �ړ����̌��ʉ����Đ�
                audioSource.PlayOneShot(sound1);
                // ���\�b�h�������������g�p
                Vector2Int playerIndex = GetPlayerIndex();
                // �ړ��������֐���
                MoveNumber(playerIndex, playerIndex + new Vector2Int(1, 0));
                //PrintArray();
            }

            // ���ړ�
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // �ړ����̌��ʉ����Đ�
                audioSource.PlayOneShot(sound1);
                Vector2Int playerIndex = GetPlayerIndex();

                // �ړ��������֐���
                MoveNumber(playerIndex, playerIndex + new Vector2Int(-1, 0));
                //PrintArray();
            }

            // ��ړ�
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // �ړ����̌��ʉ����Đ�
                audioSource.PlayOneShot(sound1);
                Vector2Int playerIndex = GetPlayerIndex();

                // �ړ��������֐���
                MoveNumber(playerIndex, playerIndex + new Vector2Int(0, -1));
                //PrintArray();
            }

            // ���ړ�
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // �ړ����̌��ʉ����Đ�
                audioSource.PlayOneShot(sound1);
                Vector2Int playerIndex = GetPlayerIndex();

                // �ړ��������֐���
                MoveNumber(playerIndex, playerIndex + new Vector2Int(0, 1));
                //PrintArray();
            }

            // �X�e�[�W���Z�b�g
            if (Input.GetKeyDown(KeyCode.R))
            {
                audioSource.PlayOneShot(sound2); // ���ʉ����Đ�
                ResetGame();
            }
        }

        //�����N���A���Ă�����
        if (IsCleard())
        {
            if (!hasPlayedClearSound) // �N���A���̌��ʉ����܂��Đ�����Ă��Ȃ��ꍇ
            {
                audioSource.PlayOneShot(audioSource.clip); // ���ʉ����Đ�
                hasPlayedClearSound = true; // �t���O��ݒ肵�čĐ����ꂽ���Ƃ��L�^����
            }
            // �Q�[���I�u�W�F�N�g��SetActive���\�b�h���g���L����
            clearText.SetActive(true);
            nextText.SetActive(true);

            // �L�[���͂Ŏ��̃X�e�[�W��
            if (Input.GetKeyDown(KeyCode.Return))
            {
                clearText.SetActive(false); // �N���A�e�L�X�g������
                nextText.SetActive(false);
                if (currentStage <= 3)
                {
                    currentStage += 1; // ���̃X�e�[�W��
                }
                ResetGame(); // �X�e�[�W�̃��Z�b�g
                hasPlayedClearSound = false; // ���Z�b�g��Ƀt���O�����Z�b�g����
            }
        }
    }
}
