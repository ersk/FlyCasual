﻿using Ship;
using Upgrade;

namespace UpgradesList.FirstEdition
{
    public class AgentKallus : GenericUpgrade
    {
        public AgentKallus() : base()
        {
            UpgradeInfo = new UpgradeCardInfo(
                "Agent Kallus",
                UpgradeType.Crew,
                cost: 2,
                isLimited: true,
                restriction: new FactionRestriction(Faction.Imperial),
                abilityType: typeof(Abilities.FirstEdition.AgentKallusAbility)
            );
        }        
    }
}

namespace Abilities.FirstEdition
{
    public class AgentKallusAbility : GenericAbility
    {

        public GenericShip AgentKallusSelectedTarget;

        public override void ActivateAbility()
        {
            Phases.Events.OnSetupStart += RegisterAgentKallusAbility;
        }

        public override void DeactivateAbility()
        {
            Phases.Events.OnSetupStart -= RegisterAgentKallusAbility;
        }

        protected void RegisterAgentKallusAbility()
        {
            Triggers.RegisterTrigger(new Trigger()
            {
                Name = "Agent Kallus decision",
                TriggerType = TriggerTypes.OnGameStart,
                TriggerOwner = HostShip.Owner.PlayerNo,
                EventHandler = SelectAgentKallusTarget,
                Skippable = true
            });
        }

        protected void SelectAgentKallusTarget(object Sender, System.EventArgs e)
        {
            AgentKallusDecisionSubPhase selectAgentKallusTargetDecisionSubPhase = (AgentKallusDecisionSubPhase)Phases.StartTemporarySubPhaseNew(
                Name,
                typeof(AgentKallusDecisionSubPhase),
                Triggers.FinishTrigger
            );

            selectAgentKallusTargetDecisionSubPhase.DescriptionShort = "Agent Kallus";
            selectAgentKallusTargetDecisionSubPhase.DescriptionLong = "Assign the Haunted condition to 1 enemy ship";
            selectAgentKallusTargetDecisionSubPhase.ImageSource = HostUpgrade;

            foreach (var enemyShip in Roster.GetPlayer(Roster.AnotherPlayer(HostShip.Owner.PlayerNo)).Ships)
            {
                selectAgentKallusTargetDecisionSubPhase.AddDecision(
                    enemyShip.Value.ShipId + ": " + enemyShip.Value.PilotInfo.PilotName,
                    delegate { SelectTarget(enemyShip.Value); }
                );
            }

            GenericShip bestEnemyAce = GetEnemyPilotWithHighestSkill();
            selectAgentKallusTargetDecisionSubPhase.DefaultDecisionName = bestEnemyAce.ShipId + ": " + bestEnemyAce.PilotInfo.PilotName;

            selectAgentKallusTargetDecisionSubPhase.RequiredPlayer = HostShip.Owner.PlayerNo;

            selectAgentKallusTargetDecisionSubPhase.Start();
        }

        protected virtual void SelectTarget(GenericShip targetShip)
        {
            Messages.ShowInfo("Agent Kallus is hunting " + targetShip.PilotInfo.PilotName + " (" + targetShip.ShipId + ")");

            AgentKallusSelectedTarget = targetShip;

            HostShip.OnGenerateDiceModifications += AddAgentKallusDiceModification;

            SubPhases.DecisionSubPhase.ConfirmDecision();
        }

        protected virtual void AddAgentKallusDiceModification(GenericShip host)
        {
            ActionsList.GenericAction newAction = new ActionsList.AgentKallusDiceModification()
            {
                ImageUrl = HostUpgrade.ImageUrl,
                AgentKallusSelectedTarget = AgentKallusSelectedTarget
            };
            host.AddAvailableDiceModificationOwn(newAction);
        }

        private GenericShip GetEnemyPilotWithHighestSkill()
        {
            GenericShip bestAce = null;
            int maxPilotSkill = 0;
            foreach (var enemyShip in Roster.GetPlayer(Roster.AnotherPlayer(HostShip.Owner.PlayerNo)).Ships)
            {
                if (enemyShip.Value.State.Initiative > maxPilotSkill)
                {
                    bestAce = enemyShip.Value;
                    maxPilotSkill = enemyShip.Value.State.Initiative;
                }
            }
            return bestAce;
        }

        protected class AgentKallusDecisionSubPhase : SubPhases.DecisionSubPhase { }
    }
}

namespace ActionsList
{

    public class AgentKallusDiceModification : GenericAction
    {
        public GenericShip AgentKallusSelectedTarget;

        public AgentKallusDiceModification()
        {
            Name = DiceModificationName = "Agent Kallus";

            IsTurnsOneFocusIntoSuccess = true;
        }

        public override bool IsDiceModificationAvailable()
        {
            bool result = false;

            switch (Combat.AttackStep)
            {
                case CombatStep.Attack:
                    if (Combat.Defender.ShipId == AgentKallusSelectedTarget.ShipId) result = true;
                    break;
                case CombatStep.Defence:
                    if (Combat.Attacker.ShipId == AgentKallusSelectedTarget.ShipId) result = true;
                    break;
                default:
                    break;
            }

            return result;
        }

        public override int GetDiceModificationPriority()
        {
            int result = 0;

            if (Combat.CurrentDiceRoll.Focuses > 0)
            {
                result = 100;
            }

            return result;
        }

        public override void ActionEffect(System.Action callBack)
        {
            if (Combat.CurrentDiceRoll.Focuses > 0)
            {
                Combat.CurrentDiceRoll.Change(DieSide.Focus, DieSide.Success, 1);
            }
            else
            {
                Messages.ShowError("This die roll had no Focus results to change");
            }
            callBack();
        }

    }

}