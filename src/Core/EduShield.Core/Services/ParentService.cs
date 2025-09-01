using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Interfaces;
using EduShield.Core.Enums;
using AutoMapper;

namespace EduShield.Core.Services;

/// <summary>
/// Service implementation for Parent business logic operations
/// </summary>
public class ParentService : IParentService
{
    private readonly IParentRepository _parentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository; // Added IUserRepository

    public ParentService(IParentRepository parentRepository, IStudentRepository studentRepository, IMapper mapper, IUserRepository userRepository)
    {
        _parentRepository = parentRepository;
        _studentRepository = studentRepository;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<ParentResponse>> GetAllAsync()
    {
        var parents = await _parentRepository.GetAllAsync();
        return parents.Select(MapToResponse);
    }

    public async Task<ParentResponse?> GetByIdAsync(Guid id)
    {
        var parent = await _parentRepository.GetByIdAsync(id);
        return parent != null ? MapToResponse(parent) : null;
    }

    public async Task<ParentResponse?> GetByEmailAsync(string email)
    {
        var parent = await _parentRepository.GetByEmailAsync(email);
        return parent != null ? MapToResponse(parent) : null;
    }

    public async Task<ParentResponse?> GetByUserIdAsync(Guid userId)
    {
        var parent = await _parentRepository.GetByUserIdAsync(userId);
        return parent != null ? MapToResponse(parent) : null;
    }

    public async Task<ParentResponse?> GetWithChildrenByIdAsync(Guid id)
    {
        var parent = await _parentRepository.GetWithChildrenByIdAsync(id);
        return parent != null ? MapToResponse(parent) : null;
    }

    public async Task<IEnumerable<ParentResponse>> GetByTypeAsync(ParentType parentType)
    {
        var parents = await _parentRepository.GetByTypeAsync(parentType);
        return parents.Select(MapToResponse);
    }

    public async Task<IEnumerable<ParentResponse>> GetByCityAsync(string city)
    {
        var parents = await _parentRepository.GetByCityAsync(city);
        return parents.Select(MapToResponse);
    }

    public async Task<IEnumerable<ParentResponse>> GetByStateAsync(string state)
    {
        var parents = await _parentRepository.GetByStateAsync(state);
        return parents.Select(MapToResponse);
    }

    public async Task<IEnumerable<ParentResponse>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ParentResponse>();

        var parents = await _parentRepository.SearchByNameAsync(searchTerm);
        return parents.Select(MapToResponse);
    }

    public async Task<IEnumerable<ParentResponse>> GetEmergencyContactsAsync()
    {
        var parents = await _parentRepository.GetEmergencyContactsAsync();
        return parents.Select(MapToResponse);
    }

    public async Task<IEnumerable<ParentResponse>> GetAuthorizedForPickupAsync()
    {
        var parents = await _parentRepository.GetAuthorizedForPickupAsync();
        return parents.Select(MapToResponse);
    }

    public async Task<ParentResponse> CreateAsync(CreateParentRequest request)
    {
        // Validate request
        var (isValid, errors) = await ValidateAsync(request);
        if (!isValid)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
        }

        // Check if email already exists in both User and Parent tables
        if (await _userRepository.ExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }
        
