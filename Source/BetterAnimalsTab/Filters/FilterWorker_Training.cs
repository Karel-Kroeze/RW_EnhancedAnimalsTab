// FilterWorker_Training.cs
// Copyright Karel Kroeze, 2017-2017

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalTab {
    public class FilterWorker_Training: FilterWorker {
        private readonly DefMap<TrainableDef, bool> allowed = new DefMap<TrainableDef, bool>();
        private IEnumerable<TrainableDef> Trainables => DefDatabase<TrainableDef>.AllDefsListForReading;

        public override FilterState State {
            get => allowed.Values().Any(v => v) ? FilterState.Inclusive : FilterState.Inactive;
            set => throw new InvalidOperationException("FilterWorker_Training.set_State() should never be called");
        }

        public override bool Allows(Pawn pawn) {
            if (State == FilterState.Inactive) {
                return true;
            }

            if (pawn?.training == null) {
                return false;
            }

            foreach (TrainableDef trainable in Trainables) {
                if (allowed[trainable] && pawn.training.HasLearned(trainable)) {
                    return true;
                }
            }

            return false;
        }

        public override void Clicked() {
            List<FloatMenuOption> options = new List<FloatMenuOption> {
                new FloatMenuOption("AnimalTab.All".Translate(), Deactivate)
            };
            foreach (TrainableDef trainable in Trainables) {
                options.Add(new FloatMenuOption_Persistent(trainable.LabelCap, () => Toggle(trainable), extraPartWidth: 30f, extraPartOnGUI: rect => DrawOptionExtra(rect, trainable)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public void Toggle(TrainableDef trainable) {
            allowed[trainable] = !allowed[trainable];
            MainTabWindow_Animals.Instance.Notify_PawnsChanged();
        }

        public void Deactivate() {
            allowed.SetAll(false);
            MainTabWindow_Animals.Instance.Notify_PawnsChanged();
        }

        private bool DrawOptionExtra(Rect rect, TrainableDef trainable) {
            if (State == FilterState.Inclusive && allowed[trainable]) {
                Rect checkRect = new Rect(rect.xMax - rect.height + Constants.Margin, rect.yMin, rect.height, rect.height).ContractedBy(7f);
                GUI.DrawTexture(checkRect, Widgets.CheckboxOnTex);
            }
            return false;
        }

        public override string GetTooltip() {
            if (State == FilterState.Inactive) {
                return "AnimalTab.FilterInactiveTip".Translate();
            }

            List<string> _allowed = ( from trainable in Trainables where allowed[trainable] select trainable.label ).ToList();
            return "AnimalTab.TrainableFilterTip".Translate(_allowed.ToStringList("AnimalTab.Or".Translate()));
        }
    }
}
