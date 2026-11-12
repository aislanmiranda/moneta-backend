namespace Moneta.Application.DateControl;

/// <summary>Soma por mês: paga, vencida (não paga com vencimento anterior a hoje) e pendente
/// (não paga com vencimento hoje ou futuro).</summary>
public record MonthlyDashboardMonthDto(int Month, decimal Paid, decimal Overdue, decimal Pending);

public record MonthlyDashboardDto(int Year, List<MonthlyDashboardMonthDto> Months);

/// <summary>Classificação de um lançamento para o dashboard.</summary>
public enum DashboardBucket { Paid = 0, Overdue = 1, Pending = 2 }

/// <summary>Linha agregada (mês + classificação) calculada no banco.</summary>
public record DashboardAggregateRow(int Month, DashboardBucket Bucket, decimal Total);
