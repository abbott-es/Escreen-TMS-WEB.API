using AutoMapper;
using FluentValidation;
using LanguageExt;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service
{
    public class GenericService<TEntity, TDto> : IGenericService<TDto>
        where TEntity : class
        where TDto : class
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

        public virtual async Task<Either<string, TDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id, ct);
                return entity == null ? "Entity not found" : _mapper.Map<TDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by id");
                return $"Error fetching entity: {ex.Message}";
            }
        }

        public virtual async Task<Either<string, IEnumerable<TDto>>> GetAllAsync(CancellationToken ct = default, params string[] includePaths)
        {
            try
            {
                var entities = await _repository.GetAllAsync(ct, true, includePaths
                );
                return _mapper.Map<IEnumerable<TDto>>(entities).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all entities");
                return $"Error fetching all entities: {ex.Message}";
            }
        }

        public virtual async Task<Either<string, Guid>> AddAsync(TDto dto, CancellationToken ct = default)
        {
            try
            {
                var validate = await _validator.ValidateAsync(dto, ct);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    return string.Join("; ", errors.Select(e => $"{e.ErrorMessage}"));
                }

                var entity = _mapper.Map<TEntity>(dto);
                await _unitOfWork.ExecuteAsync(async c =>
                {
                    await _repository.AddAsync(entity, c);
                }, ct);

                var idProp = typeof(TEntity).GetProperty("Id");
                return idProp != null ? (Guid)idProp.GetValue(entity)! : Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity");
                return $"Error adding entity: {ex.Message}";
            }
        }

        public virtual async Task<Either<string, bool>> UpdateAsync(TDto dto, CancellationToken ct = default)
        {
            try
            {
                var entity = _mapper.Map<TEntity>(dto);
                await _unitOfWork.ExecuteAsync(async c =>
                {
                    _repository.Update(entity);
                }, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity");
                return $"Error updating entity: {ex.Message}";
            }
        }

        public virtual async Task<Either<string, bool>> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteAsync(async c =>
                {
                    await _repository.DeleteRangeAsync(ids, c);
                }, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entities");
                return $"Error deleting entities: {ex.Message}";
            }
        }
    }
}
