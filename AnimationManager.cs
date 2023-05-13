using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourNamespace
{
    [ExecuteAlways]
    public class AnimationManager : MonoBehaviour
    {
                public enum UpdateType
        {
            Always, OnChange, Never
        }
        public bool IsSleeping
        {
            get
            {
                return _IsSleeping;
            }
            set
            {
                if(value != IsSleeping && !value)
                {
                    Animator.Play(currentState);
                }

                _IsSleeping = value;
                if (SetObjectActiveOnSleep)
                {
                    Animator.enabled = !_IsSleeping;
                }
                else
                {
                    if (!Animator.gameObject.activeInHierarchy && SetAnimatorObjectToTrue)
                    {
                        Animator.enabled = true;
                    }
                }
            }
        }
        [SerializeField]
        private bool _IsSleeping;
        public bool UseAnimatorDistanceCulling = false, SetObjectActiveOnSleep = false;
        [ShowIf("@UseAnimatorDistanceCulling == true")]
        public UpdateType DistanceCullingUpdateLoop;
        [ShowIf("@SetObjectActiveOnSleep == false")]
        public bool SetAnimatorObjectToTrue = false;
        public Animator Animator { get {return _Animator; } }
        [SerializeField]
        private Animator _Animator;
        public string currentState { get { return _currentState; } }
        [SerializeField, ReadOnly]
        private string _currentState;
        private string NextState;
        private float NextFadingDuration;
        public float AnimationSpeed { get { return Animator.speed; } set { Animator.speed = value; } }
        public float TimeAnimationPlayed
        {
            get { return Time.unscaledTime - TimeStateEntered; }
        }
        public float AnimationLength { get { return Animator.GetCurrentAnimatorStateInfo(0).length; } }
        public int LoopCount { get { return Mathf.FloorToInt(TimeAnimationPlayed / AnimationLength); } }
        public float LoopCountFloat { get { return TimeAnimationPlayed / AnimationLength; } }
        private float TimeStateEntered;
        [SerializeField]
        private float ExtraFadeTime = 0.0001f;
        public float TimeScale = 1;
        public float MaxDistanceToCameraForActive = 25;
        private Transform cam
        {
            get
            {
                if (_cam == null)
                {
                    _cam = Camera.main.transform;
                }
                return _cam;
            }
        }
        private Transform _cam;
        public void UpdateCameraDistance()
        {
            if (UseAnimatorDistanceCulling)
            {
                if (Mathf.Abs((cam.transform.position - transform.position).magnitude) > MaxDistanceToCameraForActive)
                {
                    IsSleeping = true;
                }
                else
                {
                    IsSleeping = false;
                }
            }
        }
        public bool IsFading { 
        get{
                if (FadeEnd > Time.unscaledTime) {
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
                return Mathf.Clamp(FadeEnd - Time.unscaledTime, 0, Mathf.Infinity);
            }
        }

        private bool NextChangeActive;
        private float FadeEnd = -1;
        private OnAnimatorMoveEvent invoker;
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

            if(invoker == null)
            {
                invoker = _Animator.gameObject.GetComponent<OnAnimatorMoveEvent>();
                if (invoker == null)
                {
                    invoker = _Animator.gameObject.AddComponent<OnAnimatorMoveEvent>();
                }
            }
            else
            {
                //control if its on right object
                if (invoker.gameObject != _Animator.gameObject)
                {
                    DestroyImmediate(invoker);
                    invoker = _Animator.gameObject.GetComponent<OnAnimatorMoveEvent>();
                    if (invoker == null)
                    {
                        invoker = _Animator.gameObject.AddComponent<OnAnimatorMoveEvent>();
                    }
                }
            }
            UpdateOnAnimatorEvent();
        }
        public void ForceChangeAnimationState(string newState, float FadingDuration, float AnimationStateSpeed)
        {
            if (DistanceCullingUpdateLoop == UpdateType.Always)
            {
                UpdateCameraDistance();
            }
            if (DistanceCullingUpdateLoop == UpdateType.OnChange)
            {
                UpdateCameraDistance();
            }


            AnimationSpeed = AnimationStateSpeed * TimeScale;

            if (IsFading)
            {
                NextFadingDuration = FadingDuration;
                NextState = newState;

                if (!NextChangeActive)
                {
                    if (IsSleeping)
                    {
                        Animator.Play(newState);
                    }
                    else
                    {
                        Invoke("NextChangeAnimationState", TimeRemainingToFadeEnd + ExtraFadeTime);
                    }

                    NextChangeActive = true;
                }
            }
            else
            {
                TimeStateEntered = Time.unscaledTime;
                FadeEnd = Time.unscaledTime + FadingDuration;
                if (IsSleeping)
                {
                    Animator.Play(newState);
                }
                else
                {
                    Animator.CrossFadeInFixedTime(newState, FadingDuration);
                }

                _currentState = newState;
                NextChangeActive = false;
            }
        }
        public void ChangeAnimationState(string newState, float FadingDuration, float AnimationStateSpeed)
        {
            if (DistanceCullingUpdateLoop == UpdateType.Always)
            {
                UpdateCameraDistance();
            }
            if (_currentState == newState) return;
            if(DistanceCullingUpdateLoop == UpdateType.OnChange)
            {
                UpdateCameraDistance();
            }


            AnimationSpeed =  AnimationStateSpeed * TimeScale; 

            if (IsFading)
            {
                NextFadingDuration = FadingDuration;
                NextState = newState;

                if (!NextChangeActive)
                {
                    if (IsSleeping)
                    {
                        Animator.Play(newState);
                    }
                    else
                    {
                        Invoke("NextChangeAnimationState", TimeRemainingToFadeEnd + ExtraFadeTime);
                    }

                    NextChangeActive = true;
                }
            }
            else
            {
                TimeStateEntered = Time.unscaledTime;
                FadeEnd = Time.unscaledTime + FadingDuration;
                if(IsSleeping)
                {
                    Animator.Play(newState);
                }
                else
                {
                    Animator.CrossFadeInFixedTime(newState, FadingDuration);
                }

                _currentState = newState;
                NextChangeActive = false;
            }
        }
        public void ChangeAnimationState(string newState, float FadingDuration)
        {
            if (DistanceCullingUpdateLoop == UpdateType.Always)
            {
                UpdateCameraDistance();
            }
            if (_currentState == newState) return;
            if (DistanceCullingUpdateLoop == UpdateType.OnChange)
            {
                UpdateCameraDistance();
            }

            AnimationSpeed = TimeScale;

            if (IsFading)
            {
                NextFadingDuration = FadingDuration;
                NextState = newState;

                if (!NextChangeActive)
                {
                    if (IsSleeping)
                    {
                        Animator.Play(newState);
                    }
                    else
                    {
                        Invoke("NextChangeAnimationState", TimeRemainingToFadeEnd + ExtraFadeTime);
                    }
                    NextChangeActive = true;
                }
            }
            else
            {
                TimeStateEntered = Time.unscaledTime;
                FadeEnd = Time.unscaledTime + FadingDuration;
                if (IsSleeping)
                {
                    Animator.Play(newState);
                }
                else
                {
                    Animator.CrossFadeInFixedTime(newState, FadingDuration);
                }

                _currentState = newState;
                NextChangeActive = false;
            }
        }
        public void ChangeAnimationState(string newState)
        {
            if (DistanceCullingUpdateLoop == UpdateType.Always)
            {
                UpdateCameraDistance();
            }
            if (_currentState == newState) return;
            if (DistanceCullingUpdateLoop == UpdateType.OnChange)
            {
                UpdateCameraDistance();
            }

            AnimationSpeed = TimeScale;

            if (IsFading)
            {
                NextFadingDuration = 0;
                NextState = newState;

                if (!NextChangeActive)
                {
                    if (IsSleeping)
                    {
                        Animator.Play(newState);
                    }
                    else
                    {
                        Invoke("NextChangeAnimationState", TimeRemainingToFadeEnd + ExtraFadeTime);
                    }

                    NextChangeActive = true;
                }
            }
            else
            {
                TimeStateEntered = Time.unscaledTime;
                FadeEnd = Time.unscaledTime + 0;
                if (IsSleeping)
                {
                    Animator.Play(newState);
                }
                else
                {
                    Animator.CrossFadeInFixedTime(newState, 0);
                }

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
        private void UpdateOnAnimatorEvent()
        {
            invoker.eventOnAnimatorMove.AddListener(EventOnAnimatorMove.Invoke);
            invoker.eventOnAnimatorIK.AddListener(EventOnAnimatorIK.Invoke);
        }
        public UnityEvent EventOnAnimatorMove, EventOnAnimatorIK;
    }
}
