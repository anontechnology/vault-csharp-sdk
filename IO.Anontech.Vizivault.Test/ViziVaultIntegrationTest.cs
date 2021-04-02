using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using IO.Anontech.Vizivault.Tagging;

namespace IO.Anontech.Vizivault.Test {
  public class ViziVaultIntegrationTest {

    string encryptionKey;

    string decryptionKey;

    string baseUrl = "http://localhost:8083";

    string apiKey = "aaa";

    public ViziVaultIntegrationTest () {
      
      decryptionKey = System.IO.File.ReadAllText(@"..\..\..\decryptionKey.txt");
      encryptionKey = System.IO.File.ReadAllText(@"..\..\..\encryptionKey.txt");
    }

    [Fact]
    public async Task RoundTripData() {
      ViziVault vault = new ViziVault(new Uri(baseUrl)).WithApiKey(apiKey).WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      // Create two attributes
      AttributeDefinition attributeDef1 = new AttributeDefinition("TestAttribute1");
      AttributeDefinition attributeDef2 = new AttributeDefinition("TestAttribute2") {
        Repeatable = true
      };
      
      await vault.StoreAttributeDefinitionAsync(attributeDef1);
      await vault.StoreAttributeDefinitionAsync(attributeDef2);
      
      // Add values of both attributes
      User sentUser = new User("exampleUser");
      AttributeValue attribute1 = new AttributeValue(attributeDef1.Name) {
        Value = "exampleUser's first name"
      };
      
      sentUser.AddAttribute(attribute1);
      sentUser.AddAttribute(attributeDef2.Name, "exampleUser's last name");
      sentUser.AddAttribute(attributeDef2.Name, "exampleUser's other last name");
      try {
        await vault.SaveAsync(sentUser);

        User receivedUser = await vault.FindByUserAsync("exampleUser");
        
        Assert.Equal(attribute1.GetValueAs<string>(), receivedUser.GetAttribute(attributeDef1.Name).GetValueAs<string>());
        Assert.Equal(3, receivedUser.Attributes.Count());
        Assert.Equal(2, receivedUser.GetAttributes(attributeDef2.Name).Count);

        // Remove one attribute
        receivedUser.ClearAttribute(attributeDef1.Name);
        await vault.SaveAsync(receivedUser);
        
        User receivedUserAfterDeletion = await vault.FindByUserAsync("exampleUser");
        Assert.Null(receivedUserAfterDeletion.GetAttribute(attributeDef1.Name));

      } finally {
        await vault.PurgeAsync(sentUser.Id);
      }
    }

    [Fact]
    public async Task TestSearch() {
      ViziVault vault = new ViziVault(new Uri(baseUrl)).WithApiKey(apiKey).WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      // Create two attributes
      AttributeDefinition attributeDef1 = new AttributeDefinition("TestAttribute1") {
        Indexed = true
      };
      AttributeDefinition attributeDef2 = new AttributeDefinition("TestAttribute2");
      await vault.StoreAttributeDefinitionAsync(attributeDef1);
      await vault.StoreAttributeDefinitionAsync(attributeDef2);

      User user1 = new User("user1");
      user1.AddAttribute(attributeDef1.Name, "common first name");

      User user2 = new User("user2");
      user2.AddAttribute(attributeDef1.Name, "common first name");
      user2.AddAttribute(attributeDef2.Name, "user2's last name");

      try {
        await vault.SaveAsync(user1);
        await vault.SaveAsync(user2);

        SearchRequest searchRequest = new SearchRequest();
        searchRequest.AddValueQuery(attributeDef1.Name, "common first name");
        searchRequest.Attributes = new List<string>{attributeDef2.Name};

        List<AttributeValue> results = await vault.SearchAsync(searchRequest, 0, 10);
        Assert.Equal(3, results.Count);
        Assert.Contains(results,
          (result) => attributeDef1.Name.Equals(result.AttributeKey) && user1.Id.Equals(result.UserId)
        );
        Assert.Contains(results,
          (result) => attributeDef1.Name.Equals(result.AttributeKey) && user2.Id.Equals(result.UserId)
        );
        Assert.Contains(results,
          (result) => attributeDef2.Name.Equals(result.AttributeKey) && user2.Id.Equals(result.UserId)
        );
      } finally {
        await vault.PurgeAsync(user1.Id);
        await vault.PurgeAsync(user2.Id);
      }
    }

