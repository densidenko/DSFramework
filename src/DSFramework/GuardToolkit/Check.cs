﻿using DSFramework.Collections;
using DSFramework.Extensions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DSFramework.GuardToolkit
{
    [DebuggerStepThrough]
    public static class Check
    {
        private const string AGAINST_MESSAGE = "Assertion evaluation failed with 'false'.";
        private const string IMPLEMENTS_MESSAGE = "Type '{0}' must implement type '{1}'.";
        private const string INHERITS_FROM_MESSAGE = "Type '{0}' must inherit from type '{1}'.";
        private const string IS_TYPE_OF_MESSAGE = "Type '{0}' must be of type '{1}'.";
        private const string IS_EQUAL_MESSAGE = "Compared objects must be equal.";
        private const string IS_POSITIVE_MESSAGE = "Argument '{0}' must be a positive value. Value: '{1}'.";
        private const string IS_TRUE_MESSAGE = "True expected for '{0}' but the condition was False.";
        private const string NOT_NEGATIVE_MESSAGE = "Argument '{0}' cannot be a negative value. Value: '{1}'.";

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>(T value, [InvokerParameterName] [NotNull] string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T? NotNull<T>(T? value, [InvokerParameterName] [NotNull] string parameterName)
            where T : struct
        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotNullOrEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentException($"{parameterName} can not be null or empty!", parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotNullOrWhiteSpace(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"{parameterName} can not be null, empty or white space!", parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentException(parameterName + " can not be null or empty!", parameterName);
            }

            return value;
        }
        
        public static void ArgumentNotEmpty(Guid arg, string argName)
        {
            if (arg == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Argument '{0}' cannot be an empty guid.", argName), argName);
            }
        }

        public static void ArgumentInRange<T>(T arg, T min, T max, string argName)
            where T : struct, IComparable<T>
        {
            if (arg.CompareTo(min) < 0 || arg.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(argName,
                                                      "The argument '{0}' must be between '{1}' and '{2}'.".FormatCurrent(argName, min, max));
            }
        }

        public static void ArgumentNotOutOfLength(string arg, int maxLength, string argName)
        {
            if (arg.Trim().Length > maxLength)
            {
                throw new ArgumentException(argName, "Argument '{0}' cannot be more than {1} characters long.".FormatCurrent(argName, maxLength));
            }
        }

        public static void ArgumentNotNegative<T>(T arg, string argName, string message = NOT_NEGATIVE_MESSAGE)
            where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default) < 0)
            {
                throw new ArgumentOutOfRangeException(argName, message.FormatInvariant(argName, arg));
            }
        }

        public static void ArgumentNotZero<T>(T arg, string argName)
            where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default) == 0)
            {
                throw new ArgumentOutOfRangeException(argName,
                                                      string.Format(CultureInfo.CurrentCulture,
                                                                    "Argument '{0}' must be greater or less than zero. Value: '{1}'.",
                                                                    argName,
                                                                    arg));
            }
        }

        public static void InheritsFrom<TBase>(Type type)
        {
            InheritsFrom<TBase>(type, INHERITS_FROM_MESSAGE.FormatInvariant(type.FullName, typeof(TBase).FullName));
        }

        public static void InheritsFrom<TBase>(Type type, string message)
        {
            if (type.BaseType != typeof(TBase))
            {
                throw new InvalidOperationException(message);
            }
        }
        
        public static void Implements<TInterface>(Type type, string message = IMPLEMENTS_MESSAGE)
        {
            if (!typeof(TInterface).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(message.FormatInvariant(type.FullName, typeof(TInterface).FullName));
            }
        }

        public static void IsTypeOf<TType>(object instance)
        {
            IsTypeOf<TType>(instance, IS_TYPE_OF_MESSAGE.FormatInvariant(instance.GetType().Name, typeof(TType).FullName));
        }

        public static void IsTypeOf<TType>(object instance, string message)
        {
            if (!(instance is TType))
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void IsEqual<TException>(object compare, object instance, string message = IS_EQUAL_MESSAGE)
            where TException : Exception
        {
            if (!compare.Equals(instance))
            {
                throw (TException)Activator.CreateInstance(typeof(TException), message);
            }
        }

        public static void ArgumentIsPositive<T>(T arg, string argName, string message = IS_POSITIVE_MESSAGE)
            where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default) < 1)
            {
                throw new ArgumentOutOfRangeException(argName, message.FormatInvariant(argName));
            }
        }

        public static void ArgumentIsTrue(bool arg, string argName, string message = IS_TRUE_MESSAGE)
        {
            if (!arg)
            {
                throw new ArgumentException(message.FormatInvariant(argName), argName);
            }
        }

        public static void ArgumentIsEnumType(Type type, string argName)
        {
            NotNull(type, argName);
            if (!type.IsEnum)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Type '{0}' must be a valid Enum type.", type.FullName),
                                            argName);
            }
        }

        public static void ArgumentIsEnumType<TEnum>(object arg, string argName)
            where TEnum : struct
        {
            NotNull(arg, argName);
            if (!Enum.IsDefined(typeof(TEnum), arg))
            {
                throw new ArgumentOutOfRangeException(argName,
                                                      string.Format(CultureInfo.CurrentCulture,
                                                                    "The value of the argument '{0}' provided for the enumeration '{1}' is invalid.",
                                                                    argName,
                                                                    typeof(TEnum).FullName));
            }
        }

        public static void PagingArgsValid(int indexArg, long sizeArg, string indexArgName, string sizeArgName)
        {
            ArgumentNotNegative(indexArg, indexArgName, "PageIndex cannot be below 0");
            if (indexArg > 0)
            {
                ArgumentIsPositive(sizeArg, sizeArgName, "PageSize cannot be below 1 if a PageIndex greater 0 was provided.");
            }
            else
            {
                ArgumentNotNegative(sizeArg, sizeArgName);
            }
        }

        public static bool HasConsecutiveChars(string inputText, int sequenceLength = 3)
        {
            var charEnumerator = StringInfo.GetTextElementEnumerator(inputText);
            var currentElement = string.Empty;
            var count = 1;
            while (charEnumerator.MoveNext())
            {
                if (currentElement == charEnumerator.GetTextElement())
                {
                    if (++count >= sequenceLength)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 1;
                    currentElement = charEnumerator.GetTextElement();
                }
            }

            return false;
        }
    }
}