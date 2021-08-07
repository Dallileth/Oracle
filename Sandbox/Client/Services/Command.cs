using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sandbox.Client.Services
{
    public class Command:ICommand
    {
        public string Name { get;private set; }
        public bool HideErrors { get; set; }
        Func<object,Task> _action { get; set; }
        static ISnackbar _snack { get; set; }
        bool _running = false;
        public bool Running { get
            {
                return _running;
            }
            private set
            {
                if (_running != value)
                {
                    _running = value;
                    CanExecuteChanged?.Invoke(this, null);
                }
            }
        }
        public EventCallback<bool> RunningChanged;
        Func<Exception, Task> _exception_handler;

        public static void Init(ISnackbar snack)
        {
            _snack = snack;
        }
        Action _state_has_changed;
        public Command(string name, Action statehaschanged, Func<object, Task> action,Func<Exception,Task> exceptionhandler=null,bool hide_errors=false)
        {
            Name = name;
            _action = action;
            _state_has_changed = statehaschanged;
            _exception_handler = exceptionhandler;
            HideErrors = hide_errors;
        }
        
        

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return !Running;
        }

        public async void Execute(object parameter)
        {
            //note: if you were to do
            //  await command.Execute(...);
            //  Console.WriteLine("outside");
            //the output would be
            //  starting
            //  outside
            //  ended
            try
            {
                Running = true;
                _state_has_changed?.Invoke();
                await _action(parameter);
            }
            catch (ProblemException problem)
            {
                string message = $"{(string.IsNullOrWhiteSpace(problem.Title) ? $"{Name} Error: " : $"{problem.Title}: ")}{problem.Detail}";
                if (!HideErrors)
                {
                    _snack.Add(message, Severity.Error);
                }
                
                if (_exception_handler != null)
                    await _exception_handler(problem);
            }
            catch (Exception generic_exception)
            {
                if (!HideErrors)
                {
                    _snack.Add($"{Name} Error: Something went wrong", Severity.Error);
                }
                
                if (_exception_handler != null)
                    await _exception_handler(generic_exception);
            }
            finally
            {
                Running = false;
                _state_has_changed?.Invoke();
            }
        }
    }




}
