﻿using Ship;
using Upgrade;
using SubPhases;
using Tokens;

namespace UpgradesList.FirstEdition
{
    public class ElectronicBaffle : GenericUpgrade
    {
        public ElectronicBaffle() : base()
        {
            UpgradeInfo = new UpgradeCardInfo(
                "Electronic Baffle",
                UpgradeType.Sensor,
                cost: 1,
                abilityType: typeof(Abilities.FirstEdition.ElectronicBaffleAbility)
            );
        }        
    }
}

namespace Abilities.FirstEdition
{

    public class ElectronicBaffleAbility : GenericAbility
    {

        //This ability is gonna be checked on each token assigned to HostShip
        public override void ActivateAbility()
        {
            HostShip.OnTokenIsAssigned += RegisterElectronicBaffle;
            HostShip.OnShipIsDestroyed += RegisterCleanUp;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnTokenIsAssigned -= RegisterElectronicBaffle;
        }


        private void RegisterCleanUp(GenericShip ship, bool isFled)
        {
            RegisterAbilityTrigger(TriggerTypes.OnShipIsDestroyed, CleanUp);
        }


        private void CleanUp(object sender, System.EventArgs e)
        {
            HostShip.OnTokenIsAssigned -= RegisterElectronicBaffle;
            Triggers.FinishTrigger();
        }


        private void RegisterElectronicBaffle(object sender, GenericToken token)
        {
            if (token is StressToken)
            {
                RegisterAbilityTrigger(TriggerTypes.OnTokenIsAssigned, ShowUseEBStress);
            }

            if (token is IonToken)
            {
                RegisterAbilityTrigger(TriggerTypes.OnTokenIsAssigned, ShowUseEBIon);
            }
        }


        private void ShowUseEBIon(object sender, System.EventArgs e)
        {
            AskToUseAbility(
                HostUpgrade.UpgradeInfo.Name,
                AlwaysUseByDefault,
                RemoveIon,
                descriptionLong: "Do you want to suffer 1 damage to discard Ion Token?",
                imageHolder: HostUpgrade
            );
        }


        private void ShowUseEBStress(object sender, System.EventArgs e)
        {
            AskToUseAbility(
                HostUpgrade.UpgradeInfo.Name,
                AlwaysUseByDefault,
                RemoveStress,
                descriptionLong: "Do you want to suffer 1 damage to discard Stress Token?",
                imageHolder: HostUpgrade
            );
        }


        private void RemoveIon(object sender, System.EventArgs e)
        {
            //This token could be intercepted by other ability
            if (HostShip.Tokens.HasToken(typeof(Tokens.IonToken)))
            {
                Messages.ShowInfo("Electronic Baffle removes 1 ion token at the cost of 1 damage");
                HostShip.Tokens.RemoveToken(typeof(Tokens.IonToken), delegate { SufferDamage(); });

            }
        }


        private void RemoveStress(object sender, System.EventArgs e)
        {
            //This token could be intercepted by other ability
            if (HostShip.Tokens.HasToken(typeof(Tokens.StressToken)))
            {
                Messages.ShowInfo("Electronic Baffle removes 1 stress token at the cost of 1 damage");
                HostShip.Tokens.RemoveToken(typeof(Tokens.StressToken), delegate { SufferDamage(); });

            }
        }

        private void SufferDamage()
        {
            DamageSourceEventArgs harpoonconditionDamage = new DamageSourceEventArgs()
            {
                Source = "Electronic Baffle",
                DamageType = DamageTypes.CardAbility
            };

            HostShip.Damage.TryResolveDamage(1, harpoonconditionDamage, DecisionSubPhase.ConfirmDecision);
        }
    }
}