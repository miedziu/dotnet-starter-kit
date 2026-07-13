using FluentValidation;
using FSH.Framework.Web.Validation;
using FSH.Modules.Identity.Contracts.v1.Sessions.GetAllSessions;

namespace FSH.Modules.Identity.Features.v1.Sessions.GetAllSessions;

public sealed class GetAllSessionsValidator : AbstractValidator<GetAllSessionsQuery>
{
    public GetAllSessionsValidator()
    {
        Include(new PagedQueryValidator<GetAllSessionsQuery>());
    }
}