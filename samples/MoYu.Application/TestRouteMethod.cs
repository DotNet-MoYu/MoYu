namespace MoYu.Application;

[Route("sys"), ApiDescriptionSettings(ForceWithRoutePrefix = true)]
public class TestRouteMethod : IDynamicApiController
{
    [HttpGet("getDesc")]
    public string GetDescription(string e)
    {
        return "MoYu";
    }

    public string GetDescription2(string e)
    {
        return "MoYu";
    }
}