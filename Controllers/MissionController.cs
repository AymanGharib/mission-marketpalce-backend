using Microsoft.AspNetCore.Mvc;
using MissionGenerator.Models;
using MissionGenerator.Services;

namespace MissionGenerator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MissionController : ControllerBase
{
    private readonly GeminiService _geminiService;

    public MissionController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    /// <summary>
    /// Génère une fiche mission à partir d'une description utilisateur en langage naturel.
    /// </summary>
    /// <param name="request">Objet contenant le prompt texte.</param>
    /// <returns>Objet Mission généré automatiquement.</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(Mission), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GenerateMission([FromBody] PromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { error = "Prompt is empty" });

        try
        {
            var mission = await _geminiService.GetMissionFromPromptAsync(request.Prompt);
            return Ok(mission);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to generate mission", details = ex.Message });
        }
    }
}
