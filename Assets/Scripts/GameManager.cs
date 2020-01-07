using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PlayerController player;

    Vector3 checkpoint;

    public Vector3 Checkpoint { get => checkpoint; set => checkpoint = value; }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        checkpoint = player.gameObject.transform.position;
    }

    public void PlayerDied()
    {
        player.transform.position = checkpoint;
    }
}
