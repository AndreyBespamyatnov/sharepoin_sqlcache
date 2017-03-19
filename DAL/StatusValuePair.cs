namespace Navicon.SP.Components.SqlCache.DAL
{
    using System;

    using Navicon.SP.Common.Extensions;

    public sealed class StatusValuePair<T>
    {
        private readonly T _instance;

        /// <summary> Используется в случаях, когда возвращаемое значение является значением по умолчанию для типа. Иначе свойство HasValue вернет false. 
        /// </summary>
        private readonly bool _hasValue;

        public StatusValuePair(T instance, ErrorCode errorCode, bool hasValue = false)
        {
            this._instance = instance;
            this.ErrorCode = errorCode;
            this._hasValue = hasValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public T Value
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new NullReferenceException(string.Format("Value has not been set. Error code is: '{0}'", this.ErrorCode));
                }

                return this._instance;
            }
        }

        public bool HasValue
        {
            get
            {
                return this._hasValue || !this._instance.IsDefault();
            }
        }

        public ErrorCode ErrorCode { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static implicit operator StatusValuePair<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentException("Cannot instantiate StatusValuePair<T> with null.", "instance");
            }

            return new StatusValuePair<T>(instance, ErrorCode.NoError);
        }

        /// <summary>
        ///     TODO: consider usage of operators, especially this one
        /// </summary>
        public static implicit operator T(StatusValuePair<T> instance)
        {
            return instance.Value;
        }
    }
}