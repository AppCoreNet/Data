// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;

namespace AppCore.Data
{
    /// <summary>
    /// Provides base class for exceptions related to entities.
    /// </summary>
    public abstract class EntityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        protected EntityException(string message, Exception? innerException)
          : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        protected EntityException(string message)
            : this(message, null)
        {
        }
    }
}