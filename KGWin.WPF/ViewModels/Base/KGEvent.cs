namespace KGWin.WPF.ViewModels.Base
{    
    public class KGEvent
    {
        public string EventName { get; }
        public object? Sender { get; }
        public EventArgs Args { get; }

        public KGEvent(string eventName, object? sender, EventArgs args)
        {
            EventName = eventName;
            Sender = sender;
            Args = args;
        }
    }
}
