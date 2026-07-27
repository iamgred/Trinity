//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
namespace TeamServer.Exceptions
{
    public class ListenerCreationException : Exception
    {
    public ListenerCreationException() : base() { }
    public ListenerCreationException(string message) : base(message) { }
    public ListenerCreationException(string message, Exception inner) : base(message, inner) { }
    }
}
