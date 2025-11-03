using AutoMapper;
using FluentValidation;
using LanguageExt;
using System.Net;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IGeneric;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service.Generic
{
    public class GenericService<TEntity, TDto> : IGenericService<TDto>
        where TEntity : class
        where TDto : IBaseDto
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly IValidator<TDto> _validator;
        protected readonly IAppLogger<TEntity> _logger;

        public GenericService(
            IUnitOfWork unitOfWork,
            IRepository<TEntity> repository,
            IMapper mapper,
            IValidator<TDto> validator,
            IAppLogger<TEntity> logger)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public virtual async Task<Either<ApiResponse<string>, ApiResponse<TDto>>> GetByIdAsync(GenericFromQueryDto genericQuery, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(genericQuery.ID, ct, genericQuery.Includes);
                return entity == null
                    ? Prelude.Left(ApiResponse<string>.Fail([$"{typeof(TDto).Name.Substring(0, typeof(TDto).Name.Length - 3)} not found: {genericQuery.ID}"], HttpStatusCode.NotFound))
                    : Prelude.Right(ApiResponse<TDto>.Ok(_mapper.Map<TDto>(entity)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by id");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public virtual async Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<TDto>>>> GetAllAsync(CancellationToken ct = default, params string[] includePaths)
        {
            try
            {
                var entities = await _repository.GetAllAsync(null, ct, true, includePaths
                );
                return Prelude.Right(ApiResponse<IEnumerable<TDto>>.Ok(_mapper.Map<IEnumerable<TDto>>(entities).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all entities");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public virtual async Task<Either<ApiResponse<string>, ApiResponse<Guid>>> AddAsync(TDto dto, CancellationToken ct = default)
        {
            try
            {
                var validate = await _validator.ValidateAsync(dto, ct);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    return Prelude.Left(ApiResponse<string>.Fail(errors.Select(x => x.ErrorMessage).ToList(), HttpStatusCode.BadRequest));
                }

                var entity = _mapper.Map<TEntity>(dto);
                await _unitOfWork.ExecuteAsync(async c =>
                {
                    await _repository.AddAsync(entity, c);
                }, ct);

                var idProp = typeof(TEntity).GetProperty("ID");
                return Prelude.Right(ApiResponse<Guid>.Ok(idProp != null ? (Guid)idProp.GetValue(entity)! : Guid.Empty, HttpStatusCode.OK, "Created Successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public virtual async Task<Either<ApiResponse<string>, ApiResponse<string>>> UpdateAsync(TDto dto, CancellationToken ct = default, params string[] includePaths)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(dto.ID, ct, includePaths);
                if (existingEntity == null)
                {
                    return Prelude.Left(ApiResponse<string>.Fail([$"This entity id is not found: {dto.ID}"], HttpStatusCode.NotFound));
                }

                _mapper.Map(dto, existingEntity);

                var lambda = LambdaBuilder.BuildNavigationExpressions<TEntity>(includePaths);
                await _unitOfWork.ExecuteAsync(async c =>
                {
                    _repository.Update(existingEntity, lambda);
                }, ct);
                return Prelude.Right(ApiResponse<string>.Ok("Update Successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public virtual async Task<Either<ApiResponse<string>, ApiResponse<string>>> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["No IDs provided"]));
                }
                await _unitOfWork.ExecuteAsync(async c =>
                {
                    await _repository.DeleteRangeAsync(ids, c);
                }, ct);
                return Prelude.Right(ApiResponse<string>.Ok("Deleted Successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entities");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }
    }
}
