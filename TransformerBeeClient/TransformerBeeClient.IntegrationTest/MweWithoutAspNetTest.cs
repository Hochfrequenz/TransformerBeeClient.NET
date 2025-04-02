using EDILibrary;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// A minimal working example on how to use this library without ASP.NET
/// </summary>
public class MweWithoutAspNetTest
{
    /// <summary>
    /// in asp.net applications, there's a service collection that is used to create the http client factory for you
    /// </summary>
    internal class MyHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient { BaseAddress = new Uri("http://localhost:5021") };
        }
    }

    [Fact]
    public async Task Test_Transformer_Bee_Communication()
    {
        IHttpClientFactory myFactory = new MyHttpClientFactory();
        ITransformerBeeAuthenticator myAuthenticator = new NoAuthenticator(); // or use ClientIdClientSecretAuthenticator
        var client = new TransformerBeeRestClient(myFactory, myAuthenticator);
        await client.ConvertToBo4e(
            "UNA:+,? 'UNB+UNOC:3+9912345789012:500+9909876543210:500+230703:1059+ASDFHGJ'UNH+11223344556678+UTILMD:D:11A:UN:S1.1'BGM+E01+918273746512345678901'DTM+137:202306300558?+00:303'NAD+MS+9912345789012::293'NAD+MR+9909876543210::293'IDE+24+918273746512345678901'IMD++Z36+Z13'DTM+92:202212312300?+00:303'STS+7++E01'LOC+Z16+78889918283'LOC+Z17+DE0000111122223333444455556667778'RFF+Z13:55001'SEQ+Z01'CCI+Z30++Z07'CCI+Z19++11X0-0000-0116-J'CCI+++Z15'CCI+++Z88'CAV+Z74:::Z09'CAV+Z73:::Z11'SEQ+Z12'QTY+Z16:0:P1'SEQ+Z03'CCI+++E13'CAV+Z30:::788811123'SEQ+Z75'CCI+Z61++ZG1'NAD+Z09+++Schaefer:Ulrike:::Frau:Z01'NAD+Z04+++Schaefer:Ulrike:::Frau:Z01+Flughafenstrasse::64+Vilseck++92247+DE'NAD+DP++++Flughafenstrasse::64+Vilseck++92247+DE'NAD+Z05+++Schaefer:Ulrike:::Frau:Z01+Flughafenstrasse::64+Vilseck++92247+DE'UNT+31+11223344556678'UNZ+1+ASDFHGJ'",
            EdifactFormatVersion.FV2310
        );
    }
}
