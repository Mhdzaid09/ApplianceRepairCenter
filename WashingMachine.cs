namespace ApplianceRepairCenter.Models;

/// <summary>
/// Washing machine: hourly labor + parts + surcharge when the motor must be replaced.
/// </summary>
public class WashingMachine : Appliance
{
    private const decimal HourlyRate = 12m;
    private const decimal MotorReplacementFee = 30m;

    private bool _motorReplaced;

    public WashingMachine(string serialNumber, string brand, string customerName, string problemDescription,
                          DateTime receivedDate, DateTime expectedDeliveryDate, bool motorReplaced = false)
        : base(serialNumber, brand, customerName, problemDescription, receivedDate, expectedDeliveryDate)
    {
        _motorReplaced = motorReplaced;
    }

    public bool MotorReplaced
    {
        get => _motorReplaced;
        set => _motorReplaced = value;
    }

    public override string ApplianceType => "Washing Machine";

    public override decimal CalculateRepairCost()
    {
        decimal cost = (decimal)LaborHours * HourlyRate + PartsCost;
        if (_motorReplaced)
            cost += MotorReplacementFee;
        return cost;
    }
}
