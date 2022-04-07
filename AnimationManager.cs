using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourNamespace
{
    [ExecuteAlways]
    public class AnimationManager : MonoBehaviour
    {
        public Animator Animator { get {return _Animator; } }
        [SerializeField]
        private Animator _Animator;

        public string currentState { get { return _currentState; } }
        [SerializeField]
        private string _currentState;
        private string NextState;
        private float NextFadingDuration;
        [SerializeField]
        private float ExtraFadeTime = 0.001f;
        public bool IsFading { 
        get{
                if (FadeEnd > Time.time) {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public float TimeRemainingToFadeEnd
        {
            get
            {
                return Mathf.Clamp(FadeEnd - Time.time, 0, Mathf.Infinity);
            }
        }

        private bool NextChangeActive;
        private float FadeEnd = -1;
        void Start()
        {
            UpdateAnimatorReference();
        }
        private void OnValidate()
        {
            UpdateAnimatorReference();
        }
        private void UpdateAnimatorReference()
        {
            if (_Animator == null)
            {
                _Animator = GetComponent<Animator>();
                if (_Animator == null)
                {
                    _Animator = gameObject.AddComponent<Animator>();
                }
            }
        }
        public void ChangeAnimationState(string newState, float FadingDuration)
        {
            if (_currentState == newState) return;

            if (IsFading)
            {
                NextFadingDuration = FadingDuration;
                NextState = newState;

                if (!NextChangeActive)
                {
                    Invoke("NextChangeAnimationState", TimeRemainingToFadeEnd + ExtraFadeTime);

                    NextChangeActive = true;
                }
            }
            else
            {
                FadeEnd = Time.time + FadingDuration;
                Animator.CrossFadeInFixedTime(newState, FadingDuration);

                _currentState = newState;
                NextChangeActive = false;
            }
        }
        private void NextChangeAnimationState()
        {
            if (!IsFading)
            {
                //to prevent infinite loop
                ChangeAnimationState(NextState, NextFadingDuration);
                NextChangeActive = false;
            }
        }
    }
}
