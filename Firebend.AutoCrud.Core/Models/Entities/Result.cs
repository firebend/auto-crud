using System.Collections.Generic;

namespace Firebend.AutoCrud.Core.Models.Entities;

public class Result
{
    public bool WasSuccessful { get; set; }
    public string Message { get; set; }

    public static Result Success() => new Result { WasSuccessful = true };

    public static Result<T> Success<T>(T model) => new Result<T> { WasSuccessful = true, Model = model };

    public static Result Error(string message) => new Result { WasSuccessful = false, Message = message };
}

public class Result<TModel> : Result
{
    public TModel Model { get; set; }

    public static new Result<TModel> Error(string message) => new Result<TModel> { WasSuccessful = false, Message = message };
}

public class ModelError
{
    public string PropertyPath { get; set; }
    public string Error { get; set; }
}

public static class ModelStateResult
{
    public static ModelStateResult<TModel> Success<TModel>(TModel model) => new ModelStateResult<TModel> { WasSuccessful = true, Model = model };
}

public class ModelStateResult<TModel> : Result<TModel>
{
    private readonly List<ModelError> _errors = new List<ModelError>();
    public IReadOnlyList<ModelError> Errors => _errors;

    public ModelStateResult<TModel> AddError(string path, string error)
    {
        WasSuccessful = false;
        _errors.Add(new ModelError { PropertyPath = path, Error = error });
        return this;
    }

    public static ModelStateResult<TModel> Error(string path, string error) => new ModelStateResult<TModel>().AddError(path, error);
}
