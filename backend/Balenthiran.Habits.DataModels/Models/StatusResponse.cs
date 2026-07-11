namespace Balenthiran.Habits.DataModels.Models;

/// <summary>
/// The system-status payload as it appears on the wire — a shape the status route
/// assembles rather than one the service returns directly (hence the <c>*Response</c>
/// suffix). A concrete record so the OpenAPI document describes it and the frontend
/// codegen produces a typed hook.
/// </summary>
public record StatusResponse(string Version, string FriendlyStatus, DateTime Timestamp);
