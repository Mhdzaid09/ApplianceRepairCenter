namespace ApplianceRepairCenter.Models;

/// <summary>
/// Refrigerator: hourly labor + parts + optional refrigerant gas refill fee.
/// </summary>
public class Refrigerator : Appliance
{
    private const decimal HourlyRate = 15m;
    private const decimal GasRefillFee = 40m;

    private bool _needsGasRefill;

    public Refrigerator(string serialNumber, string brand, string customerName, string problemDescription,
                        DateTime receivedDate, DateTime expectedDeliveryDate, bool needsGasRefill = false)
        : base(serialNumber, brand, customerName, problemDescription, receivedDate, expectedDeliveryDate)
    {
        _needsGasRefill = needsGasRefill;
    }

    public bool NeedsGasRefill
    {
        get => _needsGasRefill;
        set => _needsGasRefill = value;
    }

    public override string ApplianceType => "Refrigerator";

    public override decimal CalculateRepairCost()
    {
        decimal cost = (decimal)LaborHours * HourlyRate + PartsCost;
        if (_needsGasRefill)
            cost += GasRefillFee;
        return cost;
    }
}
