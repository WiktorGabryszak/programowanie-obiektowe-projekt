using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DijkstraVisualization.ViewModels;

namespace DijkstraVisualization.Views
{
    public partial class EditEdgeDialog : Window
    {
        private readonly EdgeViewModel _edge;
        private TextBox? _nameTextBox;
        private NumericUpDown? _weightNumeric;
        private CheckBox? _lockWeightCheckBox;
        private NumericUpDown? _colorR;
        private NumericUpDown? _colorG;
        private NumericUpDown? _colorB;
        private Button? _saveButton;
        private Button? _cancelButton;

        public EditEdgeDialog()
        {
            InitializeComponent();
            _edge = null!;
        }

        public EditEdgeDialog(EdgeViewModel edge)
        {
            InitializeComponent();
            _edge = edge;
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _nameTextBox = this.FindControl<TextBox>("NameTextBox");
            _weightNumeric = this.FindControl<NumericUpDown>("WeightNumeric");
            _lockWeightCheckBox = this.FindControl<CheckBox>("LockWeightCheckBox");
            _colorR = this.FindControl<NumericUpDown>("ColorR");
            _colorG = this.FindControl<NumericUpDown>("ColorG");
            _colorB = this.FindControl<NumericUpDown>("ColorB");
            _saveButton = this.FindControl<Button>("SaveButton");
            _cancelButton = this.FindControl<Button>("CancelButton");

            if (_nameTextBox != null)
            {
                _nameTextBox.Text = _edge.Name;
            }

            if (_weightNumeric != null)
            {
                _weightNumeric.Value = (decimal)_edge.Weight;
            }

            if (_lockWeightCheckBox != null)
            {
                _lockWeightCheckBox.IsChecked = _edge.IsWeightLocked;
            }

            if (_colorR != null && _colorG != null && _colorB != null)
            {
                _colorR.Value = _edge.CustomColor.R;
                _colorG.Value = _edge.CustomColor.G;
                _colorB.Value = _edge.CustomColor.B;
            }

            if (_saveButton != null)
            {
                _saveButton.Click += OnSaveClick;
            }

            if (_cancelButton != null)
            {
                _cancelButton.Click += OnCancelClick;
            }
        }

        private void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (_nameTextBox != null)
            {
                _edge.Name = _nameTextBox.Text ?? string.Empty;
            }

            if (_weightNumeric != null)
            {
                _edge.Weight = (double)(_weightNumeric.Value ?? 0);
            }

            if (_lockWeightCheckBox != null)
            {
                _edge.IsWeightLocked = _lockWeightCheckBox.IsChecked ?? false;
            }
            
            if (_colorR != null && _colorG != null && _colorB != null)
            {
                var r = (byte)Math.Clamp((int)(_colorR.Value ?? 255), 0, 255);
                var g = (byte)Math.Clamp((int)(_colorG.Value ?? 255), 0, 255);
                var b = (byte)Math.Clamp((int)(_colorB.Value ?? 255), 0, 255);
                _edge.CustomColor = Color.FromRgb(r, g, b);
            }
            
            Close();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
