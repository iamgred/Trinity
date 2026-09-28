//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using Trinity.Shared.Errors;

namespace Trinity.Shared.Results
{
    public class Result<T> where T : class
    {
        public bool IsSuccess { get; init; }
        public Error Error { get; init; }
        public T? Response { get; init; }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//\
        /// <summary>
        /// Initializes a new instance of Result<T> with the specified success state, error, and response.
        /// </summary>
        /// <param name="isSuccess">True to indicate a successful result; otherwise false.</param>
        /// <param name="error">The error code; must be Error.None when isSuccess is true, and must not be Error.None when isSuccess is
        /// false.</param>
        /// <param name="response">The response value for a successful result, or default(T) for a failed result.</param>
        /// <exception cref="ArgumentException">Thrown when isSuccess and error are inconsistent (isSuccess is true but error != Error.None, or isSuccess is
        /// false but error == Error.None).</exception>
        private Result(bool isSuccess, Error error, T response)
        {
            if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            {
                throw new ArgumentException("Invalid result", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
            Response = response;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a Result with the specified success state and associated error.
        /// </summary>
        /// <remarks>Enforces the invariant that a successful result has Error.None and a failed result
        /// has a non-None error.</remarks>
        /// <param name="isSuccess">True to indicate success; false to indicate failure.</param>
        /// <param name="error">Error code associated with the result; must be Error.None when isSuccess is true.</param>
        /// <exception cref="ArgumentException">Thrown if isSuccess and error are inconsistent (for example, success with a non-None error or failure with
        /// Error.None).</exception>
        private Result(bool isSuccess, Error error)
        {
            if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            {
                throw new ArgumentException("Invalid result", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static implicit operator Result<T>(T value) => new(true, Error.None, value);
        public static implicit operator Result<T>(Error error) => new(false, error);
    }

    public class Result
    {
        public bool IsSuccess { get; init; }
        public Error Error { get; init; }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a Result with the specified success state and associated error.
        /// </summary>
        /// <remarks>Enforces the invariant that a successful result has Error.None and a failed result
        /// has a non-None error.</remarks>
        /// <param name="isSuccess">True to indicate success; false to indicate failure.</param>
        /// <param name="error">Error code associated with the result; must be Error.None when isSuccess is true.</param>
        /// <exception cref="ArgumentException">Thrown if isSuccess and error are inconsistent (for example, success with a non-None error or failure with
        /// Error.None).</exception>
        private Result(bool isSuccess, Error error)
        {
            if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            {
                throw new ArgumentException("Invalid result", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static implicit operator Result(Error error) => new(false, error);

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//