using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Sitko.Core.Blazor.FluentValidation
{
    public class BlazorFluentValidator : ComponentBase
    {
        [CascadingParameter] EditContext CurrentEditContext { get; set; }

        [Inject] FluentValidator FluentValidator { get; set; }

        [Inject] IServiceProvider ServiceProvider { get; set; }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            EditContext previousEditContext = CurrentEditContext;

            await base.SetParametersAsync(parameters);

            if (CurrentEditContext == null)
                throw new InvalidOperationException($"{nameof(BlazorFluentValidator)} requires a cascading " +
                                                    $"parameter of type {nameof(EditContext)}.");

            if (CurrentEditContext != previousEditContext)
                EditContextChanged();
        }

        private void EditContextChanged()
        {
            FluentValidator.InitializeEditContext(CurrentEditContext, ServiceProvider);
        }
    }
}
