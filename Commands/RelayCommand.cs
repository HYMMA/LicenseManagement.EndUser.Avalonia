using System;
using System.Windows.Input;

namespace LicenseManagement.EndUser.Avalonia.Commands;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _executeAction;
    private readonly Predicate<object?>? _canExecutePredicate;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecutePredicate = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return _canExecutePredicate?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        _executeAction(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
