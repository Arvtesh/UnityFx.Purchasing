// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Shared functionality of initialize/fetch operations.
	/// </summary>
	internal abstract class StoreConfigOperation : StoreOperation
	{
		#region interface

		public StoreConfigOperation(StoreOperationContainer parent, StoreOperationId opId, AsyncCallback asyncCallback, object asyncState)
			: base(parent, opId, asyncCallback, asyncState, null, null)
		{
		}

		public void Initiate()
		{
			Store.GetStoreConfig(GetConfigCallback, GetConfigErrorCallback);
		}

		public void SetCompleted()
		{
			if (TrySetResult(null))
			{
				InvokeCompleted();
			}
		}

		public void SetFailed(StoreFetchError reason)
		{
			Console.TraceError(Type, reason.ToString());

			if (TrySetException(new StoreFetchException(reason)))
			{
				InvokeFailed(reason, null);
			}
		}

		public void SetFailed(StoreFetchError reason, Exception e)
		{
			Console.TraceException(Type, e);

			if (TrySetException(new StoreFetchException(reason, e)))
			{
				InvokeFailed(reason, e);
			}
		}

		public void SetFailed(Exception e, bool completedSynchronously = false)
		{
			Console.TraceException(Type, e);

			if (TrySetException(e, completedSynchronously))
			{
				InvokeFailed(e is StoreFetchException sfe ? sfe.Reason : StoreFetchError.Unknown, e);
			}
		}

		protected abstract void InvokeCompleted();
		protected abstract void InvokeFailed(StoreFetchError reason, Exception e);
		protected abstract void Initiate(StoreConfig storeConfig);

		#endregion

		#region implementation

		private void GetConfigCallback(StoreConfig storeConfig)
		{
			if (!IsCompleted)
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
					SetFailed(new ArgumentNullException(nameof(storeConfig)));
				}
			}
		}

		private void GetConfigErrorCallback(Exception e)
		{
			if (!IsCompleted)
			{
				SetFailed(e);
			}
		}

		#endregion
	}
}
