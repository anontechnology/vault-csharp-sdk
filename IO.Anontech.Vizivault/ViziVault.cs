using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IO.Anontech.Vizivault {

  public class ViziVault {
    
    private Uri baseUrl;
    
    private readonly HttpRequestHeaders headers;

    private readonly HttpClient httpClient;

    private readonly JsonSerializerOptions options;

    /// <summary>
    /// Creates a new ViziVault client object.
    /// Before using the object, it is necessary to populate the base URL, encryption key, decryption key, and optionally API key.
    /// </summary>
    public ViziVault(Uri url) {
      httpClient = new HttpClient();
      baseUrl = url;
      headers = httpClient.DefaultRequestHeaders;
      
      options =  new JsonSerializerOptions {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {
          new JsonStringEnumConverter()
        }
      };
    }

    /// <summary>
    /// Sets the ViziVault API key to use for all requests.
    /// </summary>
    /// <param name="apiKey">The API key to use</param>
    /// <returns>this</returns>
    public ViziVault WithApiKey(string apiKey) {
      headers.Add("Authorization", $"Bearer {apiKey}");
      return this;
    }

    /// <summary>
    /// Sets the RSA encryption key that will be used to store data.
    /// </summary>
    /// <param name="encryptionKey">The encryption key to use</param>
    /// <returns>this</returns>
    public ViziVault WithEncryptionKey(string encryptionKey) {
      headers.Add("X-Encryption-Key", encryptionKey);
      return this;
    }

    /// <summary>
    /// Sets the RSA decryption key that will be used to store data.
    /// </summary>
    /// <param name="decryptionKey">The decryption key to use</param>
    /// <returns>this</returns>
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

    /// <summary>
    /// Retrieves attributes and metadata for an entity with the specified id.
    /// </summary>
    /// <param name="entityId">The id of the entity to retrieve</param>
    /// <returns>An Entity object containing entity-level metadata and a list of attributes</returns>
    public async Task<Entity> FindByEntityAsync(string entityId) {
      List<Attribute> data = await Get<List<Attribute>>($"/entities/{entityId}/attributes");
      Entity entity = await Get<Entity>($"/entities/{entityId}");
      foreach(Attribute attr in data) {
        entity.AddAttributeWithoutPendingChange(attr);
      }
      return entity;
    }

    /// <summary>
    /// Retrieves attributes and metadata for a user with the specified id.
    /// </summary>
    /// <param name="userid">The id of the user to retrieve</param>
    /// <returns>A User object containing user-level metadata and a list of attributes</returns>
    public async Task<User> FindByUserAsync(string userId) {
      List<Attribute> data = await Get<List<Attribute>>($"/users/{userId}/attributes");
      User entity = await Get<User>($"/users/{userId}");
      foreach(Attribute attr in data) {
        entity.AddAttributeWithoutPendingChange(attr);
      }
      return entity;
    }

    /// <summary>
    /// Commits changes that have been made to an entity or user object. This includes adding attributes, deleting attributes, and updating entity-level metadata.
    /// </summary>
    /// <param name="entity">An entity or user object that has been modified locally</param>
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

    /// <summary>
    /// Deletes all attributes for an entity or user.
    /// </summary>
    /// <param name="entity">The id of the user or entity to delete</param>
    public async Task PurgeAsync(string userid) {
      await Delete($"/users/{userid}/data");
    }

    /// <summary>
    /// Creates or modifies an attribute definition.
    /// </summary>
    /// <param name="attributeDefinition">An attribute definition, which will be saved in the vault</param>
    public async Task StoreAttributeDefinitionAsync(AttributeDefinition attributeDefinition) {
      await Post("/attributes", attributeDefinition);
    }

    /// <summary>
    /// Retrieves metadata for an attribute definition.
    /// </summary>
    /// <param name="attributeKey">The name of the attribute definition to retrieve</param>
    /// <returns>An attribute definition with the requested name, if one exists</returns>
    public async Task<AttributeDefinition> GetAttributeDefinitionAsync(string attributeKey) {
      return await Get<AttributeDefinition>($"/attributes/{attributeKey}");
    }

    /// <summary>
    /// Retrieves metadata for all attribute definitions.
    /// </summary>
    /// <returns>A list containing all attribute definitions that exist in the vault</returns>
    public async Task<List<AttributeDefinition>> GetAttributeDefinitionsAsync() {
      return await Get<List<AttributeDefinition>>("/attributes/");
    }

    /// <summary>
    /// Creates or modifies a tag.
    /// </summary>
    /// <param name="tag">A tag metadata object, which will be saved in the vault</param>
    public async Task StoreTagAsync(Tag tag) {
      await Post("/tags", tag);
    }

    /// <summary>
    /// Retrieves metadata for a tag.
    /// </summary>
    /// <param name="tag">The name of the tag to retrieve</param>
    /// <returns>A tag with the requested name, if one exists</returns>
    public async Task<Tag> GetTagAsync(string tag) {
      return await Get<Tag>($"/tags/{tag}");
    }

    /// <summary>
    /// Retrieves metadata for all tags.
    /// </summary>
    /// <returns>A list containing all tags that exist in the vault</returns>
    public async Task<List<Tag>> GetTagsAsync() {
      return await Get<List<Tag>>("/tags");
    }

    /// <summary>
    /// Removes a tag from the system, and untags it from all entities, users, attributes, and attribute definitions.
    /// </summary>
    /// <param name="tag">The name of the tag to delete</param>
    /// <returns>A boolean value representing whether the tag was deleted successfully</returns>
    public async Task<bool> DeleteTagAsync(String tag) {
      try{
        await Delete($"/tags/{tag}");
        return true;
      } catch(VaultResponseException) {
        return false;
      }
    }

    /// <summary>
    /// Creates or modifies a regulation.
    /// </summary>
    /// <param name="regulation">A regulation metadata object, which will be saved in the vault</param>
    public async Task StoreRegulationAsync(Regulation regulation) {
      await Post("/regulations", regulation);
    }

    /// <summary>
    /// Retrieves metadata for a regulation.
    /// </summary>
    /// <param name="regulation">The name of the regulation to retrieve</param>
    /// <returns>A regulation with the requested name, if one exists</returns>
    public async Task<Regulation> GetRegulationAsync(string regulation) {
      return await Get<Regulation>($"/regulations/{regulation}");
    }

    /// <summary>
    /// Retrieves metadata for all regulations.
    /// </summary>
    /// <returns>A list containing all regulations that exist in the vault</returns>
    public async Task<List<Regulation>> GetRegulationsAsync() {
      return await Get<List<Regulation>>("/regulations");
    }

    /// <summary>
    /// Removes a regulation from the system.
    /// </summary>
    /// <param name="regulation">The name of the regulation to delete</param>
    /// <returns>A boolean value representing whether the regulation was deleted successfully</returns>
    public async Task<bool> DeleteRegulationAsync(String regulation) {
      try{
        await Delete($"/regulations/{regulation}");
        return true;
      } catch(VaultResponseException) {
        return false;
      }
    }

    /// <summary>
    /// Retrieves a paginated list of attributes that match various criteria.
    /// Attributes that are indexed can be searched by value; attributes that are not indexed can only be searched by metadata.
    /// </summary>
    /// <param name="searchRequest">The criteria to use when searching</param>
    /// <param name="page">The index of the page to retrieve</param>
    /// <param name="count">How many results each page should consist of</param>
    /// <returns>One page of the list of matching attributes</returns>
    public async Task<List<Attribute>> SearchAsync(SearchRequest searchRequest, int page, int count){
      return await PostAndReturn<PaginatedSearchRequest, List<Attribute>>("/search", new PaginatedSearchRequest{Query = searchRequest, Page = page, Count = count});
    }

    /// <summary>
    /// Obtains a single attribute from the vault that matches a datapoint id
    /// </summary>
    /// <param name="dataPointId">The datapoint id of the attribute to retrieve</param>
    /// <returns>An attribute with the specified datapoint id, if one exists</returns>
    public async Task<Attribute> GetDataPointAsync(string dataPointId) {
      return await Get<Attribute>($"/data/{dataPointId}");
    }
  }
}