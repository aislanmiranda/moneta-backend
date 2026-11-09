namespace AgentOffice.Application.Modules;

public interface IModuleService
{
    Task<List<ModuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<ModuleDto> CreateAsync(CreateModuleDto input, CancellationToken ct = default);
}
