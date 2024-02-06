# transformer.bee Client (.NET)

This library is a .NET HTTP client for transformer.bee aka edifact-bo4e-converter.
We also maintain a [Python version](https://github.com/Hochfrequenz/TransformerBeeClient.py) of this client.

It allows you to convert EDIFACT messages to BO4E and vice versa by speaking to Hochfrequenz's transformer.bee service.
Note that the actual conversion happens in the transformer.bee service/backend, this library only provides a convenient way to use its API.

## How to use this library

### Prerequisites / Account

First of all, you need an account to use transformer.bee.
Ask info@hochfrequenz.de or ping [@JoschaMetze](https://github.com/joschametze) on GitHub to get one.

You can check if your account is working by logging [into our stage environment](https://transformerstage.utilibee.io/app/).

### Using the client

Install it from nuget [TransformerBeeClient](https://www.nuget.org/packages/TransformerBeeClient) ![Nuget Package](https://badgen.net/nuget/v/TransformerBeeClient):

```bash
dotnet add package TransformerBeeClient
```

### Authentication

You need to provide something that implements `ITransformerBeeAuthenticator` to the `TransformerBeeClient`.

#### No Authentication

If you're hosting transformer.bee in the same network or your localhost and there is no authentication, you can use the `NoAuthenticator`.

```csharp
using TransformerBeeClient;
var myAuthenticator = new NoAuthenticator();
```
Its name says it all 😉 - but you still need it.

#### OAuth2 Client and Secret
If, which is more likely, Hochfrequenz provided you with a client Id and secret, you can use the `ClientIdClientSecretAuthenticator` class like this:

```csharp
using TransformerBeeClient;
var myAuthenticator = new ClientIdClientSecretAuthenticator("YOUR_CLIENT_ID", "YOUR_CLIENT_SECRET");
```

#### Base Address
The `HttpClient` instance used by the `TransformerBeeClient` has to have a `BaseAddress` set.
Use e.g. `https://transformerstage.utilibee.io` for our test system.

### Use with ASP.NET Core
This library is thought to be  primarily used in ASP.NET Core applications.
That's why it assumes that you have an `IHttpClientFactory` available in your dependency injection container.
See the [`ExampleAspNetCoreApplication/Program.cs`](TransformerBeeClient/ExampleAspNetCoreApplication/Program.cs) for a minimal working example.

### Use without ASP.NET Core
If you're not using ASP.NET Core, you can still use this library but setting up th `IHttpClientFactory` comes with a bit of boilerplate.
See the [`MweWithoutAspNetTest.cs`](TransformerBeeClient/TransformerBeeClient.IntegrationTest/MweWithoutAspNetTest.cs) for a minimal working example.

## Development

### Integration Tests

To run the integration test login to your docker to access the transformer.bee image.

```bash
docker login ghcr.io -u YOUR_GITHUB_USERNAME
```

then paste your PAT similarly to described in the [integration test CI pipeline](.github/workflows/integrationtests.yml)

### Release (CI/CD)

To release a new version of this library, [create a new release](https://github.com/Hochfrequenz/transformer.bee_client.net/releases/new) in GitHub.
Make sure its tag starts with `v` and the version number, e.g. `v1.2.3`.
Tags without a release wont trigger the release workflow; This enforces that you have to write a changelog before releasing.
Releases are not restricted to the main branch but we prefer them to happen there.

## Related Tools and Context
This repository is part of the [Hochfrequenz Libraries and Tools for a truly digitized market communication](https://github.com/Hochfrequenz/digital_market_communication/).

## Hochfrequenz
[Hochfrequenz Unternehmensberatung GmbH](https://www.hochfrequenz.de) is a Grünwald (near Munich) based consulting company with offices in Berlin and Bremen and attractive remote options.
We're not only a main contributor for open source software for German utilities but, according to [Kununu ratings](https://www.kununu.com/de/hochfrequenz-unternehmensberatung1), also among the most attractive employers within the German energy market. Applications of talented developers are welcome at any time!
Please consider visiting our [career page](https://www.hochfrequenz.de/index.php/karriere/aktuelle-stellenausschreibungen/full-stack-entwickler) (German only).
