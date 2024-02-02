# transformer.bee Client (.NET)

This library is a .NET HTTP client for transformer.bee aka edifact-bo4e-converter.

It allows you to convert EDIFACT messages to BO4E and vice versa by speaking to Hochfreqeunz's transformer.bee service.
Note that the actual conversion happens in the transformer.bee service/backend, this library only provides a convenient way to use its API.

## How to use this library

### Prerequisites / Account

First of all, you need an account to use transformer.bee.
Ask info@hochfrequenz.de or ping @JoschaMetze on GitHub to get one.

You can check if your account is working by logging [into our stage environment](https://transformerstage.utilibee.io/app/).

### Using the client
![Nuget Package](https://badgen.net/nuget/v/TransformerBeeClient)
Install it from nuget [TransformerBeeClient](https://www.nuget.org/packages/TransformerBeeClient):

```bash
dotnet add package TransformerBeeClient
```

### Authentication
You need to provide something that implements `ITransformerBeeAuthenticator` to the `TransformerBeeClient`.

#### No Authentication
If you're hosting transformer.bee in the same network and there is no authentication, you can use the `NoAuthenticator`.
```csharp
using TransformerBeeClient;
var myAuthenticator = new NoAuthenticationProvider();
```
This library is thought to be used in ASP.NET Core applications.
That's why it assumes that you have an `IHttpClientFactory` available in your dependency injection container.

#### OAuth2 Client and Secret
If, which is more likely, Hochfrequenz provided you with a client ID and secret, you can use the `ClientIdClientSecretAuthenticator` class like this:
Then, setup your dependency injection container like this:
```csharp
using TransformerBeeClient;
var myAuthenticator = new ClientIdClientSecretAuthenticationProvider("YOUR_CLIENT_ID", "YOUR_CLIENT_SECRET");
```

...todo
```csharp

using TransformerBeeClient;
// ...
builder.Services.AddHttpClient();
builder.Services.AddTransient<ICanConvertToBo4e, TransformerBeeRestClient>();
builder.Services.AddTransient<ICanConvertToEdifact, TransformerBeeRestClient>();
```

If you're not using ASP.NET Core, you can still use this library, by using [this little workaround](https://chat.openai.com/share/fa63110a-646e-4fd1-aacb-3d449c285750).

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
