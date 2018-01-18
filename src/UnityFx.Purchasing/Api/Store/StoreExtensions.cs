// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
#if !NET35
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
#endif

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Extensions for <see cref="IStoreService"/>.
	/// </summary>
	public static class StoreExtensions
	{
		/// <summary>
		/// Blocks calling thread until the operation is completed.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is <see langword="null"/>.</exception>
		public static void Join(this IStoreOperation op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			if (op.Exception != null)
			{
#if NET35
				throw op.Exception;
#else
				ExceptionDispatchInfo.Capture(op.Exception).Throw();
#endif
			}
		}

		/// <summary>
		/// Blocks calling thread until the operation is completed.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is <see langword="null"/>.</exception>
		public static T Join<T>(this IStoreOperation<T> op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			if (op.Exception != null)
			{
#if NET35
				throw op.Exception;
#else
				ExceptionDispatchInfo.Capture(op.Exception).Throw();
#endif
			}

			return op.Result;
		}
	}
}
