using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HermaFx.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class KeyRegexAttribute : RegularExpressionAttribute
	{
		public string DefaultErrorMessage { get; } = "'{0}' must be a dictionary with string keys, and the keys must be in the format of '{1}'.";

		public KeyRegexAttribute(string pattern)
			: base(pattern)
		{
		}

		private static bool IsStringKeyedIDictionary(Type type)
			=> type.IsGenericType
				&& (type.GetGenericTypeDefinition() == typeof(IDictionary<,>) || type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
				&& type.GetGenericArguments()[0] == typeof(string);

		private static IEnumerable<string> GetKeys(Type type, object value)
			=> value is null
				? Array.Empty<string>()
				: type.GetMethod("get_Keys").Invoke(value, null) as IEnumerable<string>;

		private static bool IsStringKeyedDictionary(Type type, object value, out IEnumerable<string> keys)
		{
			if (IsStringKeyedIDictionary(type))
			{
				keys = GetKeys(type, value);
				return true;
			}

			foreach (var interfaceType in type.GetInterfaces())
			{
				if (IsStringKeyedIDictionary(interfaceType))
				{
					keys = GetKeys(type, value);
					return true;
				}
			}

			keys = Array.Empty<string>();
			return false;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var type = value != null
				? value.GetType()
				: validationContext.ObjectType.GetProperty(validationContext.DisplayName).PropertyType;

			if (!IsStringKeyedDictionary(type, value, out IEnumerable<string> keys)
				|| keys?.Any(x => x.IsNullOrWhiteSpace() || !base.IsValid(x)) == true)
			{
				return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new[] { validationContext.DisplayName });
			}

			return ValidationResult.Success;
		}

		public override string FormatErrorMessage(string propertyName)
		{
			if (string.IsNullOrEmpty(ErrorMessageResourceName) && string.IsNullOrEmpty(ErrorMessage))
				ErrorMessage = DefaultErrorMessage;

			return string.Format(ErrorMessageString, propertyName, Pattern);
		}
	}
}
