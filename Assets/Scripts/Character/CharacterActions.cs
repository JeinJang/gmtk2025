using System.Collections;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterGridMovement))]
    public class CharacterActions : MonoBehaviour
    {
        [SerializeField] private ActionEventChannelSO _triggerActionEvent;
        [SerializeField] private VoidEventChannelSO _actionEndEvent;

        [Header("Test")]
        [MMInspectorButton("TestLeftOneCell")]
        public bool LeftButton;
        [MMInspectorButton("TestRightOneCell")]
        public bool RightButton;
        [MMInspectorButton("TestUpOneCell")]
        public bool UpButton;
        [MMInspectorButton("TestDownOneCell")]
        public bool DownButton;

        public float actionDuration = 1f;

        CharacterGridMovement _characterGridMovement;

        protected virtual void Awake()
        {
            this._characterGridMovement = this.GetComponent<CharacterGridMovement>();
        }

        private void OnEnable()
        {
            _triggerActionEvent.OnEventRaised += OnAction;
        }

        private void OnDisable()
        {
            _triggerActionEvent.OnEventRaised -= OnAction;
        }

        private void OnAction()
        {
            if (_triggerActionEvent.actionType == ActionType.MOVE_FORWARD)
            {
                TestUpOneCell();
            }
            else if (_triggerActionEvent.actionType == ActionType.MOVE_BACKWARD)
            {
                TestDownOneCell();
            }
            else if (_triggerActionEvent.actionType == ActionType.MOVE_LEFT)
            {
                TestLeftOneCell();
            }
            else if (_triggerActionEvent.actionType == ActionType.MOVE_RIGHT)
            {
                TestRightOneCell();
            }
            else
            {
                StartCoroutine(DoNothing());
            }
        }


        public virtual void TestLeftOneCell()
        {
            StartCoroutine(OneCell(Vector2.left));
        }

        public virtual void TestRightOneCell()
        {
            StartCoroutine(OneCell(Vector2.right));
        }
        
        public virtual void TestUpOneCell()
        {
            StartCoroutine(OneCell(Vector2.up));
        }

        public virtual void TestDownOneCell()
        {
            StartCoroutine(OneCell(Vector2.down));
        }

        public IEnumerator DoNothing()
        {
            yield return new WaitForSeconds(actionDuration);
            _actionEndEvent.RaiseEvent();
        }

        /// <summary>
        /// An internal coroutine used to move the character one cell in the specified direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected virtual IEnumerator OneCell(Vector2 direction)
        {
            _characterGridMovement.SetMovement(direction);
            yield return null;
            _characterGridMovement.SetMovement(Vector2.zero);
            yield return new WaitForSeconds(actionDuration);
            _actionEndEvent.RaiseEvent();
        }
    }
}