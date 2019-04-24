// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Text;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic purchase exception.
	/// </summary>
	[Serializable]
	public sealed class PurchaseValidationException : StoreException
	{
		#region data

		private readonly Product _product;

		#endregion

		#region interface

		/// <summary>
		/// Gets the product.
		/// </summary>
		public Product Product
		{
			get
			{
				return _product;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationException"/> class.
		/// </summary>
		public PurchaseValidationException(Product product)
			: base(GetMessage(product, null))
		{
			_product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationException"/> class.
		/// </summary>
		public PurchaseValidationException(Product product, string reason)
			: base(GetMessage(product, reason))
		{
			_product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationException"/> class.
		/// </summary>
		public PurchaseValidationException(Product product, string reason, Exception innerException)
			: base(GetMessage(product, reason), innerException)
		{
			_product = product;
		}

		#endregion

		#region implementation

		private static string GetMessage(Product product, string reason)
		{
			var text = new StringBuilder();

			text.AppendFormat("In-app purchase validation failed for product {0}.", product.definition.id);

			if (!string.IsNullOrEmpty(reason))
			{
				text.AppendFormat(" {0}.", reason);
			}

			return text.ToString();
		}

		#endregion
	}
}
