using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	public class NumericSpinEditor : NSView
	{
		protected NumericTextField numericEditor;
		protected NSStepper stepper;
		protected bool editing;


		public event EventHandler ValueChanged;
		public event EventHandler StepperValueChanged;
		public event EventHandler FieldEditingEnded;

		public ValidationType NumericMode {
			get { return numericEditor.NumericMode; }
			set {
				numericEditor.NumericMode = value;
				Reset ();
			}
		}

		public string StringValue {
			get { return numericEditor.StringValue; }
			set { numericEditor.StringValue = value; }
		}

		public string PlaceholderString {
			get { return ((NSTextFieldCell)numericEditor.Cell).PlaceholderString; }
			set { ((NSTextFieldCell)numericEditor.Cell).PlaceholderString = value; }
		}

		public NSStepper Stepper {
			get { return stepper; }
		}

		public override CGSize IntrinsicContentSize {
			get {
				var baseSize = stepper.IntrinsicContentSize;
				return new CGSize (baseSize.Width + 20, baseSize.Height);
			}
		}

		public NSColor BackgroundColor {
			get {
				return numericEditor.BackgroundColor;
			}
			set {
				numericEditor.BackgroundColor = value;
			}
		}

		public override nfloat BaselineOffsetFromBottom {
			get { return numericEditor.BaselineOffsetFromBottom; }
		}

		public int Digits {
			get { return (int)formatter.MaximumFractionDigits; }
			set { formatter.MaximumFractionDigits = value; }
		}

		public virtual double Value {
			get { return stepper.DoubleValue; }
			set { SetValue (value); }
		}

		public bool Wrap {
			get { return stepper.ValueWraps; }
			set { stepper.ValueWraps = value; }
		}

		public double MinimumValue {
			get { return stepper.MinValue; }
			set {
				stepper.MinValue = value;
				formatter.Minimum = new NSNumber (value);
			}
		}

		public double MaximumValue {
			get { return stepper.MaxValue; }
			set {
				stepper.MaxValue = value;
				formatter.Maximum = new NSNumber (value);
			}
		}

		public double IncrementValue {
			get { return stepper.Increment; }
			set { stepper.Increment = value; }
		}

		public bool Enabled {
			get {
				return numericEditor.Enabled;
			}
			set {
				numericEditor.Enabled = value;
				stepper.Enabled = value;
			}
		}

		NSNumberFormatter formatter;
		public NSNumberFormatter Formatter {
			get {
				return formatter;
			}
			set {
				formatter = value;
				numericEditor.Formatter = formatter;
			}
		}

		public bool IsIndeterminate {
			get {
				return !string.IsNullOrEmpty (numericEditor.StringValue);
			}
			set {
				if (value)
					numericEditor.StringValue = string.Empty;
				else
					numericEditor.DoubleValue = stepper.DoubleValue;
			}
		}

		public bool Editable {
			get {
				return numericEditor.Editable;
			}
			set {
				numericEditor.Editable = value;
				stepper.Enabled = value;
			}
		}

		public NSNumberFormatterStyle NumberStyle {
			get { return formatter.NumberStyle; }
			set {
				formatter.NumberStyle = value;
			}
		}

		protected virtual void OnConfigureNumericTextField ()
		{
			numericEditor.Formatter = formatter;
		}

		public virtual void Reset ()
		{
		}

		public NumericSpinEditor ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			var controlSize = NSControlSize.Small;

			stepper = new NSStepper {
				TranslatesAutoresizingMaskIntoConstraints = false,
				ValueWraps = false,
			};
			stepper.Cell.ControlSize = controlSize;

			formatter = new NSNumberFormatter {
				FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
				Locale = NSLocale.CurrentLocale,
				MaximumFractionDigits = 15,
				NumberStyle = NSNumberFormatterStyle.Decimal,
				UsesGroupingSeparator = false,
			};

			numericEditor = new NumericTextField {
				Alignment = NSTextAlignment.Right,
				Formatter = formatter,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			numericEditor.Cell.ControlSize = controlSize;
			numericEditor.Cell.Font = NSFont.ControlContentFontOfSize (NSFont.SystemFontSizeForControlSize (controlSize));

			stepper.Activated += (s, e) => {
				OnStepperActivated (s, e);
			};

			numericEditor.ValidatedEditingEnded += (s, e) => {
				OnFieldEditingEnded (s, e);
			};

			AddSubview (stepper);
			AddSubview (numericEditor);

			this.DoConstraints (new[] {
				numericEditor.ConstraintTo (this, (n, c) => n.Width == c.Width - 17),
				stepper.ConstraintTo (numericEditor, (s, n) => s.Left == n.Right + 5),
				stepper.ConstraintTo (numericEditor, (s, n) => s.Top == n.Top + 1),
			});
		}

		protected virtual void SetStepperActivated ()
		{
			SetValue (stepper.DoubleValue);
		}

		protected void OnStepperActivated (object sender, EventArgs e)
		{
			if (!editing) {
				editing = true;
				SetStepperActivated ();
				OnStepperValueChanged ();
				if (ValueChanged != null)
					ValueChanged (this, EventArgs.Empty);
				editing = false;
			}
		}

		protected virtual void OnStepperValueChanged ()
		{
			if (StepperValueChanged != null)
				StepperValueChanged (this, EventArgs.Empty);
		}

		protected void OnFieldEditingEnded (object sender, EventArgs e)
		{
			if (!editing) {
				editing = true;
				SetValue (numericEditor.StringValue);
				if (FieldEditingEnded != null)
					FieldEditingEnded (this, EventArgs.Empty);
				if (ValueChanged != null)
					ValueChanged (this, EventArgs.Empty);
				editing = false;
			}
		}

		public virtual void SetValue (string value)
		{
			//Regulates maximun and minium out of range
			stepper.DoubleValue = CoerceValue (FieldValidation.FixInitialValue (value, Value.ToEditorString ()).ToEditorDouble ());
			numericEditor.StringValue = FieldValidation.RoundDoubleValue (stepper.DoubleValue.ToEditorString (), NumericMode == ValidationType.Decimal ? FieldValidation.DefaultXcodeMaxRoundDigits : 0);
		}

		public virtual void SetValue (double value)
		{
			//Regulates maximun and minium out of range
			stepper.DoubleValue = CoerceValue (value);
			numericEditor.StringValue = FieldValidation.RoundDoubleValue (stepper.DoubleValue.ToEditorString (), NumericMode == ValidationType.Decimal ? FieldValidation.DefaultXcodeMaxRoundDigits : 0);
		}

		protected double CoerceValue (double val)
		{
			return FieldValidation.CoerceValue (val, MinimumValue, MaximumValue);
		}

		public void AddAccessoryView (NSControl control)
		{
			AddSubview (control);
			AddConstraints (new NSLayoutConstraint[] {
				NSLayoutConstraint.Create (control, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 1),
				NSLayoutConstraint.Create (control, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1, 0),
				//NSLayoutConstraint.Create (control, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1, 80),
				NSLayoutConstraint.Create (stepper, NSLayoutAttribute.Right, NSLayoutRelation.Equal, control, NSLayoutAttribute.Left, 1, -6)
			});
			// tODO RemoveConstraint (rightSideConstraint);
		}

		public void SimulateTextInput (string input)
		{
			numericEditor.StringValue = input;
			OnFieldEditingEnded (numericEditor, EventArgs.Empty);
		}

		public void GoUp ()
		{
			stepper.DoubleValue = stepper.DoubleValue + IncrementValue;
			OnStepperActivated (stepper, EventArgs.Empty);
		}

		public void GoDown ()
		{
			stepper.DoubleValue = stepper.DoubleValue - IncrementValue;
			OnStepperActivated (stepper, EventArgs.Empty);
		}
	}
}
