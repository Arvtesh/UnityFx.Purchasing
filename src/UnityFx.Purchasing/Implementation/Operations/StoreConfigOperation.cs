// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
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
		#endregion

		#region interface

		public StoreConfigOperation(StoreService store, StoreOperationType opId, AsyncCallback asyncCallback, object asyncState)
			: base(store, opId, asyncCallback, asyncState, null)
		{
		}

		public void SetCompleted()
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

		protected abstract void InvokeCompleted(StoreFetchError reason, Exception e);
		protected abstract void Initiate(StoreConfig storeConfig);

		protected override void OnStarted()
		{
			base.OnStarted();

			var op = Store.GetStoreConfig();

			if (op == null)
			{
				throw new StoreFetchException(this, StoreFetchError.StoreConfigUnavailable);
			}
			else if (!op.TryAddCompletionCallback(OnGetConfigCompleted, Store.SyncContext))
			{
				OnGetConfigCompleted(op);
			}
		}

		#endregion

		#region implementation

		private void OnGetConfigCompleted(IAsyncOperation op)
		{
			if (op.IsCompletedSuccessfully)
			{
				TryInitiate((op as IAsyncOperation<StoreConfig>).Result);
			}
			else
			{
				SetFailed(StoreFetchError.StoreConfigUnavailable, op.Exception);
			}
		}

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
