namespace ApplianceRepairCenter.Models;

/// <summary>
/// Lifecycle stages of an appliance inside the repair center.
/// </summary>
public enum RepairStatus
{
    Received,     // Appliance has just been received from the customer
    UnderRepair,  // Technician is working on it
    Repaired,     // Repair finished, waiting for customer pickup
    Delivered     // Returned to the customer
}
