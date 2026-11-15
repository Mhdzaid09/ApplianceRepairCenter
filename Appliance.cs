using ApplianceRepairCenter.Events;
using ApplianceRepairCenter.Helpers;
using ApplianceRepairCenter.Interfaces;

namespace ApplianceRepairCenter.Models;

/// <summary>
/// Abstract base class for all appliances. Implements <see cref="IRepairable"/>,
/// holds the common state and declares the abstract cost calculation.
/// </summary>
public abstract class Appliance : IRepairable
{
    // ---------- Static members ----------
    private static int _totalRegistered;
    private static int _totalRepairedOrDelivered;

    /// <summary>Total number of appliances ever registered in the system.</summary>
    public static int TotalRegistered => _totalRegistered;

    /// <summary>Number of appliances whose repair is finished (Repaired or Delivered).</summary>
    public static int TotalFinished => _totalRepairedOrDelivered;

    // ---------- Private fields (Encapsulation) ----------
    private readonly string _serialNumber;      // read-only after creation
    private readonly DateTime _receivedDate;    // read-only after creation
    private readonly string _brand;             // read-only after creation
    private string _customerName;
    private string _problemDescription;
    private DateTime _expectedDeliveryDate;
    private double _laborHours;
    private decimal _partsCost;
    private RepairStatus _status;

    // ---------- Events ----------
    public event ApplianceStatusChangedHandler? StatusChanged;
    public event DeliveryOverdueHandler? DeliveryOverdue;

    // ---------- Constructor ----------
    protected Appliance(string serialNumber, string brand, string customerName,
                        string problemDescription, DateTime receivedDate, DateTime expectedDeliveryDate)
    {
        if (!Validator.IsValidSerialNumber(serialNumber))
            throw new ArgumentException("Invalid serial number. Expected format: XX-1234 (e.g. RF-10234).", nameof(serialNumber));
        if (!Validator.IsValidName(brand))
            throw new ArgumentException("Brand name is invalid.", nameof(brand));
        if (!Validator.IsValidName(customerName))
            throw new ArgumentException("Customer name is invalid.", nameof(customerName));
        if (!Validator.IsValidName(problemDescription))
            throw new ArgumentException("Problem description is invalid.", nameof(problemDescription));
        if (!Validator.IsValidDeliveryDate(receivedDate, expectedDeliveryDate))
            throw new ArgumentException("Expected delivery date cannot be before the received date.", nameof(expectedDeliveryDate));

        _serialNumber = serialNumber;
        _brand = brand.Trim();
        _customerName = customerName.Trim();
        _problemDescription = problemDescription.Trim();
        _receivedDate = receivedDate.Date;
        _expectedDeliveryDate = expectedDeliveryDate.Date;
        _status = RepairStatus.Received;

        _totalRegistered++;
    }

    // ---------- Properties ----------
    public string SerialNumber => _serialNumber;
    public string Brand => _brand;
    public DateTime ReceivedDate => _receivedDate;
    public RepairStatus Status => _status;

    public string CustomerName
    {
        get => _customerName;
        set
        {
            if (!Validator.IsValidName(value))
                throw new ArgumentException("Customer name is invalid.");
            _customerName = value.Trim();
        }
    }

    public string ProblemDescription
    {
        get => _problemDescription;
        set
        {
            if (!Validator.IsValidName(value))
                throw new ArgumentException("Problem description is invalid.");
            _problemDescription = value.Trim();
        }
    }

    public DateTime ExpectedDeliveryDate
    {
        get => _expectedDeliveryDate;
        set
        {
            if (_status == RepairStatus.Delivered)
                throw new InvalidOperationException("Cannot change the delivery date of a delivered appliance.");
            if (!Validator.IsValidDeliveryDate(_receivedDate, value))
                throw new ArgumentException("Expected delivery date cannot be before the received date.");
            _expectedDeliveryDate = value.Date;
        }
    }

    public double LaborHours
    {
        get => _laborHours;
        set
        {
            if (!Validator.IsNonNegative(value))
                throw new ArgumentException("Labor hours cannot be negative.");
            _laborHours = value;
        }
    }

    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            if (!Validator.IsNonNegative(value))
                throw new ArgumentException("Parts cost cannot be negative.");
            _partsCost = value;
        }
    }

    /// <summary>Appliance type name shown in reports (overridden by subclasses).</summary>
    public abstract string ApplianceType { get; }

    // ---------- Abstract operation (Polymorphism) ----------
    /// <summary>Each appliance type calculates its repair cost in its own way.</summary>
    public abstract decimal CalculateRepairCost();

    // ---------- Status workflow ----------
    public void StartRepair()
    {
        if (_status != RepairStatus.Received)
            throw new InvalidOperationException($"Cannot start repair: appliance is currently '{_status}'.");
        ChangeStatus(RepairStatus.UnderRepair);
    }

    public void CompleteRepair()
    {
        if (_status != RepairStatus.UnderRepair)
            throw new InvalidOperationException($"Cannot complete repair: appliance is currently '{_status}'.");
        ChangeStatus(RepairStatus.Repaired);
    }

    public void Deliver()
    {
        if (_status != RepairStatus.Repaired)
            throw new InvalidOperationException($"Cannot deliver: appliance is currently '{_status}'.");
        ChangeStatus(RepairStatus.Delivered);
    }

    private void ChangeStatus(RepairStatus newStatus)
    {
        RepairStatus oldStatus = _status;
        _status = newStatus;

        if (newStatus == RepairStatus.Repaired)
            _totalRepairedOrDelivered++;

        StatusChanged?.Invoke(this, oldStatus, newStatus);
    }

    /// <summary>
    /// Fires <see cref="DeliveryOverdue"/> if the repair is not finished and the expected date has passed.
    /// </summary>
    public bool CheckDelay(DateTime today)
    {
        bool notFinished = _status == RepairStatus.Received || _status == RepairStatus.UnderRepair;
        if (notFinished && today.Date > _expectedDeliveryDate)
        {
            int daysLate = (today.Date - _expectedDeliveryDate).Days;
            DeliveryOverdue?.Invoke(this, daysLate);
            return true;
        }
        return false;
    }

    public string GetSummary()
    {
        return $"[{ApplianceType}] {_serialNumber} | {_brand} | Customer: {_customerName} | " +
               $"Status: {_status} | Due: {_expectedDeliveryDate:yyyy-MM-dd} | Cost: {CalculateRepairCost():F2}";
    }

    public override string ToString() => GetSummary();
}
