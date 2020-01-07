using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    public LayerMask groundLayer;

    private bool onGround = false;

    private bool onRightWall = false;
    private bool onLeftWall = false;

    private float leftWidth;
    private float rightWidth;

    private float centerXOffset;
    private float centerYOffset;

    public float leftWallDist = 0;
    public float rightWallDist = 0;

    public float xPos;

    public bool OnGround { get => onGround;}
    public bool OnRightWall { get => onRightWall; }
    public bool OnLeftWall { get => onLeftWall; }

    // Start is called before the first frame update
    void Start()
    {
        leftWidth = GetComponent<BoxCollider2D>().size.y / 1.1f;
        rightWidth = GetComponent<BoxCollider2D>().size.y / 1.1f;

        centerXOffset = GetComponent<BoxCollider2D>().offset.x;
        centerYOffset = GetComponent<BoxCollider2D>().offset.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rightPos = (Vector2)transform.position + new Vector2(xPos + centerXOffset,centerYOffset);
        Vector2 leftPos = (Vector2)transform.position + new Vector2(-xPos + centerXOffset,centerYOffset);

        onRightWall = Physics2D.OverlapBox(rightPos, new Vector2(rightWallDist, rightWidth),0,groundLayer);
        onLeftWall = Physics2D.OverlapBox(leftPos, new Vector2(leftWallDist, leftWidth),0,groundLayer);

        ExtDebug.DrawBox(rightPos, new Vector2(rightWallDist / 2, rightWidth / 2),Quaternion.identity,Color.blue);
        ExtDebug.DrawBox(leftPos, new Vector2(leftWallDist / 2, leftWidth / 2),Quaternion.identity,Color.green);
    }
}
