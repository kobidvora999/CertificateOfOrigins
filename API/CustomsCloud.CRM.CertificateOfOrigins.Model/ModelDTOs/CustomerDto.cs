namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public int? GenderId { get; set; }
    public bool IsCorporation { get; set; }
    public bool IsMalkar { get; set; }
    public bool IsFromShaam { get; set; }
    public int? VatNumber { get; set; }
    public DateTimeOffset? VatOpenDate { get; set; }
    public string? VatOpenDateStr { get; set; }
    public int? VatStatusId { get; set; }
    public DateTimeOffset? VatStatusDate { get; set; }
    public bool IsValidVatNumber { get; set; }
    public int? CustomerStatusByRashamId { get; set; }
    public int? VatReportingGroupId { get; set; }
    public DateTimeOffset? DateOfImmigration { get; set; }
    public string? ImmigrationDate { get; set; }
    public string? Activities { get; set; }
    public string? Indications { get; set; }
    public string? ExternalIdNum { get; set; }
    public string? Address { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
    public DateTimeOffset? BorderCrossingEndDate { get; set; }
    public DateTimeOffset? BorderCrossingStartDate { get; set; }
    public int? CountryId { get; set; }
    public int? CustomerTypeSpecificId { get; set; }
    public string? CustomerTypeSpecificName { get; set; }
    public string? CustomerTypeGeneralName { get; set; }
    public bool IsActive { get; set; }
    public bool IsActiveInVat { get; set; }
    public string? LocalFirstName { get; set; }
    public string? LocalLastName { get; set; }
    public string? EnglishFirstName { get; set; }
    public string? EnglishLastName { get; set; }
    public int? ShaamFinancialInsitituteInMalkarId { get; set; }
    public int? TaxDeductionCustomerTypeId { get; set; }
    public DateTimeOffset? LastShaamDetailsBeforeIrrevesibleActionUpdate { get; set; }
    public bool CanImportExportCommercial { get; set; }
    public bool IsPalestinian { get; set; }
    public int? AuthoritiyId { get; set; }
    public bool IsMinor { get; set; }
    public string? EconomicBranchVat { get; set; }
    public int? CustomerTypeGeneralId { get; set; }
    public List<CustomerAddressDto> Addresses { get; set; } = new();
    public PassportParamsDto? PassportParams { get; set; }
    public List<PassportParamsDto> AllPassportParams { get; set; } = new();
    public AddressDto? AddressByPurposeType { get; set; }
    public List<CustomerActivityDto> ActiveActivityTypes { get; set; } = new();
}
