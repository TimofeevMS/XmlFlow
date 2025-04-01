using System.Xml;

using XmlFlow.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace XmlFlow.Example.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly IPipelineStarter _starter;

    public ImportController(IPipelineStarter starter) => _starter = starter;

    [HttpPost]
    [Produces("application/xml")]
    public async Task<IActionResult> Import([FromBody] string xml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            
            await _starter.Start(doc);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}