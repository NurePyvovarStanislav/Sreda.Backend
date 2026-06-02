namespace Sreda.Backend.Domain.Enums;

/// <summary>
/// Тип системного события для журнала (Event Log).
/// </summary>
public enum EventType
{
    SystemStarted = 0,
    VoiceRecognized = 1,
    CommandReceived = 2,
    CommandExecuted = 3,
    CommandRejected = 4,
    DeviceUpdated = 5,
    DeviceHeartbeat = 6,
    DeviceOffline = 7,
    Error = 8
}
