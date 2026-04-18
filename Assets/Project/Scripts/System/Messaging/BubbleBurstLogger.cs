using System;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace Assets.Project.Scripts.System.Messaging
{
    public class BubbleBurstLogger : IStartable, IDisposable
    {
        private readonly ISubscriber<BubbleBurstResolvedMessage> _bubbleBurstSubscriber;
        private IDisposable _subscription;

        public BubbleBurstLogger(ISubscriber<BubbleBurstResolvedMessage> bubbleBurstSubscriber)
        {
            _bubbleBurstSubscriber = bubbleBurstSubscriber;
        }

        public void Start()
        {
            _subscription = _bubbleBurstSubscriber.Subscribe(HandleBubbleBurst);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        private void HandleBubbleBurst(BubbleBurstResolvedMessage message)
        {
            Debug.Log($"Bubble burst: {message.BubbleType}, count: {message.BurstCount}");
        }
    }
}
