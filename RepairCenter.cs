using ApplianceRepairCenter.Models;

namespace ApplianceRepairCenter.Services;

/// <summary>
/// Main class: stores a list of base-type appliances and processes them polymorphically.
/// It subscribes to each appliance's events and keeps a notification log.
/// </summary>
public class RepairCenter
{
    private readonly List<Appliance> _appliances = new();
    private readonly List<string> _notifications = new();

    public IReadOnlyList<Appliance> Appliances => _appliances;
    public IReadOnlyList<string> Notifications => _notifications;

    /// <summary>Raised to the UI so notifications can be printed immediately.</summary>
    public event Action<string>? NotificationRaised;

    public void AddAppliance(Appliance appliance)
    {
        if (_appliances.Any(a => a.SerialNumber == appliance.SerialNumber))
            throw new InvalidOperationException($"An appliance with serial '{appliance.SerialNumber}' already exists.");

        appliance.StatusChanged += OnStatusChanged;
        appliance.DeliveryOverdue += OnDeliveryOverdue;
        _appliances.Add(appliance);
    }

    public Appliance? Find(string serialNumber)
        => _appliances.FirstOrDefault(a => a.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase));

    // ---------- Polymorphic processing ----------
    public decimal GetTotalCost()
        => _appliances.Sum(a => a.CalculateRepairCost());

    public decimal GetTotalCostByStatus(RepairStatus status)
        => _appliances.Where(a => a.Status == status).Sum(a => a.CalculateRepairCost());

    public int CountByStatus(RepairStatus status)
        => _appliances.Count(a => a.Status == status);

    public IEnumerable<Appliance> GetByStatus(RepairStatus status)
        => _appliances.Where(a => a.Status == status);

    /// <summary>Appliances that were repaired (finished, or already delivered).</summary>
    public IEnumerable<Appliance> GetRepaired()
        => _appliances.Where(a => a.Status == RepairStatus.Repaired || a.Status == RepairStatus.Delivered);

    /// <summary>Appliances still in the center (received or under repair).</summary>
    public IEnumerable<Appliance> GetPending()
        => _appliances.Where(a => a.Status == RepairStatus.Received || a.Status == RepairStatus.UnderRepair);

    public int CheckAllDelays(DateTime today)
    {
        int count = 0;
        foreach (Appliance appliance in _appliances)
        {
            if (appliance.CheckDelay(today))
                count++;
        }
        return count;
    }

    // ---------- Event handlers ----------
    private void OnStatusChanged(Appliance appliance, RepairStatus oldStatus, RepairStatus newStatus)
    {
        string message = newStatus == RepairStatus.Repaired
            ? $"Repair COMPLETED for {appliance.ApplianceType} {appliance.SerialNumber} (customer: {appliance.CustomerName}). Total cost: {appliance.CalculateRepairCost():F2}"
            : $"{appliance.ApplianceType} {appliance.SerialNumber}: status changed {oldStatus} -> {newStatus}";
        Publish(message);
    }

    private void OnDeliveryOverdue(Appliance appliance, int daysLate)
    {
        Publish($"WARNING: {appliance.ApplianceType} {appliance.SerialNumber} (customer: {appliance.CustomerName}) is {daysLate} day(s) late.");
    }

    private void Publish(string message)
    {
        _notifications.Add(message);
        NotificationRaised?.Invoke(message);
    }
}
