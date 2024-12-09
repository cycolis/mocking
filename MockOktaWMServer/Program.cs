using MockOktaWireMockServer;

var mockServer = MockServerConfig.CreateMockServer();
Console.WriteLine($"WireMock server running at: {mockServer.Urls[0]}");

Console.WriteLine("Press Ctrl+C to shut down.");
await Task.Delay(-1); // Keep the application running