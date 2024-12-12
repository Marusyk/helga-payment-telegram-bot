using System.Text;
using Azure;
using Azure.Data.Tables;

namespace Helga.Function;

public sealed class DataStorage
{
    private readonly TableClient _tableClient;

    public DataStorage(string connectionString)
    {
        string tableName = "payments";
        TableServiceClient tableServiceClient = new TableServiceClient(connectionString);
        _tableClient = tableServiceClient.GetTableClient(tableName);
    }

    public string GetPayments()
    {
        const int paymentCount = 31;
        const decimal apartmentTotal = 73680m;
        const decimal parkingTotal = 22230m;

        var entities = _tableClient.Query<TableEntity>();

        var apartmentEntities = entities.Where(e => e.PartitionKey == PaymentType.Apartment.ToString());
        var apartmentPaymentCount = apartmentEntities.Count();
        decimal apartmentPaymentPercentage = (decimal)apartmentPaymentCount * 100 / paymentCount;
        var apartmentPaymentTotal = apartmentEntities.Sum(e => Convert.ToDecimal(e["Price"]));
        decimal apartmentTotalPercentage = (decimal)apartmentPaymentTotal * 100 / apartmentTotal;
        var apartmentTotalUnpaid = apartmentTotal - apartmentPaymentTotal;

        var parkingEntities = entities.Where(e => e.PartitionKey == PaymentType.Parking.ToString());
        var parkingPaymentCount = parkingEntities.Count();
        decimal parkingPaymentPercentage = (decimal)parkingPaymentCount * 100 / paymentCount;
        var parkingPaymentTotal = parkingEntities.Sum(e => Convert.ToDecimal(e["Price"]));
        decimal parkingTotalPercentage = (decimal)parkingPaymentTotal * 100 / parkingTotal;
        var parkingTotalUnpaid = parkingTotal - parkingPaymentTotal;

        var totalPaid = apartmentPaymentTotal + parkingPaymentTotal;
        decimal totalPaidPercentage = (decimal)totalPaid * 100 / (apartmentTotal + parkingTotal);
        var totalUnpaid = apartmentTotalUnpaid + parkingTotalUnpaid;

        StringBuilder sb = new();
        sb.AppendLine($"Платежів за квартиру: {apartmentPaymentCount}/{paymentCount} ({apartmentPaymentPercentage:F2}%)");
        sb.AppendLine($"Сплачено: {apartmentPaymentTotal}$ з {apartmentTotal}$");
        sb.AppendLine(GePtrogressBar((double)apartmentTotalPercentage));
        sb.AppendLine($"Залишилося: {apartmentTotalUnpaid}$");
        sb.AppendLine(Environment.NewLine);
        sb.AppendLine($"Платежів за паркінг: {parkingPaymentCount}/{paymentCount} ({parkingPaymentPercentage:F2}%)");
        sb.AppendLine($"Сплачено: {parkingPaymentTotal}$ з {parkingTotal}$");
        sb.AppendLine(GePtrogressBar((double)parkingTotalPercentage));
        sb.AppendLine($"Залишилося: {parkingTotalUnpaid}$");
        sb.AppendLine(Environment.NewLine);
        sb.AppendLine($"Сплачено: {totalPaid}$ з {apartmentTotal + parkingTotal}$");
        sb.AppendLine(GePtrogressBar((double)totalPaidPercentage, "🟦"));
        sb.AppendLine($"Залишилося: {totalUnpaid}$");

        return sb.ToString();
    }

    public async Task AddPayment(decimal price, decimal rate, PaymentType paymentType)
    {
        var entity = new TableEntity(paymentType.ToString(), Guid.NewGuid().ToString())
        {
            { "Price", price },
            { "Date", DateTimeOffset.UtcNow },
            { "CurrencyRate", rate }
        };

        await _tableClient.AddEntityAsync(entity);
    }

    private static string GePtrogressBar(double percentage, string fillChar = "🟩")
    {
        int length = 10;
        int filledLength = (int)Math.Round(length * percentage / 100.0);
        StringBuilder sb = new();
        string emptyChar = "⬛";

        for (int i = 0; i < filledLength; i++)
        {
            sb.Append(fillChar);
        }
        for (int i = filledLength; i < length; i++)
        {
            sb.Append(emptyChar);
        }
        sb.Append($" {percentage:F2}%");
        return sb.ToString();
    }
}

public enum PaymentType
{
    Apartment,
    Parking
}
