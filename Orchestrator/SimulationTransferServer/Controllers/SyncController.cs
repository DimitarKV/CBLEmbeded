﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services;
using ServiceLayer.Types;
using SimulationTransferServer.Dto;

namespace SimulationTransferServer.Controllers;

[ApiController]
[Route("/[controller]/[action]")]
public class SyncController(IRobotService robotService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> All([FromForm] SyncAllDto dto)
    {
        await robotService.WriteToDisplay(new WriteToDisplayMessage(dto.swing1Rotation.ToString()));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Display([FromForm] SyncDisplayDto dto)
    {
        await robotService.WriteToDisplay(new WriteToDisplayMessage(dto.Text));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ClearDisplay()
    {
        await robotService.WriteToDisplay(new WriteToDisplayMessage(""));
        return Ok();
    }

    public async Task<IActionResult> ReadColorSensor()
    {
        var message = await robotService.ReadColorSensorData(new ReadColorSensorMessage());
        return Ok(message);
    }

    [HttpPost]
    public async Task<IActionResult> SetServoPos([FromBody] List<ServoPosDto> dto)
    {
        await robotService.SetServoPos(new SetServoPositionsMessage() {ServoParameters = dto});
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MoveBelt([FromBody] MoveBeltMessage message)
    {
        await robotService.MoveBelt(message);
        return Ok();
    }
}