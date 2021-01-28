// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.Unity
{
    /// <summary>
    /// A helper class for running Unity Coroutines without having to inherit from <see cref="MonoBehaviour"/>.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        #region Member Variables
        static private CoroutineRunner instance;
        #endregion // Member Variables

        #region Internal Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static private void Initialize()
        {
            if (instance == null)
            {
                instance = new GameObject(nameof(CoroutineRunner)).AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(instance.gameObject);
            }
        }
        #endregion // Internal Methods

        #region Public Methods
        /// <summary>
        /// Starts the coroutine stored in <paramref name="routine"/>.
        /// </summary>
        /// <param name="routine">
        /// The function to start.
        /// </param>
        new static public void StartCoroutine(IEnumerator routine)
        {
            // Validate
            if (routine == null) throw new ArgumentNullException(nameof(routine));

            // Pass to MonoBehaviour
            ((MonoBehaviour)instance).StartCoroutine(routine);
        }

        /// <summary>
        /// Stops the coroutine stored in <paramref name="routine"/>.
        /// </summary>
        /// <param name="routine">
        /// The function to stop.
        /// </param>
        new static public void StopCoroutine(IEnumerator routine)
        {
            // Validate
            if (routine == null) throw new ArgumentNullException(nameof(routine));

            // Pass to MonoBehaviour
            ((MonoBehaviour)instance).StopCoroutine(routine);
        }
        #endregion // Public Methods
    }
}