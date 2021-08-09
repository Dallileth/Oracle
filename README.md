## Demo

* [Framework Demo](https://sandboxserver20210807113429.azurewebsites.net)
* Oracle (Under construction)

## Usage

### Design Goals
* Minimize API surface area
* Reduce Razor property and method code pollution
* Reduce boiler-plate code from application logic 

### Components

#### Commands

* Reuses ICommand pattern used in WPF. Wraps delegates in boilerplate code
* Manages `{Running}` states
* Exceptions thrown by the delegate are thrown to the Snackbar
* Caught `ProblemExceptions`, such as those thrown by the WebAPI, show more detail
* Minimize extraneous razor properties and methods
```
<MudTextField @bind-Value="@body.Username" Disabled="@logincommand.Running"/>
<MudTextField @bind-Value="@body.Password" Disabled="@logincommand.Running"/>
<MyButton Command="logincommand" Disabled="@(string.IsNullOrWhiteSpace(loginbody.UserName) || string.IsNullOrWhiteSpace(loginbody.Password) )"/>
@code
{
    LoginBody loginbody;
    Command logincommand;
    
    protected override void OnInitialized()
    {
      logincommand=new Command(
        "Login",StateHasChanged,
        async (obj)=>
        {
            //post loginbody and set authorization
            //Problem/Forbid/etc are automatically displayed in the Snackbar
        })
        
    }
}
```

#### RazorVM<VIEWMODEL,REFRESHMODEL>

* Reuses MVVM pattern used in WPF. Wraps data-fetching in boilerplate code
* `VIEWMODEL Generate(REFRESHMODEL)` generates data
* `Refresh` manages `ViewState` between `{Loaded, Error, Empty, Loading}` 
* `ViewException` reports the error when `VIEWSTATE` is `{Error}`
```
@implements RazorVM<string,(int,int)>
@if(ViewState==ViewState.Loading)
{
   <MudSkeleton/>
}
else if (ViewState==ViewState.Error)
{
   <MudAlert Severity="Severity.Error">@ViewException.Message</MudAlert>   
}
else if (ViewState==ViewState.Empty)
{
    <MudAlert Severity="Severity.Info">No Content</MudAlert>
}
else
{
    <MudAlert Severity="Severity.Success">@ViewModel</MudAlert>
}
<MudButton OnClick="@(e=>Refresh((3,2)))" Disabled=@(ViewState==ViewState.Loading)>Add 3+2</MudButton>

@code
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //populate with initial data
            await Refresh((3, 2));
        }
    }

    protected override async Task<string> Generate((int, int) sum)
    {
        await Task.Delay(5000);
        //fetch data
        return (sum.Item1+sum.Item2).ToString();
    }      
}
```

#### WebAPI / Authorization

* Endpoints are protected server-side by blocking access with WebController attributes and, optionally, controller code
* Endpoints are protected client-side by hiding views using `AuthorizedViews`
* Endpoints are protected with middle-ware by timing out tokens and updating views

### Todo
* Commands: Parameterize exception handling
* Commands: Task cancellation
* RazorVM: Expose a RefreshCommand?
* WebAPI: Almost unauthorized warning
