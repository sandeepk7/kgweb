namespace KGWin.WPF.ViewModels.Base
{
    public class EventAggregator
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<TEvent>(Action<TEvent> action)
        {
            if (!_subscribers.TryGetValue(typeof(TEvent), out var list))
            {
                list = new List<Delegate>();
                _subscribers[typeof(TEvent)] = list;
            }
            list.Add(action);
        }

        public void Publish<TEvent>(TEvent eventData)
        {
            if (_subscribers.TryGetValue(typeof(TEvent), out var list))
            {
                foreach (var d in list)
                    ((Action<TEvent>)d)?.Invoke(eventData);
            }
        }
    }
}
