using System;
using System.Collections;
using _PhotoCountdown.Presentation.Audio;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Flow
{
    public abstract class GameSceneEntry : MonoBehaviour
    {
        [SerializeField] private SceneAudioInstaller _audioInstaller;

        public bool IsInitialized { get; private set; }

        public void Init(GameSession session, GameFlowController flow)
        {
            if (IsInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (session == null)
                throw new ArgumentNullException(nameof(session));

            if (flow == null)
                throw new ArgumentNullException(nameof(flow));

            if (!_audioInstaller)
                throw new MissingReferenceException($"{name} has no scene audio installer.");

            _audioInstaller.Init(session.Audio);
            OnInit(session, flow);
            IsInitialized = true;
        }

        public IEnumerator Exit()
        {
            if (!IsInitialized)
                yield break;

            yield return OnExit();
        }

        protected abstract void OnInit(GameSession session, GameFlowController flow);

        protected virtual IEnumerator OnExit()
        {
            yield break;
        }
    }
}