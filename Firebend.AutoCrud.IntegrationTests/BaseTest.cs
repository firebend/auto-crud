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

namespace Firebend.AutoCrud.IntegrationTests
{
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
        protected string BaseUrl { get; } = "http://localhost:5020/api";
        protected abstract string Url { get; }

        protected List<IFlurlResponse> Responses { get; } = new();

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

        private async Task<TReadResponse> PostAsync(TCreateRequest model)
        {
            var response = await Url.WithHeader("Authorization",
                    $"Bearer {_token}")
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
                await Url.WithHeader("Authorization",
                        $"Bearer {_token}")
                    .PostJsonAsync(model);
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void PostAssertions(IFlurlResponse response, TReadResponse model) { }

        private async Task<TReadResponse> GetAsync(TKey key)
        {
            var response = await $"{Url}/{key}".WithHeader("Authorization",
                $"Bearer {_token}").GetAsync();
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
                await $"{Url}/{key}".WithHeader("Authorization",
                    $"Bearer {_token}").GetAsync();
                Assert.Fail("Request should have return a 403 forbidden result");
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

            var response = await $"{Url}/{key}".WithHeader("Authorization",
                $"Bearer {_token}").PutJsonAsync(entity);

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
                await $"{Url}/{key}".WithHeader("Authorization",
                    $"Bearer {_token}").PutJsonAsync(entity);
                Assert.Fail("Request should have return a 403 forbidden result");
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
            var response = await Url.WithHeader("Authorization",
                    $"Bearer {_token}")
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
        }

        private async Task PageUnauthorizedAsync(string email)
        {
            //act
            var response = await Url.WithHeader("Authorization",
                    $"Bearer {_token}")
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
                if (person.DataAuth is not null && person.DataAuth.UserEmails.Any())
                {
                    person.DataAuth.UserEmails.Should().Contain(email);
                }
            }
        }

        protected virtual void PageAssertions(IFlurlResponse response,
            EntityPagedResponse<TReadResponse> responseModel)
        {
        }

