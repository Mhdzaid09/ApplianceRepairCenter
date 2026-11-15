namespace ApplianceRepairCenter.Models;

/// <summary>
/// Air conditioner: hourly labor + parts + on-site visit fee (units are usually wall-mounted).
/// </summary>
public class AirConditioner : Appliance
{
    private const decimal HourlyRate = 18m;
    private const decimal OnSiteVisitFee = 25m;

    private bool _requiresOnSiteVisit;

    public AirConditioner(string serialNumber, string brand, string customerName, string problemDescription,
                          DateTime receivedDate, DateTime expectedDeliveryDate, bool requiresOnSiteVisit = false)
        : base(serialNumber, brand, customerName, problemDescription, receivedDate, expectedDeliveryDate)
    {
        _requiresOnSiteVisit = requiresOnSiteVisit;
    }

    public bool RequiresOnSiteVisit
    {
        get => _requiresOnSiteVisit;
        set => _requiresOnSiteVisit = value;
    }

    public override string ApplianceType => "Air Conditioner";

    public override decimal CalculateRepairCost()
    {
        decimal cost = (decimal)LaborHours * HourlyRate + PartsCost;
        if (_requiresOnSiteVisit)
            cost += OnSiteVisitFee;
        return cost;
    }
}
