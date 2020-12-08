## Anontech ViziVault C# Bindings

### Project Description
AnonTech's ViziVault system is designed to make the retrieval and storage of personal and sensitive information easy. Our multi-layer encryption/decryption system will make your data secure and accessible only to memebrs of your organization on a "need-to-use" basis. Data providers, individuals, end users, and even developers can rest safe knowing that their personal data is stored securely, access is monitored, and their most personal data is kept securely, seperate from day-to-day business operations. Personal data is there when you need it most to support business operations, and disappears back into the vault when it's not needed, so that your system can be safe and secure.


### Support
Please report bugs and issues to support@anontech.io

### Requirements

### Installaion

### Authentication
You must provide an application identifier or api key for all operations, to identify you and your application to the vault for authenticaion. For data insertion, a valid encryption key is necessary. For data retrieval, a valid decryption key is necessary.

We recommend at a minimum putting your encryption and decryption key locally in a secure location, such as a local file on disk.

### Quick start

#### Attaching to your Vault

```csharp
string encryptionKey = System.Environment.GetEnvironmentVariable("ENCRYPTIONKEY");
string decryptionKey = System.Environment.GetEnvironmentVariable("DECRYPTIONKEY");
ViziVault vault = new ViziVault(url)
  .WithApiKey(apiKey)
  .WithEncryptionKey(encryptionKey)
  .WithDecryptionKey(decryptionKey);
```

#### Attributes

[Attributes](https://docs.anontech.io/glossary/datapoint/) are how the ViziVault ecosystem organizes your data. Every attribute consists of three main components: a user id, which represents who the data is about; a value, which is some piece of information about the user; and an attribute name, which expresses the relationship between the user and the value. For example, in an online retail application, there would be an attribute for shipping addresses, an attribute for billing addresses, and an attribute for credit card information.

#### Adding an Attribute to an Entity or User

```csharp
// Retrieving all attributes for a newly-created user
User user = new User("exampleUser");
user.AddAttribute(FIRST_NAME, "Jane");
await vault.SaveAsync(user);

// Adding an attribute to entity
Entity entity = await vault.FindByEntityAsync("Client6789");
entity.AddAttribute("FULL_ADDRESS", "1 Hacker Way, Beverly Hills, CA 90210");
await vault.SaveAsync(entity);
```



### Retrieving all Attributes of an Entity or User
Retrieves all [Attributes](https://docs.anontech.io/glossary/datapoint/) for the specified entity or user. Returns a list of attribute objects.

```csharp
// Retrieving all attributes for a user
User user = await vault.FindByUserAsync("User1234");
List<Attribute> attributes = user.Attributes;

// Retrieving all attributes for an entity
Entity entity = await vault.FindByEntityAsync("Client6789");
List<Attribute> attributes = entity.Attributes;
```

### Searching

To search a vault for [Attributes](https://docs.anontech.io/glossary/datapoint/) , pass in a SearchRequest. A list of matching Attributes will be returned. For more information, read about [ViziVault Search](https://docs.anontech.io/tutorials/search/).

```csharp
List<Attribute> attributes = await vault.SearchAsync(new SearchRequest("LAST_NAME", "Doe"));
```

### Deleting User Attributes
```csharp
// Purging all user attributes
User user = await vault.FindByUserAsync("User1234");
await vault.PurgeAsync(user.Id);

// Removing specific attribute
User user = await vault.FindByUserAsync("User1234");
user.ClearAttribute("LAST_NAME");
await vault.SaveAsync(user);
```

### Attribute Definitions

[Attribute definitions](https://docs.anontech.io/glossary/attribute/) define an object that contains all relevant metadata for attributes with a given `key`. This is how tags and regulations become associated with attributes. Attributes can contain a schema to further break down the structure of their value. Display names and hints can also be added to the Attribute Definition for ease of use and readability.

#### Storing an Attribute Definition in the Vault

To store an Attribute Definition, create an AttributeDefinition object and save it to the Vault. The following code details the various properties of the AttributeDefinition object.

```csharp
AttributeDefinition attributeDef = new AttributeDefinition() {
  name = "Billing Address",
  tags = {"geographic_location", "financial"},
  hint = "{ line_one: \"1 Hacker Way\", line_two: \"Apt. 53\", city: \"Menlo Park\", state: \"California\", postal_code: \"94025-1456\" country: \"USA\" }",
  SetSchema(PrimitiveSchema.String); // For simple, unstsructured dat,
  SchemaFromClass(typeof(YourModel)); // Alternatively, creating a schema to store objects of a clas,
  repeatable = false,
  indexed = false
};

await vault.StoreAttributeDefinitionAsync(attribute);
```

### Tags

Similar to [Regulations](https://docs.anontech.io/glossary/regulation/) , [Tags](https://docs.anontech.io/api/tags/) are user-defined strings that can be applied to Attributes to aid in classification and searching.


#### Storing a Tag in the vault

To store a new [Tag](https://docs.anontech.io/api/tags/) , create a Tag object and save it to the Vault.

```csharp
await vault.StoreTagAsync(new Tag("Financial Data"));
```

#### Retrieving Tags from the Vault

[Tags](https://docs.anontech.io/api/tags/) can be retrieved as a list of Tag objects or as a single Tag if the specific Tag is specified.

```csharp
// Retrieving all tags
List<Tag> tags = await vault.GetTagsAsync();

// Retrieving specific tag
String tag = await vault.GetTagAsync("Financial Data");
```

#### Deleting Tags from the Vault

To remove a [Tag](https://docs.anontech.io/api/tags/) , specify the Tag to be removed. A boolean denoting the status of the operation will be returned.

```csharp
// Removing a specific tag
bool removed = await vault.DeleteTagAsync("Financial Data");
```

### Regulations

A regulation object represents a governmental regulation that impacts how you can use the data in your vault. Each data point can have a number of regulations associated with it, which makes it easier to ensure your use of the data is compliant. You can tag data points with regulations when entering them into the system, or specify rules that the system will use to automatically tag regulations for you.

#### Storing a Regulation in the Vault

To store a [Regulation](https://docs.anontech.io/glossary/regulation/) to the Vault, create a new Regulation object and save it to the Vault. The constructor takes the key, name, and url of the Regulation.


```csharp
// Storing a regulation
Regulation regulation = new Regulation() {
    Key = "GDPR",
    Name = "General Data Protection Regulation",
    Url = "https://gdpr.eu/",
    Rule = new UserRule("GEOGRAPHIC_REGION", UserRule.UserValuePredicate.Eq, "EU")
};
await vault.StoreRegulationAsync(regulation);
```

#### Retrieving Regulations from the Vault

[Regulations](https://docs.anontech.io/glossary/regulation/) can be retrieved as a list of Regulation objects or as a single Regulation if the specific Regulation is specified.

```csharp
/// Retrieving all regulations
List<Regulation> regulations = await vault.GetRegulationsAsync();

// Retrieving specific regulation
Regulation regulation = await vault.GetRegulationAsync("GDPR");
```

#### Deleting Regulations from the Vault

To remove a [Regulation](https://docs.anontech.io/glossary/regulation/) , specify the Regulation to be removed. A boolean denoting the status of the operation will be returned.

```csharp
// Removing a specific regulation
bool removed = await vault.DeleteRegulationAsync("GDPR");
```

