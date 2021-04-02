# Anontech ViziVault C# Bindings

## Project Description
AnonTech's ViziVault system is designed to make the retrieval and storage of personal and sensitive information easy. Our multi-layer encryption/decryption system will make your data secure and accessible only to memebrs of your organization on a "need-to-use" basis. Data providers, individuals, end users, and even developers can rest safe knowing that their personal data is stored securely, access is monitored, and their most personal data is kept securely, seperate from day-to-day business operations. Personal data is there when you need it most to support business operations, and disappears back into the vault when it's not needed, so that your system can be safe and secure.

## Support
Please report bugs and issues to support@anontech.io

## Requirements

`System.Text.Json`

## Installation

The AnonTech ViziVault Bindings for C# are available through NuGet as [Anontech-Vizivault-Client](https://www.nuget.org/packages/Anontech-Vizivault-Client/).

## Authentication
You must provide an application identifier or api key for all operations, to identify you and your application to the vault for authenticaion. For data insertion, a valid encryption key is necessary. For data retrieval, a valid decryption key is necessary.

We recommend at a minimum putting your encryption and decryption key locally in a secure location, such as a local file on disk.

## Quick start

### Connectiong to your vault

```csharp
string encryptionKey = System.Environment.GetEnvironmentVariable("VIZIVAULT_ENCRYPTION_KEY");
string decryptionKey = System.Environment.GetEnvironmentVariable("VIZIVAULT_DECRYPTION_KEY");
ViziVault vault = new ViziVault(url)
  .WithApiKey(apiKey)
  .WithEncryptionKey(encryptionKey)
  .WithDecryptionKey(decryptionKey);
```
## Attributes

