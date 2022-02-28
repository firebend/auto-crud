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
using Firebend.AutoCrud.IntegrationTests.Interfaces;
using Firebend.AutoCrud.IntegrationTests.Models;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
using FluentAssertions;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.IntegrationTests
{
    public abstract class BaseTest<
        TKey,
        TCreateRequest,
        TUpdateRequest,
        TReadResponse,
        TExport>
        where TKey : struct
        where TReadResponse : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TUpdateRequest : class
        where TExport : class
    {
        public abstract string Url { get; }

        public List<IFlurlResponse> Responses { get; } = new();

        public TKey CreatedKey { get; set; }

        public abstract Task<TCreateRequest> GenerateCreateRequestAsync();

        protected abstract Task<TUpdateRequest> GenerateUpdateRequestAsync(TCreateRequest createRequest);

        protected abstract Task<JsonPatchDocument> GeneratePatchAsync();

        private static void Log(string message) => Trace.WriteLine(message);

        protected void SaveResponse(IFlurlResponse response) => Responses.Add(response);

        public static async Task<T> RetryAsync<T>(Func<Task<T>> func, int times = 10)
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
            var response = !string.IsNullOrEmpty(_token)
                ? await Url.WithHeader("Authorization",
                        $"Bearer {_token}")
                    .PostJsonAsync(model)
                : await Url.PostJsonAsync(model);

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

        protected virtual void PostAssertions(IFlurlResponse response, TReadResponse model)
        {
        }

        public async Task<TReadResponse> GetAsync(TKey key)
        {
            var response = await $"{Url}/{key}".GetAsync();
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<TReadResponse>();
            responseModel.Should().NotBeNull();

            GetAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void GetAssertions(IFlurlResponse response, TReadResponse model)
        {
        }

        public async Task<TReadResponse> PutAsync(TKey key, TUpdateRequest entity)
        {
            entity.Should().NotBeNull();
            key.Should().NotBeNull();
            key.Should().NotBeSameAs(default(TKey));

            var response = await $"{Url}/{key}".PutJsonAsync(entity);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<TReadResponse>();
            responseModel.Should().NotBeNull();

            PutAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void PutAssertions(IFlurlResponse response, TReadResponse model)
        {
        }

        public async Task<EntityPagedResponse<TReadResponse>> PageAsync()
        {
            //act
            var response = await Url
                .SetQueryParam("pagenumber", 1)
                .SetQueryParam("pageSize", 10)
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

            return responseModel;
        }

        protected virtual void PageAssertions(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)
        {
        }

        public async Task<EntityPagedResponse<TReadResponse>> SearchAsync(string search)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)> DoSearch()
            {
                var searchResponse = await Url
                    .SetQueryParam("pagenumber", 1)
                    .SetQueryParam("pageSize", 10)
                    .SetQueryParam("search", search)
                    .GetAsync();

                searchResponse.Should().NotBeNull();
                searchResponse.StatusCode.Should().Be(200);

                var searchResponseModel = await searchResponse.GetJsonAsync<EntityPagedResponse<TReadResponse>>();
                searchResponseModel.Should().NotBeNull();
                searchResponseModel.Data.Should().NotBeNullOrEmpty();
                searchResponseModel.TotalRecords.Should().BeGreaterOrEqualTo(1);

                return (searchResponse, searchResponseModel);
            }

            search.Should().NotBeNullOrEmpty();

            var (response, responseModel) = await RetryAsync(DoSearch);

            SearchAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void SearchAssertions(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)
        {
        }

        public async Task DeleteAsync(TKey id)
        {
            id.Should().NotBeNull();
            id.Should().NotBe(default(TKey));

            var deleteResponse = await $"{Url}/{id}".DeleteAsync();

            deleteResponse.Should().NotBeNull();
            deleteResponse.StatusCode.Should().Be(200);

            var deleteResponseModel = await deleteResponse.GetJsonAsync<TReadResponse>();
            deleteResponseModel.Should().NotBeNull();

            DeleteAssertions(deleteResponse, deleteResponseModel);

            SaveResponse(deleteResponse);
        }

        protected virtual void DeleteAssertions(IFlurlResponse response, TReadResponse responseModel)
        {
        }

        public async Task<TReadResponse> PatchAsync(TKey id, JsonPatchDocument patchDocument)
        {
            id.Should().NotBeNull();
            id.Should().NotBe(default(TKey));

            var patchJsonResponse = await $"{Url}/{id}".PatchJsonAsync(patchDocument);
            patchJsonResponse.Should().NotBeNull();
            patchJsonResponse.StatusCode.Should().Be(200);

            var patchJsonResponseModel = await patchJsonResponse.GetJsonAsync<TReadResponse>();
            patchJsonResponseModel.Should().NotBeNull();

            PatchAssertions(patchJsonResponse, patchJsonResponseModel);

            SaveResponse(patchJsonResponse);

            return patchJsonResponseModel;
        }

        protected virtual void PatchAssertions(IFlurlResponse response, TReadResponse responseModel)
        {
        }

        public async Task ExportToCsvAsync()
        {
            var response = await $"{Url}/export/csv"
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
                    new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null }))
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

        protected virtual void CsvResponseAssertions(IFlurlResponse response)
        {
        }

        protected virtual void CsvRecordsAssertions(List<TExport> records)
        {
        }

        public async Task<EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>>> GetChangeTracking(TKey key, int operationCount)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>> responseModel)> DoSearch()
            {
                var httpResponse = await $"{Url}/{key}/changes"
                    .SetQueryParam("pagenumber", 1)
                    .SetQueryParam("pageSize", 10)
                    .SetQueryParam("doCount", true)
                    .GetAsync();

                httpResponse.Should().NotBeNull();
                httpResponse.StatusCode.Should().Be(200);

                var httpResponseModel = await httpResponse.GetJsonAsync<EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>>>();

                httpResponseModel.Should().NotBeNull();
                httpResponseModel.Data.Should().NotBeNullOrEmpty();
                httpResponseModel.Data.Should().HaveCountGreaterOrEqualTo(operationCount);
                httpResponseModel.Data.Select(x => x.UserEmail).Should().NotContainNulls();
                httpResponseModel.Data.Select(x => x.Source).Should().NotContainNulls();

                return (httpResponse, httpResponseModel);
            }

            var (response, responseModel) = await RetryAsync(DoSearch, 30);

            ChangeTrackingAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void ChangeTrackingAssertions(IFlurlResponse response,
            EntityPagedResponse<ChangeTrackingResponseModel<TKey, TReadResponse>> responseModel)
        {
        }

        public async Task<MultiResult<TReadResponse>> PostMultipleAsync(TCreateRequest[] models)
        {
            var response = await $"{Url}/multiple".PostJsonAsync(models);
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<MultiResult<TReadResponse>>();
            responseModel.Created.Should().NotBeNullOrEmpty();
            responseModel.Created.Count().Should().Be(models.Length);
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

        protected virtual void PostMultipleAssertions(IFlurlResponse response, MultiResult<TReadResponse> responseModel)
        {
        }
        public async Task<CustomFieldViewModelRead> PostCustomFieldsAsync(TKey entityId)
        {
            var faked = CustomFieldFaker.Faker.Generate();
            var response = await $"{Url}/{entityId}/custom-fields".PostJsonAsync(faked);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<CustomFieldViewModelRead>();
            responseModel.Should().NotBeNull();
            responseModel.Id.Should().NotBeEmpty();
            responseModel.EntityId.Should().NotBeEmpty();
            responseModel.Key.Should().NotBeNullOrWhiteSpace();
            responseModel.Value.Should().NotBeNullOrWhiteSpace();
            responseModel.CreatedDate.Should().BeAfter(DateTimeOffset.MinValue);
            responseModel.ModifiedDate.Should().BeAfter(DateTimeOffset.MinValue);

            PostCustomFieldsAssertions(response, responseModel);

            var entity = await GetAsync(entityId);
            var customField = entity.CustomFields.First(x => x.Id == responseModel.Id);
            customField.Value.Should().BeEquivalentTo(faked.Value);
            customField.Key.Should().BeEquivalentTo(faked.Key);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void PostCustomFieldsAssertions(IFlurlResponse rsponse, CustomFieldViewModelRead responseModel)
        {
        }

        public async Task<CustomFieldViewModelRead> PutCustomFieldsAsync(TKey entityId, Guid id)
        {
            var faked = CustomFieldFaker.Faker.Generate();
            var response = await $"{Url}/{entityId}/custom-fields/{id}".PutJsonAsync(faked);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<CustomFieldViewModelRead>();

            responseModel.Should().NotBeNull();
            responseModel.Id.Should().NotBeEmpty();
            responseModel.EntityId.Should().NotBeEmpty();
            responseModel.Key.Should().NotBeNullOrWhiteSpace();
            responseModel.Value.Should().NotBeNullOrWhiteSpace();
            responseModel.ModifiedDate.Should().BeAfter(DateTimeOffset.MinValue);

            PutCustomFieldsAssertions(response, responseModel);

            var entity = await GetAsync(entityId);
            var customField = entity.CustomFields.First(x => x.Id == id);
            customField.Value.Should().BeEquivalentTo(faked.Value);
            customField.Key.Should().BeEquivalentTo(faked.Key);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void PutCustomFieldsAssertions(IFlurlResponse response, CustomFieldViewModelRead responseModel)
        {
        }

        public async Task<CustomFieldViewModelRead> PatchCustomFieldsAsync(TKey entityId, Guid id)
        {
            var color = new Faker().Commerce.Color();
            var patch = PatchFaker.MakeReplacePatch<CustomFieldViewModel, string>(x => x.Value, color);

            var response = await $"{Url}/{entityId}/custom-fields/{id}".PatchJsonAsync(patch);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<CustomFieldViewModelRead>();
            responseModel.Should().NotBeNull();
            responseModel.Id.Should().NotBeEmpty();
            responseModel.EntityId.Should().NotBeEmpty();
            responseModel.Key.Should().NotBeNullOrWhiteSpace();
            responseModel.Value.Should().NotBeNullOrWhiteSpace();
            responseModel.Value.Should().BeEquivalentTo(color);
            responseModel.ModifiedDate.Should().BeAfter(DateTimeOffset.MinValue);

            var entity = await GetAsync(entityId);
            var customField = entity.CustomFields.First(x => x.Id == id);
            customField.Value.Should().BeEquivalentTo(color);


            PatchCustomFieldsAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void PatchCustomFieldsAssertions(IFlurlResponse response, CustomFieldViewModelRead responseModel)
        {
        }

        public async Task<EntityPagedResponse<CustomFieldViewModelRead>> SearchCustomFieldsAsync(string key)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<CustomFieldViewModelRead> responseModel)> DoSearch()
            {
                var httpResponse = await $"{Url}/custom-fields/"
                    .SetQueryParam("pageNumber", "1")
                    .SetQueryParam("pageSize", "10")
                    .SetQueryParam("key", key)
                    .GetAsync();

                httpResponse.Should().NotBeNull();
                httpResponse.StatusCode.Should().Be(200);

                var httpResponseModel = await httpResponse.GetJsonAsync<EntityPagedResponse<CustomFieldViewModelRead>>();

                httpResponseModel.Should().NotBeNull();
                httpResponseModel.Data.Should().NotBeNullOrEmpty();

                var customField = httpResponseModel.Data.First();
                customField.Should().NotBeNull();
                customField.Id.Should().NotBeEmpty();
                customField.EntityId.Should().NotBeEmpty();
                customField.Key.Should().NotBeNullOrWhiteSpace();
                customField.Value.Should().NotBeNullOrWhiteSpace();
                customField.ModifiedDate.Should().BeAfter(DateTimeOffset.MinValue);

                return (httpResponse, httpResponseModel);
            }

            var (searchResponse, searchResponseModel) = await RetryAsync(DoSearch);

            SearchCustomFieldsAssertions(searchResponse, searchResponseModel);

            SaveResponse(searchResponse);

            return searchResponseModel;
        }

        protected virtual void SearchCustomFieldsAssertions(IFlurlResponse response, EntityPagedResponse<CustomFieldViewModelRead> responseModel)
        {
        }

        public async Task<CustomFieldViewModelRead> DeleteCustomFieldsAsync(TKey entityId, Guid id)
        {
            var response = await $"{Url}/{entityId}/custom-fields/{id}".DeleteAsync();

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<CustomFieldViewModelRead>();
            responseModel.Should().NotBeNull();
            responseModel.Id.Should().NotBeEmpty();
            responseModel.EntityId.Should().NotBeEmpty();
            responseModel.Key.Should().NotBeNullOrWhiteSpace();
            responseModel.Value.Should().NotBeNullOrWhiteSpace();
            responseModel.ModifiedDate.Should().BeAfter(DateTimeOffset.MinValue);

            var entity = await GetAsync(entityId);
            entity.CustomFields.Should().NotContain(x => x.Id == id);

            DeleteCustomFieldsAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        protected virtual void DeleteCustomFieldsAssertions(IFlurlResponse response, CustomFieldViewModelRead responseModelRead)
        {
        }

        protected virtual bool AuthenticationRequired => false;
        protected virtual string AuthenticationUrl => string.Empty;
        protected abstract Task<UserInfoPostDto> GenerateAuthenticateRequestAsync();
        private string _token;
        protected virtual async Task Authenticate()
        {
            if (string.IsNullOrEmpty(AuthenticationUrl))
            {
                throw new NotImplementedException("The AuthenticationUrl property must be set.");
            }

            var authenticateRequest = await GenerateAuthenticateRequestAsync();
            authenticateRequest.Password = "123456@Qwerty";
            var response = await AuthenticationUrl.PostJsonAsync(authenticateRequest);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetStringAsync();

            responseModel.Should().NotBeNull();

            _token = responseModel;
        }

        protected virtual async Task EndToEndAsync(Func<TReadResponse, string> searchSelector)
        {
            if (AuthenticationRequired)
            {
                await Authenticate();
            }

            var createRequest = await GenerateCreateRequestAsync();

            var result = await PostAsync(createRequest);

            var updateRequest = await GenerateUpdateRequestAsync(createRequest);
            await PutAsync(result.Id, updateRequest);

            var pathRequest = await GeneratePatchAsync();
            var patched = await PatchAsync(result.Id, pathRequest);

            var search = searchSelector(patched);

            await GetAsync(result.Id);
            await PageAsync();
            await SearchAsync(search);
            await ExportToCsvAsync();

            var changes = await GetChangeTracking(result.Id, 3);

            var added = changes
                .Data
                .FirstOrDefault(x => x.Action.EqualsIgnoreCaseAndWhitespace("added"));

            added.Should().NotBeNull();
            added?.EntityId.Should().BeEquivalentTo(result.Id);

            var ids = new List<TKey> { result.Id };

            var tasks = Enumerable
                .Range(0, 5)
                .Select(async _ => await GenerateCreateRequestAsync())
                .ToArray();

            var entities = await Task.WhenAll(tasks);

            var multiResult = await PostMultipleAsync(entities);

            ids.AddRange(multiResult.Created.Select(x => x.Id));

            var addedCustomField = await PostCustomFieldsAsync(CreatedKey);
            await PutCustomFieldsAsync(CreatedKey, addedCustomField.Id);
            var patchedCustomField = await PatchCustomFieldsAsync(CreatedKey, addedCustomField.Id);
            await SearchCustomFieldsAsync(patchedCustomField.Key);
            await DeleteCustomFieldsAsync(CreatedKey, addedCustomField.Id);

            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }
    }
}
