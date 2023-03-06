using System;
using Avalonia.Controls;
using SuperPasses.ViewModels;

namespace SuperPasses.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SaveConfig();
        }
    }
}