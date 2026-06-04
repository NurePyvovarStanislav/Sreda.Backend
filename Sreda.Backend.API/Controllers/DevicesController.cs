using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.Devices.Commands;
using Sreda.Backend.Application.Features.Devices.Queries;
using Sreda.Backend.Contracts.DTOs.Devices;

namespace Sreda.Backend.API.Controllers;

[Route("api/devices")]
public class DevicesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetDevicesQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetDeviceByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new CreateDeviceCommand(
            request.Code, request.Localizations, request.Type, request.IsActive), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new UpdateDeviceCommand(
            id, request.Localizations, request.Type, request.IsActive), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new DeleteDeviceCommand(id), cancellationToken));

    [HttpGet("{id:int}/state")]
    public async Task<IActionResult> GetState(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetDeviceStateQuery(id), cancellationToken));

    [HttpPut("{id:int}/state")]
    public async Task<IActionResult> UpdateState(
        int id, [FromBody] UpdateDeviceStateRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new UpdateDeviceStateCommand(
            id, request.IsOnline, request.StateJson), cancellationToken));

    /// <summary>Надсилає команду пристрою через шлюз (MQTT).</summary>
    [HttpPost("{id:int}/command")]
    public async Task<IActionResult> SendCommand(
        int id, [FromBody] SendDeviceCommandRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new SendDeviceCommandCommand(
            id, request.Command, request.Parameters), cancellationToken));
}
