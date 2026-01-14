using System;

namespace BetterExceptions.Interfaces
{
    public interface IExceptionHandler
    {
        void HandleException(Exception exception);
    }
}
