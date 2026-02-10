namespace EduConnect.Shared.Enums;

/// <summary>
/// How a contract is billed: by credit hours or by monthly subscription.
/// </summary>
public enum BillingType
{
    /// <summary>Contract has a pool of hours; each session deducts from RemainingHours.</summary>
    Hourly = 1,
    /// <summary>Subscription valid until SubscriptionPeriodEnd; no hour deduction.</summary>
    Monthly = 2
}
