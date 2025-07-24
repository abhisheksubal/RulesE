using NCalc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RuleEngine.Extensions
{
    internal static class NCalcCustomFunctions
    {
        public static void Evaluate(string name, FunctionArgs args)
        {
            switch (name.ToLowerInvariant())
            {
                case "capitalize":
                    Capitalize(args);
                    break;
                case "contains":
                    Contains(args);
                    break;
                case "endswith":
                    EndsWith(args);
                    break;
                case "indexof":
                    IndexOf(args);
                    break;
                case "join":
                    Join(args);
                    break;
                case "lastindexof":
                    LastIndexOf(args);
                    break;
                case "length":
                    Length(args);
                    break;
                case "padleft":
                    PadLeft(args);
                    break;
                case "replace":
                    Replace(args);
                    break;
                case "split":
                    Split(args);
                    break;
                case "startswith":
                    StartsWith(args);
                    break;
                case "substring":
                    Substring(args);
                    break;
                case "tolower":
                    ToLower(args);
                    break;
                case "toupper":
                    ToUpper(args);
                    break;
                case "trim":
                    Trim(args);
                    break;
                case "typeof":
                    TypeOf(args);
                    break;
                case "try":
                    Try(args);
                    break;
            }
        }

        #region Copied and Adapted String Functions

        private static void Capitalize(FunctionArgs args)
        {
            if (args.Parameters.Length != 1)
            {
                throw new ArgumentException("capitalize() requires one string parameter.");
            }
            string param1 = (string)args.Parameters[0].Evaluate();
            if (string.IsNullOrEmpty(param1))
            {
                args.Result = param1;
                return;
            }
            args.Result = char.ToUpperInvariant(param1[0]) + param1.Substring(1).ToLowerInvariant();
        }

        private static void Contains(FunctionArgs args)
        {
            if (args.Parameters.Length != 2)
            {
                throw new ArgumentException("contains() requires two string parameters.");
            }
            string haystack = (string)args.Parameters[0].Evaluate();
            string needle = (string)args.Parameters[1].Evaluate();
            args.Result = haystack.Contains(needle);
        }

        private static void EndsWith(FunctionArgs args)
        {
            if (args.Parameters.Length != 2)
            {
                throw new ArgumentException("endsWith() requires two string parameters.");
            }
            string longString = (string)args.Parameters[0].Evaluate();
            string shortString = (string)args.Parameters[1].Evaluate();
            args.Result = longString.EndsWith(shortString, StringComparison.InvariantCulture);
        }

        private static void IndexOf(FunctionArgs args)
        {
            if (args.Parameters.Length != 2)
            {
                throw new ArgumentException("indexOf() requires two string parameters.");
            }
            string longString = (string)args.Parameters[0].Evaluate();
            string shortString = (string)args.Parameters[1].Evaluate();
            args.Result = longString.IndexOf(shortString, StringComparison.InvariantCulture);
        }

        private static void Join(FunctionArgs args)
        {
             if (args.Parameters.Length != 2)
             {
                 throw new ArgumentException("join() requires a list and a separator string.");
             }
             var list = (System.Collections.IList)args.Parameters[0].Evaluate();
             string separator = (string)args.Parameters[1].Evaluate();
             args.Result = string.Join(separator, list.Cast<object>().Select(o => o?.ToString() ?? ""));
        }

        private static void LastIndexOf(FunctionArgs args)
        {
            if (args.Parameters.Length != 2)
            {
                throw new ArgumentException("lastIndexOf() requires two string parameters.");
            }
            string longString = (string)args.Parameters[0].Evaluate();
            string shortString = (string)args.Parameters[1].Evaluate();
            args.Result = longString.LastIndexOf(shortString, StringComparison.InvariantCulture);
        }

        private static void Length(FunctionArgs args)
        {
            if (args.Parameters.Length != 1)
            {
                throw new ArgumentException("length() requires one parameter.");
            }
            object value = args.Parameters[0].Evaluate();
            if (value is string s)
            {
                args.Result = s.Length;
            }
            else if (value is System.Collections.IList list)
            {
                args.Result = list.Count;
            }
            else
            {
                throw new ArgumentException("length() parameter must be a string or a list.");
            }
        }

        private static void PadLeft(FunctionArgs args)
        {
            if (args.Parameters.Length != 3)
            {
                throw new ArgumentException("padLeft() requires three parameters: string, total width, padding char.");
            }
            string stringToPad = (string)args.Parameters[0].Evaluate();
            int totalWidth = Convert.ToInt32(args.Parameters[1].Evaluate());
            char paddingChar = Convert.ToChar(args.Parameters[2].Evaluate());
            args.Result = stringToPad.PadLeft(totalWidth, paddingChar);
        }

        private static void Replace(FunctionArgs args)
        {
            if (args.Parameters.Length != 3)
            {
                throw new ArgumentException("replace() requires three string parameters.");
            }
            string original = (string)args.Parameters[0].Evaluate();
            string oldValue = (string)args.Parameters[1].Evaluate();
            string newValue = (string)args.Parameters[2].Evaluate();
            args.Result = original.Replace(oldValue, newValue);
        }

        private static void Split(FunctionArgs args)
        {
             if (args.Parameters.Length != 2)
             {
                 throw new ArgumentException("split() requires a string and a separator string.");
             }
             string input = (string)args.Parameters[0].Evaluate();
             string separator = (string)args.Parameters[1].Evaluate();
             var parts = input.Split(new[] { separator }, StringSplitOptions.None);
             args.Result = parts.Cast<object>().ToList();
        }

        private static void StartsWith(FunctionArgs args)
        {
            if (args.Parameters.Length != 2)
            {
                throw new ArgumentException("startsWith() requires two string parameters.");
            }
            string longString = (string)args.Parameters[0].Evaluate();
            string shortString = (string)args.Parameters[1].Evaluate();
            args.Result = longString.StartsWith(shortString, StringComparison.InvariantCulture);
        }

        private static void Substring(FunctionArgs args)
        {
            if (args.Parameters.Length < 2 || args.Parameters.Length > 3)
            {
                throw new ArgumentException("substring() requires two or three parameters.");
            }
            string input = (string)args.Parameters[0].Evaluate();
            int startIndex = Convert.ToInt32(args.Parameters[1].Evaluate());
            if (args.Parameters.Length == 3)
            {
                int length = Convert.ToInt32(args.Parameters[2].Evaluate());
                args.Result = input.Substring(startIndex, length);
            }
            else
            {
                args.Result = input.Substring(startIndex);
            }
        }

        private static void ToLower(FunctionArgs args)
        {
            if (args.Parameters.Length != 1)
            {
                throw new ArgumentException("toLower() requires one string parameter.");
            }
            string param1 = (string)args.Parameters[0].Evaluate();
            args.Result = param1.ToLowerInvariant();
        }

        private static void ToUpper(FunctionArgs args)
        {
            if (args.Parameters.Length != 1)
            {
                throw new ArgumentException("toUpper() requires one string parameter.");
            }
            string param1 = (string)args.Parameters[0].Evaluate();
            args.Result = param1.ToUpperInvariant();
        }

        private static void Trim(FunctionArgs args)
        {
            if (args.Parameters.Length != 1)
            {
                throw new ArgumentException("trim() requires one string parameter.");
            }
            string param1 = (string)args.Parameters[0].Evaluate();
            args.Result = param1.Trim();
        }

        #endregion
        
        #region New Type-Checking and Safe Evaluation Functions

        private static void TypeOf(FunctionArgs args)
        {
            if (args.Parameters.Length != 1)
            {
                throw new ArgumentException("typeOf() requires exactly one parameter.");
            }
            object value = args.Parameters[0].Evaluate();
            args.Result = value?.GetType().Name ?? null;
        }

        private static void Try(FunctionArgs args)
        {
            if (args.Parameters.Length != 2)
            {
                throw new ArgumentException("try() requires two parameters: the expression to try and a fallback value.");
            }

            try
            {
                args.Result = args.Parameters[0].Evaluate();
            }
            catch (Exception)
            {
                args.Result = args.Parameters[1].Evaluate();
            }
        }

        #endregion
    }
}