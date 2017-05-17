using System;
using System.Drawing;
using System.Text;

namespace Xamarin.PropertyEditing
{
	public static class StringConversionExtensions
	{
		static readonly System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

		public static string ToEditorString (this bool value)
		{
			return value ? "true" : "false";
		}

		public static double ToEditorDouble (this string value)
		{
			if (string.IsNullOrEmpty (value))
				return 0;

			double result;
			if (ToEditorDoubleIfPossible (value, out result))
				return result;

			// Constraint multilpliers can be specified as '9:5'. we should treat this as the float '1.8'
			// They can also be specified as 9/5, which should be treated as the float 1.8 as well.
			var parts = value.Split (':');
			if (parts.Length != 2)
				parts = value.Split ('/');

			if (parts.Length == 2) {
				try {
					var first = double.Parse (parts[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					var second = double.Parse (parts[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					return first / second;
				}
				catch {
					throw new ArgumentException (string.Format ("The value '{0}' could not be parsed as a float", value));
				}
			}
			throw new NotSupportedException (string.Format ("Unsupported number format '{0}'", value));
		}

		public static float ToEditorFloat (this string value)
		{
			return (float)value.ToEditorDouble ();
		}

		public static SizeF ToEditorSize (this string value)
		{
			var parts = value.Split (',');
			if (parts.Length != 2)
				throw new ArgumentException (string.Format ("The value '{0}' was not a Size", value));
			return new SizeF (parts[0].Trim ().ToEditorFloat (), parts[1].Trim ().ToEditorFloat ());
		}

		public static bool ToEditorFloatIfPossible (this string value, out float number)
		{
			double result;
			var retVal = value.ToEditorDoubleIfPossible (out result);
			number = (float)result;
			return retVal;
		}

		public static bool ToEditorDoubleIfPossible (this string value, out double number)
		{
			number = 0;
			return double.TryParse (value, System.Globalization.NumberStyles.Any, invariantCulture.NumberFormat, out number);
		}

		public static bool ToEditorBool (this string value)
		{
			return "true".Equals (value, StringComparison.OrdinalIgnoreCase) ? true : false;
		}

		public static string ToStoryboardBool (this string value)
		{
			return bool.Parse (value).ToStoryboardBool ();
		}

		public static string ToStoryboardBool (this bool value)
		{
			return value ? "YES" : "NO";
		}

		public static string ToStoryboardBool (this bool? value)
		{
			if (!value.HasValue)
				return null;
			return value.Value ? "YES" : "NO";
		}

		public static bool FromStoryboardBool (this string value)
		{
			return "YES".Equals (value);
		}

		public static T ToEditorEnum<T> (this string value)
		{
			return (T)Enum.Parse (typeof (T), value, true);
		}

		public static string ToEditorString (this int value)
		{
			return value.ToString (invariantCulture.NumberFormat);
		}

		public static string ToEditorString (this double value)
		{
			return value == 0d ? "0.0" : value.ToString (invariantCulture.NumberFormat);
		}

		public static string ToEditorString (this float value)
		{
			return value == 0f ? "0.0" : value.ToString (invariantCulture.NumberFormat);
		}

		public static int ToEditorDigitsCount (this double value)
		{
			var baseDigits = ((int)Math.Truncate (value)).ToString ().Length;
			var fullDigits = value.ToString (invariantCulture.NumberFormat).Length;
			return Math.Max (0, fullDigits - baseDigits - 1);
		}

		public static string ToEditorString (this PointF value)
		{
			return string.Format ("{0}, {1}",
				value.X.ToEditorString (),
				value.Y.ToEditorString ());
		}

		public static string ToDesignerString (this RectangleF value)
		{
			return string.Format ("{0}, {1}, {2}, {3}",
				value.X.ToEditorString (),
				value.Y.ToEditorString (),
				value.Width.ToEditorString (),
				value.Height.ToEditorString ());
		}

		public static string ToEditorString (this SizeF value)
		{
			return string.Format ("{0}, {1}",
				value.Width.ToEditorString (),
				value.Height.ToEditorString ());
		}

		/// <summary>
		/// Transform a camel or Pascal cased string into one with spaces
		/// </summary>
		public static string ToHumanString (this object obj)
		{
			var str = obj.ToString ();
			var sb = new StringBuilder (str.Length + 10);
			sb.Append (char.ToUpper (str[0]));
			for (int i = 1; i < str.Length; i++) {
				var chr = str[i];
				if (char.IsUpper (chr) && i < str.Length - 1 && (!char.IsUpper (str, i - 1) || !char.IsUpper (str, i + 1)))
					sb.Append (' ');
				sb.Append (chr);
			}
			return sb.ToString ().Trim ();
		}

		/// <summary>
		/// Transform a Pascal case C# value to a camel case value used in storyboards
		/// </summary>
		public static string ToCamelCase (this string value)
		{
			if (string.IsNullOrEmpty (value))
				return value;
			var first = value[0];
			return char.ToLowerInvariant (first) + value.Substring (1);
		}

		/// <summary>
		/// Transform a camel case value used in storyboards to a Pascal case C# value
		/// </summary>
		public static string ToPascalCase (this string value)
		{
			if (string.IsNullOrEmpty (value))
				return value;
			var first = value[0];
			return char.ToUpperInvariant (first) + value.Substring (1);
		}

		/// <summary>
		/// Handle versions like '3035' and also '3035.24'. By default System.Version blows up if there's no '.'
		/// </summary>
		public static Version ToVersion (this string value)
		{
			if (string.IsNullOrEmpty (value))
				return new Version ();
			if (value.Contains ("."))
				return Version.Parse (value);
			return new Version (int.Parse (value), 0);
		}
	}
}
