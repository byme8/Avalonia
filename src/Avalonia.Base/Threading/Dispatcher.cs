using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;

namespace Avalonia.Threading
{
    /// <summary>
    /// Provides services for managing work items on a thread.
    /// </summary>
    /// <remarks>
    /// In Avalonia, there is usually only a single <see cref="Dispatcher"/> in the application -
    /// the one for the UI thread, retrieved via the <see cref="UIThread"/> property.
    /// </remarks>
    public class Dispatcher : IDispatcher
    {
        private IPlatformThreadingInterface? _platform;

        public static Dispatcher UIThread { get; } =
            new Dispatcher(AvaloniaLocator.Current.GetService<IPlatformThreadingInterface>());

        public Dispatcher(IPlatformThreadingInterface? platform)
        {
            _platform = platform;
            JobRunner = new JobRunner(platform);

            if (_platform != null)
            {
                _platform.Signaled += JobRunner.RunJobs;
            }
        }

        public JobRunner JobRunner { get; }

        /// <summary>
        /// Checks that the current thread is the UI thread.
        /// </summary>
        public bool CheckAccess() => _platform?.CurrentThreadIsLoopThread ?? true;

        /// <summary>
        /// Checks that the current thread is the UI thread and throws if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current thread is not the UI thread.
        /// </exception>
        public void VerifyAccess()
        {
            if (!CheckAccess())
                throw new InvalidOperationException("Call from invalid thread");
        }

        /// <summary>
        /// Runs the dispatcher's main loop.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token used to exit the main loop.
        /// </param>
        public void MainLoop(CancellationToken cancellationToken)
        {
            var platform = AvaloniaLocator.Current.GetRequiredService<IPlatformThreadingInterface>();
            cancellationToken.Register(() => platform.Signal(DispatcherPriority.Send));
            platform.RunLoop(cancellationToken);
        }

        /// <summary>
        /// Runs continuations pushed on the loop.
        /// </summary>
        public void RunJobs()
        {
            JobRunner.RunJobs(null);
        }

        /// <summary>
        /// Use this method to ensure that more prioritized tasks are executed
        /// </summary>
        /// <param name="minimumPriority"></param>
        public void RunJobs(DispatcherPriority minimumPriority) => JobRunner.RunJobs(minimumPriority);

        /// <summary>
        /// Use this method to check if there are more prioritized tasks
        /// </summary>
        /// <param name="minimumPriority"></param>
        public bool HasJobsWithPriority(DispatcherPriority minimumPriority) =>
            JobRunner.HasJobsWithPriority(minimumPriority);

        /// <inheritdoc/>
        public Task InvokeAsync(Action action, DispatcherPriority priority = default)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            return JobRunner.InvokeAsync(action, priority);
        }

        /// <inheritdoc/>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority = default)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return JobRunner.InvokeAsync(function, priority);
        }

        /// <inheritdoc/>
        public Task InvokeAsync(Func<Task> function, DispatcherPriority priority = default)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return JobRunner.InvokeAsync(function, priority).Unwrap();
        }

        /// <inheritdoc/>
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, DispatcherPriority priority = default)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            return JobRunner.InvokeAsync(function, priority).Unwrap();
        }

        /// <inheritdoc/>
        public void Post(Action action, DispatcherPriority priority = default)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            JobRunner.Post(action, priority);
        }

        /// <inheritdoc/>
        public void Post(SendOrPostCallback action, object? arg, DispatcherPriority priority = default)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            JobRunner.Post(action, arg, priority);
        }

        /// <summary>
        /// This is needed for platform backends that don't have internal priority system (e. g. win32)
        /// To ensure that there are no jobs with higher priority
        /// </summary>
        /// <param name="currentPriority"></param>
        internal void EnsurePriority(DispatcherPriority currentPriority)
        {
            if (currentPriority == DispatcherPriority.MaxValue)
                return;
            currentPriority += 1;
            JobRunner.RunJobs(currentPriority);
        }

        /// <summary>
        /// Allows unit tests to change the platform threading interface.
        /// </summary>
        internal void UpdateServices()
        {
            if (_platform != null)
            {
                _platform.Signaled -= JobRunner.RunJobs;
            }

            _platform = AvaloniaLocator.Current.GetService<IPlatformThreadingInterface>();
            JobRunner.UpdateServices();

            if (_platform != null)
            {
                _platform.Signaled += JobRunner.RunJobs;
            }
        }
    }
}
