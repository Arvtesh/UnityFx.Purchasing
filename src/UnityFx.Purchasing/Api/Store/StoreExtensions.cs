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

#if !NET35
		/// <summary>
		/// Initializes the store. Does nothing (returns a completed task) if already initialized.
		/// </summary>
		/// <param name="store">The store service.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous initialize operation from other operations.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="store"/> is <see langword="null"/>.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if initialization fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap"/>
		/// <seealso cref="FetchAsync(IStoreService, object)"/>
		/// <seealso cref="PurchaseAsync(IStoreService, string, object)"/>
		/// <seealso cref="IStoreService"/>
		public static Task InitializeAsync(this IStoreService store, object stateObject = null)
		{
			if (store == null)
			{
				throw new ArgumentNullException(nameof(store));
			}

			return Task.Factory.FromAsync(store.BeginInitialize, store.EndInitialize, stateObject);
		}

		/// <summary>
		/// Fetches product information from the store.
		/// </summary>
		/// <param name="store">The store service.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous initialize operation from other operations.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="store"/> is <see langword="null"/>.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if fetching fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap"/>
		/// <seealso cref="InitializeAsync(IStoreService, object)"/>
		/// <seealso cref="PurchaseAsync(IStoreService, string, object)"/>
		/// <seealso cref="IStoreService"/>
		public static Task FetchAsync(this IStoreService store, object stateObject = null)
		{
			if (store == null)
			{
				throw new ArgumentNullException(nameof(store));
			}

			return Task.Factory.FromAsync(store.BeginFetch, store.EndFetch, stateObject);
		}

		/// <summary>
		/// Initiates purchasing the specified product.
		/// </summary>
		/// <param name="store">The store service.</param>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous initialize operation from other operations.</param>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="store"/> or <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases (for example another purchase operation is pending).</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if the store initialization/fetch triggered/awaited by the call fails.</exception>
		/// <exception cref="StorePurchaseException">Thrown in case of purchase-related errors.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap"/>
		/// <seealso cref="InitializeAsync(IStoreService, object)"/>
		/// <seealso cref="FetchAsync(IStoreService, object)"/>
		/// <seealso cref="IStoreService"/>
		public static Task<PurchaseResult> PurchaseAsync(this IStoreService store, string productId, object stateObject)
		{
			if (store == null)
			{
				throw new ArgumentNullException(nameof(store));
			}

			return Task.Factory.FromAsync(store.BeginPurchase, store.EndPurchase, productId, stateObject);
		}
#endif
	}
}
