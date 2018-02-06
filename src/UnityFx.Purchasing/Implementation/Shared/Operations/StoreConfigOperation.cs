// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Shared functionality of initialize/fetch operations.
	/// </summary>
	internal abstract class StoreConfigOperation : StoreOperation
	{
		#region data

		private class CompletionSource : AsyncCompletionSource<StoreConfig>
		{
			private readonly StoreConfigOperation _op;
			private bool _completed;

			public CompletionSource(StoreConfigOperation op)
			{
				_op = op;
			}

			public override bool TrySetCanceled()
			{
				if (!_completed)
				{
					_completed = true;
					_op.SetFailedQueued(null);
					return true;
				}

				return false;
			}

			public override bool TrySetException(Exception e)
			{
				if (!_completed)
				{
					_completed = true;
					_op.SetFailedQueued(e);
					return true;
				}

				return false;
			}

			public override bool TrySetResult(StoreConfig result)
			{
				if (!_completed)
				{
					_completed = true;
					_op.TryInitiateQueued(result);
					return true;
				}

				return false;
			}
		}

		private int _threadId;

		#endregion

		#region interface

		public StoreConfigOperation(IStoreOperationOwner parent, StoreOperationType opId, AsyncCallback asyncCallback, object asyncState)
			: base(parent, opId, asyncCallback, asyncState, null)
		{
			_threadId = Thread.CurrentThread.ManagedThreadId;
		}

		public new void SetCompleted()
		{
			if (TrySetCompleted(false))
			{
				InvokeCompleted(StoreFetchError.None, null);
			}
		}

		public void SetFailed(StoreFetchError reason)
		{
			TraceError(reason.ToString());

			if (TrySetException(new StoreFetchException(this, reason), false))
			{
				InvokeCompleted(reason, Exception);
			}
		}

		public void SetFailed(StoreFetchError reason, Exception e)
		{
			TraceException(e);

			if (TrySetException(new StoreFetchException(this, reason, e), false))
			{
				InvokeCompleted(reason, e);
			}
		}

		public void SetFailed(Exception e, bool completedSynchronously = false)
		{
			TraceException(e);

			if (TrySetException(e, completedSynchronously))
			{
				InvokeCompleted(e is StoreFetchException sfe ? sfe.Reason : StoreFetchError.Unknown, e);
			}
		}

		public IAsyncCompletionSource<StoreConfig> GetCompletionSource()
		{
			return new CompletionSource(this);
		}

		protected abstract void InvokeCompleted(StoreFetchError reason, Exception e);
		protected abstract void Initiate(StoreConfig storeConfig);

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

		private void TryInitiateQueued(StoreConfig storeConfig)
		{
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

		private void SetFailedQueued(Exception e)
		{
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

		#endregion
	}
}
