#region Licence Info 
/*
  Copyright 2011 · Artem Tikhomirov																					
                                                                          
  Licensed under the Apache License, Version 2.0 (the "License");					
  you may not use this file except in compliance with the License.					
  You may obtain a copy of the License at																	
                                                                          
      http://www.apache.org/licenses/LICENSE-2.0														
                                                                          
  Unless required by applicable law or agreed to in writing, software			
  distributed under the License is distributed on an "AS IS" BASIS,				
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
  See the License for the specific language governing permissions and			
  limitations under the License.																						
*/
#endregion

using System;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
  public class CouchSessionSaveTests
  {
    private readonly SimpleEntity entity = new SimpleEntity {
      Id = "doc1",
      Name = "John Smith"
    };

    [Fact]
    public void ShouldThrowOnSameInstanseSave()
    {
      Assert.Throws<ArgumentException>(() => DoSave(
        action: session => {
          session.Save(entity);
          session.Save(entity);
          return new DocumentInfo();
        }));
    }

    [Fact]
    public void ShouldThrowOnSaveWithRevision()
    {
      Assert.Throws<ArgumentException>(() => 
        DoSave(
          new SimpleEntity {
            Id = "doc1",
            Revision = "42-1a517022a0c2d4814d51abfedf9bfee7",
            Name = "John Smith"
      }));
    }

    [Fact]
    public void ShouldThrowOnNullEntity()
    {
      var session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
      Assert.Throws<ArgumentNullException>(() => session.Save<SimpleEntity>(null));
    }

    [Fact]
    public void ShouldReturnRevisionAndId()
    {
      var documentInfo = DoSave();
      Assert.Equal(entity.Id, documentInfo.Id);
      Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", documentInfo.Revision);
    }

    [Fact]
    public void ShouldReturnFillRevisionPropertyOnEntity()
    {
      DoSave();
      Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
    }

    [Fact]
    public void ShouldAssignOnSaveIfNoneWasAssignedBefore()
    {
      var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
      couchApiMock
        .Setup(ca => ca.SaveDocumentToDb(It.IsAny<string>(), It.IsAny<Document>()))
        .Returns(
          (string docId, JObject doc) =>
          new
          {
            id = docId,
            rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
          }.ToDocument()
        );

      var savingEntity = new SimpleEntity
      {
        Name = "John Smith"
      };
      var session = new CouchSession(Default.Settings, couchApiMock.Object);
      var docInfo = session.Save(savingEntity);

      Assert.NotNull(savingEntity.Id);
      Assert.NotEqual(string.Empty, savingEntity.Id);
      Assert.Equal(savingEntity.Id, docInfo.Id);
    }

    private DocumentInfo DoSave(
      SimpleEntity savingEntity = null, 
      Mock<ICouchApi> couchApiMock = null,
      Func<ISession, DocumentInfo> action = null)
    {
      savingEntity = savingEntity ?? entity;
      if (couchApiMock == null)
      {
        couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
        couchApiMock
          .Setup(ca => ca.SaveDocumentToDb(It.IsAny<string>(), It.IsAny<Document>()))
          .Returns(new {
            id = savingEntity == null? null: savingEntity.Id,
            rev = "42-1a517022a0c2d4814d51abfedf9bfee7"
          }.ToJsonFragment());
      }
      
      var session = new CouchSession(Default.Settings, couchApiMock.Object);

      if (action == null)
        return session.Save(savingEntity);
      else
        return action(session);
    }
  }
}