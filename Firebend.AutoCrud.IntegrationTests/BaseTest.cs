using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using CsvHelper;
using CsvHelper.Configuration;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.IntegrationTests.Models;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Firebend.AutoCrud.IntegrationTests;

public abstract class BaseTest<
    TKey,
    TCreateRequest,
    TUpdateRequest,
    TReadResponse,
    TExport>
    where TKey : struct
    where TCreateRequest : class, IEntityViewModelBase
    where TUpdateRequest : class, IEntityViewModelBase
    where TReadResponse : class, IEntityViewModelBase, IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TExport : class, IEntityViewModelExport
{
    protected abstract string Url { get; }

    protected List<IFlurlResponse> Responses { get; } = [];

    private TKey CreatedKey { get; set; }

    protected abstract Task<TCreateRequest> GenerateCreateRequestAsync();

    protected abstract Task<TUpdateRequest> GenerateUpdateRequestAsync(TCreateRequest createRequest);

    protected abstract Task<JsonPatchDocument> GeneratePatchAsync();

    private static void Log(string message) => Trace.WriteLine(message);

    private void SaveResponse(IFlurlResponse response) => Responses.Add(response);

    private static async Task<T> RetryAsync<T>(Func<Task<T>> func, int times = 10)
    {
        var i = 0;

        while (i < times)
        {
            try
            {
                return await func();
            }
            catch
            {
                i++;
                Log($"-------------- Attempt #{i} ----------------");
                await Task.Delay(TimeSpan.FromMilliseconds(500 * i));

                if (i >= times)
                {
                    throw;
                }
            }
        }

        throw new Exception();
    }

    public async Task<TReadResponse> PostAsync(TCreateRequest model)
    {
        var response = await Url.WithAuth()
            .PostJsonAsync(model);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(201);

        var responseModel = await response.GetJsonAsync<TReadResponse>();

        responseModel.Should().NotBeNull();
        responseModel.Id.Should().NotBeNull();
        responseModel.Id.ToString().Should().NotBeEmpty();

        PostAssertions(response, responseModel);

        SaveResponse(response);

        CreatedKey = responseModel.Id;

        return responseModel;
    }

    private async Task PostUnauthorizedAsync(TCreateRequest model)
    {
        try
        {
            var response = await Url.WithAuth()
                .PostJsonAsync(model);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PostAssertions(IFlurlResponse response, TReadResponse model) { }

    private async Task<TReadResponse> GetAsync(TKey key)
    {
        var response = await $"{Url}/{key}".WithAuth().GetAsync();
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<TReadResponse>();
        responseModel.Should().NotBeNull();

        GetAssertions(response, responseModel);

        SaveResponse(response);

        return responseModel;
    }

    private async Task GetUnauthorizedAsync(TKey key)
    {
        try
        {
            var response = await $"{Url}/{key}".WithAuth().GetAsync();
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void GetAssertions(IFlurlResponse response, TReadResponse model) { }

    private async Task PutAsync(TKey key, TUpdateRequest entity)
    {
        entity.Should().NotBeNull();
        key.Should().NotBeNull();
        key.Should().NotBeSameAs(default(TKey));

        var response = await $"{Url}/{key}".WithAuth().PutJsonAsync(entity);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<TReadResponse>();
        responseModel.Should().NotBeNull();

        PutAssertions(response, responseModel);

        SaveResponse(response);
    }

    private async Task PutUnauthorizedAsync(TKey key, TUpdateRequest entity)
    {
        entity.Should().NotBeNull();
        key.Should().NotBeNull();
        key.Should().NotBeSameAs(default(TKey));

        try
        {
            var response = await $"{Url}/{key}".WithAuth().PutJsonAsync(entity);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PutAssertions(IFlurlResponse response, TReadResponse model) { }

    private async Task PageAsync()
    {
        //act
        var response = await Url.WithAuth()
            .SetQueryParam("pagenumber", 1)
            .SetQueryParam("pageSize", 10)
            .SetQueryParam("isDeleted", "false")
            .SetQueryParam("orderBy", "createdDate:desc")
            .SetQueryParam("CreatedStartDate", "1968-08-09")
            .SetQueryParam("CreatedEndDate", "2068-08-09")
            .SetQueryParam("ModifiedStartDate", "1968-08-09")
            .SetQueryParam("ModifiedEndDate", "2068-08-09")
            .SetQueryParam("doCount", true)
            .GetAsync();

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<EntityPagedResponse<TReadResponse>>();

        responseModel.Should().NotBeNull();
        responseModel.Data.Should().NotBeNullOrEmpty();
        responseModel.TotalRecords.Should().HaveValue();
        responseModel.TotalRecords.Should().BeGreaterOrEqualTo(1);

        PageAssertions(response, responseModel);

        SaveResponse(response);
    }

    private async Task PageUnauthorizedAsync(string email)
    {
        //act
        var response = await Url.WithAuth()
            .SetQueryParam("pagenumber", 1)
            .SetQueryParam("pageSize", 10)
            .SetQueryParam("doCount", true)
            .GetAsync();

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<EntityPagedResponse<TReadResponse>>();

        responseModel.Should().NotBeNull();
        foreach (var person in responseModel.Data)
        {
            if (person.DataAuth is not null && person.DataAuth.UserEmails.Length != 0)
            {
                person.DataAuth.UserEmails.Should().Contain(email);
            }
        }
    }

    protected virtual void PageAssertions(IFlurlResponse response,
        EntityPagedResponse<TReadResponse> responseModel)
    {
    }

    protected async Task SearchAsync(string search,
        Func<IFlurlRequest, IFlurlRequest> configureRequest = null)
    {
        async Task<(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)> DoSearch()
        {
            var request = Url.WithAuth()
                .SetQueryParam("pagenumber", 1)
                .SetQueryParam("pageSize", 10)
                .SetQueryParam("search", search);

            request = configureRequest?.Invoke(request) ?? request;

            var searchResponse = await request.GetAsync();

            searchResponse.Should().NotBeNull();
            searchResponse.StatusCode.Should().Be(200);

            var searchResponseModel = await searchResponse.GetJsonAsync<EntityPagedResponse<TReadResponse>>();
            searchResponseModel.Should().NotBeNull();
            searchResponseModel.Data.Should().NotBeNullOrEmpty("search should contain {0}", search);
            searchResponseModel.TotalRecords.Should().BeGreaterOrEqualTo(1);

            return (searchResponse, searchResponseModel);
        }

        search.Should().NotBeNullOrEmpty();

        var (response, responseModel) = await RetryAsync(DoSearch);

        SearchAssertions(response, responseModel);

        SaveResponse(response);
    }

    private async Task SearchUnauthorizedAsync(string search, string email)
    {
        async Task<(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)> DoSearch()
        {
            var searchResponse = await Url.WithAuth()
                .SetQueryParam("pagenumber", 1)
                .SetQueryParam("pageSize", 10)
                .SetQueryParam("search", search)
                .GetAsync();

            searchResponse.Should().NotBeNull();
            searchResponse.StatusCode.Should().Be(200);

            var searchResponseModel = await searchResponse.GetJsonAsync<EntityPagedResponse<TReadResponse>>();
            searchResponseModel.Should().NotBeNull();
            foreach (var person in searchResponseModel.Data)
            {
                if (person.DataAuth is not null && person.DataAuth.UserEmails.Length != 0)
                {
                    person.DataAuth.UserEmails.Should().Contain(email);
                }
            }

            return (searchResponse, searchResponseModel);
        }

        search.Should().NotBeNullOrEmpty();

        await RetryAsync(DoSearch);
    }

    protected virtual void SearchAssertions(IFlurlResponse response,
        EntityPagedResponse<TReadResponse> responseModel)
    {
    }

    private async Task DeleteAsync(TKey id)
    {
        id.Should().NotBeNull();
        id.Should().NotBe(default(TKey));

        var deleteResponse = await $"{Url}/{id}".WithAuth().DeleteAsync();

        deleteResponse.Should().NotBeNull();
        deleteResponse.StatusCode.Should().Be(200);

        var deleteResponseModel = await deleteResponse.GetJsonAsync<TReadResponse>();
        deleteResponseModel.Should().NotBeNull();

        DeleteAssertions(deleteResponse, deleteResponseModel);

        SaveResponse(deleteResponse);
    }

    private async Task DeleteUnauthorizedAsync(TKey id)
    {
        id.Should().NotBeNull();
        id.Should().NotBe(default(TKey));

        try
        {
            var response = await $"{Url}/{id}".WithAuth().DeleteAsync();
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void DeleteAssertions(IFlurlResponse response, TReadResponse responseModel) { }


    private async Task UndoDeleteAsync(TKey id)
    {
        id.Should().NotBeNull();
        id.Should().NotBe(default(TKey));

        var deleteResponse = await $"{Url}/{id}/undo-delete".WithAuth().PostAsync();

        deleteResponse.Should().NotBeNull();
        deleteResponse.StatusCode.Should().Be(200);

        var deleteResponseModel = await deleteResponse.GetJsonAsync<TReadResponse>();
        deleteResponseModel.Should().NotBeNull();

        if (deleteResponseModel is IActiveEntity activeEntity)
        {
            activeEntity.IsDeleted.Should().BeFalse();
        }

        UndoDeleteAssertions(deleteResponse, deleteResponseModel);

        SaveResponse(deleteResponse);
    }

    private async Task UndoDeleteUnauthorizedAsync(TKey id)
    {
        id.Should().NotBeNull();
        id.Should().NotBe(default(TKey));

        try
        {
            var response = await $"{Url}/{id}/undo-delete".WithAuth().PostAsync();
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void UndoDeleteAssertions(IFlurlResponse response, TReadResponse responseModel) { }

    private async Task<TReadResponse> PatchAsync(TKey id, JsonPatchDocument patchDocument)
    {
        id.Should().NotBeNull();
        id.Should().NotBe(default(TKey));

        var patchJsonResponse = await $"{Url}/{id}".WithAuth().PatchJsonAsync(patchDocument);
        patchJsonResponse.Should().NotBeNull();
        patchJsonResponse.StatusCode.Should().Be(200);

        var patchJsonResponseModel = await patchJsonResponse.GetJsonAsync<TReadResponse>();
        patchJsonResponseModel.Should().NotBeNull();

        PatchAssertions(patchJsonResponse, patchJsonResponseModel);

        SaveResponse(patchJsonResponse);

        return patchJsonResponseModel;
    }


    private async Task PatchUnauthorizedAsync(TKey id, JsonPatchDocument patchDocument)
    {
        id.Should().NotBeNull();
        id.Should().NotBe(default(TKey));

        try
        {
            var response = await $"{Url}/{id}".WithAuth().PatchJsonAsync(patchDocument);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PatchAssertions(IFlurlResponse response, TReadResponse responseModel) { }

    private async Task ExportToCsvAsync()
    {
        var response = await $"{Url}/export/csv".WithAuth()
            .SetQueryParam("pagenumber", 1.ToString())
            .SetQueryParam("pageSize", 10.ToString())
            .SetQueryParam("doCount", true.ToString())
            .SetQueryParam("fileName", "temp")
            .GetAsync();

        //assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseString = await response.GetStringAsync();
        responseString.Should().NotBeNullOrWhiteSpace();

        using (var stringReader = new StringReader(responseString))
        {
            using (var csv = new CsvReader(stringReader,
                       new CsvConfiguration(CultureInfo.InvariantCulture)
                       {
                           HeaderValidated = null,
                           MissingFieldFound = null
                       }))
            {
                var records = csv.GetRecords<TExport>().ToList();
                records.Should().NotBeEmpty();
                records.FirstOrDefault().Should().NotBeNull();

                CsvRecordsAssertions(records);
            }
        }

        CsvResponseAssertions(response);

        SaveResponse(response);
    }

    private async Task ExportToCsvUnauthorizedAsync(TKey unauthorizedId)
    {
        var response = await $"{Url}/export/csv".WithAuth()
            .SetQueryParam("pagenumber", 1.ToString())
            .SetQueryParam("pageSize", 10.ToString())
            .SetQueryParam("doCount", true.ToString())
            .SetQueryParam("fileName", "temp")
            .GetAsync();

        //assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseString = await response.GetStringAsync();
        responseString.Should().NotContain(unauthorizedId.ToString());
    }

    protected virtual void CsvResponseAssertions(IFlurlResponse response) { }

    protected virtual void CsvRecordsAssertions(List<TExport> records) { }

    private async Task<EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>>>
        GetChangeTrackingAsync(
            TKey key, int operationCount)
    {
        async Task<(IFlurlResponse response, EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>>
            responseModel)> DoSearch()
        {
            var httpResponse = await $"{Url}/{key}/changes".WithAuth()
                .SetQueryParam("pagenumber", 1)
                .SetQueryParam("pageSize", 10)
                .SetQueryParam("doCount", true)
                .GetAsync();

            httpResponse.Should().NotBeNull();
            httpResponse.StatusCode.Should().Be(200);

            var httpResponseModel = await httpResponse
                .GetJsonAsync<EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>>>();

            httpResponseModel.Should().NotBeNull();
            httpResponseModel.Data.Should().NotBeNullOrEmpty();
            httpResponseModel.Data.Should().HaveCountGreaterOrEqualTo(operationCount);
            httpResponseModel.Data.Select(x => x.UserEmail).Should().NotContainNulls();
            httpResponseModel.Data.Select(x => x.Source).Should().NotContainNulls();

            foreach (var data in httpResponseModel
                         .Data
                         .Where(x => x.Action.EqualsIgnoreCaseAndWhitespace("Update")))
            {
                data.Changes.Should().NotBeNullOrEmpty();
            }

            return (httpResponse, httpResponseModel);
        }

        var (response, responseModel) = await RetryAsync(DoSearch, 5);

        ChangeTrackingAssertions(response, responseModel);

        SaveResponse(response);

        return responseModel;
    }

    private async Task GetChangeTrackingUnauthorizedAsync(
        TKey key)
    {
        try
        {
            var response = await $"{Url}/{key}/changes".WithAuth()
                .SetQueryParam("pagenumber", 1)
                .SetQueryParam("pageSize", 10)
                .SetQueryParam("doCount", true)
                .GetAsync();
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void ChangeTrackingAssertions(IFlurlResponse response,
        EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>> responseModel)
    {
    }

    private async Task<MultiResult<TReadResponse>> PostMultipleAsync(IEnumerable<TCreateRequest> models)
    {
        var response = await $"{Url}/multiple".WithAuth().PostJsonAsync(models);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<MultiResult<TReadResponse>>();
        responseModel.Created.Should().NotBeNullOrEmpty();
        responseModel.Created.Count().Should().Be(models.Count());
        (responseModel.Errors?.Count() ?? 0).Should().BeLessOrEqualTo(0);

        foreach (var entity in responseModel.Created)
        {
            entity.Id.Should().NotBeNull();
            entity.Id.ToString().Should().NotBeEmpty();
        }

        PostMultipleAssertions(response, responseModel);

        SaveResponse(response);

        return responseModel;
    }

    private async Task PostMultipleUnauthorizedAsync(IEnumerable<TCreateRequest> models)
    {
        try
        {
            var response = await $"{Url}/multiple".WithAuth().PostJsonAsync(models);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PostMultipleAssertions(IFlurlResponse response,
        MultiResult<TReadResponse> responseModel)
    {
    }

    private async Task<CustomFieldViewModel> PostCustomFieldsAsync(TKey entityId)
    {
        var faked = CustomFieldFaker.Faker.Generate();
        var response = await $"{Url}/{entityId}/custom-fields".WithAuth().PostJsonAsync(faked);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<CustomFieldViewModel>();
        responseModel.Should().NotBeNull();
        responseModel.Id.Should().NotBeEmpty();
        responseModel.Key.Should().NotBeNullOrWhiteSpace();
        responseModel.Value.Should().NotBeNullOrWhiteSpace();

        PostCustomFieldsAssertions(response, responseModel);

        var entity = await GetAsync(entityId);
        var customField = entity.CustomFields.First(x => x.Id == responseModel.Id);
        customField.Value.Should().BeEquivalentTo(faked.Value);
        customField.Key.Should().BeEquivalentTo(faked.Key);

        SaveResponse(response);

        return responseModel;
    }

    private async Task PostCustomFieldsUnauthorizedAsync(TKey entityId)
    {
        try
        {
            var faked = CustomFieldFaker.Faker.Generate();
            var response = await $"{Url}/{entityId}/custom-fields".WithAuth().PostJsonAsync(faked);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PostCustomFieldsAssertions(IFlurlResponse response,
        CustomFieldViewModel responseModel)
    {
    }

    private async Task<CustomFieldViewModel> PutCustomFieldsAsync(TKey entityId, Guid id)
    {
        var faked = CustomFieldFaker.Faker.Generate();
        var response = await $"{Url}/{entityId}/custom-fields/{id}".WithAuth().PutJsonAsync(faked);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<CustomFieldViewModel>();

        responseModel.Should().NotBeNull();
        responseModel.Id.Should().NotBeEmpty();
        responseModel.Key.Should().NotBeNullOrWhiteSpace();
        responseModel.Value.Should().NotBeNullOrWhiteSpace();

        PutCustomFieldsAssertions(response, responseModel);

        var entity = await GetAsync(entityId);
        var customField = entity.CustomFields.First(x => x.Id == id);
        customField.Value.Should().BeEquivalentTo(faked.Value);
        customField.Key.Should().BeEquivalentTo(faked.Key);

        SaveResponse(response);
        return responseModel;
    }

    private async Task PutCustomFieldsUnauthorizedAsync(TKey entityId, Guid id)
    {
        try
        {
            var faked = CustomFieldFaker.Faker.Generate();
            var response = await $"{Url}/{entityId}/custom-fields/{id}".WithAuth().PutJsonAsync(faked);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PutCustomFieldsAssertions(IFlurlResponse response,
        CustomFieldViewModel responseModel)
    {
    }

    private async Task<CustomFieldViewModel> PatchCustomFieldsAsync(TKey entityId, Guid id)
    {
        var color = new Faker().Commerce.Color();
        var patch = PatchFaker.MakeReplacePatch<CustomFieldViewModel, string>(x => x.Value, color);

        var response = await $"{Url}/{entityId}/custom-fields/{id}".WithAuth().PatchJsonAsync(patch);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<CustomFieldViewModel>();
        responseModel.Should().NotBeNull();
        responseModel.Id.Should().NotBeEmpty();
        responseModel.Key.Should().NotBeNullOrWhiteSpace();
        responseModel.Value.Should().NotBeNullOrWhiteSpace();
        responseModel.Value.Should().BeEquivalentTo(color);

        var entity = await GetAsync(entityId);
        var customField = entity.CustomFields.First(x => x.Id == id);
        customField.Value.Should().BeEquivalentTo(color);


        PatchCustomFieldsAssertions(response, responseModel);

        SaveResponse(response);

        return responseModel;
    }

    private async Task PatchCustomFieldsUnauthorizedAsync(TKey entityId, Guid id)
    {
        try
        {
            var color = new Faker().Commerce.Color();
            var patch = PatchFaker.MakeReplacePatch<CustomFieldViewModel, string>(x => x.Value, color);

            var response = await $"{Url}/{entityId}/custom-fields/{id}".WithAuth().PatchJsonAsync(patch);
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void PatchCustomFieldsAssertions(IFlurlResponse response,
        CustomFieldViewModel responseModel)
    {
    }

    private async Task SearchCustomFieldsAsync(string key)
    {
        async Task<(IFlurlResponse response, EntityPagedResponse<CustomFieldViewModel> responseModel)>
            DoSearch()
        {
            var httpResponse = await $"{Url}/custom-fields/".WithAuth()
                .SetQueryParam("pageNumber", "1")
                .SetQueryParam("pageSize", "10")
                .SetQueryParam("key", key)
                .SetQueryParam("orderBy", "createdDate:desc")
                .GetAsync();

            httpResponse.Should().NotBeNull();
            httpResponse.StatusCode.Should().Be(200);

            var httpResponseModel =
                await httpResponse.GetJsonAsync<EntityPagedResponse<CustomFieldViewModel>>();

            httpResponseModel.Should().NotBeNull();
            httpResponseModel.Data.Should().NotBeNullOrEmpty();

            var customField = httpResponseModel.Data.First();
            customField.Should().NotBeNull();
            customField.Id.Should().NotBeEmpty();
            customField.Key.Should().NotBeNullOrWhiteSpace();
            customField.Value.Should().NotBeNullOrWhiteSpace();

            return (httpResponse, httpResponseModel);
        }

        var (searchResponse, searchResponseModel) = await RetryAsync(DoSearch);

        SearchCustomFieldsAssertions(searchResponse, searchResponseModel);

        SaveResponse(searchResponse);
    }

    private async Task SearchCustomFieldsUnauthorizedAsync(string key, Guid shouldNotReturn)
    {
        async Task<(IFlurlResponse response, EntityPagedResponse<CustomFieldViewModel> responseModel)>
            DoSearch()
        {
            var httpResponse = await $"{Url}/custom-fields/".WithAuth()
                .SetQueryParam("pageNumber", "1")
                .SetQueryParam("pageSize", "10")
                .SetQueryParam("key", key)
                .SetQueryParam("orderBy", "createdDate:desc")
                .GetAsync();

            httpResponse.Should().NotBeNull();
            httpResponse.StatusCode.Should().Be(200);

            var httpResponseModel =
                await httpResponse.GetJsonAsync<EntityPagedResponse<CustomFieldViewModel>>();

            httpResponseModel.Should().NotBeNull();
            httpResponseModel.Data.Should().NotContain(x => x.Id == shouldNotReturn);

            return (httpResponse, httpResponseModel);
        }

        await RetryAsync(DoSearch);
    }

    protected virtual void SearchCustomFieldsAssertions(IFlurlResponse response,
        EntityPagedResponse<CustomFieldViewModel> responseModel)
    {
    }

    private async Task DeleteCustomFieldsAsync(TKey entityId, Guid id)
    {
        var response = await $"{Url}/{entityId}/custom-fields/{id}".WithAuth().DeleteAsync();

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseModel = await response.GetJsonAsync<CustomFieldViewModel>();
        responseModel.Should().NotBeNull();
        responseModel.Id.Should().NotBeEmpty();
        responseModel.Key.Should().NotBeNullOrWhiteSpace();
        responseModel.Value.Should().NotBeNullOrWhiteSpace();

        var entity = await GetAsync(entityId);
        entity.CustomFields.Should().NotContain(x => x.Id == id);

        DeleteCustomFieldsAssertions(response, responseModel);

        SaveResponse(response);
    }

    private async Task DeleteCustomFieldsUnauthorizedAsync(TKey entityId, Guid id)
    {
        try
        {
            var response = await $"{Url}/{entityId}/custom-fields/{id}".WithAuth().DeleteAsync();
            Assert.Fail($"Request should have return a 403 forbidden result but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(403);
        }
    }

    protected virtual void DeleteCustomFieldsAssertions(IFlurlResponse response,
        CustomFieldViewModel responseModelRead)
    {
    }

    protected async Task EndToEndAsync(Func<TReadResponse, string> searchSelector, bool doExport = true)
    {
        await TestRunner.Authenticate();
        var createRequest = await GenerateCreateRequestAsync();

        var created = await PostAsync(createRequest);

        var updateRequest = await GenerateUpdateRequestAsync(createRequest);
        await PutAsync(created.Id, updateRequest);

        var pathRequest = await GeneratePatchAsync();
        var patched = await PatchAsync(created.Id, pathRequest);

        var search = searchSelector(patched);

        await GetAsync(created.Id);
        await PageAsync();
        await SearchAsync(search);

        if (doExport)
        {
            await ExportToCsvAsync();
            await ExportToSpreadsheetAsync();
        }

        var changes = await GetChangeTrackingAsync(created.Id, 3);

        var added = changes
            .Data
            .FirstOrDefault(x => x.Action.EqualsIgnoreCaseAndWhitespace("added"));

        added.Should().NotBeNull();
        added?.EntityId.Should().BeEquivalentTo(created.Id);

        var ids = new List<TKey> { created.Id };

        var tasks = Enumerable
            .Range(0, 5)
            .Select(async _ => await GenerateCreateRequestAsync())
            .ToArray();

        var entities = await Task.WhenAll(tasks);

        var multiResult = await PostMultipleAsync(entities);

        ids.AddRange(multiResult.Created.Select(x => x.Id));

        var addedCustomField = await PostCustomFieldsAsync(CreatedKey);
        await AssertCustomFieldOnEntity(CreatedKey, addedCustomField);

        var updatedCustomField = await PutCustomFieldsAsync(CreatedKey, addedCustomField.Id);
        await AssertCustomFieldOnEntity(CreatedKey, updatedCustomField);

        var patchedCustomField = await PatchCustomFieldsAsync(CreatedKey, addedCustomField.Id);
        await AssertCustomFieldOnEntity(CreatedKey, patchedCustomField);

        await SearchCustomFieldsAsync(patchedCustomField.Key);

        await DeleteCustomFieldsAsync(CreatedKey, addedCustomField.Id);
        await AssertCustomFieldNotOnEntity(CreatedKey, addedCustomField.Id);

        await CheckResourceAuthorizationAsync(created, search, addedCustomField);
        await TestRunner.Authenticate();

        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }

        if (created is IActiveEntity)
        {
            foreach (var id in ids)
            {
                await UndoDeleteAsync(id);
            }
        }
    }

    private async Task AssertCustomFieldOnEntity(TKey entityId, CustomFieldViewModel customField)
    {
        var entity = await GetAsync(entityId);
        entity.CustomFields.Should().Contain(x =>
            x.Id == customField.Id && x.Key == customField.Key && x.Value == customField.Value);
    }

    private async Task AssertCustomFieldNotOnEntity(TKey entityId, Guid customFieldId)
    {
        var entity = await GetAsync(entityId);
        entity.CustomFields.Should().NotContain(x =>
            x.Id == customFieldId);
    }

    private async Task CheckResourceAuthorizationAsync(TReadResponse result, string search,
        CustomFieldViewModel customField)
    {
        await TestRunner.Authenticate(TestConstants.UnauthorizedTestUser);

        var createRequest = await GenerateCreateRequestAsync();
        await PostUnauthorizedAsync(createRequest);

        var updateRequest = await GenerateUpdateRequestAsync(createRequest);
        await PutUnauthorizedAsync(result.Id, updateRequest);

        var patchRequest = await GeneratePatchAsync();
        await PatchUnauthorizedAsync(result.Id, patchRequest);
        await DeleteUnauthorizedAsync(result.Id);
        await UndoDeleteUnauthorizedAsync(result.Id);

        await GetUnauthorizedAsync(result.Id);
        await PageUnauthorizedAsync(TestConstants.UnauthorizedTestUser.Email);
        await SearchUnauthorizedAsync(search, TestConstants.UnauthorizedTestUser.Email);
        await ExportToCsvUnauthorizedAsync(result.Id);
        await GetChangeTrackingUnauthorizedAsync(result.Id);

        var tasks = Enumerable
            .Range(0, 5)
            .Select(async _ => await GenerateCreateRequestAsync())
            .ToArray();

        var entities = await Task.WhenAll(tasks);
        await PostMultipleUnauthorizedAsync(entities);

        await PostCustomFieldsUnauthorizedAsync(CreatedKey);
        await PutCustomFieldsUnauthorizedAsync(CreatedKey, customField.Id);
        await PatchCustomFieldsUnauthorizedAsync(CreatedKey, customField.Id);
        await DeleteCustomFieldsUnauthorizedAsync(CreatedKey, customField.Id);
        await SearchCustomFieldsUnauthorizedAsync(customField.Key, customField.Id);
    }
    private async Task ExportToSpreadsheetAsync()
    {
        var response = await $"{Url}/export/spreadsheet".WithAuth()
            .SetQueryParam("pagenumber", 1.ToString())
            .SetQueryParam("pageSize", 10.ToString())
            .SetQueryParam("doCount", true.ToString())
            .SetQueryParam("fileName", "temp")
            .GetAsync();

        //assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseBytes = await response.GetBytesAsync();
        responseBytes.Should().NotBeNullOrEmpty();

        SaveResponse(response);
    }
}
