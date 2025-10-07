namespace KGWin.WPF.Models
{
    public class KGWebMessage<T>
    {
        public FromJsRequestType Type { get; set; }
        public T Data { get; set; }
    }
}
