using System;

public class UnnamedException : Exception
{
    public UnnamedException() { }

    public UnnamedException(string message)
        : base(message) { }

    public UnnamedException(string message, Exception inner)
        : base(message, inner) { }
}