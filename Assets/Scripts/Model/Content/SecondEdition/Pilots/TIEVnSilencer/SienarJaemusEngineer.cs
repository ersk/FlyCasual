﻿using Upgrade;

namespace Ship
{
    namespace SecondEdition.TIEVnSilencer
    {
        public class SienarJaemusEngineer : TIEVnSilencer
        {
            public SienarJaemusEngineer() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Sienar-Jaemus Engineer",
                    1,
                    50
                );

                ImageUrl = "https://squadbuilder.fantasyflightgames.com/card_images/en/8f7c4680fbc001169baf6538ab259e9b.png";
            }
        }
    }
}
