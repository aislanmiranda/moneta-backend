using Moneta.Domain;

namespace Moneta.Tests;

public class AppTimeTests
{
    [Fact]
    public void ToUtc_converte_horario_local_para_instante_utc()
    {
        var utc = AppTime.ToUtc(new DateOnly(2026, 11, 15), new TimeOnly(9, 0));

        // America/Sao_Paulo é UTC-3 (sem horário de verão desde 2019).
        Assert.Equal(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero), utc);
    }

    [Fact]
    public void ToUtc_horario_tarde_vira_dia_seguinte_em_utc()
    {
        var utc = AppTime.ToUtc(new DateOnly(2026, 11, 15), new TimeOnly(22, 30));

        Assert.Equal(new DateTimeOffset(2026, 11, 16, 1, 30, 0, TimeSpan.Zero), utc);
    }
}
