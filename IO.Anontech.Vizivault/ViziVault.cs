using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using IO.Anontech.Vizivault.Tagging;

namespace IO.Anontech.Vizivault {

  public class ViziVault {
    
    private Uri baseUrl;
    
    private readonly HttpRequestHeaders headers;

    private readonly HttpClient httpClient;

    private readonly JsonSerializerOptions options;

    public ViziVault() {
      httpClient = new HttpClient();
      headers = httpClient.DefaultRequestHeaders;
      
      options =  new JsonSerializerOptions {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {
          new JsonStringEnumConverter()
        }
      };
    }

    public ViziVault WithBaseUrl(Uri url) {
      this.baseUrl = url;
      return this;
    }

    public ViziVault WithApiKey(string apiKey) {
      headers.Add("Authorization", $"Bearer {apiKey}");
      return this;
    }

    public ViziVault WithEncryptionKey(string encryptionKey) {
      headers.Add("X-Encryption-Key", encryptionKey);
      return this;
    }

    public ViziVault WithDecryptionKey(string decryptionKey) {
      headers.Add("X-Decryption-Key", decryptionKey);
      return this;
    }

    private async Task<T> Get<T>(string url){
      HttpResponseMessage response = await httpClient.GetAsync(baseUrl + url);
      if(!response.IsSuccessStatusCode) {
        ApiError error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), options);
        throw new VaultResponseException(error.Message, response.StatusCode);
      }
      string responseContent = await response.Content.ReadAsStringAsync();
      return JsonSerializer.Deserialize<ApiData<T>>(responseContent, options).Data;
    }

    private async Task Delete(string url){
      HttpResponseMessage response = await httpClient.DeleteAsync(baseUrl + url);
      if(!response.IsSuccessStatusCode) {
        ApiError error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), options);
        throw new VaultResponseException(error.Message, response.StatusCode);
      }
    }

    private async Task Post<I>(string url, I body){
      string requestBody = JsonSerializer.Serialize<object>(body, options); // Polymorphism necessitates Serialize<object>
      HttpResponseMessage response = await httpClient.PostAsync(baseUrl + url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
      if(!response.IsSuccessStatusCode) {
        string content = await response.Content.ReadAsStringAsync();
        ApiError error = JsonSerializer.Deserialize<ApiError>(content, options);
        throw new VaultResponseException(error.Message, response.StatusCode);
      }
    }
    
    private async Task<O> PostAndReturn<I, O>(string url, I body){
      HttpResponseMessage response = await httpClient.PostAsync(baseUrl + url, new StringContent(JsonSerializer.Serialize<object>(body, options), Encoding.UTF8, "application/json"));
      if(!response.IsSuccessStatusCode) {
        ApiError error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), options);
        throw new VaultResponseException(error.Message, response.StatusCode);
      }
      return JsonSerializer.Deserialize<ApiData<O>>(await response.Content.ReadAsStringAsync(), options).Data;
    }

    public async Task<Entity> FindByEntityAsync(string entityId) {
      List<Attribute> data = await Get<List<Attribute>>($"/entities/{entityId}/attributes");
      Entity entity = await Get<Entity>($"/entities/{entityId}");
      foreach(Attribute attr in data) {
        entity.AddAttributeWithoutPendingChange(attr);
      }
      return entity;
    }

    public async Task<User> FindByUserAsync(string userId) {
      List<Attribute> data = await Get<List<Attribute>>($"/users/{userId}/attributes");
      User entity = await Get<User>($"/users/{userId}");
      foreach(Attribute attr in data) {
        entity.AddAttributeWithoutPendingChange(attr);
      }
      return entity;
    }

    public async Task SaveAsync(Entity entity) {
      
      await Task.WhenAll(from attribute in entity.DeletedAttributes select Delete($"/users/{entity.Id}/attributes/{attribute}"));
      entity.DeletedAttributes.Clear();

      await Post(entity is User ? "/users" : "/entities", new EntityDefinition(entity));

      if(entity.ChangedAttributes.Count > 0) {
        StorageRequest request = new StorageRequest {
          Data = new List<Attribute>(entity.ChangedAttributes)
        };

        await Post($"/users/{entity.Id}/attributes", request);
        entity.ChangedAttributes.Clear();
      }

    }

    public async Task PurgeAsync(Entity entity) {
      await Delete($"/users/{entity.Id}/data");
      entity.Purge();
    }

    public async Task StoreAttributeDefinitionAsync(AttributeDefinition attribute) {
      await Post("/attributes", attribute);
    }

    public async Task<AttributeDefinition> GetAttributeDefinitionAsync(string attributeKey) {
      return await Get<AttributeDefinition>($"/attributes/{attributeKey}");
    }

    public async Task<List<AttributeDefinition>> GetAttributeDefinitionsAsync() {
      return await Get<List<AttributeDefinition>>("/attributes/");
    }

    public async Task StoreTagAsync(Tag tag) {
      await Post("/tags", tag);
    }

    public async Task<Tag> GetTagAsync(string tag) {
      return await Get<Tag>($"/tags/{tag}");
    }

    public async Task<List<Tag>> GetTagsAsync() {
      return await Get<List<Tag>>("/tags");
    }

    public async Task<bool> DeleteTagAsync(String tag) {
      try{
        await Delete($"/tags/{tag}");
        return true;
      } catch(VaultResponseException) {
        return false;
      }
    }

    public async Task StoreRegulationAsync(Regulation regulation) {
      await Post("/regulations", regulation);
    }

    public async Task<Regulation> GetRegulationAsync(string regulation) {
      return await Get<Regulation>($"/regulations/{regulation}");
    }

    public async Task<List<Regulation>> GetRegulationsAsync() {
      return await Get<List<Regulation>>("/regulations");
    }

    public async Task<bool> DeleteRegulationAsync(String regulation) {
      try{
        await Delete($"/regulations/{regulation}");
        return true;
      } catch(VaultResponseException) {
        return false;
      }
    }


    public async Task<List<Attribute>> SearchAsync(SearchRequest searchRequest, int page, int count){
      return await PostAndReturn<PaginatedSearchRequest, List<Attribute>>("/search", new PaginatedSearchRequest{Query = searchRequest, Page = page, Count = count});
    }

    public async Task<Attribute> GetDataPointAsync(string dataPointId) {
      return await Get<Attribute>($"/data/{dataPointId}");
    }
  }
}