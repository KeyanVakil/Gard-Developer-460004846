namespace GardPortal.DTOs;

public class SummaryDto
{
    public int TotalActivePolicies { get; set; }
    public decimal TotalInsuredValue { get; set; }
    public decimal TotalAnnualPremium { get; set; }
    public int OpenClaimsCount { get; set; }
    public decimal OutstandingClaimsValue { get; set; }
}

public class CategoryCountDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalEstimatedAmount { get; set; }
}

public class CoverageCountDto
{
    public string CoverageType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalInsuredValue { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalEstimatedAmount { get; set; }
}
