using Microsoft.AspNetCore.Components;
using Sandbox.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.Client.Pages
{
    public enum ViewState
    {
        Loading,
        Empty,
        Loaded,
        Error
    };
    public abstract class RazorVM<VIEWMODEL,REFRESHMODEL> :ComponentBase
    {        
        public ViewState ViewState
        {
            get
            {
                return
                    Loading ? ViewState.Loading :
                    ViewException != null ? ViewState.Error :
                    ViewModel==null || ViewModel.Equals( default(VIEWMODEL)) ? ViewState.Empty :
                    ViewState.Loaded;
            }
        }
        bool Loading { get; set; } = true;

        public VIEWMODEL ViewModel { get; protected set; }
        protected REFRESHMODEL RefreshModel { get; private set; }
        protected Exception ViewException { get; set; }

        protected abstract Task<VIEWMODEL> Generate(REFRESHMODEL model);
        
        public async Task Refresh(REFRESHMODEL model)
        {
            try
            {
                RefreshModel = model;
                Loading = true;
                ViewException = null;
                StateHasChanged();
                ViewModel = await Generate(model);
            }
            catch (Exception problem)
            {
                ViewException = problem;
            }
            finally
            {
                Loading = false;
                StateHasChanged();
            }
        }


    }
}
