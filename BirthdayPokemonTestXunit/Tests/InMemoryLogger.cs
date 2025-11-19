namespace BirthdayPokemonTestXunit.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// An in-memory, thread-safe <see cref="ILogger"/> implementation intended for unit tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this logger when you want to assert that particular messages, event ids or log levels were
    /// produced by the system under test. The implementation records entries in memory and exposes a
    /// snapshot API so test code can inspect logs without risking concurrent-modification exceptions.
    /// </para>
    /// <para>Design goals:</para>
    /// <list type="bullet">
    ///   <item><description>Simple to use from tests: instantiate <c>new InMemoryLogger&lt;T&gt;()</c>.</description></item>
    ///   <item><description>Thread-safe append and snapshot operations so test assertions are reliable.</description></item>
    ///   <item><description>Store enough context (LogLevel, EventId, Exception, Message) to make assertions precise.</description></item>
    /// </list>
    /// <para>
    /// Example usage:
    /// <code>
    /// var logger = new InMemoryLogger&lt;MyService&gt;();
    /// var svc = new MyService(logger); // accepts ILogger&lt;MyService&gt;
    /// svc.Run();
    /// var snapshot = logger.GetEntriesSnapshot();
    /// Assert.Contains(snapshot.Select(e => e.Message), m => m.Contains("expected text"));
    /// </code>
    /// </para>
    /// </remarks>
    public class InMemoryLogger : ILogger
    {
        // Lock used to synchronize writes and snapshot reads. Kept internal to avoid external locking mistakes.
        private readonly Lock _lock = new();

        /// <summary>
        /// The raw recorded log entries. For production code this would be a stream to a log sink; for tests
        /// an in-memory list is easier to assert against. Accessing this collection directly from tests is
        /// supported but <see cref="GetEntriesSnapshot"/> is the preferred safe option.
        /// </summary>
        public List<(LogLevel Level, EventId Id, Exception? Ex, string Message)> Entries = [];

        /// <summary>
        /// Begins a logical operation scope. This implementation does not track scopes; it returns null.
        /// The method exists to satisfy the <see cref="ILogger"/> contract used by many Microsoft
        /// APIs, and keeps tests simple when the system under test calls <c>BeginScope</c>.
        /// </summary>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        /// <summary>
        /// Always enabled in test scenarios so that assertions can rely on the presence of log lines.
        /// </summary>
        /// <param name="logLevel">The log level queried.</param>
        /// <returns>Always <c>true</c> for the in-memory logger.</returns>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Records a single structured log entry. This method is thread-safe and will append the resolved
        /// message produced by the provided <paramref name="formatter"/>.
        /// </summary>
        /// <typeparam name="TState">The state type provided by the caller.</typeparam>
        /// <param name="logLevel">The severity of the log entry.</param>
        /// <param name="eventId">Optional event identifier.</param>
        /// <param name="state">The structured state object passed to the logger.</param>
        /// <param name="exception">Optional exception associated with the log event.</param>
        /// <param name="formatter">A function that formats the state and exception into a message string.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
          Func<TState, Exception?, string> formatter)
        {
            // Keep the append atomic with the lock so snapshots can be taken reliably without race conditions.
            lock (_lock)
            {
                Entries.Add(new(logLevel, eventId, exception, formatter(state, exception)));
            }
        }

        /// <summary>
        /// Returns whether a log with the specified level and id was recorded. Uses a lock to avoid races
        /// with concurrent writers.
        /// </summary>
        /// <param name="logLevel">The log level to search for.</param>
        /// <param name="id">The event id to search for.</param>
        /// <returns><c>true</c> if an entry exists; otherwise <c>false</c>.</returns>
        public bool Has(LogLevel logLevel, EventId id)
        {
            lock (_lock)
            {
                return Entries.Exists(x => x.Level == logLevel && x.Id == id);
            }
        }

        /// <summary>
        /// Returns a thread-safe snapshot of the recorded entries as an array. Tests should use this
        /// snapshot for assertions to avoid collection-modified exceptions when the system under test
        /// is concurrently writing logs.
        /// </summary>
        /// <returns>A stable array copy of the recorded log entries at the time of the call.</returns>
        public IReadOnlyList<(LogLevel Level, EventId Id, Exception? Ex, string Message)> GetEntriesSnapshot()
        {
            lock (_lock)
            {
                return Entries.ToArray();
            }
        }
    }

    /// <summary>
    /// Generic convenience type so tests can inject <see cref="ILogger{TCategoryName}"/> easily.
    /// Example: <c>var logger = new InMemoryLogger&lt;MyService&gt;();</c>
    /// </summary>
    public class InMemoryLogger<T> : InMemoryLogger, ILogger<T>;
}