The ViziVault ecosystem organizes your data using the concept of [attributes](https://docs.anontech.io/glossary/attribute). Every data point consists of three main components: a user id, which represents who the data is about; a value, which is some piece of information about the user; and an attribute, which expresses the relationship between the user and the value. For example, in an online retail application, there would be an attribute for shipping addresses, an attribute for billing addresses, an attribute for credit card information, and so on.

### Adding an attribute to an entity or user

Attributes are stored as `key`/`value` pairs of strings. Both users and entities can have attributes added to them. Some attributes are repeatable, such that multiple values can be stored for the same user; others are not repeatable, such that adding a new value to a user will overwrite any previous values. You can control whether an attribute is repeatable by modifying the associated [attribute definition](https://docs.anontech.io/glossary/attribute-definition).

```csharp
// Adding an attribute to user
User user = await vault.FindByUserAsync("User1234");
user.AddAttribute("FIRST_NAME", "Jane");
await vault.SaveAsync(user);

// Adding an attribute to entity
Entity entity = await vault.FindByEntityAsync("Client6789");
entity.AddAttribute("FULL_ADDRESS", "1 Hacker Way, Beverly Hills, CA 90210");
await vault.SaveAsync(entity);

// Adding an attribute with additional metadata to a user
AttributeValue attribute = new Attribute("LAST_NAME") {
    Tags = new List<String>{"ExampleTag"},
    Value = "Smith"
};
user.AddAttribute(attribute);
await vault.SaveAsync(user);
```

### Retrieving attributes of an entity or User

Once a User or Entity object has been retrieved from the vault, it is possible to inspect some or all of its attributes.

```csharp
// Retrieving all attributes for a user
User user = await vault.FindByUserAsync("User1234");
List<Attribute> userAttributes = user.Attributes;

// Retrieving all attributes for an entity
Entity entity = await vault.FindByEntityAsync("Client6789");
List<Attribute> entityAttributes = entity.Attributes;

// Retrieving a specific non-repeatable attribute for a user
Attribute attribute = user.GetAttribute("FIRST_NAME");

// Retrieving multiple values for a repeatable attribute
List<Attribute> attributes = user.GetAttributes("SHIPPING_ADDRESS");
```

### Deleting User Attributes
```csharp
// Purging all user attributes
await vault.PurgeAsync("User1234");

// Removing specific attribute
User user = await vault.FindByUserAsync("User1234");
user.ClearAttribute("LAST_NAME");
await vault.SaveAsync(user);
```

### Searching

To search a vault for [attributes](https://docs.anontech.io/glossary/datapoint/) , pass in a SearchRequest. A list of matching Attributes will be returned. For more information, read about [ViziVault Search](https://docs.anontech.io/tutorials/search/).

```csharp
int pageIndex = 0;
int maxCount = 25;
List<Attribute> attributes = await vault.SearchAsync(new SearchRequest("LAST_NAME", "Doe"), pageIndex, maxCount);
```

## Attribute definitions

[Attribute definitions](https://docs.anontech.io/glossary/attribute-definition/) define an object that contains all relevant metadata for attributes with a given `key`. This is how tags and regulations become associated with attributes, and how the [schema](https://docs.anontech.io/tutorials/attribute-schemas) detailing the expected structure of the attribute's values is specified. Display names and hints can also be added to the attribute definition for ease of use and readability.

### Storing an attribute definition in the vault

To store an attribute definition, create an AttributeDefinition object and save it to the vault. The following code details the various properties of the AttributeDefinition object.

```csharp
AttributeDefinition attributeDef = new AttributeDefinition() {
  name = "Billing Address",
  tags = {"geographic_location", "financial"},
  hint = "{ line_one: \"1 Hacker Way\", line_two: \"Apt. 53\", city: \"Menlo Park\", state: \"California\", postal_code: \"94025-1456\" country: \"USA\" }",
  SetSchema(PrimitiveSchema.String); // For simple, unstsructured data
  SchemaFromClass(typeof(YourModel)); // Alternatively, creating a schema to store objects of a class
  repeatable = false,
  indexed = false
};

await vault.StoreAttributeDefinitionAsync(attribute);
```

## Tags

[Tags](https://docs.anontech.io/api/tags/) are user-defined strings that can be applied to attributes to aid in classification and searching.


### Storing a tag in the vault

To store a new tag, create a Tag object and save it to the vault.

```csharp
await vault.StoreTagAsync(new Tag("Financial Data"));
```

### Retrieving tags from the vault

Tags can be retrieved as a list of Tag objects or as a single tag.

```csharp
// Retrieving all tags
List<Tag> tags = await vault.GetTagsAsync();

// Retrieving specific tag
String tag = await vault.GetTagAsync("Financial Data");
```

### Deleting tags from the vault

To remove a tag, specify the tag to be removed. A boolean denoting the status of the operation will be returned.

```csharp
// Removing a specific tag
bool removed = await vault.DeleteTagAsync("Financial Data");
```

## Regulations

A [regulation](https://docs.anontech.io/glossary/regulation/) object represents a governmental regulation that impacts how you can use the data in your vault. Each data point can have a number of regulations associated with it, which makes it easier to ensure your use of the data is compliant. You can tag data points with regulations when entering them into the system, or specify rules that the system will use to automatically tag regulations for you.

### Storing a regulation in the Vault

To store a regulation to the vault, create a Regulation object, set its key and its display name along with a URL pointing to further information about it, and call `StoreRegulationAsync`. To automatically apply regulations to incoming data, [rules](https://docs.anontech.io/tutorials/regulation-rules) can be specified.


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

### Retrieving regulations from the vault

Regulations can be retrieved as a list of Regulation objects or by requesting a single regulation by its key.

```csharp
/// Retrieving all regulations
List<Regulation> regulations = await vault.GetRegulationsAsync();

// Retrieving specific regulation
Regulation regulation = await vault.GetRegulationAsync("GDPR");
```

### Deleting regulations from the vault

To remove a regulation, specify the key of the regulation to be removed. A boolean denoting the status of the operation will be returned.

```csharp
// Removing a specific regulation
bool removed = await vault.DeleteRegulationAsync("GDPR");
```

