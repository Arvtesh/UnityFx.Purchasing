// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// tt
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
		/// Gets a read-only collection of the store products.
		/// </summary>
		public IStoreProductCollection<T> ProductsEx => _products;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="purchasingModule">A purchasing module. Typically an instance of built-in <c>StandardPurchasingModule</c>.</param>
		protected StoreService(IPurchasingModule purchasingModule)
			: base(purchasingModule)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="purchasingModule">A purchasing module. Typically an instance of built-in <c>StandardPurchasingModule</c>.</param>
		/// <param name="syncContext">A synchronization context used to forward execution to the main thread.</param>
		protected StoreService(IPurchasingModule purchasingModule, SynchronizationContext syncContext)
			: base(purchasingModule, syncContext)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="name">Name of the store service (<c>Purchasing</c> is used by default).</param>
		/// <param name="purchasingModule">A purchasing module. Typically an instance of built-in <c>StandardPurchasingModule</c>.</param>
		protected StoreService(string name, IPurchasingModule purchasingModule)
			: base(name, purchasingModule)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{T}"/> class.
		/// </summary>
		/// <param name="name">Name of the store service (<c>Purchasing</c> is used by default).</param>
		/// <param name="purchasingModule">A purchasing module. Typically an instance of built-in <c>StandardPurchasingModule</c>.</param>
		/// <param name="syncContext">A synchronization context used to forward execution to the main thread.</param>
		protected StoreService(string name, IPurchasingModule purchasingModule, SynchronizationContext syncContext)
			: base(name, purchasingModule, syncContext)
		{
		}

		/// <summary>
		/// tttt
		/// </summary>
		/// <param name="unityProduct"><c>Unity3d</c> product.</param>
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

		#region implementation

		private void ResetProducts()
		{
			_products.Clear();

			foreach (var unityProduct in Products)
			{
				_products.Add(unityProduct.definition.id, CreateProduct(unityProduct));
			}
		}

		#endregion
	}
}
