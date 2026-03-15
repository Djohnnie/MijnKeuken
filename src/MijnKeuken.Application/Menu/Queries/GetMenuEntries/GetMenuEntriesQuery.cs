using MediatR;
using MijnKeuken.Application.Menu.DTOs;

namespace MijnKeuken.Application.Menu.Queries.GetMenuEntries;

public record GetMenuEntriesQuery(DateOnly From, DateOnly To) : IRequest<List<MenuEntryDto>>;
