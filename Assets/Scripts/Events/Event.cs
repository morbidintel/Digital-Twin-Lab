using System.Collections.Generic;
using System;

namespace GeorgeChew.UnityAssessment.EventMessaging
{
    /// <summary>
    /// An event messaging class that facilitates subscribing and publishing of events, 
    /// allowing for the transfer of data using the Object class.
    /// 
    /// <code>
    /// Event myEvent = new();
    /// Action&lt;object&gt; callback = _ => Debug.Log("Event triggered");
    /// myEvent += callback;
    /// myEvent.Publish();
    /// myEvent -= callback;
    /// </code>
    /// </summary>
    public class Event
    {
        private List<Action<object>> subscriptions = new();
        private List<Action<object>> oneTimeSubscriptions = new();

        /// <summary>
        /// Register a subscription to the event.
        /// </summary>
        /// <param name="handler">The callback to be invoked on publishing.</param>
        public void Subscribe(Action<object> handler)
        {
            subscriptions.Add(handler);
        }

        /// <summary>
        /// Register a subscription to an event but the subscription will only be invoked once.
        /// </summary>
        /// <param name="handler">The callback to be invoked only once on publishing.</param>
        public void SubscribeOnce(Action<object> handler)
        {
            oneTimeSubscriptions.Add(handler);
        }

        /// <summary>
        /// Deregister a subscription from an event.
        /// </summary>
        /// <param name="handler">The callback to be deregistered</param>
        public void Unsubscribe(Action<object> handler)
        {
            subscriptions.Remove(handler);
        }

        /// <summary>
        /// Publish an event which will invoke all the subscriptions.
        /// </summary>
        /// <param name="eventData"></param>
        public void Publish(object eventData = null)
        {
            subscriptions.ForEach(s => s.Invoke(eventData));
            oneTimeSubscriptions.ForEach(s => s?.Invoke(eventData));
            oneTimeSubscriptions.Clear();
        }

        /// <summary>
        /// Operator override, same as <see cref="Subscribe(Action{object})"/>
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Event operator +(Event event1, Action<object> handler)
        {
            event1.Subscribe(handler);
            return event1;
        }

        /// <summary>
        /// Operator override, same as <see cref="Unsubscribe(Action{object})"/>
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Event operator -(Event event1, Action<object> handler)
        {
            event1.Unsubscribe(handler);
            return event1;
        }
    }
}