using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

namespace Firebend.AutoCrud.Web;

public partial class ControllerConfigurator<TBuilder, TKey, TEntity>
{
    /// <summary>
    /// Adds resource authorization to Create requests using the abstract create controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddCreateResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateResourceAuthorization(
        string policy = CreateAuthorizationRequirement.DefaultPolicy)
        => this.AddResourceAuthorization(CreateControllerType(),
            typeof(EntityCreateAuthorizationFilter<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType,
                CreateViewModelType), policy);

    /// <summary>
    /// Adds resource authorization to Create requests using the abstract create controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddCreateMultipleResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddCreateMultipleResourceAuthorization(
        string policy = CreateMultipleAuthorizationRequirement.DefaultPolicy)
        => this.AddResourceAuthorization(CreateMultipleControllerType(),
            typeof(EntityCreateAuthorizationFilter<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, CreateMultipleViewModelWrapperType),
            policy);

    /// <summary>
    /// Adds resource authorization to DELETE requests using the abstract delete controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddDeleteResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddDeleteResourceAuthorization(
        string policy = DeleteAuthorizationRequirement.DefaultPolicy)
        => this.AddResourceAuthorization(DeleteControllerType(),
            typeof(EntityDeleteAuthorizationFilter<TKey, TEntity>), policy);

    /// <summary>
    /// Adds resource authorization to GET requests using the abstract read controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddReadResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadResourceAuthorization(
        string policy = ReadAuthorizationRequirement.DefaultPolicy)
        => this.AddResourceAuthorization(ReadControllerType(),
            typeof(EntityReadAuthorizationFilter<TKey, TEntity>), policy);

    /// <summary>
    /// Adds resource authorization to GET `/all` requests using the abstract read all controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddReadAllResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddReadAllResourceAuthorization(
        string policy = ReadAllAuthorizationRequirement.DefaultPolicy)
        => this.AddResourceAuthorization(ReadAllControllerType(),
            typeof(EntityReadAuthorizationFilter<TKey, TEntity>), policy);

    /// <summary>
    /// Adds resource authorization to PUT requests using the abstract update controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddUpdateResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddUpdateResourceAuthorization(
        string policy = UpdateAuthorizationRequirement.DefaultPolicy) =>
        this.AddResourceAuthorization(UpdateControllerType(),
            typeof(EntityUpdateAuthorizationFilter<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType,
                UpdateViewModelType), policy);

    /// <summary>
    /// Adds resource authorization to all requests that modify an entity (Create, Update, and Delete) and use the abstract controllers
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddAlterResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddAlterResourceAuthorization(
        string policy)
    {
        AddCreateResourceAuthorization(policy);
        AddCreateMultipleResourceAuthorization(policy);
        AddDeleteResourceAuthorization(policy);
        AddUpdateResourceAuthorization(policy);

        return this;
    }

    /// <summary>
    /// Adds resource authorization to all requests that read an entity (Read, Read all, and Search) and use the abstract controllers
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddQueryResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddQueryResourceAuthorization(
        string policy)
    {
        AddReadResourceAuthorization(policy);
        AddReadAllResourceAuthorization(policy);

        return this;
    }

    /// <summary>
    /// Add all resource authorization to all controllers
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorization("Policy")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorization(string policy)
    {
        AddCreateResourceAuthorization(policy);
        AddCreateMultipleResourceAuthorization(policy);
        AddDeleteResourceAuthorization(policy);
        AddUpdateResourceAuthorization(policy);
        AddReadResourceAuthorization(policy);
        AddReadAllResourceAuthorization(policy);

        return this;
    }

    /// <summary>
    /// Add all resource authorization to all controllers
    /// </summary>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .AddResourceAuthorization()
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorization()
    {
        AddCreateResourceAuthorization();
        AddCreateMultipleResourceAuthorization();
        AddDeleteResourceAuthorization();
        AddUpdateResourceAuthorization();
        AddReadResourceAuthorization();
        AddReadAllResourceAuthorization();

        return this;
    }
}
