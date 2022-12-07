
using Grpc.Net.Client;
using XTC.FMP.MOD.Hotspot3D.LIB.Proto;

public class IntegrationTest : IntegrationTestBase
{
    [Fact]
    public override async Task Test()
    {
        var channel = GrpcChannel.ForAddress("https://localhost:19000", new GrpcChannelOptions());
        throw new NotImplementedException();

        /*
        var clientHealthy = new Healthy.HealthyClient(channel);
        var response = await clientHealthy.EchoAsync(new HealthyEchoRequest { Msg = "hello" });
        Assert.Equal(0, response.Status.Code);
        */
    }
}
