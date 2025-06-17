using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HelpdeskSystem.BLL.Models;
using HelpdeskSystem.BLL.Services;

namespace HelpdeskSystem.UI.ViewModels
{
    /// <summary>
    /// Base ViewModel implementing INotifyPropertyChanged
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Command implementation for MVVM pattern
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);
    }

    /// <summary>
    /// Main Window ViewModel
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private readonly ITicketService _ticketService;
        private BaseViewModel _currentViewModel;
        private string _statusMessage;

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public User CurrentUser => _authService.CurrentUser;
        public bool IsUserLoggedIn => CurrentUser != null;
        public bool IsTechnician => CurrentUser?.IsTechnician == true;

        public ICommand ShowLoginCommand { get; }
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowCreateTicketCommand { get; }
        public ICommand ShowMyTicketsCommand { get; }
        public ICommand ShowAttendTicketsCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(IAuthenticationService authService, ITicketService ticketService)
        {
            _authService = authService;
            _ticketService = ticketService;

            ShowLoginCommand = new RelayCommand(_ => ShowLogin());
            ShowDashboardCommand = new RelayCommand(_ => ShowDashboard(), _ => IsUserLoggedIn);
            ShowCreateTicketCommand = new RelayCommand(_ => ShowCreateTicket(), _ => IsUserLoggedIn);
            ShowMyTicketsCommand = new RelayCommand(_ => ShowMyTickets(), _ => IsUserLoggedIn);
            ShowAttendTicketsCommand = new RelayCommand(_ => ShowAttendTickets(), _ => IsTechnician);
            LogoutCommand = new RelayCommand(_ => Logout(), _ => IsUserLoggedIn);

            ShowLogin();
        }

        private void ShowLogin()
        {
            CurrentViewModel = new LoginViewModel(_authService, this);
        }

        private void ShowDashboard()
        {
            CurrentViewModel = new DashboardViewModel(_ticketService);
        }

        private void ShowCreateTicket()
        {
            CurrentViewModel = new CreateTicketViewModel(_ticketService, CurrentUser);
        }

        private void ShowMyTickets()
        {
            CurrentViewModel = new MyTicketsViewModel(_ticketService, CurrentUser);
        }

        private void ShowAttendTickets()
        {
            CurrentViewModel = new AttendTicketsViewModel(_ticketService, CurrentUser);
        }

        private async void Logout()
        {
            await _authService.LogoutAsync();
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsUserLoggedIn));
            OnPropertyChanged(nameof(IsTechnician));
            ShowLogin();
        }

        public void OnLoginSuccess()
        {
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsUserLoggedIn));
            OnPropertyChanged(nameof(IsTechnician));
            ShowDashboard();
        }
    }

    /// <summary>
    /// Login ViewModel
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private readonly MainViewModel _mainViewModel;
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isLoggingIn;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set => SetProperty(ref _isLoggingIn, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthenticationService authService, MainViewModel mainViewModel)
        {
            _authService = authService;
            _mainViewModel = mainViewModel;
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
        }

        private bool CanLogin()
        {
            return !IsLoggingIn && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task LoginAsync()
        {
            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = await _authService.AuthenticateAsync(Username, Password);
                if (user != null)
                {
                    _mainViewModel.OnLoginSuccess();
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }

    /// <summary>
    /// Dashboard ViewModel
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ITicketService _ticketService;
        private TicketStatistics _statistics;
        private bool _isLoading;

        public TicketStatistics Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
            RefreshCommand = new RelayCommand(async _ => await LoadStatisticsAsync());

            Task.Run(LoadStatisticsAsync);
        }

        private async Task LoadStatisticsAsync()
        {
            IsLoading = true;
            try
            {
                Statistics = await _ticketService.GetStatistics();
            }
            catch (Exception ex)
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>
    /// Create Ticket ViewModel
    /// </summary>
    public class CreateTicketViewModel : BaseViewModel
    {
        private readonly ITicketService _ticketService;
        private readonly User _currentUser;
        private string _selectedTicketType;
        private string _equipment;
        private string _issue;
        private string _software;
        private string _description;
        private string _statusMessage;
        private bool _isSubmitting;

        public string SelectedTicketType
        {
            get => _selectedTicketType;
            set
            {
                SetProperty(ref _selectedTicketType, value);
                OnPropertyChanged(nameof(IsHardwareTicket));
                OnPropertyChanged(nameof(IsSoftwareTicket));
            }
        }

        public string Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        public string Issue
        {
            get => _issue;
            set => SetProperty(ref _issue, value);
        }

        public string Software
        {
            get => _software;
            set => SetProperty(ref _software, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetProperty(ref _isSubmitting, value);
        }

        public bool IsHardwareTicket => SelectedTicketType == "Hardware";
        public bool IsSoftwareTicket => SelectedTicketType == "Software";

        public ICommand SubmitTicketCommand { get; }
        public ICommand ClearFormCommand { get; }

        public CreateTicketViewModel(ITicketService ticketService, User currentUser)
        {
            _ticketService = ticketService;
            _currentUser = currentUser;

            SubmitTicketCommand = new RelayCommand(async _ => await SubmitTicketAsync(), _ => CanSubmitTicket());
            ClearFormCommand = new RelayCommand(_ => ClearForm());
        }

        private bool CanSubmitTicket()
        {
            if (IsSubmitting || string.IsNullOrWhiteSpace(SelectedTicketType))
                return false;

            if (IsHardwareTicket)
                return !string.IsNullOrWhiteSpace(Equipment) && !string.IsNullOrWhiteSpace(Issue);

            if (IsSoftwareTicket)
                return !string.IsNullOrWhiteSpace(Software) && !string.IsNullOrWhiteSpace(Description);

            return false;
        }

        private async Task SubmitTicketAsync()
        {
            IsSubmitting = true;
            StatusMessage = string.Empty;

            try
            {
                Ticket ticket = SelectedTicketType switch
                {
                    "Hardware" => new HardwareTicket
                    {
                        Equipment = Equipment,
                        Issue = Issue,
                        SubmittedBy = _currentUser.UserId
                    },
                    "Software" => new SoftwareTicket
                    {
                        Software = Software,
                        Description = Description,
                        SubmittedBy = _currentUser.UserId
                    },
                    _ => throw new ArgumentException("Invalid ticket type")
                };

                var ticketId = await _ticketService.CreateTicket(ticket);
                StatusMessage = $"Ticket #{ticketId} created successfully!";
                ClearForm();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating ticket: {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private void ClearForm()
        {
            SelectedTicketType = string.Empty;
            Equipment = string.Empty;
            Issue = string.Empty;
            Software = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// My Tickets ViewModel
    /// </summary>
    public class MyTicketsViewModel : BaseViewModel
    {
        private readonly ITicketService _ticketService;
        private readonly User _currentUser;
        private ObservableCollection<Ticket> _tickets;
        private Ticket _selectedTicket;
        private bool _isLoading;

        public ObservableCollection<Ticket> Tickets
        {
            get => _tickets;
            set => SetProperty(ref _tickets, value);
        }

        public Ticket SelectedTicket
        {
            get => _selectedTicket;
            set => SetProperty(ref _selectedTicket, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand RefreshCommand { get; }

        public MyTicketsViewModel(ITicketService ticketService, User currentUser)
        {
            _ticketService = ticketService;
            _currentUser = currentUser;
            Tickets = new ObservableCollection<Ticket>();

            RefreshCommand = new RelayCommand(async _ => await LoadTicketsAsync());

            Task.Run(LoadTicketsAsync);
        }

        private async Task LoadTicketsAsync()
        {
            IsLoading = true;
            try
            {
                var tickets = await _ticketService.GetUserTickets(_currentUser.UserId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Tickets.Clear();
                    foreach (var ticket in tickets)
                    {
                        Tickets.Add(ticket);
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>
    /// Attend Tickets ViewModel (for technicians)
    /// </summary>
    public class AttendTicketsViewModel : BaseViewModel
    {
        private readonly ITicketService _ticketService;
        private readonly User _currentUser;
        private ObservableCollection<Ticket> _unattendedTickets;
        private Ticket _selectedTicket;
        private string _attendanceDescription;
        private AttendanceStatus _selectedAttendanceStatus;
        private bool _isLoading;
        private bool _isAttending;

        public ObservableCollection<Ticket> UnattendedTickets
        {
            get => _unattendedTickets;
            set => SetProperty(ref _unattendedTickets, value);
        }

        public Ticket SelectedTicket
        {
            get => _selectedTicket;
            set => SetProperty(ref _selectedTicket, value);
        }

        public string AttendanceDescription
        {
            get => _attendanceDescription;
            set => SetProperty(ref _attendanceDescription, value);
        }

        public AttendanceStatus SelectedAttendanceStatus
        {
            get => _selectedAttendanceStatus;
            set => SetProperty(ref _selectedAttendanceStatus, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsAttending
        {
            get => _isAttending;
            set => SetProperty(ref _isAttending, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AttendTicketCommand { get; }

        public AttendTicketsViewModel(ITicketService ticketService, User currentUser)
        {
            _ticketService = ticketService;
            _currentUser = currentUser;
            UnattendedTickets = new ObservableCollection<Ticket>();

            RefreshCommand = new RelayCommand(async _ => await LoadUnattendedTicketsAsync());
            AttendTicketCommand = new RelayCommand(async _ => await AttendTicketAsync(), _ => CanAttendTicket());

            Task.Run(LoadUnattendedTicketsAsync);
        }

        private bool CanAttendTicket()
        {
            return !IsAttending && SelectedTicket != null &&
                   !string.IsNullOrWhiteSpace(AttendanceDescription);
        }

        private async Task LoadUnattendedTicketsAsync()
        {
            IsLoading = true;
            try
            {
                var tickets = await _ticketService.GetUnattendedTickets();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UnattendedTickets.Clear();
                    foreach (var ticket in tickets)
                    {
                        UnattendedTickets.Add(ticket);
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AttendTicketAsync()
        {
            IsAttending = true;
            try
            {
                var success = await _ticketService.AttendTicket(
                    SelectedTicket.Id,
                    _currentUser.UserId,
                    AttendanceDescription,
                    SelectedAttendanceStatus);

                if (success)
                {
                    await LoadUnattendedTicketsAsync();
                    AttendanceDescription = string.Empty;
                    SelectedTicket = null;
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
            finally
            {
                IsAttending = false;
            }
        }
    }
}