    [Fact]
    public async Task TestGetAttributeByDatapointId() {
      ViziVault vault = new ViziVault(new Uri(baseUrl)).WithApiKey(apiKey).WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      AttributeDefinition attributeDef = new AttributeDefinition("TestAttribute1");
      await vault.StoreAttributeDefinitionAsync(attributeDef);

      User sentUser = new User("exampleUser");
      sentUser.AddAttribute(attributeDef.Name, "some data");
      
      try {
        await vault.SaveAsync(sentUser);

        User receivedUser = await vault.FindByUserAsync(sentUser.Id);
        AttributeValue first = receivedUser.GetAttribute(attributeDef.Name);
        AttributeValue second = await vault.GetDataPointAsync(receivedUser.GetAttribute(attributeDef.Name).DataPointId);
        Assert.Equal(first.GetValueAs<string>(), second.GetValueAs<string>());

      } finally {
        await vault.PurgeAsync(sentUser.Id);
      }
    }

    
    [Fact]
    public async Task TestTags() {
      ViziVault vault = new ViziVault(new Uri(baseUrl)).WithApiKey(apiKey).WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      AttributeDefinition attributeDef1 = new AttributeDefinition("TestAttribute1") {
        Tags = new List<string>{"tag1"}
      };
      await vault.StoreAttributeDefinitionAsync(attributeDef1);

      User sentUser = new User("exampleUser") {
        Tags = new List<string>{"tag2"}
      };

      try {
        AttributeValue attribute1 = new AttributeValue(attributeDef1.Name) {
          Value = "exampleUser's first name",
          Tags = new List<string>{"tag3"},
        };
        sentUser.AddAttribute(attribute1);
        await vault.SaveAsync(sentUser);

        AttributeValue receivedAttribute = (await vault.FindByUserAsync("exampleUser")).GetAttribute(attributeDef1.Name);
        Assert.Collection(receivedAttribute.Tags,
          (tag) => Assert.Equal("tag1", tag),
          (tag) => Assert.Equal("tag2", tag),
          (tag) => Assert.Equal("tag3", tag)
        );

        Tag tag1 = await vault.GetTagAsync("tag1");
        await vault.StoreTagAsync(tag1);

        Tag tag4 = new Tag("tag4");
        await vault.StoreTagAsync(tag4);

        List<Tag> allTags = await vault.GetTagsAsync();
        Assert.Contains(allTags, (tag) => tag.Name.Equals("tag1"));
        Assert.Contains(allTags, (tag) => tag.Name.Equals("tag2"));
        Assert.Contains(allTags, (tag) => tag.Name.Equals("tag3"));
        Assert.Contains(allTags, (tag) => tag.Name.Equals("tag4"));

        await vault.DeleteTagAsync("tag1");
        await vault.DeleteTagAsync("tag2");
        await vault.DeleteTagAsync("tag3");
        await vault.DeleteTagAsync("tag4");

        //Assert.Throws<VaultResponseException>(() => vault.GetTagAsync("tag5").Result);
        Assert.False(await vault.DeleteTagAsync("tag5"));

        allTags = await vault.GetTagsAsync();
        Assert.DoesNotContain(allTags, (tag) => tag.Name.Equals("tag1"));
        Assert.DoesNotContain(allTags, (tag) => tag.Name.Equals("tag2"));
        Assert.DoesNotContain(allTags, (tag) => tag.Name.Equals("tag3"));
        Assert.DoesNotContain(allTags, (tag) => tag.Name.Equals("tag4"));
        Assert.DoesNotContain(allTags, (tag) => tag.Name.Equals("tag5"));
        
      } finally {
        await vault.PurgeAsync(sentUser.Id);
      }
    }

    [Fact]
    public async Task TestRegulations() {
      ViziVault vault = new ViziVault(new Uri(baseUrl)).WithApiKey(apiKey).WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      Regulation regulation = new Regulation() {
        Name = "Regulation Name",
        Key = "RegulationKey"
      };

      AttributeDefinition attributeDef = new AttributeDefinition("TestAttribute1");
      await vault.StoreAttributeDefinitionAsync(attributeDef);

      ConjunctiveRule rootRule = new ConjunctiveRule();
      rootRule.AddRule(new AttributeRule(new List<string>{attributeDef.Name}, AttributeRule.AttributeListOperator.Any));
      rootRule.AddRule(new UserRule(attributeDef.Name, UserRule.UserValuePredicate.Eq, "Test Attribute Value"));

      regulation.Rule = rootRule;
      await vault.StoreRegulationAsync(regulation);

      Regulation receivedRegulation = await vault.GetRegulationAsync(regulation.Key);

      Assert.Equal(regulation.Name, receivedRegulation.Name);
      
      Assert.Contains(await vault.GetRegulationsAsync(), (r) => r.Name.Equals(regulation.Name));

      await vault.DeleteRegulationAsync(regulation.Key);
      
      Assert.DoesNotContain(await vault.GetRegulationsAsync(), (r) => r.Name.Equals(regulation.Name));
    }

    [Fact]
    public async Task TestAttributeDefinitions() {
      ViziVault vault = new ViziVault(new Uri(baseUrl)).WithApiKey(apiKey).WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      AttributeDefinition attributeDef = new AttributeDefinition("TestAttribute1") {
        Indexed = true
      };
      await vault.StoreAttributeDefinitionAsync(attributeDef);

      AttributeDefinition received = await vault.GetAttributeDefinitionAsync(attributeDef.Name);
      Assert.True(received.Indexed);

      List<AttributeDefinition> allAttributes = await vault.GetAttributeDefinitionsAsync();
      Assert.Contains(allAttributes, (a) => attributeDef.Name.Equals(a.Key));
    }

    /*[Fact]
    public async Task TestErrorHandlingAsync() {
      ViziVault vault = new ViziVault().WithBaseUrl(new Uri("http://localhost:8083")).WithApiKey("aaa").WithDecryptionKey(decryptionKey).WithEncryptionKey(encryptionKey);

      AttributeDefinition attributeDef = new AttributeDefinition("InvalidAttribute???");

      assertThrows(VaultResponseException.class, () -> vault.storeAttributeDefinition(attributeDef));
    }*/
  }
}
