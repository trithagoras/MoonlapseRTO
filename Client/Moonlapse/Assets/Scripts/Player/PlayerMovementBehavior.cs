using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonlapse.Networking;

public class PlayerMovementBehavior : CharacterMovementBehaviour
{
    public bool isChatting;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (isChatting)
        {
            move = Vector3.zero;
        }
        else
        {
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
        }

        var oldVelocity = velocity;

        base.Update();

        if (velocity != oldVelocity)
        {
            NetworkState.SendPacket(Packet.ConstructMovePacket(move.x, move.y));
        }

    }
}
