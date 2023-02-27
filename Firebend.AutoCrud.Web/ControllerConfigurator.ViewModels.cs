using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Implementations.Patching;
using Firebend.AutoCrud.Web.Implementations.ViewModelMappers;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity, TVersion>
{
    public Type CreateViewModelType { get; private set; }
    public Type SearchViewModelType { get; private set; }
    public Type UpdateViewModelType { get; private set; }
    public Type UpdateViewModelBodyType { get; private set; }
    public Type ReadViewModelType { get; private set; }
    public Type CreateMultipleViewModelWrapperType { get; private set; }
    public Type CreateMultipleViewModelType { get; private set; }

    private void ViewModelGuard(string msg)
    {
        if (GetRegisteredControllers().Any())
        {
            throw new Exception($"Controllers are already registered. {msg}");
        }
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <param name="viewModelType">The type of the view model to use</param>
    /// <param name="viewModelMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel(typeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a Create view model before adding controllers");

        CreateViewModelType = viewModelType;

        var mapper = typeof(ICreateViewModelMapper<,,,>)
            .MakeGenericType(Builder.EntityType, Builder.EntityType, typeof(TVersion), viewModelType);

        Builder.WithRegistration(mapper, viewModelMapper, mapper);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
    /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel<ViewModel, ViewModelWrapper>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    {
        ViewModelGuard("Please register a Create view model before adding controllers");

        CreateViewModelType = typeof(TViewModel);

        Builder.WithRegistration<ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel>, TViewModelMapper>();

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <param name="from">A callback function that maps the view model to the entity class</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel<ViewModel>(viewModel => {
    ///              var e = new WeatherForecast();
    ///              viewModel?.Body?.CopyPropertiesTo(e);
    ///              return e;
    ///          }))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateViewModel<TViewModel>(
        Func<TViewModel, TEntity> from)
        where TViewModel : class
    {
        ViewModelGuard("Please register read view model before adding controllers.");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TVersion, TViewModel>(@from);

        CreateViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel>>(instance);

        return this;
    }

    // TODO TS: fix these docs
    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <param name="viewModelType">The type of the view model to use</param>
    /// <param name="viewModelMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel(typeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchViewModel<TSearchModel>(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a Create view model before adding controllers");

        SearchViewModelType = viewModelType;

        var mapper = typeof(ISearchViewModelMapper<,,,,>)
            .MakeGenericType(Builder.EntityType, Builder.EntityType, typeof(TVersion), viewModelType, typeof(TSearchModel));

        Builder.WithRegistration(mapper, viewModelMapper, mapper);

        return this;
    }

    // TODO TS: fix these docs
    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
    /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel<ViewModel, ViewModelWrapper>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchViewModel<TViewModel, TSearchModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : ISearchViewModelMapper<TKey, TEntity, TVersion, TViewModel, TSearchModel>
        where TSearchModel : class
    {
        ViewModelGuard("Please register a Create view model before adding controllers");

        SearchViewModelType = typeof(TViewModel);

        Builder.WithRegistration<ISearchViewModelMapper<TKey, TEntity, TVersion, TViewModel, TSearchModel>, TViewModelMapper>();

        return this;
    }

    // TODO TS: fix these docs
    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <param name="from">A callback function that maps the view model to the entity class</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel<ViewModel>(viewModel => {
    ///              var e = new WeatherForecast();
    ///              viewModel?.Body?.CopyPropertiesTo(e);
    ///              return e;
    ///          }))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchViewModel<TViewModel, TSearchModel>(
        Func<TViewModel, TSearchModel> from)
        where TViewModel : class
        where TSearchModel : class
    {
        ViewModelGuard("Please register read view model before adding controllers.");

        var instance = new FunctionSearchViewModelMapper<TKey, TEntity, TVersion, TViewModel, TSearchModel>(@from);

        Builder.WithRegistrationInstance<ISearchViewModelMapper<TKey, TEntity, TVersion, TViewModel, TSearchModel>>(instance);

        SearchViewModelType = typeof(TViewModel);

        return this;
    }

    // TODO TS: fix these docs
    /// <summary>
    /// Specify a custom view model to use for the entity Create endpoint
    /// </summary>
    /// <param name="from">A callback function that maps the view model to the entity class</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateViewModel<ViewModel>(viewModel => {
    ///              var e = new WeatherForecast();
    ///              viewModel?.Body?.CopyPropertiesTo(e);
    ///              return e;
    ///          }))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithSearchViewModel<TSearchModel>()
        where TSearchModel : class, new()
    {
        ViewModelGuard("Please register read view model before adding controllers.");

        var instance = new FunctionSearchViewModelMapper<TKey, TEntity, TVersion, TSearchModel, TSearchModel>(x =>
            x?.CopyPropertiesTo(new TSearchModel()));

        Builder.WithRegistrationInstance<ISearchViewModelMapper<TKey, TEntity, TVersion, TSearchModel, TSearchModel>>(instance);

        SearchViewModelType = typeof(TSearchModel);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Read endpoint
    /// </summary>
    /// <param name="viewModelType">The type of the view model to use</param>
    /// <param name="viewModelMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithReadViewModel(typeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithReadViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a read view model before adding controllers");

        ReadViewModelType = viewModelType;

        var mapper = typeof(IReadViewModelMapper<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), viewModelType);

        Builder.WithRegistration(mapper, viewModelMapper, mapper);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Read endpoint
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
    /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithReadViewModel<ViewModel, ViewModelWrapper>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithReadViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    {
        ViewModelGuard("Please register a read view model before adding controllers");

        ReadViewModelType = typeof(TViewModel);

        Builder.WithRegistration<IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel>, TViewModelMapper>();

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Read endpoint
    /// </summary>
    /// <param name="to">A callback function that maps the entity to the view model class</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithReadViewModel<ViewModel>(entity => new ViewModel(entity)))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithReadViewModel<TViewModel>(
        Func<TEntity, TViewModel> to)
        where TViewModel : class
    {
        ViewModelGuard("Please registered read view model before adding controllers");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TVersion, TViewModel>(to);

        ReadViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel>>(instance);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Update endpoint
    /// </summary>
    /// <param name="viewModelType">The type of the view model to use</param>
    /// <param name="viewModelMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithUpdateViewModel(typeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithUpdateViewModel(
        Type viewModelType,
        Type viewModelBodyType,
        Type viewModelMapper)
    {
        ViewModelGuard("Please register an Update view model before adding controllers");

        UpdateViewModelType = viewModelType;
        UpdateViewModelBodyType = viewModelBodyType;

        var mapper = typeof(IUpdateViewModelMapper<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), viewModelType);

        Builder.WithRegistration(mapper, viewModelMapper, mapper);

        var @interface = typeof(ICopyOnPatchPropertyAccessor<,,>).MakeGenericType(typeof(TEntity), typeof(TVersion), UpdateViewModelType);
        var @class = typeof(CopyOnPatchPropertyAccessor<,,>).MakeGenericType(typeof(TEntity), typeof(TVersion), UpdateViewModelType);
        var instance = Activator.CreateInstance(@class);

        Builder.WithRegistrationInstance(@interface, instance);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Update endpoint
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
    /// <typeparam name="TViewModelBody">The type of the body of the view model</typeparam>
    /// <typeparam name="TViewModelMapper">The type of the view model mapper to use</typeparam>
    /// <param name="copyOnPatchPropertyNames">A list of names of properties to copy from the original to the updated entity when patching.
    /// Use this for properties on the entity that are not mapped from the view model, but are modified separately.</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithUpdateViewModel<ViewModel, ViewModelWrapper>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithUpdateViewModel<TViewModel, TViewModelBody, TViewModelMapper>(
        List<string> copyOnPatchPropertyNames = null)
        where TViewModelBody : class
        where TViewModel : class
        where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    {
        ViewModelGuard("Please register a update view model before adding controllers");

        UpdateViewModelType = typeof(TViewModel);
        UpdateViewModelBodyType = typeof(TViewModelBody);

        Builder.WithRegistration<IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel>, TViewModelMapper>();
        Builder.WithRegistrationInstance<ICopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>>(
            new CopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>(copyOnPatchPropertyNames));

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Update endpoint
    /// </summary>
    /// <param name="from">A callback function that maps the view model to the entity class</typeparam>
    /// <param name="to">A callback function that maps the entity to the view model class</typeparam>
    /// <param name="copyOnPatchPropertyNames">A list of names of properties to copy from the original to the updated entity when patching.
    /// Use this for properties on the entity that are not mapped from the view model, but are modified separately.</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithUpdateViewModel<ViewModel>(viewModel => {
    ///              var e = new WeatherForecast();
    ///              viewModel?.Body?.CopyPropertiesTo(e);
    ///              return e;
    ///          }))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithUpdateViewModel<TViewModel, TViewModelBody>(
        Func<TViewModel, TEntity> from,
        Func<TEntity, TViewModel> to,
        List<string> copyOnPatchPropertyNames = null)
        where TViewModel : class
        where TViewModelBody : class
    {
        ViewModelGuard("Please register a update view model before adding controllers");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TVersion, TViewModel>(@from, to);

        UpdateViewModelType = typeof(TViewModel);
        UpdateViewModelBodyType = typeof(TViewModelBody);

        Builder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel>>(instance);
        Builder.WithRegistrationInstance<ICopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>>(
            new CopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>(copyOnPatchPropertyNames));

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create `/multiple` endpoint
    /// </summary>
    /// <param name="viewWrapper">The type of the view model wrapper to use</param>
    /// <param name="view">The type of the view model to use</param>
    /// <param name="viewMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateMultipleViewModel(type(ViewWrapper), typeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateMultipleViewModel(Type viewWrapper,
        Type view,
        Type viewMapper)
    {
        ViewModelGuard("Please register a Update view model before adding controllers");

        CreateMultipleViewModelType = view;
        CreateMultipleViewModelWrapperType = viewWrapper;

        var mapper = typeof(ICreateMultipleViewModelMapper<,,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, typeof(TVersion), viewWrapper, view);

        Builder.WithRegistration(mapper, viewMapper, mapper);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create `/multiple` endpoint
    /// </summary>
    /// <typeparam name="TViewWrapper">The type of the view model wrapper to use</typeparam>
    /// <typeparam name="TView">The type of the view model to use</typeparam>
    /// <typeparam name="TMapper">The type of the view model mapper to use</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateMultipleViewModel<ViewWrapper, ViewModel, ViewModelMapper>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateMultipleViewModel<TViewWrapper, TView, TMapper>()
        where TView : class
        where TViewWrapper : IMultipleEntityViewModel<TView>
        where TMapper : ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>
    {
        CreateMultipleViewModelType = typeof(TView);
        CreateMultipleViewModelWrapperType = typeof(TViewWrapper);

        Builder.WithRegistration<ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>, TMapper>();

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create `/multiple` endpoint
    /// </summary>
    /// <typeparam name="TViewWrapper">The type of the view model wrapper to use</typeparam>
    /// <typeparam name="TView">The type of the view model to use</typeparam>
    /// <param name="mapperFunc">A callback function that maps a view model to the entity class</typeparam>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithCreateMultipleViewModel<ViewWrapper, ViewModel>(viewModel => {
    ///              var e = new WeatherForecast();
    ///              viewModel?.Body?.CopyPropertiesTo(e);
    ///              return e;
    ///          }))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithCreateMultipleViewModel<TViewWrapper, TView>(
        Func<TViewWrapper, TView, TEntity> mapperFunc)
        where TView : class
        where TViewWrapper : IMultipleEntityViewModel<TView>
    {
        CreateMultipleViewModelType = typeof(TView);
        CreateMultipleViewModelWrapperType = typeof(TViewWrapper);

        var instance = new FunctionCreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>(mapperFunc);

        Builder.WithRegistrationInstance<ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>>(instance);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create, Update, and Read endpoints
    /// </summary>
    /// <param name="viewModelType">The type of the view model to use</param>
    /// <param name="viewModelMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithViewModel(ypeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a view model before adding controllers");

        WithCreateViewModel(viewModelType, viewModelMapper);
        WithUpdateViewModel(viewModelType, viewModelType, viewModelMapper);
        WithReadViewModel(viewModelType, viewModelMapper);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create, Update, and Read endpoints
    /// </summary>
    /// <param name="viewModelType">The type of the view model to use</param>
    /// <param name="viewModelMapper">The type of the view model mapper to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithViewModel(ypeof(ViewModel), typeof(ViewModelMapper))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class, new()
        where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel>,
        ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel>,
        IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    {
        ViewModelGuard("Please register a view model before adding controllers");

        WithCreateViewModel<TViewModel, TViewModelMapper>();
        WithUpdateViewModel<TViewModel, TViewModel, TViewModelMapper>();
        WithReadViewModel<TViewModel, TViewModelMapper>();

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Create, Update, and Read endpoints
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to use</typeparam>
    /// <param name="to">A callback function that maps the entity to the view model class</param>
    /// <param name="from">A callback function that maps the view model to the entity class</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithViewModel<ViewModel>(
    ///             entity => new ViewModel(entity)
    ///             viewModel => new WeatherForecast(viewModel)
    ///          ))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithViewModel<TViewModel>(
        Func<TEntity, TViewModel> to,
        Func<TViewModel, TEntity> from)
        where TViewModel : class
    {
        ViewModelGuard("Please register view model before adding controllers");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TVersion, TViewModel>(@from, to);

        CreateViewModelType = typeof(TViewModel);
        UpdateViewModelType = typeof(TViewModel);
        ReadViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TVersion, TViewModel>>(instance);
        Builder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel>>(instance);
        Builder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel>>(instance);

        return this;
    }
}
