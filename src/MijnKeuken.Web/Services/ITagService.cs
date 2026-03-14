using MijnKeuken.Application.Common;
using MijnKeuken.Application.Tags.DTOs;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Web.Services;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<Result<Guid>> CreateAsync(string name, TagType type, string color);
    Task<Result> UpdateAsync(Guid id, string name, TagType type, string color);
    Task<Result> DeleteAsync(Guid id);
}
