// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Json;
using System.Net;
#if NETSTANDARD1_3
using System.Net.Http;
#endif
using System.Text;
using System.Threading.Tasks;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// Apple App Store validation helpers.
	/// </summary>
	internal static class AppStoreValidator
	{
		#region data

		private const string _statusValueName = "status";
		private const string _receiptValueName = "receipt";
		private const string _environmentValueName = "environment";
		private const string _bundleIdValueName = "bundle_id";
		private const string _appVersionValueName = "application_version";
		private const string _originalAppVersionValueName = "original_application_version";
		private const string _receiptCreationDateValueName = "receipt_creation_date";
		private const string _expirationDateValueName = "expiration_date";
		private const string _inAppValueName = "in_app";
		private const string _quantityValueName = "quantity";
		private const string _productIdValueName = "product_id";
		private const string _transactionIdValueName = "transaction_id";
		private const string _originalTransactionIdValueName = "original_transaction_id";
		private const string _purchaseDateValueName = "purchase_date";
		private const string _originalPurchaseDateValueName = "original_purchase_date";
		private const string _subscriptionExpirationDateValueName = "expires_date";
		private const string _subscriptionExpirationIntentValueName = "expiration_intent";
		private const string _subscriptionRetryFlagValueName = "is_in_billing_retry_period";
		private const string _subscriptionTrialPeriodValueName = "is_trial_period";
		private const string _subscriptionAutoRenewStatusValueName = "auto_renew_status";
		private const string _subscriptionAutoRenewPreferenceValueName = "auto_renew_product_id";
		private const string _subscriptionPriceConsentStatusValueName = "price_consent_status";
		private const string _cancellationDateValueName = "cancellation_date";
		private const string _cancellationReasonValueName = "cancellation_reason";
		private const string _appItemIdValueName = "app_item_id";
		private const string _externalVersionIdValueName = "version_external_identifier";
		private const string _webOrderLineItemIdValueName = "web_order_line_item_id";

		private static string[] _rfc3339DateTimePatterns = new string[]
		{
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ssK",

			// Fall back patterns
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK",
			"yyyy'-'MM'-'dd HH':'mm':'ss",
			"yyyy'-'MM'-'dd HH':'mm':'ss' Etc/GMT'",
			DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern,
			DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern
		};

		#endregion

		#region interface

		internal const string TestReceipt1 = @"MIISpgYJKoZIhvcNAQcCoIISlzCCEpMCAQExCzAJBgUrDgMCGgUAMIICRwYJKoZIhvcNAQcBoIICOASCAjQxggIwMAoCARQCAQEEAgwAMAsCAQ4CAQEEAwIBTjALAgEZAgEBBAMCAQMwDQIBCgIBAQQFFgMxMiswDQIBCwIBAQQFAgMETJIwDQIBDQIBAQQFAgMBOawwDgIBAQIBAQQGAgRCzLtSMA4CAQkCAQEEBgIEUDI0NzAOAgEQAgEBBAYCBDDuF88wEAIBDwIBAQQIAgYilYYST3gwEwIBAwIBAQQLDAkxMDcwNDAwMTEwEwIBEwIBAQQLDAkxMDYwMjA0NTEwFAIBAAIBAQQMDApQcm9kdWN0aW9uMBgCAQQCAQIEEN5vz31AX36y1xhxCCVk0F8wHAIBBQIBAQQU1qctljZKsomrtNVup369nUcFCrUwHgIBCAIBAQQWFhQyMDE3LTAyLTIwVDIzOjU5OjIzWjAeAgEMAgEBBBYWFDIwMTctMDItMjBUMjM6NTk6MjNaMB4CARICAQEEFhYUMjAxNy0wMS0yMFQwMDowNzoyM1owIgIBAgIBAQQaDBhjb20uZ3NuLlZlZ2FzRG9sbGFyU2xvdHMwSQIBBgIBAQRBv7bLIcE+P4IC\/lMN5wICZ73gg77W0kGnnyDAjGVhWRpfXqaAip1uH9Jo9Ux70mYxv\/WAndmW5H9I1sADSEynqXQwUgIBBwIBAQRK0ltZm5p8zZjVDC55+9zQpXjiIxwDIAoyBiCdPVlzpxQdriSDFxM\/AlobF5\/o1VROd5jpsBvDZvLdK2e\/4fkIG+d1IIrs6wUbmaWggg5lMIIFfDCCBGSgAwIBAgIIDutXh+eeCY0wDQYJKoZIhvcNAQEFBQAwgZYxCzAJBgNVBAYTAlVTMRMwEQYDVQQKDApBcHBsZSBJbmMuMSwwKgYDVQQLDCNBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9uczFEMEIGA1UEAww7QXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwHhcNMTUxMTEzMDIxNTA5WhcNMjMwMjA3MjE0ODQ3WjCBiTE3MDUGA1UEAwwuTWFjIEFwcCBTdG9yZSBhbmQgaVR1bmVzIFN0b3JlIFJlY2VpcHQgU2lnbmluZzEsMCoGA1UECwwjQXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMxEzARBgNVBAoMCkFwcGxlIEluYy4xCzAJBgNVBAYTAlVTMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApc+B\/SWigVvWh+0j2jMcjuIjwKXEJss9xp\/sSg1Vhv+kAteXyjlUbX1\/slQYncQsUnGOZHuCzom6SdYI5bSIcc8\/W0YuxsQduAOpWKIEPiF41du30I4SjYNMWypoN5PC8r0exNKhDEpYUqsS4+3dH5gVkDUtwswSyo1IgfdYeFRr6IwxNh9KBgxHVPM3kLiykol9X6SFSuHAnOC6pLuCl2P0K5PB\/T5vysH1PKmPUhrAJQp2Dt7+mf7\/wmv1W16sc1FJCFaJzEOQzI6BAtCgl7ZcsaFpaYeQEGgmJjm4HRBzsApdxXPQ33Y72C3ZiB7j7AfP4o7Q0\/omVYHv4gNJIwIDAQABo4IB1zCCAdMwPwYIKwYBBQUHAQEEMzAxMC8GCCsGAQUFBzABhiNodHRwOi8vb2NzcC5hcHBsZS5jb20vb2NzcDAzLXd3ZHIwNDAdBgNVHQ4EFgQUkaSc\/MR2t5+givRN9Y82Xe0rBIUwDAYDVR0TAQH\/BAIwADAfBgNVHSMEGDAWgBSIJxcJqbYYYIvs67r2R1nFUlSjtzCCAR4GA1UdIASCARUwggERMIIBDQYKKoZIhvdjZAUGATCB\/jCBwwYIKwYBBQUHAgIwgbYMgbNSZWxpYW5jZSBvbiB0aGlzIGNlcnRpZmljYXRlIGJ5IGFueSBwYXJ0eSBhc3N1bWVzIGFjY2VwdGFuY2Ugb2YgdGhlIHRoZW4gYXBwbGljYWJsZSBzdGFuZGFyZCB0ZXJtcyBhbmQgY29uZGl0aW9ucyBvZiB1c2UsIGNlcnRpZmljYXRlIHBvbGljeSBhbmQgY2VydGlmaWNhdGlvbiBwcmFjdGljZSBzdGF0ZW1lbnRzLjA2BggrBgEFBQcCARYqaHR0cDovL3d3dy5hcHBsZS5jb20vY2VydGlmaWNhdGVhdXRob3JpdHkvMA4GA1UdDwEB\/wQEAwIHgDAQBgoqhkiG92NkBgsBBAIFADANBgkqhkiG9w0BAQUFAAOCAQEADaYb0y4941srB25ClmzT6IxDMIJf4FzRjb69D70a\/CWS24yFw4BZ3+Pi1y4FFKwN27a4\/vw1LnzLrRdrjn8f5He5sWeVtBNephmGdvhaIJXnY4wPc\/zo7cYfrpn4ZUhcoOAoOsAQNy25oAQ5H3O5yAX98t5\/GioqbisB\/KAgXNnrfSemM\/j1mOC+RNuxTGf8bgpPyeIGqNKX86eOa1GiWoR1ZdEWBGLjwV\/1CKnPaNmSAMnBjLP4jQBkulhgwHyvj3XKablbKtYdaG6YQvVMpzcZm8w7HHoZQ\/Ojbb9IYAYMNpIr7N4YtRHaLSPQjvygaZwXG56AezlHRTBhL8cTqDCCBCIwggMKoAMCAQICCAHevMQ5baAQMA0GCSqGSIb3DQEBBQUAMGIxCzAJBgNVBAYTAlVTMRMwEQYDVQQKEwpBcHBsZSBJbmMuMSYwJAYDVQQLEx1BcHBsZSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTEWMBQGA1UEAxMNQXBwbGUgUm9vdCBDQTAeFw0xMzAyMDcyMTQ4NDdaFw0yMzAyMDcyMTQ4NDdaMIGWMQswCQYDVQQGEwJVUzETMBEGA1UECgwKQXBwbGUgSW5jLjEsMCoGA1UECwwjQXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMxRDBCBgNVBAMMO0FwcGxlIFdvcmxkd2lkZSBEZXZlbG9wZXIgUmVsYXRpb25zIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyjhUpstWqsgkOUjpjO7sX7h\/JpG8NFN6znxjgGF3ZF6lByO2Of5QLRVWWHAtfsRuwUqFPi\/w3oQaoVfJr3sY\/2r6FRJJFQgZrKrbKjLtlmNoUhU9jIrsv2sYleADrAF9lwVnzg6FlTdq7Qm2rmfNUWSfxlzRvFduZzWAdjakh4FuOI\/YKxVOeyXYWr9Og8GN0pPVGnG1YJydM05V+RJYDIa4Fg3B5XdFjVBIuist5JSF4ejEncZopbCj\/Gd+cLoCWUt3QpE5ufXN4UzvwDtIjKblIV39amq7pxY1YNLmrfNGKcnow4vpecBqYWcVsvD95Wi8Yl9uz5nd7xtj\/pJlqwIDAQABo4GmMIGjMB0GA1UdDgQWBBSIJxcJqbYYYIvs67r2R1nFUlSjtzAPBgNVHRMBAf8EBTADAQH\/MB8GA1UdIwQYMBaAFCvQaUeUdgn+9GuNLkCm90dNfwheMC4GA1UdHwQnMCUwI6AhoB+GHWh0dHA6Ly9jcmwuYXBwbGUuY29tL3Jvb3QuY3JsMA4GA1UdDwEB\/wQEAwIBhjAQBgoqhkiG92NkBgIBBAIFADANBgkqhkiG9w0BAQUFAAOCAQEAT8\/vWb4s9bJsL4\/uE4cy6AU1qG6LfclpDLnZF7x3LNRn4v2abTpZXN+DAb2yriphcrGvzcNFMI+jgw3OHUe08ZOKo3SbpMOYcoc7Pq9FC5JUuTK7kBhTawpOELbZHVBsIYAKiU5XjGtbPD2m\/d73DSMdC0omhz+6kZJMpBkSGW1X9XpYh3toiuSGjErr4kkUqqXdVQCprrtLMK7hoLG8KYDmCXflvjSiAcp\/3OIK5ju4u+y6YpXzBWNBgs0POx1MlaTbq\/nJlelP5E3nJpmB6bz5tCnSAXpm4S6M9iGKxfh44YGuv9OQnamt86\/9OBqWZzAcUaVc7HGKgrRsDwwVHzCCBLswggOjoAMCAQICAQIwDQYJKoZIhvcNAQEFBQAwYjELMAkGA1UEBhMCVVMxEzARBgNVBAoTCkFwcGxlIEluYy4xJjAkBgNVBAsTHUFwcGxlIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MRYwFAYDVQQDEw1BcHBsZSBSb290IENBMB4XDTA2MDQyNTIxNDAzNloXDTM1MDIwOTIxNDAzNlowYjELMAkGA1UEBhMCVVMxEzARBgNVBAoTCkFwcGxlIEluYy4xJjAkBgNVBAsTHUFwcGxlIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MRYwFAYDVQQDEw1BcHBsZSBSb290IENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5JGpCR+R2x5HUOsF7V55hC3rNqJXTFXsixmJ3vlLbPUHqyIwAugYPvhQCdN\/QaiY+dHKZpwkaxHQo7vkGyrDH5WeegykR4tb1BY3M8vED03OFGnRyRly9V0O1X9fm\/IlA7pVj01dDfFkNSMVSxVZHbOU9\/acns9QusFYUGePCLQg98usLCBvcLY\/ATCMt0PPD5098ytJKBrI\/s61uQ7ZXhzWyz21Oq30Dw4AkguxIRYudNU8DdtiFqujcZJHU1XBry9Bs\/j743DN5qNMRX4fTGtQlkGJxHRiCxCDQYczioGxMFjsWgQyjGizjx3eZXP\/Z15lvEnYdp8zFGWhd5TJLQIDAQABo4IBejCCAXYwDgYDVR0PAQH\/BAQDAgEGMA8GA1UdEwEB\/wQFMAMBAf8wHQYDVR0OBBYEFCvQaUeUdgn+9GuNLkCm90dNfwheMB8GA1UdIwQYMBaAFCvQaUeUdgn+9GuNLkCm90dNfwheMIIBEQYDVR0gBIIBCDCCAQQwggEABgkqhkiG92NkBQEwgfIwKgYIKwYBBQUHAgEWHmh0dHBzOi8vd3d3LmFwcGxlLmNvbS9hcHBsZWNhLzCBwwYIKwYBBQUHAgIwgbYagbNSZWxpYW5jZSBvbiB0aGlzIGNlcnRpZmljYXRlIGJ5IGFueSBwYXJ0eSBhc3N1bWVzIGFjY2VwdGFuY2Ugb2YgdGhlIHRoZW4gYXBwbGljYWJsZSBzdGFuZGFyZCB0ZXJtcyBhbmQgY29uZGl0aW9ucyBvZiB1c2UsIGNlcnRpZmljYXRlIHBvbGljeSBhbmQgY2VydGlmaWNhdGlvbiBwcmFjdGljZSBzdGF0ZW1lbnRzLjANBgkqhkiG9w0BAQUFAAOCAQEAXDaZTC14t+2Mm9zzd5vydtJ3ME\/BH4WDhRuZPUc38qmbQI4s1LGQEti+9HOb7tJkD8t5TzTYoj75eP9ryAfsfTmDi1Mg0zjEsb+aTwpr\/yv8WacFCXwXQFYRHnTTt4sjO0ej1W8k4uvRt3DfD0XhJ8rxbXjt57UXF6jcfiI1yiXV2Q\/Wa9SiJCMR96Gsj3OBYMYbWwkvkrL4REjwYDieFfU9JmcgijNq9w2Cz97roy\/5U2pbZMBjM3f3OgcsVuvaDyEO2rpzGU+12TZ\/wYdV2aeZuTJC+9jVcZ5+oVK3G72TQiQSKscPHbZNnF5jyEuAF1CqitXa5PzQCQc3sHV1ITGCAcswggHHAgEBMIGjMIGWMQswCQYDVQQGEwJVUzETMBEGA1UECgwKQXBwbGUgSW5jLjEsMCoGA1UECwwjQXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMxRDBCBgNVBAMMO0FwcGxlIFdvcmxkd2lkZSBEZXZlbG9wZXIgUmVsYXRpb25zIENlcnRpZmljYXRpb24gQXV0aG9yaXR5AggO61eH554JjTAJBgUrDgMCGgUAMA0GCSqGSIb3DQEBAQUABIIBAF\/SwmE\/g89dLI\/aTpJgXfClI4qN74L+tZZZ7xg8hUWnBwrVVzUfMYx58m2rhOra\/\/FsiniWr8N2WvdFtDasDIPRWEY4qlI0\/bg\/bJX+8WMVvPOQWE4Dz7jNlwKgJ39\/iGZdDJIak\/ws7rctQ9jktYzxoHEvRKrKujSr9zr2S8xnrxa+h+HIa4FmV0pKv1GC1jlmud4pSkYCfdGErJcXrXCVkLs5BsoARXsltZVGSmgVcnyPKeDOHHostUMTKRfIIyzSLOuV8QIRdKqhRLde7KSussVAcOkxwFpvA43HlhCPuzd5Sgp9Dc6l6+AI7wXxCsqMwNG0RoGkgM1IEYc15IE=";
		internal const string TestReceipt2 = @"MIIUCgYJKoZIhvcNAQcCoIIT+zCCE\/cCAQExCzAJBgUrDgMCGgUAMIIDqwYJKoZIhvcNAQcBoIIDnASCA5gxggOUMAoCARQCAQEEAgwAMAsCARkCAQEEAwIBAzAMAgEOAgEBBAQCAgCLMA0CAQoCAQEEBRYDMTIrMA0CAQsCAQEEBQIDD87QMA0CAQ0CAQEEBQIDAa2zMA4CAQECAQEEBgIERPUPTTAOAgEJAgEBBAYCBFAyNDkwDgIBEAIBAQQGAgQxGmWKMBACAQ8CAQEECAIGG1EQoMWlMBICAQMCAQEECgwIODAxMDEwMTEwEgIBEwIBAQQKDAg4MDEwMTAxMTAUAgEAAgEBBAwMClByb2R1Y3Rpb24wGAIBBAIBAgQQnXMfeO1hVP\/CY1X7ed95DzAcAgEFAgEBBBRpiYMFup3b4OI4hmHBExLZolkKyTAeAgEIAgEBBBYWFDIwMTctMTAtMjhUMDY6MDQ6NDBaMB4CAQwCAQEEFhYUMjAxNy0xMC0yOFQwNjowNDo0MFowHgIBEgIBAQQWFhQyMDE3LTEwLTI3VDAwOjM4OjU1WjAgAgECAgEBBBgMFmNvbS5taWl2aWVzLmhleGFnb25pdW0wRgIBBwIBAQQ+j+0bOIgogWtOUMpknTLTWmUicRWgLyBfwaZQPrtaOsv0spU4XQbmHCSfFfOrEMSg0th7Ur2TvB0ozyWmhnAwXQIBBgIBAQRVcrXLYBsp75YdcEofmI0A4KEvN6tN\/wUsk23H+8ZJWqu2vxCvT65qIBlgU2jlSBIaN39K3OYCOeyBcxmt5qDYVDeLaTx6McWosJG3pbobsZH695vKgzCCAVsCARECAQEEggFRMYIBTTALAgIGrAIBAQQCFgAwCwICBq0CAQEEAgwAMAsCAgawAgEBBAIWADALAgIGsgIBAQQCDAAwCwICBrMCAQEEAgwAMAsCAga0AgEBBAIMADALAgIGtQIBAQQCDAAwCwICBrYCAQEEAgwAMAwCAgalAgEBBAMCAQEwDAICBqsCAQEEAwIBATAMAgIGrwIBAQQDAgEAMAwCAgaxAgEBBAMCAQAwDwICBq4CAQEEBgIERagWCzAaAgIGpwIBAQQRDA8xMDAwMDAzNjMxODI5NTQwGgICBqkCAQEEEQwPMTAwMDAwMzYzMTgyOTU0MB8CAgaoAgEBBBYWFDIwMTctMTAtMjhUMDY6MDQ6NDBaMB8CAgaqAgEBBBYWFDIwMTctMTAtMjhUMDY6MDQ6NDBaMCACAgamAgEBBBcMFXNob3BoZXhhbml0ZXNwYWNrXzEwMKCCDmUwggV8MIIEZKADAgECAggO61eH554JjTANBgkqhkiG9w0BAQUFADCBljELMAkGA1UEBhMCVVMxEzARBgNVBAoMCkFwcGxlIEluYy4xLDAqBgNVBAsMI0FwcGxlIFdvcmxkd2lkZSBEZXZlbG9wZXIgUmVsYXRpb25zMUQwQgYDVQQDDDtBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9ucyBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTAeFw0xNTExMTMwMjE1MDlaFw0yMzAyMDcyMTQ4NDdaMIGJMTcwNQYDVQQDDC5NYWMgQXBwIFN0b3JlIGFuZCBpVHVuZXMgU3RvcmUgUmVjZWlwdCBTaWduaW5nMSwwKgYDVQQLDCNBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9uczETMBEGA1UECgwKQXBwbGUgSW5jLjELMAkGA1UEBhMCVVMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQClz4H9JaKBW9aH7SPaMxyO4iPApcQmyz3Gn+xKDVWG\/6QC15fKOVRtfX+yVBidxCxScY5ke4LOibpJ1gjltIhxzz9bRi7GxB24A6lYogQ+IXjV27fQjhKNg0xbKmg3k8LyvR7E0qEMSlhSqxLj7d0fmBWQNS3CzBLKjUiB91h4VGvojDE2H0oGDEdU8zeQuLKSiX1fpIVK4cCc4Lqku4KXY\/Qrk8H9Pm\/KwfU8qY9SGsAlCnYO3v6Z\/v\/Ca\/VbXqxzUUkIVonMQ5DMjoEC0KCXtlyxoWlph5AQaCYmObgdEHOwCl3Fc9DfdjvYLdmIHuPsB8\/ijtDT+iZVge\/iA0kjAgMBAAGjggHXMIIB0zA\/BggrBgEFBQcBAQQzMDEwLwYIKwYBBQUHMAGGI2h0dHA6Ly9vY3NwLmFwcGxlLmNvbS9vY3NwMDMtd3dkcjA0MB0GA1UdDgQWBBSRpJz8xHa3n6CK9E31jzZd7SsEhTAMBgNVHRMBAf8EAjAAMB8GA1UdIwQYMBaAFIgnFwmpthhgi+zruvZHWcVSVKO3MIIBHgYDVR0gBIIBFTCCAREwggENBgoqhkiG92NkBQYBMIH+MIHDBggrBgEFBQcCAjCBtgyBs1JlbGlhbmNlIG9uIHRoaXMgY2VydGlmaWNhdGUgYnkgYW55IHBhcnR5IGFzc3VtZXMgYWNjZXB0YW5jZSBvZiB0aGUgdGhlbiBhcHBsaWNhYmxlIHN0YW5kYXJkIHRlcm1zIGFuZCBjb25kaXRpb25zIG9mIHVzZSwgY2VydGlmaWNhdGUgcG9saWN5IGFuZCBjZXJ0aWZpY2F0aW9uIHByYWN0aWNlIHN0YXRlbWVudHMuMDYGCCsGAQUFBwIBFipodHRwOi8vd3d3LmFwcGxlLmNvbS9jZXJ0aWZpY2F0ZWF1dGhvcml0eS8wDgYDVR0PAQH\/BAQDAgeAMBAGCiqGSIb3Y2QGCwEEAgUAMA0GCSqGSIb3DQEBBQUAA4IBAQANphvTLj3jWysHbkKWbNPojEMwgl\/gXNGNvr0PvRr8JZLbjIXDgFnf4+LXLgUUrA3btrj+\/DUufMutF2uOfx\/kd7mxZ5W0E16mGYZ2+FogledjjA9z\/Ojtxh+umfhlSFyg4Cg6wBA3LbmgBDkfc7nIBf3y3n8aKipuKwH8oCBc2et9J6Yz+PWY4L5E27FMZ\/xuCk\/J4gao0pfzp45rUaJahHVl0RYEYuPBX\/UIqc9o2ZIAycGMs\/iNAGS6WGDAfK+PdcppuVsq1h1obphC9UynNxmbzDscehlD86Ntv0hgBgw2kivs3hi1EdotI9CO\/KBpnBcbnoB7OUdFMGEvxxOoMIIEIjCCAwqgAwIBAgIIAd68xDltoBAwDQYJKoZIhvcNAQEFBQAwYjELMAkGA1UEBhMCVVMxEzARBgNVBAoTCkFwcGxlIEluYy4xJjAkBgNVBAsTHUFwcGxlIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MRYwFAYDVQQDEw1BcHBsZSBSb290IENBMB4XDTEzMDIwNzIxNDg0N1oXDTIzMDIwNzIxNDg0N1owgZYxCzAJBgNVBAYTAlVTMRMwEQYDVQQKDApBcHBsZSBJbmMuMSwwKgYDVQQLDCNBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9uczFEMEIGA1UEAww7QXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDKOFSmy1aqyCQ5SOmM7uxfuH8mkbw0U3rOfGOAYXdkXqUHI7Y5\/lAtFVZYcC1+xG7BSoU+L\/DehBqhV8mvexj\/avoVEkkVCBmsqtsqMu2WY2hSFT2Miuy\/axiV4AOsAX2XBWfODoWVN2rtCbauZ81RZJ\/GXNG8V25nNYB2NqSHgW44j9grFU57Jdhav06DwY3Sk9UacbVgnJ0zTlX5ElgMhrgWDcHld0WNUEi6Ky3klIXh6MSdxmilsKP8Z35wugJZS3dCkTm59c3hTO\/AO0iMpuUhXf1qarunFjVg0uat80YpyejDi+l5wGphZxWy8P3laLxiX27Pmd3vG2P+kmWrAgMBAAGjgaYwgaMwHQYDVR0OBBYEFIgnFwmpthhgi+zruvZHWcVSVKO3MA8GA1UdEwEB\/wQFMAMBAf8wHwYDVR0jBBgwFoAUK9BpR5R2Cf70a40uQKb3R01\/CF4wLgYDVR0fBCcwJTAjoCGgH4YdaHR0cDovL2NybC5hcHBsZS5jb20vcm9vdC5jcmwwDgYDVR0PAQH\/BAQDAgGGMBAGCiqGSIb3Y2QGAgEEAgUAMA0GCSqGSIb3DQEBBQUAA4IBAQBPz+9Zviz1smwvj+4ThzLoBTWobot9yWkMudkXvHcs1Gfi\/ZptOllc34MBvbKuKmFysa\/Nw0Uwj6ODDc4dR7Txk4qjdJukw5hyhzs+r0ULklS5MruQGFNrCk4QttkdUGwhgAqJTleMa1s8Pab93vcNIx0LSiaHP7qRkkykGRIZbVf1eliHe2iK5IaMSuviSRSqpd1VAKmuu0swruGgsbwpgOYJd+W+NKIByn\/c4grmO7i77LpilfMFY0GCzQ87HUyVpNur+cmV6U\/kTecmmYHpvPm0KdIBembhLoz2IYrF+Hjhga6\/05Cdqa3zr\/04GpZnMBxRpVzscYqCtGwPDBUfMIIEuzCCA6OgAwIBAgIBAjANBgkqhkiG9w0BAQUFADBiMQswCQYDVQQGEwJVUzETMBEGA1UEChMKQXBwbGUgSW5jLjEmMCQGA1UECxMdQXBwbGUgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxFjAUBgNVBAMTDUFwcGxlIFJvb3QgQ0EwHhcNMDYwNDI1MjE0MDM2WhcNMzUwMjA5MjE0MDM2WjBiMQswCQYDVQQGEwJVUzETMBEGA1UEChMKQXBwbGUgSW5jLjEmMCQGA1UECxMdQXBwbGUgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxFjAUBgNVBAMTDUFwcGxlIFJvb3QgQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDkkakJH5HbHkdQ6wXtXnmELes2oldMVeyLGYne+Uts9QerIjAC6Bg++FAJ039BqJj50cpmnCRrEdCju+QbKsMflZ56DKRHi1vUFjczy8QPTc4UadHJGXL1XQ7Vf1+b8iUDulWPTV0N8WQ1IxVLFVkds5T39pyez1C6wVhQZ48ItCD3y6wsIG9wtj8BMIy3Q88PnT3zK0koGsj+zrW5DtleHNbLPbU6rfQPDgCSC7EhFi501TwN22IWq6NxkkdTVcGvL0Gz+PvjcM3mo0xFfh9Ma1CWQYnEdGILEINBhzOKgbEwWOxaBDKMaLOPHd5lc\/9nXmW8Sdh2nzMUZaF3lMktAgMBAAGjggF6MIIBdjAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH\/BAUwAwEB\/zAdBgNVHQ4EFgQUK9BpR5R2Cf70a40uQKb3R01\/CF4wHwYDVR0jBBgwFoAUK9BpR5R2Cf70a40uQKb3R01\/CF4wggERBgNVHSAEggEIMIIBBDCCAQAGCSqGSIb3Y2QFATCB8jAqBggrBgEFBQcCARYeaHR0cHM6Ly93d3cuYXBwbGUuY29tL2FwcGxlY2EvMIHDBggrBgEFBQcCAjCBthqBs1JlbGlhbmNlIG9uIHRoaXMgY2VydGlmaWNhdGUgYnkgYW55IHBhcnR5IGFzc3VtZXMgYWNjZXB0YW5jZSBvZiB0aGUgdGhlbiBhcHBsaWNhYmxlIHN0YW5kYXJkIHRlcm1zIGFuZCBjb25kaXRpb25zIG9mIHVzZSwgY2VydGlmaWNhdGUgcG9saWN5IGFuZCBjZXJ0aWZpY2F0aW9uIHByYWN0aWNlIHN0YXRlbWVudHMuMA0GCSqGSIb3DQEBBQUAA4IBAQBcNplMLXi37Yyb3PN3m\/J20ncwT8EfhYOFG5k9RzfyqZtAjizUsZAS2L70c5vu0mQPy3lPNNiiPvl4\/2vIB+x9OYOLUyDTOMSxv5pPCmv\/K\/xZpwUJfBdAVhEedNO3iyM7R6PVbyTi69G3cN8PReEnyvFteO3ntRcXqNx+IjXKJdXZD9Zr1KIkIxH3oayPc4FgxhtbCS+SsvhESPBgOJ4V9T0mZyCKM2r3DYLP3uujL\/lTaltkwGMzd\/c6ByxW69oPIQ7aunMZT7XZNn\/Bh1XZp5m5MkL72NVxnn6hUrcbvZNCJBIqxw8dtk2cXmPIS4AXUKqK1drk\/NAJBzewdXUhMYIByzCCAccCAQEwgaMwgZYxCzAJBgNVBAYTAlVTMRMwEQYDVQQKDApBcHBsZSBJbmMuMSwwKgYDVQQLDCNBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9uczFEMEIGA1UEAww7QXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkCCA7rV4fnngmNMAkGBSsOAwIaBQAwDQYJKoZIhvcNAQEBBQAEggEAgq3SBR16mLfbS\/lu91Y+EMtEYWJlEehnlwOqAj9l6rW8T7a1aKddDyBo\/ZrDPs4EEhrUcHqEMZaNkhbOrjIoVI\/F5hn5k7cCaPAraCjUHvllLHTz+00uySLwFtIlhjIsaU3UVK7j6qGya05Nb728wbIhXendEIMOzzhGEUd5ayY6B32MVA79TJN0PTvVDd8vCYWi3ve5aRTW2YH1JZcaOVIAHbTh3nZdzaHpkpHn2R7J5rcf+11ryI3MG5TlBQ0Ruu7Ex+KSdgfAdOeRl9xp0ZNArUf2+D0OehVliCbiCrQV3LThT7TIaQItN+yjPGms2zI8\/X0tV+xhY7ZZWzHt9g==";

		internal static async Task<string> ValidateReceiptRawAsync(string receipt, bool sandboxStore)
		{
			var storeUrl = sandboxStore ? "https://sandbox.itunes.apple.com/verifyReceipt" : "https://buy.itunes.apple.com/verifyReceipt";
			var json = string.Format("{{\"receipt-data\":\"{0}\"}}", receipt);

#if NET46
			var ascii = new ASCIIEncoding();
			var postBytes = Encoding.UTF8.GetBytes(json);

			var request = WebRequest.Create(storeUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = postBytes.Length;

			using (var stream = await request.GetRequestStreamAsync())
			{
				await stream.WriteAsync(postBytes, 0, postBytes.Length);
				await stream.FlushAsync();
			}

			var sendResponse = await request.GetResponseAsync();

			using (var streamReader = new StreamReader(sendResponse.GetResponseStream()))
			{
				return await streamReader.ReadToEndAsync();
			}
#else
			using (var httpClient = new HttpClient())
			{
				using (var httpContent = new StringContent(json, Encoding.ASCII, "application/json"))
				{
					using (var result = await httpClient.PostAsync(storeUrl, httpContent))
					{
						return await result.Content.ReadAsStringAsync();
					}
				}
			}
#endif
		}

		internal static async Task<AppStoreValidationResult> ValidateReceiptAsync(string receipt)
		{
			var responseString = await ValidateReceiptRawAsync(receipt, false);
			var result = new AppStoreValidationResult(responseString);
			var json = JsonValue.Parse(responseString);
			int status = json[_statusValueName];

			if (status == 0)
			{
				ParseValidationResponse(json, result);
			}
			else if (status == 21007)
			{
				// This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.
				responseString = await ValidateReceiptRawAsync(receipt, true);
				json = JsonValue.Parse(responseString);
				status = json[_statusValueName];

				if (status == 0)
				{
					ParseValidationResponse(json, result);
				}
			}

			result.StatusCode = status;
			result.Status = GetStatusText(status);

			return result;
		}

		#endregion

		#region implementation

		private static void ParseValidationResponse(JsonValue json, AppStoreValidationResult result)
		{
			var receiptData = json[_receiptValueName];

			result.Environment = json[_environmentValueName];
			result.Receipt = new AppStoreReceipt();

			ParseAppReceipt(receiptData, result.Receipt);
		}

		private static void ParseAppReceipt(JsonValue json, AppStoreReceipt receipt)
		{
			// required fields
			receipt.BundleId = json[_bundleIdValueName];
			receipt.AppVersion = json[_appVersionValueName];
			receipt.OriginalAppVersion = json[_originalAppVersionValueName];
			receipt.CreationDate = ParseDateTimeRfc3339(json[_receiptCreationDateValueName]);
			receipt.InApp = ParseInApp(json[_inAppValueName] as JsonArray);

			// optional fields
			if (json.ContainsKey(_expirationDateValueName))
			{
				receipt.ExpirationDate = ParseDateTimeRfc3339(json[_expirationDateValueName]);
			}
		}

		private static AppStoreInAppReceipt[] ParseInApp(JsonArray json)
		{
			if (json != null && json.Count > 0)
			{
				var result = new List<AppStoreInAppReceipt>(json.Count);

				foreach (var receiptNode in json)
				{
					// required fields
					var receipt = new AppStoreInAppReceipt
					{
						Quantity = receiptNode[_quantityValueName],
						ProductId = receiptNode[_productIdValueName],
						TransactionId = receiptNode[_transactionIdValueName],
						OriginalTransactionId = receiptNode[_originalTransactionIdValueName],
						PurchaseDate = ParseDateTimeRfc3339(receiptNode[_purchaseDateValueName]),
						OriginalPurchaseDate = ParseDateTimeRfc3339(receiptNode[_originalPurchaseDateValueName])
					};

					// optional fields
					if (receiptNode.ContainsKey(_subscriptionExpirationDateValueName))
					{
						receipt.SubscriptionExpirationDate = ParseDateTimeRfc3339(receiptNode[_subscriptionExpirationDateValueName]);
					}

					if (receiptNode.ContainsKey(_subscriptionExpirationIntentValueName))
					{
						var s = (string)receiptNode[_subscriptionExpirationIntentValueName];

						switch (s)
						{
							case "1":
								receipt.SubscriptionExpirationIntent = AppStoreSubcriptionExpirationReason.CustomerCanceled;
								break;

							case "2":
								receipt.SubscriptionExpirationIntent = AppStoreSubcriptionExpirationReason.BillingError;
								break;

							case "3":
								receipt.SubscriptionExpirationIntent = AppStoreSubcriptionExpirationReason.CustomerRevoked;
								break;

							case "4":
								receipt.SubscriptionExpirationIntent = AppStoreSubcriptionExpirationReason.ProductNotAvailable;
								break;

							case "5":
								receipt.SubscriptionExpirationIntent = AppStoreSubcriptionExpirationReason.Unknown;
								break;
						}
					}

					if (receiptNode.ContainsKey(_subscriptionRetryFlagValueName))
					{
						var s = (string)receiptNode[_subscriptionRetryFlagValueName];

						if (s == "1")
						{
							receipt.SubscriptionRetryFlag = true;
						}
						else if (s == "0")
						{
							receipt.SubscriptionRetryFlag = false;
						}
					}

					if (receiptNode.ContainsKey(_subscriptionTrialPeriodValueName))
					{
						var s = (string)receiptNode[_subscriptionTrialPeriodValueName];

						if (s == "true")
						{
							receipt.SubscriptionTrialPeriod = true;
						}
						else if (s == "false")
						{
							receipt.SubscriptionTrialPeriod = false;
						}
					}

					if (receiptNode.ContainsKey(_cancellationDateValueName))
					{
						receipt.CancellationDate = ParseDateTimeRfc3339(receiptNode[_cancellationDateValueName]);
					}

					if (receiptNode.ContainsKey(_cancellationReasonValueName))
					{
						var s = (string)receiptNode[_cancellationReasonValueName];

						if (s == "1")
						{
							receipt.CancellationReason = AppStorePurchaseCancellationReason.AppIssue;
						}
						else if (s == "0")
						{
							receipt.CancellationReason = AppStorePurchaseCancellationReason.Unknown;
						}
					}

					if (receiptNode.ContainsKey(_appItemIdValueName))
					{
						receipt.AppItemId = receiptNode[_appItemIdValueName];
					}

					if (receiptNode.ContainsKey(_externalVersionIdValueName))
					{
						receipt.ExternalVersionId = receiptNode[_externalVersionIdValueName];
					}

					if (receiptNode.ContainsKey(_webOrderLineItemIdValueName))
					{
						receipt.ExternalVersionId = receiptNode[_webOrderLineItemIdValueName];
					}

					if (receiptNode.ContainsKey(_subscriptionAutoRenewStatusValueName))
					{
						var s = (string)receiptNode[_subscriptionAutoRenewStatusValueName];

						if (s == "1")
						{
							receipt.SubscriptionAutoRenewStatus = true;
						}
						else if (s == "0")
						{
							receipt.SubscriptionAutoRenewStatus = false;
						}
					}

					if (receiptNode.ContainsKey(_subscriptionAutoRenewPreferenceValueName))
					{
						receipt.SubscriptionAutoRenewPreference = receiptNode[_subscriptionAutoRenewPreferenceValueName];
					}

					if (receiptNode.ContainsKey(_subscriptionPriceConsentStatusValueName))
					{
						var s = (string)receiptNode[_subscriptionPriceConsentStatusValueName];

						if (s == "1")
						{
							receipt.SubscriptionAutoRenewStatus = true;
						}
						else if (s == "0")
						{
							receipt.SubscriptionAutoRenewStatus = false;
						}
					}

					result.Add(receipt);
				}

				return result.ToArray();
			}

			return new AppStoreInAppReceipt[0];
		}

		private static string GetStatusText(int status)
		{
			switch (status)
			{
				case 0:
					return "OK";

				case 21000:
					return "The App Store could not read the JSON object you provided.";

				case 21002:
					return "The data in the receipt-data property was malformed or missing.";

				case 21003:
					return "The receipt could not be authenticated.";

				case 21004:
					return "The shared secret you provided does not match the shared secret on file for your account.";

				case 21005:
					return "The receipt server is not currently available.";

				case 21006:
					return "This receipt is valid but the subscription has expired. When this status code is returned to your server, the receipt data is also decoded and returned as part of the response.";

				case 21007:
					return "This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.";

				case 21008:
					return "This receipt is from the production environment, but it was sent to the test environment for verification. Send it to the production environment instead.";

				case 21010:
					return "This receipt could not be authorized. Treat this the same as if a purchase was never made.";
			}

			if (status >= 21100 && status <= 21199)
			{
				return "Internal data access error.";
			}

			return "Unknown";
		}

		private static DateTime ParseDateTimeRfc3339(string s)
		{
			if (!string.IsNullOrEmpty(s) && DateTime.TryParseExact(s, _rfc3339DateTimePatterns, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out var result))
			{
				return result;
			}

			return DateTime.MinValue;
		}

		#endregion
	}
}
