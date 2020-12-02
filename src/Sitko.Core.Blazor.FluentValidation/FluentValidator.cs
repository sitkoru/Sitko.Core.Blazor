using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Sitko.Core.Blazor.FluentValidation
{
    public class FluentValidator
    {
        public void InitializeEditContext(
            EditContext editContext,
            IServiceProvider serviceProvider)
        {
            if (editContext == null)
                throw new ArgumentNullException(nameof(editContext));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var messages = new ValidationMessageStore(editContext);
            editContext.OnValidationRequested +=
                (sender, eventArgs) =>
                {
                    _ = ValidateModel((EditContext)sender, messages, serviceProvider);
                };

            editContext.OnFieldChanged +=
                (sender, eventArgs) =>
                {
                    _ = ValidateField(editContext, messages, eventArgs.FieldIdentifier, serviceProvider);
                };
        }

        private async Task ValidateModel(
            EditContext editContext,
            ValidationMessageStore messages,
            IServiceProvider serviceProvider)
        {
            if (editContext == null)
                throw new ArgumentNullException(nameof(editContext));
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (editContext.Model == null)
                throw new NullReferenceException($"{nameof(editContext)}.{nameof(editContext.Model)}");


            var validationResults = new Dictionary<object, List<ValidationResult>>();
            await ValidateModelInternal(serviceProvider, editContext.Model, validationResults);

            messages.Clear();
            editContext.NotifyValidationStateChanged();

            foreach (var (model, modelValidationResults) in validationResults)
            {
                IEnumerable<ValidationFailure> validationFailures = modelValidationResults.SelectMany(x => x.Errors);
                foreach (var validationError in validationFailures)
                    messages.Add(new FieldIdentifier(model, validationError.PropertyName),
                        validationError.ErrorMessage);
            }


            editContext.NotifyValidationStateChanged();
        }

        private static async Task ValidateModelInternal(IServiceProvider serviceProvider, object model,
            Dictionary<object, List<ValidationResult>> validationResults)
        {
            IEnumerable<IValidator> validators = GetValidatorsForObject(model, serviceProvider);

            var validationContext = new ValidationContext<object>(model);
            validationResults[model] = new List<ValidationResult>();
            foreach (IValidator validator in validators)
            {
                var validationResult = await validator.ValidateAsync(validationContext);
                validationResults[model].Add(validationResult);
            }

            foreach (var property in model.GetType().GetProperties())
            {
                if (property.PropertyType.IsClass && !property.PropertyType.IsAbstract)
                {
                    if (typeof(IEnumerable<>).IsAssignableFrom(property.PropertyType))
                    {
                        if (property.GetValue(model) is IEnumerable propertyValue)
                        {
                            foreach (var item in propertyValue)
                            {
                                await ValidateModelInternal(serviceProvider, item, validationResults);
                            }
                        }
                    }
                    else
                    {
                        var propertyValue = property.GetValue(model);
                        if (propertyValue != null)
                        {
                            await ValidateModelInternal(serviceProvider, propertyValue, validationResults);
                        }
                    }
                }
            }
        }

        private async Task ValidateField(
            EditContext editContext,
            ValidationMessageStore messages,
            FieldIdentifier fieldIdentifier,
            IServiceProvider serviceProvider)
        {
            if (editContext == null)
                throw new ArgumentNullException(nameof(editContext));
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (editContext.Model == null)
                throw new NullReferenceException($"{nameof(editContext)}.{nameof(editContext.Model)}");

            var propertiesToValidate = new string[] {fieldIdentifier.FieldName};
            var fluentValidationContext =
                new ValidationContext<object>(
                    instanceToValidate: fieldIdentifier.Model,
                    propertyChain: new PropertyChain(),
                    validatorSelector: new MemberNameValidatorSelector(propertiesToValidate)
                );


            IEnumerable<IValidator> validators = GetValidatorsForObject(fieldIdentifier.Model, serviceProvider);
            var validationResults = new List<ValidationResult>();

            foreach (IValidator validator in validators)
            {
                var validationResult = await validator.ValidateAsync(fluentValidationContext);
                validationResults.Add(validationResult);
            }

            IEnumerable<string> errorMessages =
                validationResults
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .Distinct();

            messages.Clear(fieldIdentifier);
            editContext.NotifyValidationStateChanged();

            foreach (string errorMessage in errorMessages)
                messages.Add(fieldIdentifier, errorMessage);

            editContext.NotifyValidationStateChanged();
        }

        private static IEnumerable<IValidator> GetValidatorsForObject(
            object model,
            IServiceProvider serviceProvider)
        {
            Type interfaceValidatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
            return serviceProvider.GetServices(interfaceValidatorType).Cast<IValidator>();
        }
    }
}