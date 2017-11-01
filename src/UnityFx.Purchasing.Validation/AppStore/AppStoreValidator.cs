// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Json;
using System.IO;
using System.Net;
#if NETSTANDARD1_3
using System.Net.Http;
#endif
using System.Text;
using System.Threading.Tasks;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// 
	/// </summary>
	internal static class AppStoreValidator
	{
		internal const string TestReceipt = @"MIISpgYJKoZIhvcNAQcCoIISlzCCEpMCAQExCzAJBgUrDgMCGgUAMIICRwYJKoZIhvcNAQcBoIICOASCAjQxggIwMAoCARQCAQEEAgwAMAsCAQ4CAQEEAwIBTjALAgEZAgEBBAMCAQMwDQIBCgIBAQQFFgMxMiswDQIBCwIBAQQFAgMETJIwDQIBDQIBAQQFAgMBOawwDgIBAQIBAQQGAgRCzLtSMA4CAQkCAQEEBgIEUDI0NzAOAgEQAgEBBAYCBDDuF88wEAIBDwIBAQQIAgYilYYST3gwEwIBAwIBAQQLDAkxMDcwNDAwMTEwEwIBEwIBAQQLDAkxMDYwMjA0NTEwFAIBAAIBAQQMDApQcm9kdWN0aW9uMBgCAQQCAQIEEN5vz31AX36y1xhxCCVk0F8wHAIBBQIBAQQU1qctljZKsomrtNVup369nUcFCrUwHgIBCAIBAQQWFhQyMDE3LTAyLTIwVDIzOjU5OjIzWjAeAgEMAgEBBBYWFDIwMTctMDItMjBUMjM6NTk6MjNaMB4CARICAQEEFhYUMjAxNy0wMS0yMFQwMDowNzoyM1owIgIBAgIBAQQaDBhjb20uZ3NuLlZlZ2FzRG9sbGFyU2xvdHMwSQIBBgIBAQRBv7bLIcE+P4IC\/lMN5wICZ73gg77W0kGnnyDAjGVhWRpfXqaAip1uH9Jo9Ux70mYxv\/WAndmW5H9I1sADSEynqXQwUgIBBwIBAQRK0ltZm5p8zZjVDC55+9zQpXjiIxwDIAoyBiCdPVlzpxQdriSDFxM\/AlobF5\/o1VROd5jpsBvDZvLdK2e\/4fkIG+d1IIrs6wUbmaWggg5lMIIFfDCCBGSgAwIBAgIIDutXh+eeCY0wDQYJKoZIhvcNAQEFBQAwgZYxCzAJBgNVBAYTAlVTMRMwEQYDVQQKDApBcHBsZSBJbmMuMSwwKgYDVQQLDCNBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9uczFEMEIGA1UEAww7QXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwHhcNMTUxMTEzMDIxNTA5WhcNMjMwMjA3MjE0ODQ3WjCBiTE3MDUGA1UEAwwuTWFjIEFwcCBTdG9yZSBhbmQgaVR1bmVzIFN0b3JlIFJlY2VpcHQgU2lnbmluZzEsMCoGA1UECwwjQXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMxEzARBgNVBAoMCkFwcGxlIEluYy4xCzAJBgNVBAYTAlVTMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApc+B\/SWigVvWh+0j2jMcjuIjwKXEJss9xp\/sSg1Vhv+kAteXyjlUbX1\/slQYncQsUnGOZHuCzom6SdYI5bSIcc8\/W0YuxsQduAOpWKIEPiF41du30I4SjYNMWypoN5PC8r0exNKhDEpYUqsS4+3dH5gVkDUtwswSyo1IgfdYeFRr6IwxNh9KBgxHVPM3kLiykol9X6SFSuHAnOC6pLuCl2P0K5PB\/T5vysH1PKmPUhrAJQp2Dt7+mf7\/wmv1W16sc1FJCFaJzEOQzI6BAtCgl7ZcsaFpaYeQEGgmJjm4HRBzsApdxXPQ33Y72C3ZiB7j7AfP4o7Q0\/omVYHv4gNJIwIDAQABo4IB1zCCAdMwPwYIKwYBBQUHAQEEMzAxMC8GCCsGAQUFBzABhiNodHRwOi8vb2NzcC5hcHBsZS5jb20vb2NzcDAzLXd3ZHIwNDAdBgNVHQ4EFgQUkaSc\/MR2t5+givRN9Y82Xe0rBIUwDAYDVR0TAQH\/BAIwADAfBgNVHSMEGDAWgBSIJxcJqbYYYIvs67r2R1nFUlSjtzCCAR4GA1UdIASCARUwggERMIIBDQYKKoZIhvdjZAUGATCB\/jCBwwYIKwYBBQUHAgIwgbYMgbNSZWxpYW5jZSBvbiB0aGlzIGNlcnRpZmljYXRlIGJ5IGFueSBwYXJ0eSBhc3N1bWVzIGFjY2VwdGFuY2Ugb2YgdGhlIHRoZW4gYXBwbGljYWJsZSBzdGFuZGFyZCB0ZXJtcyBhbmQgY29uZGl0aW9ucyBvZiB1c2UsIGNlcnRpZmljYXRlIHBvbGljeSBhbmQgY2VydGlmaWNhdGlvbiBwcmFjdGljZSBzdGF0ZW1lbnRzLjA2BggrBgEFBQcCARYqaHR0cDovL3d3dy5hcHBsZS5jb20vY2VydGlmaWNhdGVhdXRob3JpdHkvMA4GA1UdDwEB\/wQEAwIHgDAQBgoqhkiG92NkBgsBBAIFADANBgkqhkiG9w0BAQUFAAOCAQEADaYb0y4941srB25ClmzT6IxDMIJf4FzRjb69D70a\/CWS24yFw4BZ3+Pi1y4FFKwN27a4\/vw1LnzLrRdrjn8f5He5sWeVtBNephmGdvhaIJXnY4wPc\/zo7cYfrpn4ZUhcoOAoOsAQNy25oAQ5H3O5yAX98t5\/GioqbisB\/KAgXNnrfSemM\/j1mOC+RNuxTGf8bgpPyeIGqNKX86eOa1GiWoR1ZdEWBGLjwV\/1CKnPaNmSAMnBjLP4jQBkulhgwHyvj3XKablbKtYdaG6YQvVMpzcZm8w7HHoZQ\/Ojbb9IYAYMNpIr7N4YtRHaLSPQjvygaZwXG56AezlHRTBhL8cTqDCCBCIwggMKoAMCAQICCAHevMQ5baAQMA0GCSqGSIb3DQEBBQUAMGIxCzAJBgNVBAYTAlVTMRMwEQYDVQQKEwpBcHBsZSBJbmMuMSYwJAYDVQQLEx1BcHBsZSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTEWMBQGA1UEAxMNQXBwbGUgUm9vdCBDQTAeFw0xMzAyMDcyMTQ4NDdaFw0yMzAyMDcyMTQ4NDdaMIGWMQswCQYDVQQGEwJVUzETMBEGA1UECgwKQXBwbGUgSW5jLjEsMCoGA1UECwwjQXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMxRDBCBgNVBAMMO0FwcGxlIFdvcmxkd2lkZSBEZXZlbG9wZXIgUmVsYXRpb25zIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyjhUpstWqsgkOUjpjO7sX7h\/JpG8NFN6znxjgGF3ZF6lByO2Of5QLRVWWHAtfsRuwUqFPi\/w3oQaoVfJr3sY\/2r6FRJJFQgZrKrbKjLtlmNoUhU9jIrsv2sYleADrAF9lwVnzg6FlTdq7Qm2rmfNUWSfxlzRvFduZzWAdjakh4FuOI\/YKxVOeyXYWr9Og8GN0pPVGnG1YJydM05V+RJYDIa4Fg3B5XdFjVBIuist5JSF4ejEncZopbCj\/Gd+cLoCWUt3QpE5ufXN4UzvwDtIjKblIV39amq7pxY1YNLmrfNGKcnow4vpecBqYWcVsvD95Wi8Yl9uz5nd7xtj\/pJlqwIDAQABo4GmMIGjMB0GA1UdDgQWBBSIJxcJqbYYYIvs67r2R1nFUlSjtzAPBgNVHRMBAf8EBTADAQH\/MB8GA1UdIwQYMBaAFCvQaUeUdgn+9GuNLkCm90dNfwheMC4GA1UdHwQnMCUwI6AhoB+GHWh0dHA6Ly9jcmwuYXBwbGUuY29tL3Jvb3QuY3JsMA4GA1UdDwEB\/wQEAwIBhjAQBgoqhkiG92NkBgIBBAIFADANBgkqhkiG9w0BAQUFAAOCAQEAT8\/vWb4s9bJsL4\/uE4cy6AU1qG6LfclpDLnZF7x3LNRn4v2abTpZXN+DAb2yriphcrGvzcNFMI+jgw3OHUe08ZOKo3SbpMOYcoc7Pq9FC5JUuTK7kBhTawpOELbZHVBsIYAKiU5XjGtbPD2m\/d73DSMdC0omhz+6kZJMpBkSGW1X9XpYh3toiuSGjErr4kkUqqXdVQCprrtLMK7hoLG8KYDmCXflvjSiAcp\/3OIK5ju4u+y6YpXzBWNBgs0POx1MlaTbq\/nJlelP5E3nJpmB6bz5tCnSAXpm4S6M9iGKxfh44YGuv9OQnamt86\/9OBqWZzAcUaVc7HGKgrRsDwwVHzCCBLswggOjoAMCAQICAQIwDQYJKoZIhvcNAQEFBQAwYjELMAkGA1UEBhMCVVMxEzARBgNVBAoTCkFwcGxlIEluYy4xJjAkBgNVBAsTHUFwcGxlIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MRYwFAYDVQQDEw1BcHBsZSBSb290IENBMB4XDTA2MDQyNTIxNDAzNloXDTM1MDIwOTIxNDAzNlowYjELMAkGA1UEBhMCVVMxEzARBgNVBAoTCkFwcGxlIEluYy4xJjAkBgNVBAsTHUFwcGxlIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MRYwFAYDVQQDEw1BcHBsZSBSb290IENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5JGpCR+R2x5HUOsF7V55hC3rNqJXTFXsixmJ3vlLbPUHqyIwAugYPvhQCdN\/QaiY+dHKZpwkaxHQo7vkGyrDH5WeegykR4tb1BY3M8vED03OFGnRyRly9V0O1X9fm\/IlA7pVj01dDfFkNSMVSxVZHbOU9\/acns9QusFYUGePCLQg98usLCBvcLY\/ATCMt0PPD5098ytJKBrI\/s61uQ7ZXhzWyz21Oq30Dw4AkguxIRYudNU8DdtiFqujcZJHU1XBry9Bs\/j743DN5qNMRX4fTGtQlkGJxHRiCxCDQYczioGxMFjsWgQyjGizjx3eZXP\/Z15lvEnYdp8zFGWhd5TJLQIDAQABo4IBejCCAXYwDgYDVR0PAQH\/BAQDAgEGMA8GA1UdEwEB\/wQFMAMBAf8wHQYDVR0OBBYEFCvQaUeUdgn+9GuNLkCm90dNfwheMB8GA1UdIwQYMBaAFCvQaUeUdgn+9GuNLkCm90dNfwheMIIBEQYDVR0gBIIBCDCCAQQwggEABgkqhkiG92NkBQEwgfIwKgYIKwYBBQUHAgEWHmh0dHBzOi8vd3d3LmFwcGxlLmNvbS9hcHBsZWNhLzCBwwYIKwYBBQUHAgIwgbYagbNSZWxpYW5jZSBvbiB0aGlzIGNlcnRpZmljYXRlIGJ5IGFueSBwYXJ0eSBhc3N1bWVzIGFjY2VwdGFuY2Ugb2YgdGhlIHRoZW4gYXBwbGljYWJsZSBzdGFuZGFyZCB0ZXJtcyBhbmQgY29uZGl0aW9ucyBvZiB1c2UsIGNlcnRpZmljYXRlIHBvbGljeSBhbmQgY2VydGlmaWNhdGlvbiBwcmFjdGljZSBzdGF0ZW1lbnRzLjANBgkqhkiG9w0BAQUFAAOCAQEAXDaZTC14t+2Mm9zzd5vydtJ3ME\/BH4WDhRuZPUc38qmbQI4s1LGQEti+9HOb7tJkD8t5TzTYoj75eP9ryAfsfTmDi1Mg0zjEsb+aTwpr\/yv8WacFCXwXQFYRHnTTt4sjO0ej1W8k4uvRt3DfD0XhJ8rxbXjt57UXF6jcfiI1yiXV2Q\/Wa9SiJCMR96Gsj3OBYMYbWwkvkrL4REjwYDieFfU9JmcgijNq9w2Cz97roy\/5U2pbZMBjM3f3OgcsVuvaDyEO2rpzGU+12TZ\/wYdV2aeZuTJC+9jVcZ5+oVK3G72TQiQSKscPHbZNnF5jyEuAF1CqitXa5PzQCQc3sHV1ITGCAcswggHHAgEBMIGjMIGWMQswCQYDVQQGEwJVUzETMBEGA1UECgwKQXBwbGUgSW5jLjEsMCoGA1UECwwjQXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMxRDBCBgNVBAMMO0FwcGxlIFdvcmxkd2lkZSBEZXZlbG9wZXIgUmVsYXRpb25zIENlcnRpZmljYXRpb24gQXV0aG9yaXR5AggO61eH554JjTAJBgUrDgMCGgUAMA0GCSqGSIb3DQEBAQUABIIBAF\/SwmE\/g89dLI\/aTpJgXfClI4qN74L+tZZZ7xg8hUWnBwrVVzUfMYx58m2rhOra\/\/FsiniWr8N2WvdFtDasDIPRWEY4qlI0\/bg\/bJX+8WMVvPOQWE4Dz7jNlwKgJ39\/iGZdDJIak\/ws7rctQ9jktYzxoHEvRKrKujSr9zr2S8xnrxa+h+HIa4FmV0pKv1GC1jlmud4pSkYCfdGErJcXrXCVkLs5BsoARXsltZVGSmgVcnyPKeDOHHostUMTKRfIIyzSLOuV8QIRdKqhRLde7KSussVAcOkxwFpvA43HlhCPuzd5Sgp9Dc6l6+AI7wXxCsqMwNG0RoGkgM1IEYc15IE=";

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
			int status = json["status"];

			if (status == 0)
			{
				ParseValidationResponse(json, result);
			}
			else if (status == 21007)
			{
				// This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.
				responseString = await ValidateReceiptRawAsync(receipt, true);
				json = JsonValue.Parse(responseString);
				status = json["status"];

				if (status == 0)
				{
					ParseValidationResponse(json, result);
				}
			}

			result.Status = status;
			result.StatusText = GetStatusText(status);

			return result;
		}

		private static void ParseValidationResponse(JsonValue json, AppStoreValidationResult result)
		{
			var receiptData = json["receipt"];

			result.Environment = json["environment"];
			result.Receipt = new AppStoreReceipt();

			ParseAppReceipt(receiptData, result.Receipt);
		}

		private static void ParseAppReceipt(JsonValue json, AppStoreReceipt receipt)
		{
			receipt.BundleId = json["bundle_id"];
			receipt.ApplicationVersion = json["application_version"];
			receipt.OriginalApplicationVersion = json["original_application_version"];
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
					return "This receipt is from the production environment, but it was sent to the test environment for verification.Send it to the production environment instead.";

				case 21010:
					return "This receipt could not be authorized. Treat this the same as if a purchase was never made.";
			}

			if (status >= 21100 && status <= 21199)
			{
				return "Internal data access error.";
			}

			return "Unknown";
		}
	}
}
