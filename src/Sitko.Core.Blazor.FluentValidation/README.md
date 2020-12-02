# Sitko.Core.Blazor.FluentValidation

Модуль для интеграции [FluentValidation](https://fluentvalidation.net/) в Blazor-формы. Рекурсивно валидирует модель формы используя валидаторы (`IValidator<TModel>`) из DI-контейнера.

## Установка

```xml
<PackageReference Include="Sitko.Core.Blazor.FluentValidation" Version="4.3.0"/>
```

## Использование

Подключить модуль к приложению

```c#
.AddModule<BlazorFluentValidationModule>()
```

Добавить в `_Imports.razor`:

```c#
@using BioEngine.BRC.Admin.Components.Validation
```

Добавить компонент в форму для включения валидации:

```c#
<BlazorFluentValidator/>
```

