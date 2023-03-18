namespace EventBusWithThrottling
{
    public delegate void EventHandler<in TEventArgs>(object sender, TEventArgs e);

    public class EventBus<TEventArgs>
    {
        private readonly object _eventLock = new();

        private readonly Dictionary<string, List<EventHandler<TEventArgs>>> _eventHandlers = new();

        private readonly Dictionary<string, DateTime> _lastEventTime = new();
        private readonly int _throttleTime;

        public EventBus(int throttleTime)
        {
            _throttleTime = throttleTime;
        }

        public void RegisterEvent(string eventName)
        {
            lock (_eventLock)
            {
                if (_eventHandlers.ContainsKey(eventName)) return;
                _eventHandlers.Add(eventName, new List<EventHandler<TEventArgs>>());
                _lastEventTime.Add(eventName, DateTime.MinValue);
            }
        }

        public void UnregisterEvent(string eventName)
        {
            lock (_eventLock)
            {
                if (!_eventHandlers.ContainsKey(eventName)) return;
                _eventHandlers.Remove(eventName);
                _lastEventTime.Remove(eventName);
            }
        }

        public void AddEventHandler(string eventName, EventHandler<TEventArgs> eventHandler)
        {
            lock (_eventLock)
            {
                if (_eventHandlers.ContainsKey(eventName))
                {
                    _eventHandlers[eventName].Add(eventHandler);
                }
            }
        }

        public void RemoveEventHandler(string eventName, EventHandler<TEventArgs> eventHandler)
        {
            lock (_eventLock)
            {
                if (_eventHandlers.ContainsKey(eventName))
                {
                    _eventHandlers[eventName].Remove(eventHandler);
                }
            }
        }
        
        public void DispatchEvent(string eventName, TEventArgs eventArgs)
        {
            lock (_eventLock)
            {
                if (!_eventHandlers.ContainsKey(eventName)) return;
                var lastEventTime = _lastEventTime[eventName];
                var timeSinceLastEvent = DateTime.Now - lastEventTime;

                if (!(timeSinceLastEvent.TotalMilliseconds >= _throttleTime)) return;
                foreach (var handler in _eventHandlers[eventName])
                {
                    handler.Invoke(this, eventArgs);
                }

                _lastEventTime[eventName] = DateTime.Now;
            }
        }
    }
}