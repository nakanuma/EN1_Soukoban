using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SE : MonoBehaviour
{
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)||
            Input.GetKeyDown(KeyCode.LeftArrow)||
            (Input.GetKeyDown(KeyCode.UpArrow)||
            Input.GetKeyDown(KeyCode.DownArrow))
            )
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
