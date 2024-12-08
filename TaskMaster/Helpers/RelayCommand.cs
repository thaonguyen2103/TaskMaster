using System;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }


    // Kiểm tra xem command có thể thực thi không
    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    // Thực thi command
    public void Execute(object parameter) => _execute();

    // Sự kiện thông báo khi CanExecute thay đổi
    public event EventHandler CanExecuteChanged;

    // Kích hoạt sự kiện CanExecuteChanged thủ công
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

   
    // Kiểm tra xem command có thể thực thi không
    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    // Thực thi command với tham số
    public void Execute(object parameter)
    {
        if (parameter is T param)
        {
            _execute(param);  // Chuyển đổi và truyền tham số vào action
        }
    }

    // Sự kiện thông báo khi CanExecute thay đổi
    public event EventHandler CanExecuteChanged;

    // Kích hoạt sự kiện CanExecuteChanged thủ công
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
