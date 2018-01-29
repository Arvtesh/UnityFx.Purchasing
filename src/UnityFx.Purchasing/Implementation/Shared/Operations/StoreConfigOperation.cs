// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Shared functionality of initialize/fetch operations.
	/// </summary>
	internal abstract class StoreConfigOperation : StoreOperation, IAsyncCompletionSource<StoreConfig>
	{
		#region data

		private int _threadId;
		private bool _getConfigCompleted;

		#endregion

		#region interface

		public StoreConfigOperation(IStoreOperationOwner parent, StoreOperationType opId, AsyncCallback asyncCallback, object asyncState)
			: base(parent, opId, asyncCallback, asyncState, null)
		{
			_threadId = Thread.CurrentThread.ManagedThreadId;
		}

		public void SetCompleted()
		{
			if (TrySetCompleted())
			{
				InvokeCompleted();
			}
		}

		public void SetFailed(StoreFetchError reason)
		{
			TraceError(reason.ToString());

			if (TrySetException(new StoreFetchException(this, reason)))
			{
				InvokeFailed(reason, null);
			}
		}

		public void SetFailed(StoreFetchError reason, Exception e)
		{
			TraceException(e);

			if (TrySetException(new StoreFetchException(this, reason, e)))
			{
				InvokeFailed(reason, e);
			}
		}

		public void SetFailed(Exception e, bool completedSynchronously = false)
		{
			TraceException(e);

			if (TrySetException(e, completedSynchronously))
			{
				InvokeFailed(e is StoreFetchException sfe ? sfe.Reason : StoreFetchError.Unknown, e);
			}
		}

		protected abstract void InvokeCompleted();
		protected abstract void InvokeFailed(StoreFetchError reason, Exception e);
		protected abstract void Initiate(StoreConfig storeConfig);

		#endregion

		#region IAsyncCompletionSource

		void IAsyncCompletionSource<StoreConfig>.SetResult(StoreConfig storeConfig)
		{
			if (!_getConfigCompleted)
			{
				_getConfigCompleted = true;

				// No need to queue the action if we got result synchronously.
				if (_threadId == Thread.CurrentThread.ManagedThreadId)
				{
					TryInitiate(storeConfig);
				}
				else
				{
					Store.QueueOnMainThread(
						args =>
						{
							if (!IsCompleted)
							{
								TryInitiate(args as StoreConfig);
							}
						},
						storeConfig);
				}
			}
		}

		void IAsyncCompletionSource<StoreConfig>.SetException(Exception e)
		{
			if (!_getConfigCompleted)
			{
				_getConfigCompleted = true;

				// No need to queue the action if we got result synchronously.
				if (_threadId == Thread.CurrentThread.ManagedThreadId)
				{
					SetFailed(StoreFetchError.StoreConfigUnavailable, e);
				}
				else
				{
					Store.QueueOnMainThread(
						args =>
						{
							if (!IsCompleted)
							{
								SetFailed(StoreFetchError.StoreConfigUnavailable, args as Exception);
							}
						},
						e);
				}
			}
		}

		#endregion

		#region implementation

		private void TryInitiate(StoreConfig storeConfig)
		{
			if (storeConfig != null)
			{
				try
				{
					Initiate(storeConfig);
				}
				catch (Exception e)
				{
					SetFailed(e);
				}
			}
			else
			{
				SetFailed(StoreFetchError.StoreConfigUnavailable, new ArgumentNullException(nameof(storeConfig)));
			}
		}

		#endregion
	}
}
