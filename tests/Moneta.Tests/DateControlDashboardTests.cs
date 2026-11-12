using Moneta.Application.DateControl;
using Moneta.Domain.Enums;
using Moneta.Domain.Exceptions;
using NSubstitute;

namespace Moneta.Tests;

public class DateControlDashboardTests
{
    private static readonly DateOnly Today = new(2026, 11, 12);

    [Theory]
    [InlineData(DateControlEntryStatus.Paid, "2026-01-01", DashboardBucket.Paid)]
    [InlineData(DateControlEntryStatus.Paid, "2026-12-31", DashboardBucket.Paid)]
    [InlineData(DateControlEntryStatus.Pending, "2026-11-11", DashboardBucket.Overdue)]
    [InlineData(DateControlEntryStatus.Pending, "2026-11-12", DashboardBucket.Pending)]
    [InlineData(DateControlEntryStatus.Pending, "2026-11-13", DashboardBucket.Pending)]
    // Status Overdue gravado ainda conta como vencida só se a data já passou.
    [InlineData(DateControlEntryStatus.Overdue, "2026-10-01", DashboardBucket.Overdue)]
    [InlineData(DateControlEntryStatus.Overdue, "2026-11-12", DashboardBucket.Pending)]
    public void Classify_segue_a_regra_de_negocio(DateControlEntryStatus status, string due, DashboardBucket expected) =>
        Assert.Equal(expected, DateControlEntryService.Classify(status, DateOnly.Parse(due), Today));

    [Fact]
    public void Build_devolve_12_meses_zerados_sem_dados()
    {
        var dto = DateControlEntryService.BuildMonthlyDashboard(2026, []);

        Assert.Equal(2026, dto.Year);
        Assert.Equal(Enumerable.Range(1, 12), dto.Months.Select(m => m.Month));
        Assert.All(dto.Months, m =>
        {
            Assert.Equal(0m, m.Paid);
            Assert.Equal(0m, m.Overdue);
            Assert.Equal(0m, m.Pending);
        });
    }

    [Fact]
    public void Build_distribui_totais_por_mes_e_serie()
    {
        var dto = DateControlEntryService.BuildMonthlyDashboard(2026,
        [
            new(1, DashboardBucket.Paid, 100.50m),
            new(1, DashboardBucket.Pending, 20m),
            new(3, DashboardBucket.Overdue, 75m),
            new(12, DashboardBucket.Paid, 10m),
        ]);

        Assert.Equal(new MonthlyDashboardMonthDto(1, 100.50m, 0m, 20m), dto.Months[0]);
        Assert.Equal(new MonthlyDashboardMonthDto(3, 0m, 75m, 0m), dto.Months[2]);
        Assert.Equal(new MonthlyDashboardMonthDto(12, 10m, 0m, 0m), dto.Months[11]);
        Assert.Equal(new MonthlyDashboardMonthDto(2, 0m, 0m, 0m), dto.Months[1]);
    }

    [Fact]
    public async Task Service_agrega_via_repositorio_com_o_modulo_e_ano_pedidos()
    {
        var moduleId = Guid.NewGuid();
        var repo = Substitute.For<IDateControlEntryRepository>();
        repo.GetMonthlyAggregateAsync(moduleId, 2025, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([new DashboardAggregateRow(6, DashboardBucket.Paid, 50m)]);
        var service = new DateControlEntryService(
            repo, Substitute.For<IDateControlSourceRepository>(), Substitute.For<IDateControlPartyRepository>());

        var dto = await service.GetMonthlyDashboardAsync(moduleId, 2025);

        Assert.Equal(2025, dto.Year);
        Assert.Equal(50m, dto.Months[5].Paid);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public async Task Service_rejeita_ano_invalido(int year)
    {
        var service = new DateControlEntryService(
            Substitute.For<IDateControlEntryRepository>(),
            Substitute.For<IDateControlSourceRepository>(),
            Substitute.For<IDateControlPartyRepository>());

        await Assert.ThrowsAsync<DomainValidationException>(() => service.GetMonthlyDashboardAsync(Guid.NewGuid(), year));
    }
}
