using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private DateTime _created;
    private DateTime _updated;

    public DateTime Created
    {
        get => _created;
        set => SetProperty(ref _created, value);
    }

    public DateTime Updated
    {
        get => _updated;
        set => SetProperty(ref _updated, value);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void ShowErrorView(Exception ex, Control? ownerControl = null)
    {
        var errorViewModel = new ErrorViewModel(ex);
        var errorView = new Views.ErrorView { DataContext = errorViewModel };

        if (ownerControl != null)
        {
            var topLevel = TopLevel.GetTopLevel(ownerControl);
            if (topLevel is Window parentWindow)
            {
                errorView.ShowDialog(parentWindow);
                return;
            }
        }

        errorView.Show();
    }
}
