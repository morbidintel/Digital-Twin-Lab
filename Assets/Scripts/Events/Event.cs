using System;
using System.Linq;
using System.Collections.Generic;

namespace GeorgeChew.HiverlabAssessment.EventMessaging
{
    public class Event
    {
        private List<Action<object>> subscriptions = new();
        private List<Action<object>> oneTimeSubscriptions = new();

        public void Subscribe(Action<object> handler)
        {
            subscriptions.Add(handler);
        }

        public void SubscribeOnce(Action<object> handler)
        {
            oneTimeSubscriptions.Add(handler);
        }

        public void Unsubscribe(Action<object> handler)
        {
            subscriptions.Remove(handler);
        }

        public void Publish(object eventData = null)
        {
            foreach (var sub in subscriptions.AsEnumerable().ToList())
            {
                sub?.Invoke(eventData);
            }
            oneTimeSubscriptions.ForEach(s => s?.Invoke(eventData));
            oneTimeSubscriptions.Clear();
        }

        public static Event operator +(Event event1, Action<object> handler)
        {
            event1.Subscribe(handler);
            return event1;
        }

        public static Event operator -(Event event1, Action<object> handler)
        {
            event1.Unsubscribe(handler);
            return event1;
        }
    }
}