using System;

namespace EIR_9209_2.Models;

/// <summary>
/// Represents information about a scan.
/// </summary>
public class ScanInfo
{
    /// <summary>
    /// Gets or sets the action of the scan.
    /// </summary>
    public string Action { get; set; } = "";

    /// <summary>
    /// Gets or sets the result of the scan.
    /// </summary>
    public string Result { get; set; } = "";

    /// <summary>
    /// Gets or sets the message of the scan.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the data of the scan.
    /// </summary>
    public TranscationData Data { get; set; } = new TranscationData();
}
/// <summary>
/// Represents transaction data.
/// </summary>
public class TranscationData
{
    /// <summary>
    /// Gets or sets the time when the data was added.
    /// </summary>
    public List<Transaction> Transactions { get; set; } = [];
}

/// <summary>
/// Represents a transaction.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets or sets the time when the transaction was added.
    /// </summary>
    public long TimeAdded { get; set; }
    /// <summary>
    /// Gets or sets the encoded ID.
    /// </summary>
    public string EncodedID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction date and time.
    /// </summary>
    public DateTime TransactionDateTime { get; set; }

    /// <summary>
    /// Gets or sets the cardholder ID.
    /// </summary>
    public int CardholderID { get; set; }

    /// <summary>
    /// Gets or sets the area ID.
    /// </summary>
    public int AreaID { get; set; }

    /// <summary>
    /// Gets or sets the device ID.
    /// </summary>
    public int DeviceID { get; set; }

    /// <summary>
    /// Gets or sets the cardholder data.
    /// </summary>
    public CardholderData CardholderData { get; set; } = new CardholderData();
}

/// <summary>
/// Represents cardholder data.
/// </summary>
public class CardholderData
{
    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    public string CurrentStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cardholder ID.
    /// </summary>
    public int CardholderID { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the activation date.
    /// </summary>
    public DateTime Activation { get; set; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cardholder is blocked.
    /// </summary>
    public bool Blocked { get; set; }

    /// <summary>
    /// Gets or sets the EIN.
    /// </summary>
    public string EIN { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the designation activity.
    /// </summary>
    public string DesignationActivity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duty station FDB ID.
    /// </summary>
    public string DutyStationFDBID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duty station finance number.
    /// </summary>
    public string DutyStationFinanceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the import field.
    /// </summary>
    public string ImportField { get; set; } = string.Empty;
}