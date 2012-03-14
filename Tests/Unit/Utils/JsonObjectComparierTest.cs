using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using CouchDude.Utils;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Utils
{
	public class JsonObjectComparierTest
	{
		private readonly JsonObjectComparier comparier = new JsonObjectComparier();

		[Fact]
		public void ShouldTreatNullArgumentsAreEqual() 
		{
			Assert.True(comparier.Equals(null, (JsonArray)null));
			Assert.True(comparier.Equals(null, (JsonObject)null));
			Assert.True(comparier.Equals(null, (JsonPrimitive)null));
			Assert.True(comparier.Equals(null, (JsonValue)null));
		}

		[Fact]
		public void ShouldTreatNullAndEmptyArrayAsUnEqual() 
		{
			Assert.False(comparier.Equals(null, new JsonArray()));
			Assert.False(comparier.Equals(new JsonArray(), null));
			Assert.False(comparier.Equals(null, new JsonArray() as JsonValue));
			Assert.False(comparier.Equals(new JsonArray(), null as JsonValue));
		}

		[Fact]
		public void ShouldTreatNullAndEmptyObjectAsUnEqual() 
		{
			Assert.False(comparier.Equals(null, new JsonObject()));
			Assert.False(comparier.Equals(new JsonObject(), null));
			Assert.False(comparier.Equals(null, new JsonObject() as JsonValue));
			Assert.False(comparier.Equals(new JsonObject(), null as JsonValue));
		}

		[Fact]
		public void ShouldTreatNullAndDefaultValueAsEqual()
		{
			var jsonObject = new JsonObject();
			var defaultInstance = jsonObject.ValueOrDefault("bf2360599bb84e57a2f465f2aa7f5a1b");
			Assert.True(comparier.Equals(null, defaultInstance));
			Assert.True(comparier.Equals(defaultInstance, null));
		}

		[Fact]
		public void ShouldTreatDifferentJsonValueTypesAsUnequal() 
		{
			Assert.False(comparier.Equals(new JsonArray(), new JsonObject()));	
			Assert.False(comparier.Equals(new JsonPrimitive(42), new JsonArray()));
			Assert.False(comparier.Equals(new JsonObject(), new JsonPrimitive(42)));	
		}

		[Fact]
		public void SholudReturnFixedAsHashCodeOfNull() 
		{
			var hashCodeOfZero = comparier.GetHashCode((JsonArray) null);
			Assert.Equal(hashCodeOfZero, comparier.GetHashCode((JsonObject)null));
			Assert.Equal(hashCodeOfZero, comparier.GetHashCode((JsonValue)null));
			Assert.Equal(hashCodeOfZero, comparier.GetHashCode((JsonPrimitive)null));	
		}

		[Theory]
		[InlineData(@"{}")]
		[InlineData(@"[]")]
		[InlineData(@"[""test""]")]
		[InlineData(@"[""""]")]
		[InlineData(@"["""", 42]")]
		[InlineData(@"{""key"": ""value"", ""flag"": false}")]
		[InlineData(@"{""key"": ""value"", ""subobject"":{}}")]
		[InlineData(@"{""key"": ""value"", ""subarray"":[1, 2, ""3""]}")]
		[InlineData(@"{""key"": ""value"", ""subobject"":{""subkey"": 42}, ""subarray"":[1, 2, ""3""]}")]
		public void ShouldReturnSameHashCodeForSameJson(string jsonString) 
		{
			var valueA = JsonValue.Parse(jsonString);
			var valueB = JsonValue.Parse(jsonString);
			Assert.True(comparier.Equals(valueA, valueB));
			Assert.Equal(comparier.GetHashCode(valueA), comparier.GetHashCode(valueB));
		}

		[Fact]
		public void ShouldTreatDifferentlySizedArraysAsUnequal() 
		{
			Assert.False(comparier.Equals(JsonValue.Parse("[1, 42]"), JsonValue.Parse("[1, 42, 4]")));	
		}

		[Fact]
		public void ShouldTreatArraysOfDifferentLengthAsUnequal() 
		{
			Assert.False(comparier.Equals(JsonValue.Parse("[1, 42]"), JsonValue.Parse("[1, 44]")));	
		}

		[Fact]
		public void ShouldTreatObjectsOfDifferentNumberOfPropertiesAsUnequal() 
		{
			Assert.False(comparier.Equals(
				JsonValue.Parse(@"{""propA"":""valueA"", ""propB"":""valueB"", ""propC"":""valueC""}"), 
				JsonValue.Parse(@"{""propA"":""valueA"", ""propB"":""valueB""}"))
			);	
		}

		[Fact]
		public void ShouldTreatPrimitivesOfDifferentTypesAsDifferent()
		{
			Assert.False(comparier.Equals(new JsonPrimitive(1), new JsonPrimitive(1.0)));
			Assert.False(comparier.Equals(new JsonPrimitive(1), new JsonPrimitive("1")));
			Assert.False(comparier.Equals(new JsonPrimitive(1), new JsonPrimitive(true)));
		}

		[Fact]
		public void ShouldTreatDifferentPrimitivesOfSameTypesAsDifferent()
		{
			Assert.False(comparier.Equals((JsonValue)new JsonPrimitive(1), new JsonPrimitive(2)));
			Assert.False(comparier.Equals((JsonValue)new JsonPrimitive(1.0), new JsonPrimitive(1.1)));
			Assert.False(comparier.Equals((JsonValue)new JsonPrimitive("0"), new JsonPrimitive("1")));
			Assert.False(comparier.Equals((JsonValue)new JsonPrimitive(true), new JsonPrimitive(false)));
		}
	}
}
