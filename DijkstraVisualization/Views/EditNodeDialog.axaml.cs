using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DijkstraVisualization.ViewModels;

namespace DijkstraVisualization.Views
{
    public partial class EditNodeDialog : Window
    {
        private readonly NodeViewModel _node;
        private TextBox? _nameTextBox;
        private NumericUpDown? _colorR;
        private NumericUpDown? _colorG;
        private NumericUpDown? _colorB;
        private Button? _saveButton;
        private Button? _cancelButton;

        public EditNodeDialog()
        {
            InitializeComponent();
            _node = null!;
        }

        public EditNodeDialog(NodeViewModel node)
        {
            InitializeComponent();
            _node = node;
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _nameTextBox = this.FindControl<TextBox>("NameTextBox");
            _colorR = this.FindControl<NumericUpDown>("ColorR");
            _colorG = this.FindControl<NumericUpDown>("ColorG");
            _colorB = this.FindControl<NumericUpDown>("ColorB");
            _saveButton = this.FindControl<Button>("SaveButton");
            _cancelButton = this.FindControl<Button>("CancelButton");

            if (_nameTextBox != null)
            {
                _nameTextBox.Text = _node.Name;
            }

            if (_colorR != null && _colorG != null && _colorB != null)
            {
                _colorR.Value = _node.CustomColor.R;
                _colorG.Value = _node.CustomColor.G;
                _colorB.Value = _node.CustomColor.B;
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
                _node.Name = _nameTextBox.Text ?? string.Empty;
            }
            
            if (_colorR != null && _colorG != null && _colorB != null)
            {
                var r = (byte)Math.Clamp((int)(_colorR.Value ?? 128), 0, 255);
                var g = (byte)Math.Clamp((int)(_colorG.Value ?? 128), 0, 255);
                var b = (byte)Math.Clamp((int)(_colorB.Value ?? 128), 0, 255);
                _node.CustomColor = Color.FromRgb(r, g, b);
            }
            
            Close();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
