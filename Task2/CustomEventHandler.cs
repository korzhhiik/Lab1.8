namespace Task2;

public class CustomEventHandler : IComparable<CustomEventHandler>
{
    private readonly int priority;
    public EventHandler EventHandler { get; }

    public CustomEventHandler(EventHandler eventHandler, int priority)
    {
        EventHandler = eventHandler;
        this.priority = priority;
    }

    public int CompareTo(CustomEventHandler other)
    {
        if (priority < other.priority)
        {
            return -1;
        }

        return priority > other.priority ? 1 : 0;
    }
}