using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
{
	class SessionUnitOfWork
	{
		private object lockHandle = new object();
		private bool locked;

		private readonly HashSet<DocumentEntity> documentEntities	 = new HashSet<DocumentEntity>();

		/// <summary>Attaches already persisted document entity to the unit.</summary>
		public DocumentEntity Attach(DocumentEntity documentEntity)
		{
			if (documentEntity == null) throw new ArgumentNullException("documentEntity");
 
			return documentEntity;
		}

		/// <summary>Adds new document entity (without document part actually) to the unit.</summary>
		public DocumentEntity AddNew(DocumentEntity documentEntity)
		{
			if (documentEntity == null) throw new ArgumentNullException("documentEntity");
			documentEntities.Add(documentEntity);
			return documentEntity;
		}

		/// <summary>Marks document entity as deleted from the store forsing other <see cref="SessionUnitOfWork"/> methods
		/// to behave as if it have been already removed.</summary>
		public DocumentEntity MarkAsRemoved(DocumentEntity documentEntity)
		{
			if (documentEntity == null) throw new ArgumentNullException("documentEntity");
 
			return documentEntity;
		}

		public void LockOrThrowIfAlreadyLocked(Func<Exception> exceptionFactory)
		{
			if (exceptionFactory == null) throw new ArgumentNullException("exceptionFactory");

			lock (lockHandle)
				if (locked)
				{
					var exception = exceptionFactory();
					if (exception == null)
						throw new ArgumentException("Exception should always return not-null exception");
					throw exception;
				}
				else
					locked = true;
		}

		public void Unlok()
		{
			lock (lockHandle)
				locked = false;
		}

		/// <summary>Translates session unit of work to CouchApi bulk update unit of work.</summary>
		public void ApplyChanges(IBulkUpdateUnitOfWork work)
		{
			if (work == null) throw new ArgumentNullException("work");

			foreach (var documentEntity in documentEntities)
			{
				if(documentEntity.Document == null) // It's a new document entity
				{
					documentEntity.
					work.Create(documentEntity);
				}
			}
		}
	}

	public class SessionUnitOfWorkTests
	{
		SessionUnitOfWork unitOfWork = new SessionUnitOfWork();

		[Fact]
		public void ShouldSaveNewDocuments() 
		{
			var entity = new SimpleEntity { Age = 42, Id = "user1" };
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			unitOfWork.AddNew(documentEntity);

			IDocument savedDoc = null;
			var bulkUpdateUnitOfWorkMock = new Mock<IBulkUpdateUnitOfWork>(MockBehavior.Strict);
			bulkUpdateUnitOfWorkMock.Setup(u => u.Create(It.IsAny<IDocument>())).Callback<IDocument>(d => { savedDoc = d; });

			Assert.Equal(new { _id = "user1", age = 42 }.ToJsonString(), savedDoc.ToString());
		}


		[Fact]
		public void ShouldCheckNullArguments() 
		{
			Assert.Throws<ArgumentNullException>(() => unitOfWork.Attach(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.AddNew(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.MarkAsRemoved(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.LockOrThrowIfAlreadyLocked(null));
			Assert.Throws<ArgumentNullException>(() => unitOfWork.ApplyChanges(null));
		}

		[Fact]
		public void ShouldThrowProvidedExceptionIfAlreadyLocked() 
		{
			unitOfWork.LockOrThrowIfAlreadyLocked(() => new Exception());

			var exception = new InvalidOperationException(Guid.NewGuid().ToString());
			var thrownException =
				Assert.Throws<InvalidOperationException>(() => unitOfWork.LockOrThrowIfAlreadyLocked(() => exception));
			
			Assert.Same(exception, thrownException);
		}

		[Fact]
		public void ShouldThrowArgumentExceptionOnNullReturningFactory() 
		{
			unitOfWork.LockOrThrowIfAlreadyLocked(() => new Exception());
			Assert.Throws<ArgumentException>(() => unitOfWork.LockOrThrowIfAlreadyLocked(() => null));
		}

		[Fact]
		public void ShouldNotThrowIfUnlocked() 
		{
			unitOfWork.LockOrThrowIfAlreadyLocked(() => new Exception());
			unitOfWork.Unlok();
			Assert.DoesNotThrow(() => unitOfWork.LockOrThrowIfAlreadyLocked(() => new Exception()));	
		}
	}
}
