using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace HermaFx.DataAnnotations
{
	internal class KeyRegexTests
	{
		private class KeyTypeValidationErrorModel
		{
			public bool Value0 { get; set; }

			[KeyRegex(@"^.*$")]
			public IDictionary<int, string> Values1 { get; set; }

			[KeyRegex(@"^.*$")]
			public IReadOnlyDictionary<int, string> Values2 { get; set; }

			[KeyRegex(@"^.*$")]
			public Dictionary<int, string> Values3 { get; set; }
		}

		[Test]
		public void KeyTypeValidationErrorTest()
		{
			var model = new KeyTypeValidationErrorModel() { };
			Assert.Multiple(() =>
			{
				Assert.That(ExtendedValidator.IsValid(model), Is.False);
				var exception = Assert.Throws<AggregateValidationException>(
					() => ExtendedValidator.EnsureIsValid(model),
					"#1 Object of type Model has some invalid values."
				);
				Assert.That(exception.ValidationResults, Has.Exactly(3).Items, "#2");
				Assert.That(exception.ValidationResults, Has.One.Matches<ValidationResult>(x => x.ErrorMessage == "'Values1' must be a dictionary with string keys, and the keys must be in the format of '^.*$'." && x.MemberNames.Any(x => x == "Values1")), "#2.1");
				Assert.That(exception.ValidationResults, Has.One.Matches<ValidationResult>(x => x.ErrorMessage == "'Values2' must be a dictionary with string keys, and the keys must be in the format of '^.*$'." && x.MemberNames.Any(x => x == "Values2")), "#2.2");
				Assert.That(exception.ValidationResults, Has.One.Matches<ValidationResult>(x => x.ErrorMessage == "'Values3' must be a dictionary with string keys, and the keys must be in the format of '^.*$'." && x.MemberNames.Any(x => x == "Values3")), "#2.3");
			});
		}

		private class KeyTypeValidModel
		{
			public bool Value0 { get; set; }

			[KeyRegex(@"^[a-zA-Z0-9]{1,20}$")]
			public IDictionary<string, string> Values1 { get; set; }

			[KeyRegex(@"^[a-zA-Z0-9]{1,20}$")]
			public IReadOnlyDictionary<string, string> Values2 { get; set; }

			[KeyRegex(@"^[a-zA-Z0-9]{1,20}$")]
			public Dictionary<string, string> Values3 { get; set; }
		}

		[Test]
		public void KeyValidTest()
		{
			var model = new KeyTypeValidModel()
			{
				Values1 = new Dictionary<string, string>() { { "Values1key1", "Values1value1" }, { "Values1key2", "Values1value2" } },
				Values2 = new Dictionary<string, string>() { { "Values2key1", "Values1value1" }, { "Values2key2", "Values1value2" } },
				Values3 = new Dictionary<string, string>() { { "Values3key1", "Values1value1" }, { "Values3key2", "Values1value2" } },
			};
			Assert.Multiple(() =>
			{
				Assert.That(ExtendedValidator.IsValid(model), Is.True, "#1");
				Assert.DoesNotThrow(() => ExtendedValidator.EnsureIsValid(model), "#2");
			});
		}

		[Test]
		public void KeyInvalidTest()
		{
			var model = new KeyTypeValidModel()
			{
				Values1 = new Dictionary<string, string>() { { "Values1key1", "Values1value1" }, { "Values1.key2", "Values1value2" } },
				Values2 = new Dictionary<string, string>() { { "Values2key1", "Values1value1" }, { "Values2.key2", "Values1value2" } },
				Values3 = new Dictionary<string, string>() { { "Values3key1", "Values1value1" }, { "Values3.key2", "Values1value2" } },
			};
			Assert.Multiple(() =>
			{
				Assert.That(ExtendedValidator.IsValid(model), Is.False, "#1");
				var exception = Assert.Throws<AggregateValidationException>(
					() => ExtendedValidator.EnsureIsValid(model),
					"#1 Object of type Model has some invalid values."
				);
				Assert.That(exception.ValidationResults, Has.Exactly(3).Items, "#2");
				Assert.That(exception.ValidationResults, Has.One.Matches<ValidationResult>(x => x.ErrorMessage == "'Values1' must be a dictionary with string keys, and the keys must be in the format of '^[a-zA-Z0-9]{1,20}$'." && x.MemberNames.Any(x => x == "Values1")), "#2.1");
				Assert.That(exception.ValidationResults, Has.One.Matches<ValidationResult>(x => x.ErrorMessage == "'Values2' must be a dictionary with string keys, and the keys must be in the format of '^[a-zA-Z0-9]{1,20}$'." && x.MemberNames.Any(x => x == "Values2")), "#2.2");
				Assert.That(exception.ValidationResults, Has.One.Matches<ValidationResult>(x => x.ErrorMessage == "'Values3' must be a dictionary with string keys, and the keys must be in the format of '^[a-zA-Z0-9]{1,20}$'." && x.MemberNames.Any(x => x == "Values3")), "#2.3");
			});
		}
	}
}
