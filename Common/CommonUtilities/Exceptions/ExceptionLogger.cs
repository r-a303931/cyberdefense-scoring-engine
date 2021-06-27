#nullable enable

using System;
using Microsoft.Extensions.Logging;

namespace Common.Exceptions
{
    public class ExceptionLogger : IExceptionHandler
    {
        public ILogger? Logger { get; init; }

        public void HandleException(Exception e)
        {
            Logger?.LogError(
                $"{e.Source ?? "<unknown>"}: {e.Message}\n" +
                $"{e.StackTrace}\n"
            );
        }
    }
}
