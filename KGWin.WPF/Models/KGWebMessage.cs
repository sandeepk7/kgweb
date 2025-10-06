namespace KGWin.WPF.Models
{
    public class KGWebMessage<T>
    {
        public RequestType RequestType { get; set; }
        public T Data { get; set; }
    }
}
