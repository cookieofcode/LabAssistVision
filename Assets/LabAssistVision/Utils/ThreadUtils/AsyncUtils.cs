// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.Unity
{
    /// <summary>
    /// Wraps a coroutine in an <see cref="IEnumerator"/> that handles exceptions in a safe way.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of data returned by the coroutine.
    /// </typeparam>
    public class ExceptionSafeRoutine<TResult> : IEnumerator
    {
        #region Member Variables
        private IEnumerator coroutine;
        private TaskCompletionSource<TResult> exceptionSource;
        #endregion // Member Variables

        /// <summary>
        /// Initializes a new <see cref="ExceptionSafeRoutine"/>.
        /// </summary>
        /// <param name="coroutine">
        /// The coroutine to wrap.
        /// </param>
        /// <param name="exceptionSource">
        /// The <see cref="TaskCompletionSource{TResult}"/> that will handle exceptions.
        /// </param>
        public ExceptionSafeRoutine(IEnumerator coroutine, TaskCompletionSource<TResult> exceptionSource)
        {
            // Validate
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));
            if (exceptionSource == null) throw new ArgumentNullException(nameof(exceptionSource));

            // Store
            this.coroutine = coroutine;
            this.exceptionSource = exceptionSource;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public object Current
        {
            get
            {
                try
                {
                    return coroutine.Current;
                }
                catch (Exception ex)
                {
                    exceptionSource.TrySetException(ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool MoveNext()
        {
            try
            {
                return coroutine.MoveNext();
            }
            catch (Exception ex)
            {
                exceptionSource.TrySetException(ex);
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Reset()
        {
            try
            {
                coroutine.Reset();
            }
            catch (Exception ex)
            {
                exceptionSource.TrySetException(ex);
            }
        }
    }


    /// <summary>
    /// Utilities and extension methods for helping with async operations.
    /// </summary>
    static public class AsyncUtils
    {
        #region Constants
        /// <summary>
        /// The delegate for a handler of exceptions raised by a behavior.
        /// </summary>
        /// <param name="behaviour">
        /// The behavior that raised the exception.
        /// </param>
        /// <param name="ex">
        /// The exception.
        /// </param>
        public delegate void BehaviourErrorHandler(Behaviour behaviour, Exception ex);
        #endregion // Constants

        #region Internal Methods
        /// <summary>
        /// Logs the exception and disables the behavior.
        /// </summary>
        /// <param name="behaviour">
        /// The behavior to disable.
        /// </param>
        /// <param name="ex">
        /// The exception to log.
        /// </param>
        static private void LogError(Behaviour behaviour, Exception ex, bool disable)
        {
            // Get names
            var behaviourName = behaviour.GetType().Name;
            var objName = behaviour.gameObject.name;

            // Log information
            if (disable)
            {
                Debug.LogError($"{ex.Message} -- Async void error. {behaviourName} on {objName} has been disabled.");
                behaviour.enabled = false;
            }
            else
            {
                Debug.LogError($"{ex.Message} -- Async void error ({behaviourName} on {objName}).");
            }
        }
        #endregion // Internal Methods


        #region Public Methods
        /// <summary>
        /// Runs a task as async void but passes any exception to the specified handler.
        /// </summary>
        /// <param name="behaviour">
        /// The <see cref="Behaviour"/> which is requesting the action to be run async void.
        /// </param>
        /// <param name="task">
        /// The function which yields the <see cref="Task"/> to perform.
        /// </param>
        /// <param name="onError">
        /// A delegate of type <see cref="BehaviourErrorHandler"/> which will handle the error.
        /// </param>
        static public async void RunSafeVoid(this Behaviour behaviour, Func<Task> task, BehaviourErrorHandler onError)
        {
            // Run the task and await for any exception
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                onError(behaviour, ex);
            }
        }

        /// <summary>
        /// Runs a task as async void and logs any error that occurs.
        /// </summary>
        /// <param name="behaviour">
        /// The <see cref="Behaviour"/> which is requesting the action to be run async void.
        /// </param>
        /// <param name="task">
        /// The function which yields the <see cref="Task"/> to perform.
        /// </param>
        /// <param name="disableOnError">
        /// If <c>true</c>, the behavior will be disabled if any error occurs.
        /// </param>
        static public void RunSafeVoid(this Behaviour behaviour, Func<Task> task, bool disableOnError = true)
        {
            RunSafeVoid(behaviour, task, (b, ex) => LogError(b, ex, disableOnError));
        }

        /// <summary>
        /// Wraps a coroutine with exception handling that will ensure the completion source receives the exception.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of result.
        /// </typeparam>
        /// <param name="coroutine">
        /// The coroutine to wrap.
        /// </param>
        /// <param name="exceptionSource">
        /// The <see cref="TaskCompletionSource{TResult}"/> that will handle exceptions.
        /// </param>
        /// <returns></returns>
        static public IEnumerator WithExceptionHandling<TResult>(this IEnumerator coroutine, TaskCompletionSource<TResult> exceptionSource)
        {
            return new ExceptionSafeRoutine<TResult>(coroutine, exceptionSource);
        }
        #endregion // Public Methods
    }
}
