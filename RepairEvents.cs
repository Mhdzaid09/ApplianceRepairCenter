using ApplianceRepairCenter.Models;

namespace ApplianceRepairCenter.Events;

/// <summary>
/// Delegate used to notify subscribers that an appliance changed its repair status.
/// </summary>
public delegate void ApplianceStatusChangedHandler(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus);

/// <summary>
/// Delegate used to notify subscribers that an appliance delivery is late.
/// </summary>
public delegate void DeliveryOverdueHandler(Appliance appliance, int daysLate);
