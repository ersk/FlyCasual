﻿using System.Collections;
using System.Collections.Generic;
using GameModes;
using Ship;
using System;
using System.Linq;
using Players;
using ActionsList;
using GameCommands;
using UnityEngine;

namespace SubPhases
{
    public enum TargetTypes
    {
        This,
        OtherFriendly,
        Enemy,
        Any
    }

    public class SelectShipSubPhase : GenericSubPhase
    {
        public override List<GameCommandTypes> AllowedGameCommandTypes { get { return new List<GameCommandTypes>() { GameCommandTypes.SelectShip, GameCommandTypes.PressSkip, GameCommandTypes.CancelShipSelection }; } }

        public bool IsLocked { get; set; }

        protected List<TargetTypes> targetsAllowed = new List<TargetTypes>();
        protected int minRange = 0;
        protected int maxRange = 3;

        public bool CanMeasureRangeBeforeSelection = true;

        protected Action finishAction;
        public Func<GenericShip, bool> FilterShipTargets { get; set; }
        public Func<GenericShip, int> GetAiPriority;

        public bool IsInitializationFinished;

        public virtual GenericShip TargetShip { get; set; }

        public bool ShowSkipButton = true;

        public GenericAction HostAction { get; set; }

        public Action OnSkip { get; private set; }

        public override void Start()
        {
            IsTemporary = true;

            Prepare();
            Initialize();

            CanBePaused = true;

            UpdateHelpInfo();

            base.Start();

            // If not skipped
            if (Phases.CurrentSubPhase == this)
            {
                CameraScript.RestoreCamera();

                IsReadyForCommands = true;
                IsLocked = false;
                Roster.GetPlayer(RequiredPlayer).SelectShipForAbility();
            }
        }

        public override void Prepare()
        {

        }

        public void PrepareByParameters(
            Action selectTargetAction,
            Func<GenericShip, bool> filterTargets,
            Func<GenericShip, int> getAiPriority,
            PlayerNo subphaseOwnerPlayerNo,
            bool showSkipButton,
            string abilityName,
            string description,
            IImageHolder imageSource = null,
            Action onSkip = null
        )
        {
            FilterShipTargets = filterTargets;
            GetAiPriority = getAiPriority;
            finishAction = selectTargetAction;
            RequiredPlayer = subphaseOwnerPlayerNo;
            if (showSkipButton)
            {
                UI.ShowSkipButton();
            }
            else
            {
                UI.HideSkipButton();
            }
            DescriptionShort = abilityName;
            DescriptionLong = description;
            ImageSource = imageSource;
            OnSkip = onSkip;
        }

        public override void Initialize()
        {

        }

        public void HighlightShipsToSelect()
        {
            ShowSubphaseDescription(DescriptionShort, DescriptionLong, ImageSource);
            Roster.HighlightShipsFiltered(FilterShipTargets);
            IsInitializationFinished = true;
        }

        public void AiSelectPrioritizedTarget()
        {
            List<GenericShip> filteredShips = Roster.AllShips.Values.Where(n => FilterShipTargets(n)).ToList();
            if (filteredShips == null || filteredShips.Count == 0)
            {
                SkipButton();
            }
            else
            {
                GenericShip prioritizedTarget = null;
                int maxPriority = 0;

                foreach (var ship in filteredShips)
                {
                    int calculatedPriority = GetAiPriority(ship);
                    if (calculatedPriority > maxPriority)
                    {
                        maxPriority = calculatedPriority;
                        prioritizedTarget = ship;
                    }
                }

                if (prioritizedTarget != null)
                {
                    AiSelectShipAsTarget(prioritizedTarget);
                }
                else
                {
                    SkipButton();
                }
            }
        }

        public override void Next()
        {
            Roster.AllShipsHighlightOff();
            HideSubphaseDescription();
            UI.HideSkipButton();

            Phases.CurrentSubPhase = Phases.CurrentSubPhase.PreviousSubPhase;
            UpdateHelpInfo();
        }

        public override bool ThisShipCanBeSelected(GenericShip ship, int mouseKeyIsPressed)
        {
            bool result = false;

            if (!IsInitializationFinished) return result;

            if (Roster.GetPlayer(RequiredPlayer).GetType() == typeof(HumanPlayer))
            {
                if (FilterShipTargets(ship))
                {
                    if (ship == Selection.ThisShip)
                    {
                        TryToSelectThisShip();
                    }
                    else
                    {
                        if (mouseKeyIsPressed == 1)
                        {
                            if (IsLocked) return false;
                            IsLocked = true;

                            SendSelectShipCommand(ship);
                        }
                        else if (mouseKeyIsPressed == 2)
                        {
                            if (CanMeasureRangeBeforeSelection)
                            {
                                ActionsHolder.GetRangeAndShow(Selection.ThisShip, ship);
                            }
                            else
                            {
                                Messages.ShowError("You cannot measure range before selecting another ship");
                            }
                        }
                    }
                }
                else
                {
                    if (IsLocked) return false;
                    IsLocked = true;

                    Messages.ShowErrorToHuman("You cannot select this friendly ship");
                    Selection.ThisShip.CallActionTargetIsWrong(HostAction, ship, CancelShipSelection);
                }
            }
            return result;
        }