        if (await _parentRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"Parent with email '{request.Email}' already exists.");
        }

        // Create User record for authentication
        var user = new Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = $"{request.FirstName} {request.LastName}".Trim(),
            Role = UserRole.Parent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create and save the user first
        var createdUser = await _userRepository.CreateAsync(user);

        // Create parent entity
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AlternatePhoneNumber = request.AlternatePhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Gender = request.Gender,
            Occupation = request.Occupation,
            Employer = request.Employer,
            WorkPhone = request.WorkPhone,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelationship = request.EmergencyContactRelationship,
            ParentType = request.ParentType,
            IsEmergencyContact = request.IsEmergencyContact,
            IsAuthorizedToPickup = request.IsAuthorizedToPickup,
            IsActive = request.IsActive,
            UserId = createdUser.Id // Link to the created user
        };

        var createdParent = await _parentRepository.AddAsync(parent);
        return MapToResponse(createdParent);
    }

    public async Task<ParentResponse> UpdateAsync(Guid id, UpdateParentRequest request)
    {
        // Validate request
        var (isValid, errors) = await ValidateUpdateAsync(id, request);
        if (!isValid)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
        }

        // Get existing parent
        var existingParent = await _parentRepository.GetByIdAsync(id);
        if (existingParent == null)
        {
            throw new KeyNotFoundException($"Parent with ID '{id}' not found.");
        }

        // Check if email already exists (excluding current parent)
        if (await _parentRepository.EmailExistsAsync(request.Email, id))
        {
            throw new InvalidOperationException($"Parent with email '{request.Email}' already exists.");
        }

        // Update parent properties
        existingParent.FirstName = request.FirstName;
        existingParent.LastName = request.LastName;
        existingParent.Email = request.Email;
        existingParent.PhoneNumber = request.PhoneNumber;
        existingParent.AlternatePhoneNumber = request.AlternatePhoneNumber;
        existingParent.DateOfBirth = request.DateOfBirth;
        existingParent.Address = request.Address;
        existingParent.City = request.City;
        existingParent.State = request.State;
        existingParent.PostalCode = request.PostalCode;
        existingParent.Country = request.Country;
        existingParent.Gender = request.Gender;
        existingParent.Occupation = request.Occupation;
        existingParent.Employer = request.Employer;
        existingParent.WorkPhone = request.WorkPhone;
        existingParent.EmergencyContactName = request.EmergencyContactName;
        existingParent.EmergencyContactPhone = request.EmergencyContactPhone;
        existingParent.EmergencyContactRelationship = request.EmergencyContactRelationship;
        existingParent.ParentType = request.ParentType;
        existingParent.IsEmergencyContact = request.IsEmergencyContact;
        existingParent.IsAuthorizedToPickup = request.IsAuthorizedToPickup;
        existingParent.IsActive = request.IsActive;

        var updatedParent = await _parentRepository.UpdateAsync(existingParent);
        return MapToResponse(updatedParent);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _parentRepository.DeleteAsync(id);
    }

    public async Task<bool> AddChildAsync(Guid parentId, Guid childId)
    {
        // Verify parent exists
        var parent = await _parentRepository.GetByIdAsync(parentId);
        if (parent == null)
        {
            throw new KeyNotFoundException($"Parent with ID '{parentId}' not found.");
        }

        // Verify student exists
        var student = await _studentRepository.GetByIdAsync(childId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID '{childId}' not found.");
        }

        // Check if student already has a parent
        if (student.ParentId.HasValue)
        {
            throw new InvalidOperationException($"Student '{student.FullName}' already has a parent assigned.");
        }

        // Add child to parent
        parent.AddChild(student);
        await _parentRepository.UpdateAsync(parent);
        
        // Update the student entity directly to persist the ParentId field
        // We need to ensure the student entity is properly tracked by Entity Framework
        student.ParentId = parentId;
        await _studentRepository.UpdateAsync(student);
        
        return true;
    }

    public async Task<bool> RemoveChildAsync(Guid parentId, Guid childId)
    {
        // Verify parent exists
        var parent = await _parentRepository.GetByIdAsync(parentId);
        if (parent == null)
        {
            throw new KeyNotFoundException($"Parent with ID '{parentId}' not found.");
        }

        // Check if parent has this child
        if (!parent.HasChild(childId))
        {
            throw new InvalidOperationException($"Parent does not have child with ID '{childId}'.");
        }

        // Remove child from parent
        parent.RemoveChild(childId);
        await _parentRepository.UpdateAsync(parent);
        
        // Update the student entity to persist the ParentId field change
        var student = await _studentRepository.GetByIdAsync(childId);
        if (student != null)
        {
            student.ParentId = null;
            await _studentRepository.UpdateAsync(student);
        }
        return true;
    }

    public async Task<ParentStatistics> GetStatisticsAsync()
    {
        return await _parentRepository.GetStatisticsAsync();
    }

    public Task<(bool IsValid, List<string> Errors)> ValidateAsync(CreateParentRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("FirstName is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("LastName is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");
        else if (!IsValidEmail(request.Email))
            errors.Add("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            errors.Add("PhoneNumber is required.");

        if (string.IsNullOrWhiteSpace(request.Address))
            errors.Add("Address is required.");

        if (request.DateOfBirth >= DateTime.Today)
            errors.Add("DateOfBirth must be in the past.");

        if (request.DateOfBirth < DateTime.Today.AddYears(-120))
            errors.Add("DateOfBirth is not realistic.");

        return Task.FromResult((errors.Count == 0, errors));
    }

    public Task<(bool IsValid, List<string> Errors)> ValidateUpdateAsync(Guid id, UpdateParentRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("FirstName is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("LastName is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");
        else if (!IsValidEmail(request.Email))
            errors.Add("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            errors.Add("PhoneNumber is required.");

        if (string.IsNullOrWhiteSpace(request.Address))
            errors.Add("Address is required.");

        if (request.DateOfBirth >= DateTime.Today)
            errors.Add("DateOfBirth must be in the past.");

        if (request.DateOfBirth < DateTime.Today.AddYears(-120))
            errors.Add("DateOfBirth is not realistic.");

        return Task.FromResult((errors.Count == 0, errors));
    }

    private ParentResponse MapToResponse(Parent parent)
    {
        var response = new ParentResponse
        {
            Id = parent.Id,
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            Email = parent.Email,
            PhoneNumber = parent.PhoneNumber,
            AlternatePhoneNumber = parent.AlternatePhoneNumber,
            DateOfBirth = parent.DateOfBirth,
            Address = parent.Address,
            City = parent.City,
            State = parent.State,
            PostalCode = parent.PostalCode,
            Country = parent.Country,
            Gender = parent.Gender,
            Occupation = parent.Occupation,
            Employer = parent.Employer,
            WorkPhone = parent.WorkPhone,
            EmergencyContactName = parent.EmergencyContactName,
            EmergencyContactPhone = parent.EmergencyContactPhone,
            EmergencyContactRelationship = parent.EmergencyContactRelationship,
            ParentType = parent.ParentType,
            IsEmergencyContact = parent.IsEmergencyContact,
            IsAuthorizedToPickup = parent.IsAuthorizedToPickup,
            UserId = parent.UserId,
            IsActive = parent.IsActive,
            CreatedAt = parent.CreatedAt,
            UpdatedAt = parent.UpdatedAt,
            FullName = parent.FullName,
            Age = parent.Age,
            FullAddress = parent.FullAddress,
            ChildrenCount = parent.ChildrenCount,
            IsPrimaryParent = parent.IsPrimaryParent
        };

        // Map children information
        response.Children = parent.Children.Select(child => new ParentChildInfo
        {
            Id = child.Id,
            FirstName = child.FirstName,
            LastName = child.LastName,
            RollNumber = child.RollNumber,
            Grade = child.Grade,
            Section = child.Section,
            Status = child.Status,
            EnrollmentDate = child.EnrollmentDate,
            FullName = child.FullName,
            Age = child.Age,
            IsEnrolled = child.IsEnrolled
        }).ToList();

        return response;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
