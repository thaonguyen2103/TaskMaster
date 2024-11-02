using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// Base ViewModel
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Assignment ViewModel
public class AssignmentViewModel : BaseViewModel
{
    public ObservableCollection<Assignment> Assignments { get; set; }

    public AssignmentViewModel()
    {
        Assignments = new ObservableCollection<Assignment>();
    }
}

// Comment ViewModel
public class CommentViewModel : BaseViewModel
{
    public ObservableCollection<Comment> Comments { get; set; }

    public CommentViewModel()
    {
        Comments = new ObservableCollection<Comment>();
    }
}

// Label ViewModel
public class LabelViewModel : BaseViewModel
{
    public ObservableCollection<Label> Labels { get; set; }

    public LabelViewModel()
    {
        Labels = new ObservableCollection<Label>();
    }
}

// List ViewModel
public class ListViewModel : BaseViewModel
{
    public ObservableCollection<List> Lists { get; set; }

    public ListViewModel()
    {
        Lists = new ObservableCollection<List>();
    }
}

// Member ViewModel
public class MemberViewModel : BaseViewModel
{
    public ObservableCollection<Member> Members { get; set; }

    public MemberViewModel()
    {
        Members = new ObservableCollection<Member>();
    }
}

// Note ViewModel
public class NoteViewModel : BaseViewModel
{
    public ObservableCollection<Note> Notes { get; set; }

    public NoteViewModel()
    {
        Notes = new ObservableCollection<Note>();
    }
}

// Project ViewModel
public class ProjectViewModel : BaseViewModel
{
    public ObservableCollection<Project> Projects { get; set; }

    public ProjectViewModel()
    {
        Projects = new ObservableCollection<Project>();
    }
}

// Task ViewModel
public class TaskViewModel : BaseViewModel
{
    public ObservableCollection<Task> Tasks { get; set; }

    public TaskViewModel()
    {
        Tasks = new ObservableCollection<Task>();
    }
}

// TaskLabel ViewModel
public class TaskLabelViewModel : BaseViewModel
{
    public ObservableCollection<TaskLabel> TaskLabels { get; set; }

    public TaskLabelViewModel()
    {
        TaskLabels = new ObservableCollection<TaskLabel>();
    }
}

// User ViewModel
public class UserViewModel : BaseViewModel
{
    public ObservableCollection<User> Users { get; set; }

    public UserViewModel()
    {
        Users = new ObservableCollection<User>();
    }
}

