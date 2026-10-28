//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public sealed record Error(string Code, string Description, ErrorType type)
    {
        public static readonly Error None = new (String.Empty, String.Empty, ErrorType.None);

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates an Error representing a not-found condition using the specified code and description.
        /// </summary>
        /// <param name="code">Code that identifies the not-found error.</param>
        /// <param name="description">Human-readable description of the not-found error.</param>
        /// <returns>An Error instance with ErrorType.NotFound.</returns>
        public static Error NotFound(string code, string description)
        {
            return new(code, description, ErrorType.NotFound);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates an Error representing a validation failure with the specified code and description.
        /// </summary>
        /// <remarks>Factory method that constructs an Error and sets its ErrorType to
        /// Validation.</remarks>
        /// <param name="code">Validation error code.</param>
        /// <param name="description">Human-readable validation error description.</param>
        /// <returns>An Error instance with ErrorType.Validation.</returns>
        public static Error Validation(string code, string description)
        {
            return new(code, description, ErrorType.Validation);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates an Error with ErrorType.Conflict using the specified code and description.
        /// </summary>
        /// <param name="code">The machine-readable error code.</param>
        /// <param name="description">A human-readable description of the error.</param>
        /// <returns>An Error instance with ErrorType.Conflict.</returns>
        public static Error Conflict(string code, string description)
        {
            return new(code, description, ErrorType.Conflict);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//