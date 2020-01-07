using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hells");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Hello");

        if (collision.tag.Equals("Player"))
        {
            Vector2 newPosition = new Vector2(collision.transform.position.x, 0) + new Vector2(21.3f,0);

            mainCamera.transform.Translate(newPosition);
        }
    }
}
