using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> 
        : IPipelineBehavior<TRequest,TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse: IErrorOr
    {
        private readonly IValidator<TRequest>? _validator;

        public ValidationBehavior(IValidator<TRequest>? validator= null)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationtoken)
        {
            if (_validator is null)
            {
                return await next();
            }
            var validatorResult = await _validator.ValidateAsync(request, cancellationtoken);

            if (validatorResult.IsValid)
            {
                return await next();
            }

            var errors = validatorResult.Errors
                        .ConvertAll(ValidationFailure => Error.Validation(
                            ValidationFailure.PropertyName,
                            ValidationFailure.ErrorMessage
                            ));
            return (dynamic)errors;
        }
    }
}