        public override bool AnotherShipCanBeSelected(GenericShip anotherShip, int mouseKeyIsPressed)
        {
            bool result = false;

            if (Roster.GetPlayer(RequiredPlayer).GetType() != typeof(NetworkOpponentPlayer))
            {
                if (mouseKeyIsPressed == 1)
                {
                    if (FilterShipTargets(anotherShip))
                    {
                        if (IsLocked) return false;
                        IsLocked = true;

                        SendSelectShipCommand(anotherShip);
                    }
                    else
                    {
                        if (IsLocked) return false;
                        IsLocked = true;

                        Messages.ShowErrorToHuman("You cannot select this enemy ship");
                        Selection.ThisShip.CallActionTargetIsWrong(HostAction, anotherShip, CancelShipSelection);
                    }
                }
                else if (mouseKeyIsPressed == 2)
                {
                    if (CanMeasureRangeBeforeSelection)
                    {
                        ActionsHolder.GetRangeAndShow(Selection.ThisShip, anotherShip);
                    }
                    else
                    {
                        Messages.ShowError("You cannot measure range before selecting another ship");
                    }
                }
            }
            return result;
        }

        private void AiSelectShipAsTarget(GenericShip ship)
        {
            SendSelectShipCommand(ship);
        }

        private void TryToSelectThisShip()
        {
            if (FilterShipTargets(Selection.ThisShip) && !IsLocked)
            {
                IsLocked = true;

                SendSelectShipCommand(Selection.ThisShip);
            }
            else
            {
                Messages.ShowErrorToHuman("Please select a different ship");
                CancelShipSelection();
            }
        }

        public static void SendSelectShipCommand(GenericShip ship)
        {
            JSONObject parameters = new JSONObject();
            parameters.AddField("id", ship.ShipId.ToString());

            GameMode.CurrentGameMode.ExecuteCommand
            (
                GameController.GenerateGameCommand
                (
                    GameCommandTypes.SelectShip,
                    Phases.CurrentSubPhase.GetType(),
                    Phases.CurrentSubPhase.ID,
                    parameters.ToString()
                )
            );
        }

        public static void SelectShip(int shipId)
        {
            GenericShip ship = Roster.GetShipById("ShipId:" + shipId);

            (Phases.CurrentSubPhase as SelectShipSubPhase).TargetShip = ship;

            UI.HideNextButton();
            if (ship != Selection.ThisShip) MovementTemplates.ShowRange(Selection.ThisShip, ship);

            (Phases.CurrentSubPhase as SelectShipSubPhase).InvokeFinish();
        }

        protected virtual void CancelShipSelection()
        {
            GameMode.CurrentGameMode.ExecuteServerCommand(GenerateCancelShipSelectionCommand());
        }

        private GameCommand GenerateCancelShipSelectionCommand()
        {
            return GameController.GenerateGameCommand(
                GameCommandTypes.CancelShipSelection,
                Phases.CurrentSubPhase.GetType(),
                Phases.CurrentSubPhase.ID
            );
        }

        public void CallRevertSubPhase()
        {
            RevertSubPhase();
            IsLocked = false;
        }

        public virtual void RevertSubPhase()
        {
            Phases.CurrentSubPhase = PreviousSubPhase;
            Roster.AllShipsHighlightOff();
            HideSubphaseDescription();
            Phases.CurrentSubPhase.Resume();
            UpdateHelpInfo();
        }

        public void InvokeFinish()
        {
            finishAction.Invoke();
        }

        public static void FinishSelectionNoCallback()
        {
            Phases.FinishSubPhase(Phases.CurrentSubPhase.GetType());
            Phases.CurrentSubPhase.Resume();
        }

        public static void FinishSelection()
        {
            Action callback = Phases.CurrentSubPhase.CallBack;
            FinishSelectionNoCallback();
            callback();
        }

        public override void Pause()
        {
            base.Pause();

            HideSubphaseDescription();
        }

        public override void Resume()
        {
            base.Resume();

            ShowSubphaseDescription(DescriptionShort, DescriptionLong, ImageSource);
            IsLocked = false;
        }

        public override void SkipButton()
        {
            OnSkip?.Invoke();
        }

    }

}
