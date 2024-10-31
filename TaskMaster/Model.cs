using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class UserSession
{
    public static string CurrentUsername { get; set; }
    public static string CurrentProjectID { get; set; }
    public static string CurrentProjectName { get; set; }
}

// Assignment Model
public class Assignment
{
    public string Task_ID { get; set; }
    public string User_ID { get; set; }
    public int Percent { get; set; }
    public string JoinDate { get; set; }
}

// Comment Model
public class Comment
{
    public string Task_ID { get; set; }
    public string User_ID { get; set; }
    public string Content { get; set; }
    public string Date { get; set; }
}

// Label Model
public class Label
{
    public string LabelColor { get; set; }
    public string Name { get; set; }
}

// List Model
public class List
{
    public string List_ID { get; set; }
    public string Project_ID { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
}

// Member Model
public class Member
{
    public string User_ID { get; set; }
    public string Project_ID { get; set; }
    public string Role { get; set; }
    public string JoinDate { get; set; }
}

// Note Model
public class Note
{
    public string Note_ID { get; set; }
    public string Project_ID { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Date { get; set; }
}

// Project Model
public class Project
{
    public string Project_ID { get; set; }
    public string Name { get; set; }
    public string StartDate { get; set; }
}

// Task Model
public class Task
{
    public string Task_ID { get; set; }
    public string Project_ID { get; set; }
    public string Content { get; set; }
    public string StartDate { get; set; }
    public string DueDate { get; set; }
    public string Done { get; set; }
    public string Description { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; }

}

// TaskLabel Model
public class TaskLabel
{
    public string Task_ID { get; set; }
    public string LabelColor { get; set; }
}

// User Model
public class User
{
    public string User_ID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

