﻿using System.Collections;
using System.Collections.Generic;
using Ship;
using System;
using Tokens;
using SubPhases;
using Abilities.FirstEdition;
using Upgrade;

namespace Ship
{
    namespace FirstEdition.XWing
    {
        public class JekPorkins : XWing
        {
            public JekPorkins() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Jek Porkins",
                    7,
                    26,
                    isLimited: true,
                    abilityType: typeof(JekPorkinsAbility),
                    extraUpgradeIcon: UpgradeType.Talent
                );

                ModelInfo.SkinName = "Jek Porkins";
            }
        }
    }
}

namespace Abilities.FirstEdition
{
    public class JekPorkinsAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnTokenIsAssigned += CheckAbilityConditions;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnTokenIsAssigned -= CheckAbilityConditions;
        }

        private void CheckAbilityConditions(GenericShip ship, GenericToken token)
        {
            if (token is StressToken)
            {
                RegisterAbilityTrigger(TriggerTypes.OnTokenIsAssigned, AskToUsePilotAbility);
            }
        }

        private void AskToUsePilotAbility(object sender, EventArgs e)
        {
            AskToUseAbility(
                HostShip.PilotInfo.PilotName,
                ShouldUseAbility,
                RemoveStressAndRollDice,
                descriptionLong: "Do you want to remove Stress Token and to roll 1 attack die? (On a hit result, deal 1 Facedown Damage card to this ship)",
                imageHolder: HostShip
            );
        }

        private bool ShouldUseAbility()
        {
            return HostShip.State.HullCurrent > 1;
        }

        private void RemoveStressAndRollDice(object sender, EventArgs e)
        {
            DecisionSubPhase.ConfirmDecisionNoCallback();

            HostShip.Tokens.RemoveToken(typeof(StressToken), StartRollDiceSubphase);
        }

        private void StartRollDiceSubphase()
        {
            PerformDiceCheck(
                HostShip.PilotInfo.PilotName + ": Facedown damage card on hit",
                DiceKind.Attack,
                1,
                FinishAction,
                Triggers.FinishTrigger
            );
        }

        private void FinishAction()
        {
            if (DiceCheckRoll.RegularSuccesses > 0)
            {
                SufferNegativeEffect(AbilityDiceCheck.ConfirmCheck);
            }
            else
            {
                AbilityDiceCheck.ConfirmCheck();
            }
        }

        protected virtual void SufferNegativeEffect(Action callback)
        {
            HostShip.Damage.SufferFacedownDamageCard(
                new DamageSourceEventArgs()
                {
                    Source = HostShip,
                    DamageType = DamageTypes.CardAbility
                },
                callback
            );
        }
    }
}