using ApplianceRepairCenter.Models;

namespace ApplianceRepairCenter.Interfaces;

/// <summary>
/// Abstraction: core operations shared by every appliance handled by the repair center.
/// </summary>
public interface IRepairable
{
    string SerialNumber { get; }
    RepairStatus Status { get; }

    void StartRepair();
    void CompleteRepair();
    void Deliver();
    decimal CalculateRepairCost();
    string GetSummary();
}
