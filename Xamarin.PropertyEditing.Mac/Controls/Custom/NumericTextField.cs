using System;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class NumericTextField : NSTextField
	{
		NSText CachedCurrentEditor {
			get; set;
		}

		public bool AllowNegativeValues {
			get; set;
		}

		public bool AllowRatios {
			get; set;
		}

		public ValidationType NumericMode {
			get; set;
		}

		public event EventHandler ValidatedEditingEnded;

		public NumericTextField ()
		{
			AllowNegativeValues = true;
		}

		public override bool ShouldBeginEditing (NSText fieldEditor)
		{
			CachedCurrentEditor = fieldEditor;

			if (AllowRatios)
				CachedCurrentEditor.Delegate = new RatioValidateDelegate (this);
			else {
				CachedCurrentEditor.Delegate = new NumericValidationDelegate (this);
			}
			return true;
		}

		public void RaiseValidatedEditingEnded ()
		{
			if (ValidatedEditingEnded != null)
				ValidatedEditingEnded (this, EventArgs.Empty);
		}
	}

	public abstract class TextViewValidationDelegate : NSTextViewDelegate
	{
		protected NumericTextField TextField {
			get; set;
		}

		public TextViewValidationDelegate (NumericTextField textField)
		{
			TextField = textField;
		}

		public override bool TextShouldBeginEditing (NSText textObject)
		{
			return TextField.ShouldBeginEditing (textObject);
		}

		public override bool TextShouldEndEditing (NSText textObject)
		{
			if (!ValidateFinalString (TextField.StringValue)) {
				AppKitFramework.NSBeep ();
				return false;
			}
			return TextField.ShouldEndEditing (textObject);
		}

		public override void TextDidEndEditing (NSNotification notification)
		{
			TextField.RaiseValidatedEditingEnded ();
		}

		protected abstract bool ValidateFinalString (string value);
	}

	public class NumericValidationDelegate : TextViewValidationDelegate
	{
		public NumericValidationDelegate (NumericTextField textField)
			: base (textField)
		{

		}

		protected override bool ValidateFinalString (string value)
		{
			return TextField.NumericMode == ValidationType.Decimal ?
				FieldValidation.ValidateDecimal (value, TextField.AllowNegativeValues) :
				FieldValidation.ValidateInteger (value, TextField.AllowNegativeValues);
		}
	}

	public class RatioValidateDelegate : TextViewValidationDelegate
	{
		public RatioValidateDelegate (NumericTextField textField)
			: base (textField)
		{

		}

		protected override bool ValidateFinalString (string value)
		{
			return FieldValidation.ValidateRatio (value, ValidationType.Decimal, TextField.AllowNegativeValues);
		}
	}
}
