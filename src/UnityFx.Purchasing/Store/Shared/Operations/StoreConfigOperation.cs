// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Shared functionality of initialize/fetch operations.
	/// </summary>
	internal abstract class StoreConfigOperation : StoreOperation<object>
	{
		#region interface

		public StoreConfigOperation(StoreOperationContainer parent, TraceEventId eventId)
			: base(parent, eventId, null, null)
		{
		}

		public void Initiate()
		{
			try
			{
				Store.GetStoreConfig(GetConfigCallback, GetConfigErrorCallback);
			}
			catch (Exception e)
			{
				SetFailed(e);
				throw;
			}
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
			TraceError(reason.ToString());

			if (TrySetException(new StoreFetchException(reason)))
			{
				InvokeFailed(reason, null);
			}
		}

		public void SetFailed(Exception e)
		{
			TraceException(e);

			if (TrySetException(e))
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
