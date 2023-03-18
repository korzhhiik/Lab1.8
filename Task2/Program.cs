using Task2;

public class EventHandlerManager
{
    private readonly Dictionary<int, PriorityQueue<CustomEventHandler>> eventHandlers = new();

    public void AddEventHandler(int priority, CustomEventHandler eventHandler)
    {
        if (!eventHandlers.ContainsKey(priority))
        {
            eventHandlers[priority] = new PriorityQueue<CustomEventHandler>();
        }
        eventHandlers[priority].Enqueue(eventHandler);
    }

    public void RemoveEventHandler(int priority, CustomEventHandler eventHandler)
    {
        if (eventHandlers.ContainsKey(priority))
        {
            eventHandlers[priority].Remove(eventHandler);
        }
    }

    public void PublishEvent(int priority, object sender, EventArgs e, RetryPolicy retryPolicy)
    {
        if (!eventHandlers.ContainsKey(priority)) return;

        var attempt = 1;
        while (attempt <= retryPolicy._maxRetries)
        {
            var delay = retryPolicy.GetNextDelay(attempt);
            Thread.Sleep(delay);

            var currentQueue = eventHandlers[priority];

            var customEventHandler = currentQueue.Dequeue();
            var handlers = new List<CustomEventHandler> { customEventHandler };
            foreach (var handler in handlers)
            {
                try
                {
                    handler.EventHandler(sender, e);
                }
                catch (Exception ex)
                {
                    if (attempt == retryPolicy._maxRetries)
                    {
                        throw;
                    }
                    Console.WriteLine($"Exception caught: {ex.Message}. Retrying in {delay} milliseconds.");
                }
            }

            attempt++;
        }
    }

    public class RetryPolicy
    {
        public readonly int _maxRetries;
        public readonly TimeSpan _initialDelay;
        public readonly TimeSpan _maxDelay;

        public RetryPolicy(int maxRetries, TimeSpan initialDelay, TimeSpan maxDelay)
        {
            _maxRetries = maxRetries;
            _initialDelay = initialDelay;
            _maxDelay = maxDelay;
        }

        public int GetNextDelay(int currentAttempt)
        {
            var delay = (int)(_initialDelay.TotalMilliseconds * Math.Pow(2, currentAttempt - 1));
            delay = Math.Min(delay, (int)_maxDelay.TotalMilliseconds);
            return delay + new Random().Next(0, delay / 2);
        }
    }
}

public class PriorityQueue<T> where T : IComparable<T>
{
    private readonly List<T> _list = new();

    public void Enqueue(T item)
    {
        _list.Add(item);
        var childIndex = _list.Count - 1;
        while (childIndex > 0)
        {
            var parentIndex = (childIndex - 1) / 2;
            if (_list[childIndex].CompareTo(_list[parentIndex]) >= 0)
            {
                break;
            }

            (_list[childIndex], _list[parentIndex]) = (_list[parentIndex], _list[childIndex]);
            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        var lastIndex = _list.Count - 1;
        var frontItem = _list[0];
        _list[0] = _list[lastIndex];
        _list.RemoveAt(lastIndex);

        lastIndex--;
        var parentIndex = 0;
        while (true)
        {
            var childIndex = parentIndex * 2 + 1;
            if (childIndex > lastIndex)
            {
                break;
            }

            var rightChild = childIndex + 1;
            if (rightChild <= lastIndex && _list[rightChild].CompareTo(_list[childIndex]) < 0)
            {
                childIndex = rightChild;
            }

            if (_list[parentIndex].CompareTo(_list[childIndex]) <= 0)
            {
                break;
            }

            (_list[parentIndex], _list[childIndex]) = (_list[childIndex], _list[parentIndex]);
            parentIndex = childIndex;
        }

        return frontItem;
    }

    public int Count => _list.Count;

    public void Remove(T item)
    {
        var index = _list.IndexOf(item);
        if (index < 0) return;
        _list[index] = _list[^1];
        _list.RemoveAt(_list.Count - 1);

        var parentIndex = (index - 1) / 2;
        if (_list[index].CompareTo(_list[parentIndex]) < 0)
        {
            while (index > 0)
            {
                (_list[index], _list[parentIndex]) = (_list[parentIndex], _list[index]);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }
        else
        {
            while (true)
            {
                var childIndex = index * 2 + 1;
                if (childIndex >= _list.Count)
                {
                    break;
                }
                var rightChild = childIndex + 1;
                if (rightChild < _list.Count && _list[rightChild].CompareTo(_list[childIndex]) < 0)
                {
                    childIndex = rightChild;
                }
                if (_list[index].CompareTo(_list[childIndex]) <= 0)
                {
                    break;
                }
                (_list[index], _list[childIndex]) = (_list[childIndex], _list[index]);
                index = childIndex;
            }
        }
    }
}

