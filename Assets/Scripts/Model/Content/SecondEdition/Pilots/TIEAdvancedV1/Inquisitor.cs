﻿using System.Collections.Generic;
using Upgrade;

namespace Ship
{
    namespace SecondEdition.TIEAdvancedV1
    {
        public class Inquisitor : TIEAdvancedV1
        {
            public Inquisitor() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Inquisitor",
                    3,
                    39,
                    force: 1,
                    extraUpgradeIcon: UpgradeType.ForcePower,
                    seImageNumber: 102
                );
            }
        }
    }
}