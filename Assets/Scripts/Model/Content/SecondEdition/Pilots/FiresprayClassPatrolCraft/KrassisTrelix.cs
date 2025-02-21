﻿using Arcs;
using Ship;
using System.Collections.Generic;
using Upgrade;

namespace Ship
{
    namespace SecondEdition.FiresprayClassPatrolCraft
    {
        public class KrassisTrelix : FiresprayClassPatrolCraft
        {
            public KrassisTrelix() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Krassis Trelix",
                    3,
                    65,
                    isLimited: true,
                    abilityType: typeof(Abilities.SecondEdition.KrassisTrelixAbility),
                    extraUpgradeIcons: new List<UpgradeType>() { UpgradeType.Talent, UpgradeType.Crew },
                    seImageNumber: 153
                );

                ModelInfo.SkinName = "Krassis Trelix";
            }
        }
    }
}

namespace Abilities.SecondEdition
{
    public class KrassisTrelixAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnGameStart += ChangeSpecialWeaponsRestrictions;

            AddDiceModification(
                HostShip.PilotInfo.PilotName,
                IsDiceModificationAvailable,
                GetAiPriority,
                DiceModificationType.Reroll,
                1
            );
        }

        public override void DeactivateAbility()
        {
            RestoreSpecialWeaponsRestrictions();
            RemoveDiceModification();
        }

        private bool IsDiceModificationAvailable()
        {
            return Combat.AttackStep == CombatStep.Attack && Combat.ChosenWeapon.WeaponType != WeaponTypes.PrimaryWeapon;
        }

        private int GetAiPriority()
        {
            return 90;
        }

        private void ChangeSpecialWeaponsRestrictions()
        {
            HostShip.OnGameStart -= ChangeSpecialWeaponsRestrictions;

            foreach (GenericUpgrade upgrade in HostShip.UpgradeBar.GetSpecialWeaponsAll())
            {
                GenericSpecialWeapon weapon = upgrade as GenericSpecialWeapon;
                weapon.WeaponInfo.ArcRestrictions.Add(ArcType.Rear);
            }
        }

        private void RestoreSpecialWeaponsRestrictions()
        {
            foreach (GenericUpgrade upgrade in HostShip.UpgradeBar.GetSpecialWeaponsAll())
            {
                GenericSpecialWeapon weapon = upgrade as GenericSpecialWeapon;
                weapon.WeaponInfo.ArcRestrictions.Remove(ArcType.Rear);
            }
        }
    }
}