namespace Okta.AuthServerApi.Mock.Services;

internal interface IMockServerService
{
    string Url { get; }

    void Stop();
}
