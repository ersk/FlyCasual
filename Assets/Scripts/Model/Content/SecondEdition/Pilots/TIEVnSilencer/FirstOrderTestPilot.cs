﻿using Upgrade;

namespace Ship
{
    namespace SecondEdition.TIEVnSilencer
    {
        public class FirstOrderTestPilot : TIEVnSilencer
        {
            public FirstOrderTestPilot() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "First Order Test Pilot",
                    4,
                    56,
                    extraUpgradeIcon: UpgradeType.Talent
                );

                ImageUrl = "https://squadbuilder.fantasyflightgames.com/card_images/en/568abbcd68bb174173da4e7ee92051e3.png";
            }
        }
    }
}
