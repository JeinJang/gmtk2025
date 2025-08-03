using System.Collections;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterGridMovement))]
    public class CharacterActions : MonoBehaviour
    {
        [Header("Test")]
        [MMInspectorButton("TestLeftOneCell")]
        public bool LeftButton;
        [MMInspectorButton("TestRightOneCell")]
        public bool RightButton;
        [MMInspectorButton("TestUpOneCell")]
        public bool UpButton;
        [MMInspectorButton("TestDownOneCell")]
        public bool DownButton;

        CharacterGridMovement _characterGridMovement;

        protected virtual void Awake()
        {
            this._characterGridMovement = this.GetComponent<CharacterGridMovement>();
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
        }
    }
}