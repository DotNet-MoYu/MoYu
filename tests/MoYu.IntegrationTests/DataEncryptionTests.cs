using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace MoYu.IntegrationTests;

/// <summary>
/// ���ݼӽ��ܼ��ɲ���
/// </summary>
public class DataEncryptionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper Output;
    private readonly WebApplicationFactory<Program> _factory;

    public DataEncryptionTests(ITestOutputHelper tempOutput,
        WebApplicationFactory<Program> factory)
    {
        Output = tempOutput;
        _factory = factory;
    }

    /// <summary>
    /// ���� MD5 ���ܼ���Сд����
    /// </summary>
    /// <param name="url"></param>
    /// <param name="text"></param>
    /// <param name="uppercase"></param>
    /// <returns></returns>
    [Theory]
    [InlineData("/DataEncryptionTests/TestMD5Encrypt", "MoYu", true)]
    [InlineData("/DataEncryptionTests/TestMD5Encrypt", "MoYu", false)]
    public async Task Test_MD5_Encrypt(string url, string text, bool uppercase)
    {
        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync($"{url}/{text}/{uppercase}", default);

        var content = await response.Content.ReadAsStringAsync();
        Output.WriteLine(content);
        response.EnsureSuccessStatusCode();

        Assert.Equal(uppercase ? "08C5318BD8A478A88E6D594A0142AE6C" : "08c5318bd8a478a88e6d594a0142ae6c", content);
    }

    /// <summary>
    /// ���� MD5 �Ա�
    /// </summary>
    /// <param name="url"></param>
    /// <param name="text"></param>
    /// <param name="hash"></param>
    /// <param name="uppercase"></param>
    /// <returns></returns>
    [Theory]
    [InlineData("/DataEncryptionTests/TestMD5Compare", "MoYu", "08C5318BD8A478A88E6D594A0142AE6C", true)]
    [InlineData("/DataEncryptionTests/TestMD5Compare", "MoYu", "08c5318bd8a478a88e6d594a0142ae6c", false)]
    public async Task Test_MD5_Compare(string url, string text, string hash, bool uppercase)
    {
        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync($"{url}/{text}/{hash}/{uppercase}", default);

        var content = await response.Content.ReadAsStringAsync();
        Output.WriteLine(content);
        response.EnsureSuccessStatusCode();

        Assert.True(bool.Parse(content));
    }
}