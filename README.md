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

Install the package from NuGet:

```bash
nuget install Hochfrequenz.transformer.bee.Client
```

### Use it in your code

Then, you can use the client like this:

```csharp

```

## Development

To run the integration test login to your docker to access the transformer.bee image.

```bash
docker login ghcr.io -u YOUR_GITHUB_USERNAME
```

then paste your PAT similarly to described in the [integration test CI pipeline](.github/workflows/integrationtests.yml)
