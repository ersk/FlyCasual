﻿using Ship;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tokens
{

    public class BlueTargetLockToken : GenericTargetLockToken
    {

        public BlueTargetLockToken(GenericShip host) : base(host)
        {
            Name = ImageName = "Blue Target Lock Token";
            TokenColor = TokenColors.Blue;
            TokenShape = TokenShapes.Square;
            PriorityUI = 55;
            Action = new ActionsList.TargetLockAction() { HostShip = host };
        }

    }

}
