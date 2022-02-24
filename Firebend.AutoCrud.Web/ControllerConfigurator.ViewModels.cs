using System;
using System.Linq;
using Firebend.AutoCrud.Web.Implementations.ViewModelMappers;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity>
{
    public Type CreateViewModelType { get; private set; }
    public Type UpdateViewModelType { get; private set; }
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a Create view model before adding controllers");

        CreateViewModelType = viewModelType;

        var mapper = typeof(ICreateViewModelMapper<,,>)
            .MakeGenericType(Builder.EntityType, Builder.EntityType, viewModelType);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : ICreateViewModelMapper<TKey, TEntity, TViewModel>
    {
        ViewModelGuard("Please register a Create view model before adding controllers");

        CreateViewModelType = typeof(TViewModel);

        Builder.WithRegistration<ICreateViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateViewModel<TViewModel>(
        Func<TViewModel, TEntity> from)
        where TViewModel : class
    {
        ViewModelGuard("Please register read view model before adding controllers.");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(@from);

        CreateViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TViewModel>>(instance);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a read view model before adding controllers");

        ReadViewModelType = viewModelType;

        var mapper = typeof(IReadViewModelMapper<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, viewModelType);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : IReadViewModelMapper<TKey, TEntity, TViewModel>
    {
        ViewModelGuard("Please register a read view model before adding controllers");

        ReadViewModelType = typeof(TViewModel);

        Builder.WithRegistration<IReadViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithReadViewModel<TViewModel>(
        Func<TEntity, TViewModel> to)
        where TViewModel : class
    {
        ViewModelGuard("Please registered read view model before adding controllers");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(to);

        ReadViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TViewModel>>(instance);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a Update view model before adding controllers");

        UpdateViewModelType = viewModelType;

        var mapper = typeof(IUpdateViewModelMapper<,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, viewModelType);

        Builder.WithRegistration(mapper, viewModelMapper, mapper);

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Update endpoint
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
    ///          .WithUpdateViewModel<ViewModel, ViewModelWrapper>()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TViewModel>
    {
        ViewModelGuard("Please register a update view model before adding controllers");

        UpdateViewModelType = typeof(TViewModel);

        Builder.WithRegistration<IUpdateViewModelMapper<TKey, TEntity, TViewModel>, TViewModelMapper>();

        return this;
    }

    /// <summary>
    /// Specify a custom view model to use for the entity Update endpoint
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
    ///          .WithUpdateViewModel<ViewModel>(viewModel => {
    ///              var e = new WeatherForecast();
    ///              viewModel?.Body?.CopyPropertiesTo(e);
    ///              return e;
    ///          }))
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithUpdateViewModel<TViewModel>(
        Func<TViewModel, TEntity> from)
        where TViewModel : class
    {
        ViewModelGuard("Please register a update view model before adding controllers");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(@from);

        UpdateViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TViewModel>>(instance);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleViewModel(Type viewWrapper,
        Type view,
        Type viewMapper)
    {
        ViewModelGuard("Please register a Update view model before adding controllers");

        CreateMultipleViewModelType = view;
        CreateMultipleViewModelWrapperType = viewWrapper;

        var mapper = typeof(ICreateMultipleViewModelMapper<,,,>)
            .MakeGenericType(Builder.EntityKeyType, Builder.EntityType, viewWrapper, view);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleViewModel<TViewWrapper, TView, TMapper>()
        where TView : class
        where TViewWrapper : IMultipleEntityViewModel<TView>
        where TMapper : ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>
    {
        CreateMultipleViewModelType = typeof(TView);
        CreateMultipleViewModelWrapperType = typeof(TViewWrapper);

        Builder.WithRegistration<ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>, TMapper>();

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithCreateMultipleViewModel<TViewWrapper, TView>(
        Func<TViewWrapper, TView, TEntity> mapperFunc)
        where TView : class
        where TViewWrapper : IMultipleEntityViewModel<TView>
    {
        CreateMultipleViewModelType = typeof(TView);
        CreateMultipleViewModelWrapperType = typeof(TViewWrapper);

        var instance = new FunctionCreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>(mapperFunc);

        Builder.WithRegistrationInstance<ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>>(instance);

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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel(Type viewModelType, Type viewModelMapper)
    {
        ViewModelGuard("Please register a view model before adding controllers");

        WithCreateViewModel(viewModelType, viewModelMapper);
        WithUpdateViewModel(viewModelType, viewModelMapper);
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel<TViewModel, TViewModelMapper>()
        where TViewModel : class
        where TViewModelMapper : IUpdateViewModelMapper<TKey, TEntity, TViewModel>,
        ICreateViewModelMapper<TKey, TEntity, TViewModel>,
        IReadViewModelMapper<TKey, TEntity, TViewModel>
    {
        ViewModelGuard("Please register a view model before adding controllers");

        WithCreateViewModel<TViewModel, TViewModelMapper>();
        WithUpdateViewModel<TViewModel, TViewModelMapper>();
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
    public ControllerConfigurator<TBuilder, TKey, TEntity> WithViewModel<TViewModel>(
        Func<TEntity, TViewModel> to,
        Func<TViewModel, TEntity> from)
        where TViewModel : class
    {
        ViewModelGuard("Please register view model before adding controllers");

        var instance = new FunctionViewModelMapper<TKey, TEntity, TViewModel>(@from, to);

        CreateViewModelType = typeof(TViewModel);
        UpdateViewModelType = typeof(TViewModel);
        ReadViewModelType = typeof(TViewModel);

        Builder.WithRegistrationInstance<ICreateViewModelMapper<TKey, TEntity, TViewModel>>(instance);
        Builder.WithRegistrationInstance<IUpdateViewModelMapper<TKey, TEntity, TViewModel>>(instance);
        Builder.WithRegistrationInstance<IReadViewModelMapper<TKey, TEntity, TViewModel>>(instance);

        return this;
    }
}
