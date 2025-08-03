using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace DefaultNamespace
{
    public class Spike : MonoBehaviour
    {
        [SerializeField] private VoidEventChannelSO _triggerMapActionEvent;
        [SerializeField] private VoidEventChannelSO _gameFailedEvent;

        protected const string _upParameterName = "Up";
        protected int _upAnimationParameter;
        public virtual HashSet<int> _animatorParameters { get; set; } = new HashSet<int>();
        public bool IsUp;
        
        [Header("Test")]
        [MMInspectorButton("TestUp")]
        public bool UpButton;
        [MMInspectorButton("TestDown")]
        public bool DownButton;

        Animator _animator;

        private int _counter = 0;

        void Awake()
        {
            this._animator = this.GetComponentInChildren<Animator>();
            MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _upParameterName, out _upAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
        }

        private void OnEnable()
        {
            _triggerMapActionEvent.OnEventRaised += OnAction;
        }

        private void OnDisable()
        {
            _triggerMapActionEvent.OnEventRaised -= OnAction;
        }

        void OnAction()
        {
            if (_counter == 0)
            {
                if (IsUp)
                {
                    SpikeDown();
                }
                else
                {
                    SpikeUp();
                }
                _counter++;
            }
            else
            {
                _counter = 0;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _gameFailedEvent.RaiseEvent();
            }
        }

        void TestUp()
        {
            this.SpikeUp();
        }

        void TestDown()
        {
            this.SpikeDown();
        }

        public void SpikeUp()
        {
            IsUp = true;
            this._animator.SetBool(this._upAnimationParameter, true);
        }

        public void SpikeDown()
        {
            IsUp = false;
            this._animator.SetBool(this._upAnimationParameter, false);
        }


    }
}