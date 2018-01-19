// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A completed store operation with a result.
	/// </summary>
	internal sealed class CompletedStoreOperation : IStoreOperation, IEnumerator
	{
		#region data

		private static IStoreOperation _instance;

		private EventWaitHandle _waitHandle;

		#endregion

		#region interface

		public static IStoreOperation Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new CompletedStoreOperation();
				}

				return _instance;
			}
		}

		#endregion

		#region IAsyncOperation

		public Exception Exception => null;
		public bool IsCompletedSuccessfully => true;
		public bool IsFaulted => false;
		public bool IsCanceled => false;

		#endregion

		#region IAsyncResult

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (_waitHandle == null)
				{
					var mre = new ManualResetEvent(true);

					if (Interlocked.CompareExchange(ref _waitHandle, mre, null) != null)
					{
						// Another thread created this object's event; dispose the event we just created.
						mre.Close();
					}
				}

				return _waitHandle;
			}
		}

		public object AsyncState => null;
		public bool CompletedSynchronously => true;
		public bool IsCompleted => true;

		#endregion

		#region IEnumerator

		public object Current => null;
		public bool MoveNext() => false;
		public void Reset() => throw new NotSupportedException();

		#endregion

		#region IDisposable

		public void Dispose()
		{
			_waitHandle?.Close();
			_waitHandle = null;
		}

		#endregion
	}
}
