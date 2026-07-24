using System;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Flow
{
    public abstract class GameSceneEntry : MonoBehaviour
    {
        public bool IsInitialized { get; private set; }

        public void Init(GameSession session, GameFlowController flow)
        {
            if (IsInitialized)
                throw new InvalidOperationException($"{name} is already initialized.");

            if (session == null)
                throw new ArgumentNullException(nameof(session));

            if (flow == null)
                throw new ArgumentNullException(nameof(flow));

            OnInit(session, flow);
            IsInitialized = true;
        }

        protected abstract void OnInit(GameSession session, GameFlowController flow);
    }
}