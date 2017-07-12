using System;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

using Foundation;
using AppKit;
using CoreGraphics;

using Xamarin.PropertyEditing.ViewModels;
using System.Collections.Generic;
using ObjCRuntime;
using System.Linq;
using System.Collections.ObjectModel;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PredefinedValuesEditor<T>
		: PropertyEditorControl
	{
		const string setBezelColorSelector = "setBezelColor:";

		private readonly NSComboBox comboBox;
		List<NSButton> combinableList = new List<NSButton> ();
		bool dataPopulated;

		public PredefinedValuesEditor ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.comboBox = new NSComboBox () {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BackgroundColor = NSColor.Clear,
				StringValue = String.Empty,
				Cell = {
					ControlSize = NSControlSize.Regular	
				}
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				EditorViewModel.ValueName = comboBox.SelectedValue.ToString ();
				dataPopulated = false;
			};
		}

		public override NSView FirstKeyView => this.comboBox;
		public override NSView LastKeyView => this.comboBox;

		protected PredefinedValuesViewModel<T> EditorViewModel => (PredefinedValuesViewModel<T>)ViewModel;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (e.PropertyName));
		}

		protected override void SetEnabled ()
		{
			this.comboBox.Editable = ViewModel.Property.CanWrite;
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				if (EditorViewModel.IsCombinable) {
					foreach (var item in combinableList) {
						if (item.RespondsToSelector (new Selector (setBezelColorSelector))) {
							item.BezelColor = NSColor.Red;
						}
					}
				} else {
					this.comboBox.BackgroundColor = NSColor.Red;
				}
						    
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			} else {
				if (EditorViewModel.IsCombinable) {
					foreach (var item in combinableList) {
						if (item.RespondsToSelector (new Selector (setBezelColorSelector))) {
							item.BezelColor = NSColor.Clear;
						}
					}
				} else {
					comboBox.BackgroundColor = NSColor.Clear;
				}
				SetEnabled ();
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (!dataPopulated) {
				if (EditorViewModel.IsCombinable) {
					combinableList.Clear ();

					var top = 0;
					foreach (var item in EditorViewModel.PossibleValues) {
						var BooleanEditor = new NSButton (new CGRect (0, top, 200, 24)) { TranslatesAutoresizingMaskIntoConstraints = false };
						BooleanEditor.SetButtonType (NSButtonType.Switch);
						BooleanEditor.Title = item.Key;
						BooleanEditor.State = item.Value ? NSCellStateValue.On : NSCellStateValue.Off;
						BooleanEditor.Activated += BooleanEditor_Activated;

						AddSubview (BooleanEditor);
						combinableList.Add (BooleanEditor);
						top += 24;
					}

					// Set our new RowHeight
					RowHeight = top;
				} else {
					this.comboBox.RemoveAll ();

					// Once the VM is loaded we need a one time population
					foreach (var item in EditorViewModel.PossibleValues) {
						this.comboBox.Add (new NSString (item.Key));
					}

					AddSubview (this.comboBox);

					this.DoConstraints (new[] {
						comboBox.ConstraintTo (this, (cb, c) => cb.Width == c.Width),
						comboBox.ConstraintTo (this, (cb, c) => cb.Left == c.Left),
					});
				}

				dataPopulated = true;
			}

			base.OnViewModelChanged (oldModel);
		}

		void BooleanEditor_Activated (object sender, EventArgs e)
		{
			var values = combinableList.Where (y => y.State == NSCellStateValue.On).Select (x => (T)Enum.Parse (EditorViewModel.Property.Type, x.Title)).ToList ().AsReadOnly ();

			EditorViewModel.SetValue (EditorViewModel.Property.Type, values);
			dataPopulated = false;
		}

		protected override void UpdateValue ()
		{
			if (EditorViewModel.IsCombinable) {
				foreach (var item in combinableList) {
					item.State = EditorViewModel.PossibleValues[item.Title] ? NSCellStateValue.On : NSCellStateValue.Off;
				}
			} else {
				this.comboBox.StringValue = EditorViewModel.ValueName ?? String.Empty;
			}
		}
	}
}
