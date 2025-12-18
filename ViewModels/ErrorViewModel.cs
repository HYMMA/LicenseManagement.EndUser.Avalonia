using System;
using System.Windows.Input;
using LicenseManagement.EndUser.Avalonia.Commands;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public class ErrorViewModel : BaseViewModel
{
    private string? _message;
    private string? _innerMessage;
    private string? _innerInnerMessage;

    public ErrorViewModel()
    {
        CloseCommand = new RelayCommand(_ => { }, _ => true);
    }

    public ErrorViewModel(Exception ex) : this()
    {
        Message = ex.Message;
        InnerMessage = ex.InnerException?.Message;
        InnerInnerMessage = ex.InnerException?.InnerException?.Message;
    }

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string? InnerMessage
    {
        get => _innerMessage;
        set => SetProperty(ref _innerMessage, value);
    }

    public string? InnerInnerMessage
    {
        get => _innerInnerMessage;
        set => SetProperty(ref _innerInnerMessage, value);
    }

    public ICommand CloseCommand { get; }
}
