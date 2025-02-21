﻿using System.Collections.Generic;
using Movement;
using ActionsList;
using Actions;
using Upgrade;
using BoardTools;
using System.Linq;

namespace Ship
{
    namespace SecondEdition.TIESaBomber
    {
        public class TIESaBomber : FirstEdition.TIEBomber.TIEBomber, TIE
        {
            public TIESaBomber() : base()
            {
                ShipInfo.ShipName = "TIE/sa Bomber";

                ShipInfo.UpgradeIcons.Upgrades.Add(UpgradeType.Gunner);
                ShipInfo.UpgradeIcons.Upgrades.Add(UpgradeType.Device);
                ShipInfo.UpgradeIcons.Upgrades.Remove(UpgradeType.Torpedo);

                ShipInfo.ActionIcons.AddActions(new ActionInfo(typeof(ReloadAction), ActionColor.Red));
                ShipInfo.ActionIcons.AddLinkedAction(new LinkedActionInfo(typeof(BarrelRollAction), typeof(TargetLockAction)));

                ShipAbilities.Add(new Abilities.SecondEdition.NimbleBomber());

                IconicPilots[Faction.Imperial] = typeof(TomaxBren);

                DialInfo.AddManeuver(new ManeuverHolder(ManeuverSpeed.Speed3, ManeuverDirection.Forward, ManeuverBearing.KoiogranTurn), MovementComplexity.Complex);
                DialInfo.ChangeManeuverComplexity(new ManeuverHolder(ManeuverSpeed.Speed2, ManeuverDirection.Left, ManeuverBearing.Turn), MovementComplexity.Normal);
                DialInfo.ChangeManeuverComplexity(new ManeuverHolder(ManeuverSpeed.Speed2, ManeuverDirection.Right, ManeuverBearing.Turn), MovementComplexity.Normal);

                ManeuversImageUrl = "https://vignette.wikia.nocookie.net/xwing-miniatures-second-edition/images/0/0e/Maneuver_tie_bomber.png";
            }
        }
    }
}

namespace Abilities.SecondEdition
{
    public class NimbleBomber : GenericAbility
    {
        public override string Name { get { return "Nimble Bomber"; } }

        public override void ActivateAbility()
        {
            HostShip.OnGetAvailableBombDropTemplatesOneCondition += AddNimbleBomberTemplates;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnGetAvailableBombDropTemplatesOneCondition -= AddNimbleBomberTemplates;
        }

        private void AddNimbleBomberTemplates(List<ManeuverTemplate> availableTemplates, GenericUpgrade upgrade)
        {
            List<ManeuverTemplate> templatesCopy = new List<ManeuverTemplate>(availableTemplates);

            foreach (ManeuverTemplate existingTemplate in templatesCopy)
            {
                if (existingTemplate.Bearing == ManeuverBearing.Straight && existingTemplate.Direction == ManeuverDirection.Forward)
                {
                    List<ManeuverTemplate> newTemplates = new List<ManeuverTemplate>()
                    {
                        new ManeuverTemplate(ManeuverBearing.Bank, ManeuverDirection.Right, existingTemplate.Speed, isBombTemplate: true),
                        new ManeuverTemplate(ManeuverBearing.Bank, ManeuverDirection.Left, existingTemplate.Speed, isBombTemplate: true),
                    };

                    foreach (ManeuverTemplate newTemplate in newTemplates)
                    {
                        if (!availableTemplates.Any(t => t.Name == newTemplate.Name))
                        {
                            availableTemplates.Add(newTemplate);
                        }
                    }
                }
            }

        }
    }
}
