namespace Firebend.AutoCrud.Core.Models.Entities;

public static class ModelStateResult
{
    public static ModelStateResult<TModel> Success<TModel>(TModel model) => new ModelStateResult<TModel> { WasSuccessful = true, Model = model };
}
