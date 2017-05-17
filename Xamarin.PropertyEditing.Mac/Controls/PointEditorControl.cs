using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PointEditorControl : PropertyEditorControl
	{
		internal NumericSpinEditor XEditor { get; set; }
		internal NumericSpinEditor YEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => YEditor;

		internal new PropertyViewModel<CGPoint> ViewModel {
			get { return (PropertyViewModel<CGPoint>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public PointEditorControl ()
		{
			var xLabel = new UnfocusableTextField (new CGRect (0, 4, 25, 20), "X:");

			XEditor = new NumericSpinEditor ();
			XEditor.Frame = new CGRect (25, 0, 50, 20);
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.StringValue = string.Empty;
			XEditor.ValueChanged += (sender, e) => {
				ViewModel.Value = new CGPoint (XEditor.Value, YEditor.Value);
			};

			var yLabel = new UnfocusableTextField (new CGRect (85, 4, 25, 20), "Y:");

			YEditor = new NumericSpinEditor ();
			YEditor.Frame = new CGRect (110, 0, 50, 20);
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.StringValue = string.Empty;
			YEditor.ValueChanged += (sender, e) => {
				ViewModel.Value = new CGPoint (XEditor.Value, YEditor.Value);
			};

			AddSubview (xLabel);
			AddSubview (XEditor);
			AddSubview (yLabel);
			AddSubview (YEditor);

            this.DoConstraints ( new[] {
				XEditor.ConstraintTo (this, (xe, c) => xe.Width == 50),
				YEditor.ConstraintTo (this, (ye, c) => ye.Width == 50),
			});
		}

		protected override void UpdateValue ()
		{
			XEditor.Value = ViewModel.Value.X;
			YEditor.Value = ViewModel.Value.Y;
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				XEditor.BackgroundColor = NSColor.Red;
				YEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered 1 or more errors:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				XEditor.BackgroundColor = NSColor.Clear;
				YEditor.BackgroundColor = NSColor.Clear;
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			XEditor.Editable = ViewModel.Property.CanWrite;
			YEditor.Editable = ViewModel.Property.CanWrite;
		}
	}
}
