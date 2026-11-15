using System.Text;
using ApplianceRepairCenter.Helpers;
using ApplianceRepairCenter.Models;
using ApplianceRepairCenter.Services;

Console.OutputEncoding = Encoding.UTF8;

var center = new RepairCenter();
center.NotificationRaised += message =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  >> NOTIFICATION: {message}");
    Console.ResetColor();
};

SeedSampleData(center);

bool running = true;
while (running)
{
    PrintMenu();
    string choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "1": AddAppliance(center); break;
            case "2": ShowAll(center); break;
            case "3": ChangeStatus(center, "start"); break;
            case "4": ChangeStatus(center, "complete"); break;
            case "5": ChangeStatus(center, "deliver"); break;
            case "6": SetRepairDetails(center); break;
            case "7": ShowRepairedAndPending(center); break;
            case "8": ShowStatistics(center); break;
            case "9": CheckDelays(center); break;
            case "0": running = false; break;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

Console.WriteLine("Goodbye!");

// ===================== UI helpers =====================

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("========== Home Appliance Repair Center ==========");
    Console.WriteLine("1. Register new appliance");
    Console.WriteLine("2. Show all appliances");
    Console.WriteLine("3. Start repair");
    Console.WriteLine("4. Complete repair");
    Console.WriteLine("5. Deliver appliance to customer");
    Console.WriteLine("6. Set labor hours and parts cost");
    Console.WriteLine("7. Show repaired / still under maintenance");
    Console.WriteLine("8. Statistics and total costs");
    Console.WriteLine("9. Check overdue appliances");
    Console.WriteLine("0. Exit");
    Console.Write("Your choice: ");
}

static string ReadText(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine()?.Trim() ?? "";
}

static DateTime ReadDate(string prompt)
{
    while (true)
    {
        string input = ReadText(prompt);
        if (Validator.TryParseDate(input, out DateTime date))
            return date;
        Console.WriteLine("Invalid date. Use format yyyy-MM-dd.");
    }
}

static bool ReadYesNo(string prompt)
{
    string input = ReadText(prompt + " (y/n): ").ToLowerInvariant();
    return input == "y" || input == "yes";
}

static void AddAppliance(RepairCenter center)
{
    Console.WriteLine("Type: 1) Refrigerator  2) Washing Machine  3) Air Conditioner");
    string type = ReadText("Choose type: ");

    string serial = ReadText("Serial number (e.g. RF-10234): ").ToUpperInvariant();
    string brand = ReadText("Brand: ");
    string customer = ReadText("Customer name: ");
    string problem = ReadText("Problem description: ");
    DateTime received = ReadDate("Received date (yyyy-MM-dd): ");
    DateTime expected = ReadDate("Expected delivery date (yyyy-MM-dd): ");

    Appliance appliance;
    switch (type)
    {
        case "1":
            appliance = new Refrigerator(serial, brand, customer, problem, received, expected,
                                         ReadYesNo("Needs gas refill?"));
            break;
        case "2":
            appliance = new WashingMachine(serial, brand, customer, problem, received, expected,
                                           ReadYesNo("Motor replaced?"));
            break;
        case "3":
            appliance = new AirConditioner(serial, brand, customer, problem, received, expected,
                                           ReadYesNo("Requires on-site visit?"));
            break;
        default:
            Console.WriteLine("Invalid type.");
            return;
    }

    center.AddAppliance(appliance);
    Console.WriteLine("Appliance registered successfully.");
}

static void ShowAll(RepairCenter center)
{
    if (center.Appliances.Count == 0)
    {
        Console.WriteLine("No appliances registered.");
        return;
    }

    // Polymorphism: each item calculates its own cost through the overridden method
    foreach (Appliance appliance in center.Appliances)
        Console.WriteLine(appliance.GetSummary());
}

static Appliance? AskForAppliance(RepairCenter center)
{
    string serial = ReadText("Serial number: ");
    Appliance? appliance = center.Find(serial);
    if (appliance is null)
        Console.WriteLine("Appliance not found.");
    return appliance;
}

static void ChangeStatus(RepairCenter center, string action)
{
    Appliance? appliance = AskForAppliance(center);
    if (appliance is null) return;

    switch (action)
    {
        case "start": appliance.StartRepair(); break;
        case "complete": appliance.CompleteRepair(); break;
        case "deliver": appliance.Deliver(); break;
    }
}

static void SetRepairDetails(RepairCenter center)
{
    Appliance? appliance = AskForAppliance(center);
    if (appliance is null) return;

    if (!double.TryParse(ReadText("Labor hours: "), out double hours))
    {
        Console.WriteLine("Invalid number.");
        return;
    }
    if (!decimal.TryParse(ReadText("Parts cost: "), out decimal parts))
    {
        Console.WriteLine("Invalid number.");
        return;
    }

    appliance.LaborHours = hours;
    appliance.PartsCost = parts;
    Console.WriteLine($"Updated. Current cost: {appliance.CalculateRepairCost():F2}");
}

static void ShowRepairedAndPending(RepairCenter center)
{
    Console.WriteLine("--- Repaired appliances ---");
    foreach (Appliance a in center.GetRepaired())
        Console.WriteLine(a.GetSummary());

    Console.WriteLine("--- Still under maintenance ---");
    foreach (Appliance a in center.GetPending())
        Console.WriteLine(a.GetSummary());
}

static void ShowStatistics(RepairCenter center)
{
    Console.WriteLine($"Total registered appliances (static property): {Appliance.TotalRegistered}");
    Console.WriteLine($"Total finished repairs (static property):      {Appliance.TotalFinished}");

    foreach (RepairStatus status in Enum.GetValues<RepairStatus>())
    {
        Console.WriteLine($"{status,-12}: {center.CountByStatus(status)} appliance(s), cost {center.GetTotalCostByStatus(status):F2}");
    }

    Console.WriteLine($"Total cost of all appliances: {center.GetTotalCost():F2}");
}

static void CheckDelays(RepairCenter center)
{
    int count = center.CheckAllDelays(DateTime.Today);
    if (count == 0)
        Console.WriteLine("No overdue appliances.");
}

static void SeedSampleData(RepairCenter center)
{
    DateTime today = DateTime.Today;

    var fridge = new Refrigerator("RF-10001", "Samsung", "Ahmad Ali", "Not cooling", today.AddDays(-3), today.AddDays(4), needsGasRefill: true);
    fridge.LaborHours = 2;
    fridge.PartsCost = 35m;

    var washer = new WashingMachine("WM-20001", "LG", "Sara Khaled", "Does not spin", today.AddDays(-10), today.AddDays(-2), motorReplaced: true);
    washer.LaborHours = 3;
    washer.PartsCost = 60m;

    var ac = new AirConditioner("AC-30001", "Gree", "Omar Nasser", "Water leaking", today.AddDays(-1), today.AddDays(5), requiresOnSiteVisit: true);
    ac.LaborHours = 1.5;
    ac.PartsCost = 20m;

    center.AddAppliance(fridge);
    center.AddAppliance(washer);
    center.AddAppliance(ac);
}
