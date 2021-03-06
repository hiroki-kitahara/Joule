﻿using System;
using HK.Framework.Extensions;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Joule.CharacterControllers.AI
{
    /// <summary>
    /// シンプルなAI
    /// </summary>
    public sealed class SimpleAIController : AIControllerBase
    {
        [SerializeField]
        private LayerMask targetLayerMask;
        
        [SerializeField]
        private StateElement[] elements;

        protected override void InternalAwake()
        {
            var tempElements = this.elements;
            this.elements = new StateElement[this.elements.Length];
            for (var i = 0; i < this.elements.Length; i++)
            {
                this.elements[i] = tempElements[i].Clone(this);
            }

            var initialStateIndex = 0;
            this.Change(this.elements[initialStateIndex].State, initialStateIndex);

            this.UpdateAsObservable()
                .Where(_ => this.isActiveAndEnabled)
                .SubscribeWithState(this,
                    (_, _this) =>
                    {
                        _this.elements[_this.currentStateIndex].CalculateNextState(_this, _this.elements);
                    })
                .AddTo(this);

            this.OnTriggerEnterAsObservable()
                .Where(_ => this.isActiveAndEnabled)
                .SubscribeWithState(this, (c, _this) =>
                {
                    var character = c.GetComponentInParent<Character>();
                    if (character == null)
                    {
                        return;
                    }

                    if (_this.targetLayerMask.IsIncluded(character.gameObject))
                    {
                        _this.Target = character;
                    }
                })
                .AddTo(this);
        }

        private bool CanTarget(Collider other)
        {
            var character = other.GetComponentInParent<Character>();
            if (character == null)
            {
                return false;
            }

            return this.targetLayerMask.IsIncluded(character.gameObject);
        }

        [Serializable]
        public class StateElement
        {
            [SerializeField]
            private string stateName;

            [SerializeField]
            private StateBase state;
            public StateBase State
            {
                get { return state; }
            }

            [SerializeField]
            private NextStateElement[] nextStateElements;

            public void CalculateNextState(AIControllerBase aiController, StateElement[] elements)
            {
                for (var i = 0; i < this.nextStateElements.Length; i++)
                {
                    var nextStateElement = this.nextStateElements[i];
                    if (nextStateElement.Condition.Evalution(aiController))
                    {
                        var elementIndex = Array.FindIndex(elements, s => s.stateName == nextStateElement.NextStateName);
                        Assert.AreNotEqual(elementIndex, -1, string.Format("{0}に対応する{1}がありませんでした", nextStateElement.NextStateName, typeof(StateElement)));
                        var nextState = elements[elementIndex];
                        aiController.Change(nextState.state, elementIndex);
                        break;
                    }
                }
            }

            public StateElement Clone(AIControllerBase aiController)
            {
                var instance = new StateElement();
                instance.stateName = this.stateName;
                instance.state = this.state.Clone(aiController);
                instance.nextStateElements = new NextStateElement[this.nextStateElements.Length];
                for (var i = 0; i < this.nextStateElements.Length; i++)
                {
                    instance.nextStateElements[i] = this.nextStateElements[i].Clone;
                }

                return instance;
            }
        }

        [Serializable]
        public class NextStateElement
        {
            [SerializeField]
            private string nextStateName;
            public string NextStateName
            {
                get { return nextStateName; }
            }

            [SerializeField]
            private ChangeStateConditionBase condition;
            public ChangeStateConditionBase Condition
            {
                get { return condition; }
            }

            public NextStateElement Clone
            {
                get
                {
                    var instance = new NextStateElement();
                    instance.nextStateName = this.nextStateName;
                    instance.condition = this.condition.Clone;

                    return instance;
                }
            }
        }
    }
}
