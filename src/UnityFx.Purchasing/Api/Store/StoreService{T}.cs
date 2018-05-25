﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic in-app store based on <c>Unity IAP</c> for user-defined products.
	/// </summary>
	/// <typeparam name="T">ttt</typeparam>
	/// <threadsafety static="true" instance="false"/>
	public abstract class StoreService<T> : StoreService
	{
		#region data

		private readonly StoreProductCollection<T> _products = new StoreProductCollection<T>();

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		protected StoreService()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="syncContext">A synchronization context used to forward execution to the main thread.</param>
		protected StoreService(SynchronizationContext syncContext)
			: base(syncContext)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="name">Name of the store service (<c>Purchasing</c> is used by default).</param>
		protected StoreService(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="name">Name of the store service (<c>Purchasing</c> is used by default).</param>
		/// <param name="syncContext">A synchronization context used to forward execution to the main thread.</param>
		protected StoreService(string name, SynchronizationContext syncContext)
			: base(name, syncContext)
		{
		}

		/// <summary>
		/// Creates a user-defened product for the specified <c>Unity3d</c> product.
		/// </summary>
		/// <param name="unityProduct"><c>Unity3d</c> product instance.</param>
		/// <returns>A user-defined product instance matching the <c>Unity</c> product specified.</returns>
		protected abstract T CreateProduct(Product unityProduct);

		#endregion

		#region StoreService

		/// <inheritdoc/>
		protected internal override void OnInitializeCompleted(IStoreOperationInfo op, StoreFetchError failReason, Exception e)
		{
			if (failReason == StoreFetchError.None)
			{
				ResetProducts();
			}

			base.OnInitializeCompleted(op, failReason, e);
		}

		/// <inheritdoc/>
		protected internal override void OnFetchCompleted(IStoreOperationInfo op, StoreFetchError failReason, Exception e)
		{
			if (failReason == StoreFetchError.None)
			{
				ResetProducts();
			}

			base.OnFetchCompleted(op, failReason, e);
		}

		#endregion

		#region IStoreService

		/// <inheritdoc/>
		public new IStoreProductCollection<T> Products => _products;

		#endregion

		#region implementation

		private void ResetProducts()
		{
			_products.Clear();

			foreach (var unityProduct in base.Products)
			{
				_products.Add(unityProduct.definition.id, CreateProduct(unityProduct));
			}
		}

		#endregion
	}
}