namespace UPS.WWRR.Business.Common.Enum
{
    /// <summary>
    ///     Represents the lifecycle status of a data load.
    /// </summary>
    public enum LoadStatus
    {
        ReadyForValidation = 0,
        FailedValidation = 1,
        ReadyToProcess = 2,
        Processing = 3,
        Failed = 4,
        Processed = 5,
        MissingRequiredPair = 6,
    }
}