        private async Task SearchAsync(string search)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)> DoSearch()
            {
                var searchResponse = await Url.WithHeader("Authorization",
                        $"Bearer {_token}")
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
        }

        private async Task SearchUnauthorizedAsync(string search, string email)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<TReadResponse> responseModel)> DoSearch()
            {
                var searchResponse = await Url.WithHeader("Authorization",
                        $"Bearer {_token}")
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
                    if (person.DataAuth is not null && person.DataAuth.UserEmails.Any())
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

            var deleteResponse = await $"{Url}/{id}".WithHeader("Authorization",
                $"Bearer {_token}").DeleteAsync();

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
                await $"{Url}/{id}".WithHeader("Authorization",
                    $"Bearer {_token}").DeleteAsync();
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void DeleteAssertions(IFlurlResponse response, TReadResponse responseModel) { }

        private async Task<TReadResponse> PatchAsync(TKey id, JsonPatchDocument patchDocument)
        {
            id.Should().NotBeNull();
            id.Should().NotBe(default(TKey));

            var patchJsonResponse = await $"{Url}/{id}".WithHeader("Authorization",
                $"Bearer {_token}").PatchJsonAsync(patchDocument);
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
                await $"{Url}/{id}".WithHeader("Authorization",
                    $"Bearer {_token}").PatchJsonAsync(patchDocument);
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void PatchAssertions(IFlurlResponse response, TReadResponse responseModel) { }

        private async Task ExportToCsvAsync()
        {
            var response = await $"{Url}/export/csv".WithHeader("Authorization",
                    $"Bearer {_token}")
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
                               HeaderValidated = null, MissingFieldFound = null
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
            var response = await $"{Url}/export/csv".WithHeader("Authorization",
                    $"Bearer {_token}")
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
                var httpResponse = await $"{Url}/{key}/changes".WithHeader("Authorization",
                        $"Bearer {_token}")
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

                return (httpResponse, httpResponseModel);
            }

            var (response, responseModel) = await RetryAsync(DoSearch, 30);

            ChangeTrackingAssertions(response, responseModel);

            SaveResponse(response);

            return responseModel;
        }

        private async Task GetChangeTrackingUnauthorizedAsync(
            TKey key)
        {
            try
            {
                await $"{Url}/{key}/changes".WithHeader("Authorization",
                        $"Bearer {_token}")
                    .SetQueryParam("pagenumber", 1)
                    .SetQueryParam("pageSize", 10)
                    .SetQueryParam("doCount", true)
                    .GetAsync();
                Assert.Fail("Request should have return a 403 forbidden result");
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
            var response = await $"{Url}/multiple".WithHeader("Authorization",
                $"Bearer {_token}").PostJsonAsync(models);
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
                await $"{Url}/multiple".WithHeader("Authorization",
                    $"Bearer {_token}").PostJsonAsync(models);
                Assert.Fail("Request should have return a 403 forbidden result");
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

        private async Task<CustomFieldViewModelRead> PostCustomFieldsAsync(TKey entityId)
        {
            var faked = CustomFieldFaker.Faker.Generate();
            var response = await $"{Url}/{entityId}/custom-fields".WithHeader("Authorization",
                $"Bearer {_token}").PostJsonAsync(faked);

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

        private async Task PostCustomFieldsUnauthorizedAsync(TKey entityId)
        {
            try
            {
                var faked = CustomFieldFaker.Faker.Generate();
                await $"{Url}/{entityId}/custom-fields".WithHeader("Authorization",
                    $"Bearer {_token}").PostJsonAsync(faked);
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void PostCustomFieldsAssertions(IFlurlResponse response,
            CustomFieldViewModelRead responseModel)
        {
        }

        private async Task PutCustomFieldsAsync(TKey entityId, Guid id)
        {
            var faked = CustomFieldFaker.Faker.Generate();
            var response = await $"{Url}/{entityId}/custom-fields/{id}".WithHeader("Authorization",
                $"Bearer {_token}").PutJsonAsync(faked);

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
        }

        private async Task PutCustomFieldsUnauthorizedAsync(TKey entityId, Guid id)
        {
            try
            {
                var faked = CustomFieldFaker.Faker.Generate();
                await $"{Url}/{entityId}/custom-fields/{id}".WithHeader("Authorization",
                    $"Bearer {_token}").PutJsonAsync(faked);
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void PutCustomFieldsAssertions(IFlurlResponse response,
            CustomFieldViewModelRead responseModel)
        {
        }

        private async Task<CustomFieldViewModelRead> PatchCustomFieldsAsync(TKey entityId, Guid id)
        {
            var color = new Faker().Commerce.Color();
            var patch = PatchFaker.MakeReplacePatch<CustomFieldViewModel, string>(x => x.Value, color);

            var response = await $"{Url}/{entityId}/custom-fields/{id}".WithHeader("Authorization",
                $"Bearer {_token}").PatchJsonAsync(patch);

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

        private async Task PatchCustomFieldsUnauthorizedAsync(TKey entityId, Guid id)
        {
            try
            {
                var color = new Faker().Commerce.Color();
                var patch = PatchFaker.MakeReplacePatch<CustomFieldViewModel, string>(x => x.Value, color);

                await $"{Url}/{entityId}/custom-fields/{id}".WithHeader("Authorization",
                    $"Bearer {_token}").PatchJsonAsync(patch);
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void PatchCustomFieldsAssertions(IFlurlResponse response,
            CustomFieldViewModelRead responseModel)
        {
        }

        private async Task SearchCustomFieldsAsync(string key)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<CustomFieldViewModelRead> responseModel)>
                DoSearch()
            {
                var httpResponse = await $"{Url}/custom-fields/".WithHeader("Authorization",
                        $"Bearer {_token}")
                    .SetQueryParam("pageNumber", "1")
                    .SetQueryParam("pageSize", "10")
                    .SetQueryParam("key", key)
                    .GetAsync();

                httpResponse.Should().NotBeNull();
                httpResponse.StatusCode.Should().Be(200);

                var httpResponseModel =
                    await httpResponse.GetJsonAsync<EntityPagedResponse<CustomFieldViewModelRead>>();

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
        }

        private async Task SearchCustomFieldsUnauthorizedAsync(string key)
        {
            async Task<(IFlurlResponse response, EntityPagedResponse<CustomFieldViewModelRead> responseModel)>
                DoSearch()
            {
                var httpResponse = await $"{Url}/custom-fields/".WithHeader("Authorization",
                        $"Bearer {_token}")
                    .SetQueryParam("pageNumber", "1")
                    .SetQueryParam("pageSize", "10")
                    .SetQueryParam("key", key)
                    .GetAsync();

                httpResponse.Should().NotBeNull();
                httpResponse.StatusCode.Should().Be(200);

                var httpResponseModel =
                    await httpResponse.GetJsonAsync<EntityPagedResponse<CustomFieldViewModelRead>>();

                httpResponseModel.Should().NotBeNull();
                httpResponseModel.Data.Should().BeEmpty();

                return (httpResponse, httpResponseModel);
            }

            await RetryAsync(DoSearch);
        }

        protected virtual void SearchCustomFieldsAssertions(IFlurlResponse response,
            EntityPagedResponse<CustomFieldViewModelRead> responseModel)
        {
        }

        private async Task DeleteCustomFieldsAsync(TKey entityId, Guid id)
        {
            var response = await $"{Url}/{entityId}/custom-fields/{id}".WithHeader("Authorization",
                $"Bearer {_token}").DeleteAsync();

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
        }

        private async Task DeleteCustomFieldsUnauthorizedAsync(TKey entityId, Guid id)
        {
            try
            {
                await $"{Url}/{entityId}/custom-fields/{id}".WithHeader("Authorization",
                    $"Bearer {_token}").DeleteAsync();
                Assert.Fail("Request should have return a 403 forbidden result");
            }
            catch (FlurlHttpException e)
            {
                e.StatusCode.Should().Be(403);
            }
        }

        protected virtual void DeleteCustomFieldsAssertions(IFlurlResponse response,
            CustomFieldViewModelRead responseModelRead)
        {
        }

        private string AuthenticationUrl => $"{BaseUrl}/token";
        protected abstract Task<UserInfoPostDto> GenerateAuthenticateRequestAsync();
        private string _token;

        private async Task Authenticate(UserInfoPostDto userInfo = null)
        {
            var authenticateRequest = userInfo ?? await GenerateAuthenticateRequestAsync();
            var response = await AuthenticationUrl.PostJsonAsync(authenticateRequest);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<TokenResponseModel>();

            responseModel.Should().NotBeNull();
            responseModel.Token.Should().NotBeNull();

            _token = responseModel.Token;
        }

        protected async Task EndToEndAsync(Func<TReadResponse, string> searchSelector)
        {
            await Authenticate();
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

            var changes = await GetChangeTrackingAsync(result.Id, 3);

            var added = changes
                .Data
                .FirstOrDefault(x => x.Action.EqualsIgnoreCaseAndWhitespace("added"));

            added.Should().NotBeNull();
            added?.EntityId.Should().BeEquivalentTo(result.Id);

            var ids = new List<TKey> {result.Id};

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

            await CheckResourceAuthorizationAsync(result, search, addedCustomField);
            await Authenticate();
            foreach (var id in ids)
            {
                await DeleteAsync(id);
            }
        }

        private async Task CheckResourceAuthorizationAsync(TReadResponse result, string search,
            CustomFieldViewModelRead customField)
        {
            var userEmail = "NotAuthorized@test.com";
            await Authenticate(new UserInfoPostDto {Email = userEmail, Password = "password"});

            var createRequest = await GenerateCreateRequestAsync();
            await PostUnauthorizedAsync(createRequest);

            var updateRequest = await GenerateUpdateRequestAsync(createRequest);
            await PutUnauthorizedAsync(result.Id, updateRequest);

            var patchRequest = await GeneratePatchAsync();
            await PatchUnauthorizedAsync(result.Id, patchRequest);
            await DeleteUnauthorizedAsync(result.Id);

            await GetUnauthorizedAsync(result.Id);
            await PageUnauthorizedAsync(userEmail);
            await SearchUnauthorizedAsync(search, userEmail);
            await ExportToCsvUnauthorizedAsync(result.Id);
            await GetChangeTrackingUnauthorizedAsync(result.Id);

            var tasks = Enumerable
                .Range(0, 5)
                .Select(async _ => await GenerateCreateRequestAsync())
                .ToArray();

            var entities = await Task.WhenAll(tasks);
            await PostMultipleUnauthorizedAsync(entities);

            // await PostCustomFieldsUnauthorizedAsync(CreatedKey);
            // await PutCustomFieldsUnauthorizedAsync(CreatedKey, customField.Id);
            // await PatchCustomFieldsAsync(CreatedKey, customField.Id);
            // await SearchCustomFieldsAsync(customField.Key);
            // await DeleteCustomFieldsAsync(CreatedKey, customField.Id);
        }
    }
}
