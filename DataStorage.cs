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
        var entities = _tableClient.Query<TableEntity>();

        int totalCount = entities.Count();
        int paidCount = 0;
        decimal totalPaid = 0;
        decimal totalUnpaid = 0;
        decimal totalAmount = 0;

        foreach (TableEntity entity in entities)
        {
            decimal price = Convert.ToDecimal(entity["Price"]);
            bool paid = Convert.ToBoolean(entity["Paid"]);
            totalAmount += price;
            if (paid)
            {
                paidCount += 1;
                totalPaid += price;
            }
            else
            {
                totalUnpaid += price;
            }
        }

        StringBuilder sb = new();
        //var paymentPercentage = paidCount * 100 / totalCount;
        //sb.AppendLine($"Платежів: {paidCount}/{totalCount} ({paymentPercentage}%)");
        //sb.AppendLine(Environment.NewLine);
        var paidPercentage = totalPaid * 100 / totalAmount;
        sb.AppendLine($"Сплачено: {totalPaid}$ з {totalAmount}$");
        sb.AppendLine(GePtrogressBar((double)paidPercentage));
        sb.AppendLine(Environment.NewLine);
        sb.AppendLine($"Залишилося: {totalUnpaid}$");
        return sb.ToString();
    }

    public async Task AddPayment(float rate)
    {
        var month = DateTime.Now.Month.ToString();
        var entities = _tableClient.Query<TableEntity>();
        var entity = entities.First(x => x.PartitionKey.Contains(month));
        entity["Paid"] = true;
        entity["CurrencyRate"] = rate;

        await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
    }

    private static string GePtrogressBar(double percentage)
    {
        int length = 10;
        int filledLength = (int)Math.Round(length * percentage / 100.0);
        StringBuilder sb = new();
        string fillChar = "🟩";
        string emptyChar = "⬛";

        for (int i = 0; i < filledLength; i++)
        {
            sb.Append(fillChar);
        }
        for (int i = filledLength; i < length; i++)
        {
            sb.Append(emptyChar);
        }
        sb.Append($" {percentage}%");
        return sb.ToString();
    }
}
