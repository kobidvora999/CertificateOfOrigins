using System;
using System.Collections.Generic;
using System.Linq;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.BL;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.Infrastructure.SystemTables.ExternalCommon;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.EntityValidation;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Utils;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using Microsoft.Practices.Unity;
using Exception = MalamTeam.Infrastructure.GeneralServices.EAISchema.Exception;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters;
using Customs.CRP.External.DealFile.Entities;
using Customs.Infrastructure.Lock;

namespace Customs.CRM.CertificateOfOrigins.Service
{
    public partial class CertificateOfOriginsIncomingMessageService
    {
        private readonly ServicesAdapter _servicesAdapter = new ServicesAdapter();
        private ISystemTablesUtil _systemTablesUtil
        { get { return Container.Resolve<ISystemTablesUtil>(); } }
        private readonly IStringUtil _stringUtil = UnityContainerManager.DefaultContainer.Resolve<IStringUtil>();
        private InfException _requestExceptions;

        private bool IsRelevantToCurrentVersion
        {
            get { return Configuration.GetConfig<int>(CertificateOfOriginsConstants.CustomsVersionNumber) == 2; }
        }

        private int? _certificateToUpdateId;
        private int? _destinationCountryId;
        private int _organizationUnit;
        private int _exporterID;

        public PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse InternalGetPC_MSG2280_2281_CertificateOfOriginRequest(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty() && Configuration.GetConfig<bool>("IsNeedToLockCertificateOfOrigin"))
            {
                var lockKey = request.Content.AgentRequest.certificateID;
                var locker = LockFactory.GetLock();
                ILockState lockState = null;
                try
                {
                    lockState = locker.LockUntilAsync(lockKey, DateTime.Now.AddMinutes(5)).Result;
                    if (!lockState.IsAcquired)
                    {
                        throw new InfException(EMessages.ConcurrencyErrorTryAgain);
                    }
                    return GetPC_MSG2280_2281_CertificateOfOriginRequestInner(request);
                }
                finally
                {
                    var isLockReleased = locker.SafeReleaseAsync(lockKey, lockState);
                }
            }
            else return GetPC_MSG2280_2281_CertificateOfOriginRequestInner(request);
        }

        private PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse GetPC_MSG2280_2281_CertificateOfOriginRequestInner(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                List<Exception> exceptions = null;
                CertificateOfOrigin savedCertificateOfOrigin = null;
                var bl = new CertificateOfOriginsBL(uow);
                _requestExceptions = new InfException(EMessages.RequestDataNotValid, null, new List<InfException>());
                var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();
                var exportDeclarationDetails = exportDealFileExternalServiceAdapter.GetExportDeclarationDetailsForCertificateOfOrigion(null, request.Content.AgentRequest.exportDeclarationNum);
                if (request.Content.AgentRequest.requestReasonCode != (int)ERequestReason.GetRequestStatus)
                {
                    CheckIfCertificateIsLinkedToDeclarationInAmendment(exportDeclarationDetails, request.Content.AgentRequest.exportDeclarationNum);
                }

                var certificateOfOriginMessageDetails = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails>();
                var invoiceDetails = new List<CertificateOfOriginInvoiceDetail>();
                if (request.Content.AgentRequest.requestReasonCode != (int)ERequestReason.CertificateCancellation)
                {
                    certificateOfOriginMessageDetails = GetCertificateDetailsFromMessageAndCheckFields(request);
                    invoiceDetails = CheckAndConvertInvoiceDetails(request);
                }
                savedCertificateOfOrigin = CheckRequestReasonAndGetSavedCertificate(request, bl);
                if (!certificateOfOriginMessageDetails.IsNullOrEmpty() && request.Content.AgentRequest.requestReasonCode.NotIn((int)ERequestReason.GetRequestStatus, (int)ERequestReason.CertificateCancellation))
                {
                    CheckFields(request, certificateOfOriginMessageDetails);
                }

                if (!_requestExceptions.Exceptions.IsNullOrEmpty())
                {
                    throw _requestExceptions;
                }

                CertificateOfOrigin certificateToResponse;

                //CR 156814
                switch (request.Content.AgentRequest.requestReasonCode)
                {
                    case (int)ERequestReason.GetRequestStatus:
                        certificateToResponse = savedCertificateOfOrigin;
                        break;

                    case (int)ERequestReason.CertificateCancellation:
                        bl.CancelCertificateOfOriginFromMessage(savedCertificateOfOrigin);
                        certificateToResponse = savedCertificateOfOrigin;
                        break;

                    default:
                        certificateToResponse = ConvertMessageToCertificateOfOrigin(request, certificateOfOriginMessageDetails, invoiceDetails);
                        certificateToResponse.ExportDeclarationDetailsDTO = exportDeclarationDetails;
                        bl.SaveCertificateOfOrigin(certificateToResponse, null, _certificateToUpdateId, out bool isStatusChanged, out bool isRemarksChanged);
                        if (request.Content.AgentRequest.requestReasonCode != (int)ERequestReason.EmptyCertificate && request.Content.AgentRequest.CertificateTypeCodeID != (int)ECertificateOfOriginType.NonManipulation)
                        {
                            exceptions = bl.CheckCertificateOfOriginOnDeclarationSubmited(certificateToResponse);
                        }
                        break;
                }
                var response = CreateCertificateOfOriginRequestFeedbackResponse(certificateToResponse, exceptions, request.Content.AgentRequest.requestReasonCode);
                return response;
            }
        }

        #region Check Message

        #region GetCertificateDetailsFromMessage

        /// <summary>
        /// Gets details from message and checks mandatory fields and unconditional fields by using reflection
        /// </summary>
        private Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails> GetCertificateDetailsFromMessageAndCheckFields(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            var certificteTypeId = request.Content.AgentRequest.certificateOfOriginTypeCode;
            var certificateTypeCodeEnum = CheckCertificateOfOriginTypeCodeEnum(certificteTypeId);
            if (certificateTypeCodeEnum == null) return null;
            request.Content.AgentRequest.CertificateTypeCodeID = certificateTypeCodeEnum.ID;
            request.Content.AgentRequest.CertificateTypeCodeName = certificateTypeCodeEnum.Name;
            request.Content.AgentRequest.IsCertificateTypeCodeMandatory = certificateTypeCodeEnum.IsCriterionMandatory;
            request.Content.AgentRequest.IsCustomsItemMandatory = certificateTypeCodeEnum.IsCustomsItemMandatory;
            request.Content.AgentRequest.IsZipcodeMandatory = certificateTypeCodeEnum.IsZipcodeMandatory;
            var certificateOfOriginDetails = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails>();
            List<DetailsPerCertificate> details;
            List<CertificateDetailsTypeCodeEnum> detailsTypes;
            GetCertificateOfOriginDetailsTypes(certificteTypeId, out details, out detailsTypes);
            var requestReasonCode = request.Content.AgentRequest.requestReasonCode;
            if (certificteTypeId == (int)ECertificateOfOriginType.NonManipulation)
            {
                if (request.Content.NonManipulationCertificate == null && requestReasonCode.NotIn((int)ERequestReason.EmptyCertificate, (int)ERequestReason.GetRequestStatus, (int)ERequestReason.CertificateCancellation))
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = "request.Content.NonManipulationCertificate" } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
                    return null;
                }
                if (request.Content.NonManipulationCertificate == null) return null;

                HandleNonManipulationCertificateType(request, certificteTypeId, certificateOfOriginDetails, details, detailsTypes);
            }
            else
            {
                if (request.Content.CertificateOfOrigin == null && requestReasonCode.NotIn((int)ERequestReason.EmptyCertificate, (int)ERequestReason.GetRequestStatus, (int)ERequestReason.CertificateCancellation))
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = "request.Content.CertificateOfOrigin" } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
                    return null;
                }
                if (request.Content.CertificateOfOrigin == null) return null;
                ValidateCertificateOfOriginRequestInvoiceDetail(request);
                var certificate = request.Content.CertificateOfOrigin;
                var msgCertificateFields = certificate.GetType().GetFields();
                var destinationCountryId = !certificate.DestinationCountry.IsNullOrEmpty() ? GetIdByCode<Country>(Country.PropCountryAlphaCode_2, certificate.DestinationCountry) : null;
                ValidateAndCreateCertificateOfOriginDetails(certificteTypeId, certificateOfOriginDetails, details, detailsTypes, certificate, msgCertificateFields, destinationCountryId);
            }
            return certificateOfOriginDetails;
        }

        private void ValidateCertificateOfOriginRequestInvoiceDetail(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            if (request.Content.CertificateOfOrigin.CertificateOfOriginRequestInvoiceDetail.IsNullOrEmpty())
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = "CertificateOfOriginRequestInvoiceDetail" } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
                throw _requestExceptions;
            }
            //for type defrant from non  NonManipulationCertificate
            for (int i = 0; i < request.Content.CertificateOfOrigin.CertificateOfOriginRequestInvoiceDetail.Count(); i++)
            {
                if (request.Content.CertificateOfOrigin.CertificateOfOriginRequestInvoiceDetail[i].CertificateOfOriginRequestItemDetail.IsNullOrEmpty())
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = "CertificateOfOriginRequestItemDetail" } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
                    throw _requestExceptions;
                }
                else if (request.Content.CertificateOfOrigin.CertificateOfOriginRequestInvoiceDetail[i].CertificateOfOriginRequestItemDetail.Any(c => c.MarksAndNumbers.IsNullOrEmpty()))
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = "MarksAndNumbers" } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
                }
            }
        }

        private void GetCertificateOfOriginDetailsTypes(int certificteTypeId, out List<DetailsPerCertificate> details, out List<CertificateDetailsTypeCodeEnum> detailsTypes)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var util = new CertificateOfOriginsUtil(uow);
                details = util.GetDetailsPerCertificate(certificteTypeId);
                detailsTypes = util.GetCertificateDetailsTypeCode();
            }
        }

        private void HandleNonManipulationCertificateType(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, int certificteTypeId, Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails> certificateOfOriginDetails, List<DetailsPerCertificate> details, List<CertificateDetailsTypeCodeEnum> detailsTypes)
        {
            var nonManipulation = request.Content.NonManipulationCertificate;
            var msgNonManipulationFields = nonManipulation.GetType().GetFields();
            ValidateAndCreateCertificateOfOriginDetails(certificteTypeId, certificateOfOriginDetails, details, detailsTypes, nonManipulation, msgNonManipulationFields);
            if (request.Content.CertificateOfOrigin != null)
            {
                GetCustomsHouseDetails(request, certificateOfOriginDetails, details, detailsTypes);
            }
        }

        private static void GetCustomsHouseDetails(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails> certificateOfOriginDetails, List<DetailsPerCertificate> details, List<CertificateDetailsTypeCodeEnum> detailsTypes)
        {
            var customsHouseField = request.Content.CertificateOfOrigin.GetType().GetField("CustomsHouse");
            var customsHouseFieldType =
                detailsTypes.FirstOrDefault(v => v.Enumeration.Equals(customsHouseField.Name));
            var customsHouseFieldValue = customsHouseField.GetValue(request.Content.CertificateOfOrigin) ?? String.Empty;
            var customsHouseConstraintTypeEnumId = customsHouseFieldType != null ?
                details.FirstOrDefault(v => v.CertificateDetailsTypeCodeID == customsHouseFieldType.ID) : null;
            var customsHouseDetail = new CertificateOfOriginDetails
            {
                CertificateDetailsTypeCodeID =
                                                 customsHouseFieldType != null ? customsHouseFieldType.ID : 0,
                Value = customsHouseFieldValue.ToString(),
                DisplayedValue =
                                                 customsHouseFieldValue.ToString() != string.Empty
                                                     ? customsHouseFieldValue.ToString()
                                                     : null,
                ConstraintTypeEnumId =
                                                 customsHouseConstraintTypeEnumId != null
                                                     ? customsHouseConstraintTypeEnumId.ConstraintTypeEnumID
                                                     : 0
            };
            certificateOfOriginDetails.Add(customsHouseDetail);
        }

        private void ValidateAndCreateCertificateOfOriginDetails(int certificteTypeId, Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails> certificateOfOriginDetails, List<DetailsPerCertificate> details, List<CertificateDetailsTypeCodeEnum> detailsTypes, object devisionToCheck, System.Reflection.FieldInfo[] fieldInfo, int? destinationCountryId = null)
        {
            foreach (var msgField in fieldInfo)
            {
                var fieldName = msgField.Name;
                var fieldType = detailsTypes.FirstOrDefault(v => v.Enumeration.Equals(fieldName));
                var fieldValue = msgField.GetValue(devisionToCheck);

                if (fieldType == null) continue;
                var detail = details.FirstOrDefault(v => v.CertificateDetailsTypeCodeID == fieldType.ID);
                if (detail == null) continue;

                fieldValue = fieldValue ?? string.Empty;

                var certificateDetail = new CertificateOfOriginDetails
                {
                    CertificateDetailsTypeCodeID = fieldType.ID,
                    Value = fieldValue.ToString(),
                    DisplayedValue = fieldValue.ToString() != string.Empty ? fieldValue.ToString() : null,
                    ConstraintTypeEnumId = detail.ConstraintTypeEnumID
                };

                ValidateMessageField(certificateDetail, certificteTypeId, fieldName, destinationCountryId);
                certificateOfOriginDetails.Add(certificateDetail);
            }
        }

        /// <summary>
        /// Checks mandatory fields and unconditional fields
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="certificateTypeId"></param>
        /// <param name="fieldName"></param>
        private void ValidateMessageField(CertificateOfOriginDetails detail, int certificateTypeId, string fieldName, int? destinationCountry)
        {
            if (!string.IsNullOrWhiteSpace(detail.Value))
            {
                CheckSpecificField(detail, certificateTypeId, fieldName, destinationCountry);
            }
            else
            {
                if (detail.ConstraintTypeEnumId == (int)EConstraintType.Mandatory)
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
                }
            }
        }

        private void CheckSpecificField(CertificateOfOriginDetails detail, int certificateTypeId, string fieldName, int? destinationCountry)
        {
            var detailTypeId = detail.CertificateDetailsTypeCodeID;

            switch (detailTypeId)
            {
                case (int)ECertificateDetailsType.ExporterId:
                    CheckIfExporterExist(detail.Value, fieldName);
                    if (_exporterID != 0)
                        detail.Value = _exporterID.ToString();
                    break;

                case (int)ECertificateDetailsType.ExporterCountry:
                case (int)ECertificateDetailsType.CountryOfDeclaration:
                case (int)ECertificateDetailsType.IssuingCountry:
                case (int)ECertificateDetailsType.TransirCountry:
                    CheckIfCountryInSystemAndIsrael(detail, fieldName);
                    break;

                case (int)ECertificateDetailsType.TradeAgreementCountry1:
                    CheckAgreementFirstCountry(detail, certificateTypeId, fieldName);
                    break;

                case (int)ECertificateDetailsType.TradeAgreementCountry2:
                case (int)ECertificateDetailsType.ConsigneeCountry:
                case (int)ECertificateDetailsType.OriginCountry:
                case (int)ECertificateDetailsType.CumulationCountry:
                    CheckIfCountryIsInTradeAgreement(detail, fieldName, certificateTypeId);
                    break;

                case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
                case (int)ECertificateDetailsType.OriginGroupOfCountries:
                case (int)ECertificateDetailsType.DestinationGroupOfCountries:
                case (int)ECertificateDetailsType.CumulationGroupOfCountries:
                    CheckIfCountryGroupIsInTradeAgreement(detail, fieldName, certificateTypeId);
                    break;

                case (int)ECertificateDetailsType.DateOfDeclaration:
                    CheckDeclarationDate(detail);
                    break;

                case (int)ECertificateDetailsType.IsDeclaredByExporter:
                    CheckIfDeclaredByExporter(detail);
                    ConvertBoolFieldToYesNo(detail);
                    break;

                case (int)ECertificateDetailsType.ExportDate:
                    CheckExportDate(detail);
                    break;

                case (int)ECertificateDetailsType.CityOfDeclaration:
                    CheckCityOfDeclaration(detail, fieldName);
                    break;

                case (int)ECertificateDetailsType.PlaceOfManufacture:
                    CheckCityOfDeclaration(detail, fieldName);
                    CheckIfExemptPlaceOfManufacture(detail, certificateTypeId, destinationCountry);
                    break;

                case (int)ECertificateDetailsType.ExpectedExitDate:
                    CheckExpectedExitDate(detail);
                    break;

                case (int)ECertificateDetailsType.ImportDate:
                    CheckImportDate(detail);
                    break;

                case (int)ECertificateDetailsType.IsConsigneeForPrint:
                case (int)ECertificateDetailsType.IsCumulation:
                case (int)ECertificateDetailsType.IsExportDecForPrint:
                case (int)ECertificateDetailsType.IsDeclaredByManufacturer:
                    ConvertBoolFieldToYesNo(detail);
                    break;

                case (int)ECertificateDetailsType.ExportCountry:
                    CheckExportCountry(detail, fieldName, certificateTypeId);
                    break;

                case (int)ECertificateDetailsType.PortOfEntrance:
                case (int)ECertificateDetailsType.ExitPort:
                case (int)ECertificateDetailsType.ExportPort:
                case (int)ECertificateDetailsType.PortOfShipment:
                    CheckIfInternationalSiteExist(detail);
                    break;

                    #region Cases that don't require check

                    //case (int)ECertificateDetailsType.ExporterName:
                    //    break;
                    //case (int)ECertificateDetailsType.ExporterAddress:
                    //    break;
                    //case (int)ECertificateDetailsType.ConsigneeName:
                    //    break;
                    //case (int)ECertificateDetailsType.ConsigneeAddress:
                    //    break;
                    //case (int)ECertificateDetailsType.ConsigneeRemarks:
                    //    break;
                    //case (int)ECertificateDetailsType.Transport:
                    //    break;
                    //case (int)ECertificateDetailsType.PortOfShipment:
                    //    break;
                    //case (int)ECertificateDetailsType.ZipCodeOfManufacture:
                    //    break;
                    //case (int)ECertificateDetailsType.Observations:
                    //    break;
                    //case (int)ECertificateDetailsType.Feedback:
                    //    break;
                    //case (int)ECertificateDetailsType.ImportBillOfLadingNum:
                    //    break;
                    //case (int)ECertificateDetailsType.GoodsDescription:
                    //    break;
                    //case (int)ECertificateDetailsType.DeclaringCompany:
                    //    break;
                    //case (int)ECertificateDetailsType.DeclaringPerson:
                    //    break;
                    //case (int)ECertificateDetailsType.DeclaringPosition:
                    //    break;
                    //case (int)ECertificateDetailsType.ManifestNum:
                    //    break;
                    //case (int)ECertificateDetailsType.ExportBillOFLadingNum:
                    //break;

                    #endregion Cases that don't require check
            }
        }

        private void CheckIfExemptPlaceOfManufacture(CertificateOfOriginDetails detail, int certificateTypeId, int? destinationCountry)
        {
            var exemptCountriesGlobalParam = Configuration.GetConfig<string>("CountriesExemptedFromSendingThePlaceOfManufacture").Split(',').ToList();

            if (certificateTypeId == (int)ECertificateOfOriginType.EUR1 && exemptCountriesGlobalParam.Contains(destinationCountry?.ToString()))
            {
                detail.Value = string.Empty;
                detail.DisplayedValue = string.Empty;
            }
        }

        private void ConvertBoolFieldToYesNo(CertificateOfOriginDetails detail)
        {
            var strBoolField = detail.Value;
            bool boolField;
            bool.TryParse(strBoolField, out boolField);

            detail.DisplayedValue = boolField ? "Yes" : "No";
        }

        private void CheckIfExporterExist(string customerExternalId, string fieldName)
        {
            var exporterID = _servicesAdapter.GetCustomerIDByExternalID(customerExternalId);

            if (exporterID == null)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CustomerNotInCustomers, parameters, null));
            }
            else
            {
                _exporterID = exporterID.Value;
            }
        }

        private void CheckExportCountry(CertificateOfOriginDetails detail, string fieldName, int certificateTypeID)
        {
            var country = detail.Value;
            var countryId = GetCountryId(country, fieldName);
            if (countryId == 0) return;

            var isCountryIsrael = IsCountryIsrael((countryId));
            if (!isCountryIsrael && certificateTypeID != (int)ECertificateOfOriginType.NonManipulation)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.IllegalExporterCountry, null, null));
            }
            else if (isCountryIsrael && certificateTypeID == (int)ECertificateOfOriginType.NonManipulation)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportCounrtyIllegal, null, null));
            }

            var countryCode = _systemTablesUtil.GetCodeById<Country>(countryId);
            detail.Value = countryCode.ID.ToString();
            detail.DisplayedValue = countryCode.EnglishName;
        }

        private void CheckIfCountryInSystemAndIsrael(CertificateOfOriginDetails detail, string fieldName)
        {
            var country = detail.Value;
            var detailTypeId = detail.CertificateDetailsTypeCodeID;

            var countryId = GetCountryId(country, fieldName);
            if (countryId == 0) return;

            if (!IsCountryIsrael(countryId))
            {
                switch (detailTypeId)
                {
                    case (int)ECertificateDetailsType.ExporterCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.IllegalExporterCountry, null, null));
                        break;

                    case (int)ECertificateDetailsType.IssuingCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.IssuingCountryIllegal, null, null));
                        break;

                    case (int)ECertificateDetailsType.CountryOfDeclaration:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.ExporterDecCountryIllegal, null, null));
                        break;

                    case (int)ECertificateDetailsType.ExportCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportCounrtyIllegal, null, null));
                        break;

                    case (int)ECertificateDetailsType.TransirCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.TransirCountryIllegal, null, null));
                        break;
                }
            }

            var countryCode = _systemTablesUtil.GetCodeById<Country>(countryId);
            detail.Value = countryCode.ID.ToString();
            detail.DisplayedValue = countryCode.EnglishName;
        }

        private void CheckAgreementFirstCountry(CertificateOfOriginDetails detail, int? certificateOfOriginTypeCodeId, string fieldName)
        {
            var country = detail.Value;

            if (certificateOfOriginTypeCodeId == null) return;

            if (certificateOfOriginTypeCodeId == (int)ECertificateOfOriginType.EUR1
                || certificateOfOriginTypeCodeId == (int)ECertificateOfOriginType.EURMED)
            {
                var countryId = GetCountryId(country, fieldName);
                if (countryId == 0) return;

                IsCountryIsrael(countryId);
            }

            CheckIfCountryIsInTradeAgreement(detail, fieldName, (int)certificateOfOriginTypeCodeId);
        }

        private void CheckIfCountryIsInTradeAgreement(CertificateOfOriginDetails detail, string fieldName, int certificateTypeId)
        {
            var country = detail.Value;
            var detailTypeId = detail.CertificateDetailsTypeCodeID;
            var countryId = GetCountryId(country, fieldName);
            if (countryId == 0) return;

            #region TradeAgreementService

            var isInTrade = _servicesAdapter.IsTradeAgreementForCountry(certificateTypeId, countryId, false);
            if (!isInTrade)
            {
                switch (detailTypeId)
                {
                    case (int)ECertificateDetailsType.TradeAgreementCountry2:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.SecondCountryNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.ConsigneeCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.ImporterCountryNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.OriginCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginCountryNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.DestinationCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationCountryNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.CumulationCountry:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.CumulationCountryNotInAgreement, null, null));
                        break;
                }
            }

            #endregion TradeAgreementService

            var countryCode = _systemTablesUtil.GetCodeById<Country>(countryId);
            detail.DisplayedValue = countryCode.EnglishName;
            detail.Value = countryCode.ID.ToString();
        }

        private void CheckIfCountryGroupIsInTradeAgreement(CertificateOfOriginDetails detail, string fieldName, int certificateTypeId)
        {
            var countryGroup = detail.Value;
            var detailTypeId = detail.CertificateDetailsTypeCodeID;
            var countryId = GetCountryGroupId(countryGroup, fieldName);
            if (countryId == 0) return;

            #region TradeAgreementService

            var isInTrade = _servicesAdapter.IsTradeAgreementForCountry(certificateTypeId, countryId, true);
            if (!isInTrade)
            {
                switch (detailTypeId)
                {
                    case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.GroupOfCountriesNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.OriginGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginGroupOfCountriesNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.DestinationGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationGroupOfCountriesNotInAgreement, null, null));
                        break;

                    case (int)ECertificateDetailsType.CumulationGroupOfCountries:
                        _requestExceptions.Exceptions.Add(new InfException(EMessages.CumulationGroupOfCountriesNotInAgreement, null, null));
                        break;
                }
            }

            var countryGroupCode = _systemTablesUtil.GetCodeById<CountryGroup>(countryId);
            detail.DisplayedValue = countryGroupCode.EnglishName;
            detail.Value = countryGroupCode.ID.ToString();

            #endregion TradeAgreementService
        }

        private void CheckIfSiteExistAndCustomsHouse(CertificateOfOriginDetails detail)
        {
            var siteExteralId = detail.Value;
            var siteId = GetIdByCode<SiteLookup>(SiteLookup.PropExternalSiteNumberForMessages, siteExteralId);

            if (siteId.HasValue)
            {
                var organizationUnitID = SystemTablesUtil.GetCodeById<SiteLookup>(siteId.Value)?.OrganizationUnitID;
                if (organizationUnitID.HasValue)
                {
                    CheckIfCustomsHouse(detail, organizationUnitID.Value);
                }
            }
        }

        private void CheckIfCustomsHouse(CertificateOfOriginDetails detail, int orgUnitId)
        {
            var isCustomsHouse = _servicesAdapter.IsOrganzationUnitCustomHouse(orgUnitId);
            if (!isCustomsHouse)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CustomsHouseNotInTable, null, null));
            }
            else
            {
                var orgUnitCode = _systemTablesUtil.GetCodeById<OrganizationUnitLOV>(orgUnitId);
                detail.Value = orgUnitCode.ID.ToString();
                detail.DisplayedValue = orgUnitCode.EnglishName;
                _organizationUnit = orgUnitId;
            }
        }

        private void CheckIfSiteExist(CertificateOfOriginDetails detail)
        {
            var siteExteralId = detail.Value;
            var siteId = GetIdByCode<SiteLookup>(SiteLookup.PropExternalSiteNumberForMessages, siteExteralId);

            if (siteId.HasValue)
            {
                var siteCode = _systemTablesUtil.GetCodeById<InternationalSite>(siteId.Value);
                detail.Value = siteCode.ID.ToString();
                detail.DisplayedValue = siteCode.EnglishName;
            }
        }

        private void CheckIfInternationalSiteExist(CertificateOfOriginDetails detail)
        {
            var siteExteralId = detail.Value;
            var siteId = GetIdByCode<InternationalSite>(InternationalSite.PropLocode, siteExteralId);

            if (siteId.HasValue)
            {
                var siteCode = _systemTablesUtil.GetCodeById<InternationalSite>(siteId.Value);
                detail.Value = siteCode.Locode;
                detail.DisplayedValue = siteCode.EnglishName;
            }
        }

        private void CheckDeclarationDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);

            if (date > DateTime.Today || date < DateTime.Today.AddDays(-5))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExporterDecDateIllegal, null, null));
            }
            else
            {
                detail.DisplayedValue = date.ToShortDateString();
            }
        }

        private void CheckIfDeclaredByExporter(CertificateOfOriginDetails detail)
        {
            bool isDeclaredByExporter;
            bool.TryParse(detail.Value, out isDeclaredByExporter);

            if (!isDeclaredByExporter)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.DeclaringExporter, null, null));
            }
        }

        private void CheckExportDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);

            if (date > DateTime.Today.AddMonths(3))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportDateIllegal, null, null));
            }
            else
            {
                detail.DisplayedValue = date.ToShortDateString();
            }
        }

        private void CheckExpectedExitDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);

            if (date < DateTime.Today || date > DateTime.Today.AddMonths(3))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExitDateIllegal, null, null));
            }
            else
            {
                detail.DisplayedValue = date.ToShortDateString();
            }
        }

        private void CheckImportDate(CertificateOfOriginDetails detail)
        {
            var strDate = detail.Value;
            DateTime date;
            DateTime.TryParse(strDate, out date);
            detail.DisplayedValue = date.ToShortDateString();
        }

        private void CheckCityOfDeclaration(CertificateOfOriginDetails detail, string fieldName)
        {
            var city = detail.Value;
            int cityId;
            var isParseSucceed = int.TryParse(city, out cityId);

            if (isParseSucceed)
            {
                var cityCode = _systemTablesUtil.GetCodeById<City>(cityId);

                if (cityCode == null)
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
                }
                else
                {
                    detail.DisplayedValue = cityCode.EnglishName;
                    detail.Value = cityCode.ID.ToString();
                }
            }
            else
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
            }
        }

        #endregion GetCertificateDetailsFromMessage

        #region CheckFields

        private void CheckFields(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, IEnumerable<CertificateOfOriginDetails> details)
        {
            CheckConditionalFields(request, details);
            CheckFieldForSpecificCertificate(request, details);
        }

        private void CheckConditionalFields(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, IEnumerable<CertificateOfOriginDetails> details)
        {
            var importDateDetail = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.ImportDate);
            var isCumulationDetail = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.IsCumulation);
            var consigneeCountryDetail = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.ConsigneeCountry);
            var originCountryDetail = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.OriginCountry);
            var customsHouse = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.CustomsHouse);
            var exporterCountryDetail = details.FirstOrDefault(d => d.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.ExporterCountry);

            if (importDateDetail != null)
            {
                CheckImportDate(request);
            }

            if (isCumulationDetail != null)
            {
                CheckCumulationCountry(request);
            }

            if (consigneeCountryDetail != null)
            {
                CheckConsigneeCountry(request);
            }

            if (originCountryDetail != null)
            {
                CheckPlaceOfManufactureAndZipcode(request);
            }

            if (customsHouse != null)
            {
                CheckIfSiteExistAndCustomsHouse(customsHouse);
            }

            if (request.Content.CertificateOfOrigin != null && request.Content.CertificateOfOrigin.InSufficentworkingIndSpecified && request.Content.CertificateOfOrigin.InSufficentworkingInd.Value && request.Content.CertificateOfOrigin.InsufficentWorkingText.IsNullOrEmpty())
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.FieldMandatoryWhenTheAnotherField, new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = "InsufficentWorkingText" }, new MalamValidationParameter() { FieldName = "InSufficentworkingInd" }, new MalamValidationParameter { Value = "true" } }, null));
            }
        }

        private void CheckImportDate(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            if (request.Content.AgentRequest.certificateOfOriginTypeCode != (int)ECertificateOfOriginType.NonManipulation) return;

            var importDate = request.Content.NonManipulationCertificate.ImportDate;
            var exportDate = request.Content.NonManipulationCertificate.ExportDate;
            if (importDate > exportDate || importDate < exportDate.AddYears(-1))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ImportDatetIllegal, null, null));
            }
        }

        private void CheckConsigneeCountry(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            var consigneeName = request.Content.CertificateOfOrigin.ConsigneeName;
            var consigneeAddress = request.Content.CertificateOfOrigin.ConsigneeAddress;
            var consigneeCountry = request.Content.CertificateOfOrigin.ConsigneeCountry;

            if (!string.IsNullOrWhiteSpace(consigneeName) && !string.IsNullOrWhiteSpace(consigneeAddress)
                && string.IsNullOrWhiteSpace(consigneeCountry))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ImporterCountryRequired, null, null));
            }
        }

        private void CheckCumulationCountry(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            var isCumulation = request.Content.CertificateOfOrigin.IsCumulation;
            var cumulationCountry = request.Content.CertificateOfOrigin.CumulationCountry;
            var cumulationGroupOfCountries = request.Content.CertificateOfOrigin.CumulationGroupOfCountries;

            if (isCumulation == true && string.IsNullOrWhiteSpace(cumulationCountry) && !cumulationGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CumulationCountryRequired, null, null));
            }
            else if (isCumulation == true && !string.IsNullOrWhiteSpace(cumulationCountry) && cumulationGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CountryAndCountryGroup, null, null));
            }
        }

        private void CheckPlaceOfManufactureAndZipcode(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            var originCountryId = 0;
            var originCountry = request.Content.CertificateOfOrigin.OriginCountry;
            if (string.IsNullOrWhiteSpace(originCountry)) return;

            var placeOfManufacture = request.Content.CertificateOfOrigin.PlaceOfManufacture;
            var zipCode = request.Content.CertificateOfOrigin.ZipCodeOfManufacture;
            var destinationCountry = request.Content.CertificateOfOrigin.DestinationCountry;
            var isZipcodeMandatory = request.Content.AgentRequest.IsZipcodeMandatory;

            try
            {
                originCountryId = GetIdByCode<Country>(Country.PropCountryAlphaCode_2, originCountry) ?? 0;
            }
            catch (InfException ex) { } //exception already thrown

            var exemptCountriesGlobalParam = Configuration.GetConfig<string>("CountriesExemptedFromSendingThePlaceOfManufacture").Split(',').ToList();
            var destinationCountryId = !destinationCountry.IsNullOrEmpty() ? GetIdByCode<Country>(Country.PropCountryAlphaCode_2, destinationCountry) : 0;

            if (IsCountryIsrael(originCountryId) && (!placeOfManufacture.HasValue || !zipCode.HasValue) && isZipcodeMandatory == true && !exemptCountriesGlobalParam.Contains(destinationCountryId.ToString()))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.PlaceOfManufactureAndZipcodeRequired, null, null));
            }

            if (zipCode != null && zipCode.ToString().Length < 7)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheNumOfCharactersInTheZipcodeIsLessThan7, null, null));
            }
        }

        #region CheckRequestReason

        private CertificateOfOrigin CheckRequestReasonAndGetSavedCertificate(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, CertificateOfOriginsBL certificateOfOriginsBL)
        {
            CertificateOfOrigin certificate = null;
            var requestReason = request.Content.AgentRequest.requestReasonCode;
            var requestResonCodeEnum = CheckRequestReasonCodeEnum(requestReason);
            if (requestResonCodeEnum == null) return null;
            var exportDeclarationNum = request.Content.AgentRequest.exportDeclarationNum;
            switch (requestReason)
            {
                case (int)ERequestReason.EmptyCertificate:
                    CheckIfCertificateIDHasValue(request.Content.AgentRequest.certificateID);
                    break;

                case (int)ERequestReason.CertificateCancellation:
                    certificate = CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                    if (certificate != null)
                    {
                        ChecksIfThereIsDeclarationAssociatedWithTheCertificate(certificate.ID);
                    }
                    break;

                case (int)ERequestReason.GetRequestStatus:
                    certificate = CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                    if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty())
                    {
                        CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                    }
                    break;

                case (int)ERequestReason.CertificateUpdate:
                    certificate = CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                    if (certificate != null)
                    {
                        CheckCertificateUpdate(request, certificate);
                    }
                    if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty())
                    {
                        CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                    }
                    break;

                case (int)ERequestReason.CertificateReplacement:
                    CheckIfCertificateIdToCancelHasValue(request.Content.AgentRequest.certificateIdToCancel);
                    certificate = CheckCertificateNumber(request.Content.AgentRequest.certificateIdToCancel, certificateOfOriginsBL);
                    if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty())
                    {
                        CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                        CheckIfCertificatePublishedOrCanceled(request.Content.AgentRequest.certificateID);
                    }
                    if (certificate != null)
                        ValidateCertificateInCertificateReplacement(request, certificate);
                    CheckExportDeclarationNumber(exportDeclarationNum);
                    break;

                case (int)ERequestReason.ImportCertificateReplacement:
                    CheckIfCertificateIdToCancelHasValue(request.Content.AgentRequest.certificateIdToCancel);
                    break;

                case (int)ERequestReason.RetrospectiveCertificate:
                    CheckExportDeclarationNumber(exportDeclarationNum);
                    if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty())
                    {
                        CheckIfCertificatePublishedOrCanceled(request.Content.AgentRequest.certificateID);
                    }
                    break;

                case (int)ERequestReason.NewCertificate:
                    if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty())
                    {
                        CheckCertificateNumber(request.Content.AgentRequest.certificateID, certificateOfOriginsBL);
                        CheckIfCertificatePublishedOrCanceled(request.Content.AgentRequest.certificateID);
                    }
                    CheckExportDeclarationNumber(exportDeclarationNum);
                    break;

                case (int)ERequestReason.Draft:
                    if (!request.Content.AgentRequest.certificateID.IsNullOrEmpty())
                    {
                        CheckIfCertificatePublishedOrCanceled(request.Content.AgentRequest.certificateID);
                    }
                    break;
            }
            if (requestReason != (int)ERequestReason.GetRequestStatus)
            {
                certificate = certificate ?? certificateOfOriginsBL.GetCertificateOfOriginByExternalId(request.Content.AgentRequest.certificateID);
                if (certificate != null && certificate.CertificateOfOriginStatusID.In((int)ECertificateOfOriginStatus.DeclarationMatch, (int)ECertificateOfOriginStatus.DeclarationMismatch))
                {
                    var parameters = new List<MalamValidationParameter>
                            {new MalamValidationParameter {Value = certificate.CertificateNumber}};
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.ItIsNotPossibleToTransmitACorrectionToTheCertificateWhenThereIsATaskForACustomsEmployee,
                        parameters, null));
                }
            }
            return certificate;
        }

        private void ValidateCertificateInCertificateReplacement(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, CertificateOfOrigin certificate)
        {
            if (certificate.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Published)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = certificate.CertificateNumber } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CertificateToCancelIncorrectStatus, parameters, null));
            }
            if (string.IsNullOrWhiteSpace(request.Content.AgentRequest.replacementReason))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ReplacementReasonMissing, null, null));
            }
        }

        private CertificateOfOrigin CheckCertificateNumber(string certificateID, CertificateOfOriginsBL certificateOfOriginsBL)
        {
            if (!string.IsNullOrWhiteSpace(certificateID))
            {
                var certificate = certificateOfOriginsBL.GetCertificateOfOriginByExternalId(certificateID);

                if (certificate == null)
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = certificateID } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.CertificateDoesntExist, parameters, null));
                }

                return certificate;
            }
            _requestExceptions.Exceptions.Add(new InfException(EMessages.MustSendCertificateID, null));
            return null;
        }

        private void ChecksIfThereIsDeclarationAssociatedWithTheCertificate(int certificateId)
        {
            var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();

            var leadDocumentByCertificateOfOriginDTO = exportDealFileExternalServiceAdapter.GetLeadDocumentByCertificateOfOriginId(certificateId);
            if (leadDocumentByCertificateOfOriginDTO != null)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = leadDocumentByCertificateOfOriginDTO.LeadDocumentTitle } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate, parameters, null));
            }
        }

        private void CheckIfCertificateIDHasValue(string certificateID)
        {
            if (!string.IsNullOrWhiteSpace(certificateID))
            {
                var parameters = new List<MalamValidationParameter>
                    {new MalamValidationParameter {FieldName =  "CertificateID"}};
                _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryNullValue, parameters, null));
            }
        }

        private void CheckIfCertificateIdToCancelHasValue(string certificateIdToCancel)
        {
            if (string.IsNullOrWhiteSpace(certificateIdToCancel))
            {
                var parameters = new List<MalamValidationParameter>
                    {new MalamValidationParameter {FieldName =  "certificateIdToCancel"}};
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ValueNull, parameters, null));
            }
        }

        private void CheckCertificateUpdate(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, CertificateOfOrigin certificate)
        {
            //CR 82472
            if (_exporterID != certificate.CustomerID)
            {
                var parameters = new List<MalamValidationParameter>
                        {new MalamValidationParameter {Value = certificate.CertificateNumber}};
                _requestExceptions.Exceptions.Add(new InfException(EMessages.AgentInUpdateDifferent, parameters,
                    null));
            }

            //Cannot update a nonmanipulation certificate with a certificate of origin and vice-versa
            if (request.Content.AgentRequest.CertificateTypeCodeID != certificate.TypeID)
            {
                var requestTypeCode =
                    (ECertificateOfOriginType)request.Content.AgentRequest.CertificateTypeCodeID;
                if (requestTypeCode == ECertificateOfOriginType.NonManipulation ||
                    certificate.TypeID == (int)ECertificateOfOriginType.NonManipulation)
                {
                    var parameters = new List<MalamValidationParameter>
                        {
                            new MalamValidationParameter {Value = (ECertificateOfOriginType) certificate.TypeID},
                            new MalamValidationParameter {Value = requestTypeCode}
                        };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.IllegalCertificateTypeUpdate,
                        parameters,
                        null));
                }
            }

            if (certificate.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Published
                || certificate.CertificateOfOriginStatusID == (int)ECertificateOfOriginStatus.Cancelled)
            {
                var parameters = new List<MalamValidationParameter>
                        {new MalamValidationParameter {Value = certificate.CertificateNumber}};
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CertidficateIncorrectStatus,
                    parameters, null));
            }

            _certificateToUpdateId = certificate.ID;
        }

        private void CheckExportDeclarationNumber(string declarationNum)
        {
            if (declarationNum.IsNullOrEmpty())
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportDeclarationMissing, null, null));
            }
            else
            {
                var exportDealFileExternalServiceAdapter = Container.Resolve<IExportDealFileExternalServiceAdapter>();
                var exportDeclarationDetailsDTO = exportDealFileExternalServiceAdapter.GetExportDeclarationDetailsForCertificateOfOrigion(null, declarationNum);
                if (exportDeclarationDetailsDTO != null && exportDeclarationDetailsDTO.LeadDocumentStateID.In((int)ELeadDocumentState.Canceled, (int)ELeadDocumentState.CanceledDraft, (int)ELeadDocumentState.Draft))
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { Value = declarationNum } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration, parameters, null));
                }
            }
        }

        private void CheckIfCertificatePublishedOrCanceled(string certificateID)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                var certificate = bl.GetCertificateOfOriginByExternalId(certificateID);
                if (certificate != null && certificate.CertificateOfOriginStatusID.In((int)ECertificateOfOriginStatus.Published, (int)ECertificateOfOriginStatus.Cancelled))
                {
                    var parameters = new List<MalamValidationParameter>
                        {new MalamValidationParameter {Value = certificate.CertificateNumber}};
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked,
                        parameters, null));
                }
            }
        }

        #endregion CheckRequestReason

        #region CheckFieldForSpecificCertificate

        private void CheckFieldForSpecificCertificate(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, IEnumerable<CertificateOfOriginDetails> details)
        {
            var certificateTypeId = request.Content.AgentRequest.CertificateTypeCodeID;
            var destCountryDetail = details.FirstOrDefault(v => v.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.DestinationCountry);

            switch (certificateTypeId)
            {
                case (int)ECertificateOfOriginType.EURMED:
                    CheckCountriesForEurCertificates(request, destCountryDetail);
                    break;

                case (int)ECertificateOfOriginType.EUR1:
                    CheckCountriesForEurCertificates(request, destCountryDetail);
                    break;

                case (int)ECertificateOfOriginType.NonManipulation:
                    CheckManifestNumber(request);
                    break;

                default:
                    CheckDestinationCountry(request, destCountryDetail);
                    break;
            }
        }

        private void CheckCountriesForEurCertificates(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, CertificateOfOriginDetails destCountryDetail)
        {
            var certificateTypeId = request.Content.AgentRequest.CertificateTypeCodeID;
            var tradeSecoundCountry = request.Content.CertificateOfOrigin.TradeAgreementCountry2;
            var tradeGroupOfCountries = request.Content.CertificateOfOrigin.TradeAgreementGroupOfCountries;
            var originCountry = request.Content.CertificateOfOrigin.OriginCountry;
            var originGroupOfCountries = request.Content.CertificateOfOrigin.OriginGroupOfCountries;
            var destinationCountry = request.Content.CertificateOfOrigin.DestinationCountry;
            var destinationGroupOfCountries = request.Content.CertificateOfOrigin.DestinationGroupOfCountries;

            if (string.IsNullOrWhiteSpace(tradeSecoundCountry) && !tradeGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.SecondCountryRequired, null, null));
            }

            if (string.IsNullOrWhiteSpace(originCountry) && !originGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginCountryRequired, null, null));
            }

            if (string.IsNullOrWhiteSpace(destinationCountry) && !destinationGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationCountryRequired, null, null));
            }

            if (!string.IsNullOrWhiteSpace(tradeSecoundCountry) && tradeGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CountryAndCountryGroup, null, null));
            }

            if (!string.IsNullOrWhiteSpace(originCountry) && originGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CountryAndCountryGroup, null, null));
            }

            if (!string.IsNullOrWhiteSpace(destinationCountry) && destinationGroupOfCountries.HasValue)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CountryAndCountryGroup, null, null));
            }

            if (!string.IsNullOrWhiteSpace(destinationCountry))
            {
                CheckIfDestinationCountryInAgreement(destinationCountry, certificateTypeId, destCountryDetail);
            }
        }

        private void CheckDestinationCountry(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, CertificateOfOriginDetails destCountryDetail)
        {
            var destinationCountry = request.Content.CertificateOfOrigin.DestinationCountry;
            var certificateTypeId = request.Content.AgentRequest.CertificateTypeCodeID;

            if (string.IsNullOrWhiteSpace(destinationCountry))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationCountryRequired));
            }
            else
            {
                CheckIfDestinationCountryInAgreement(destinationCountry, certificateTypeId, destCountryDetail);
            }
        }

        private void CheckIfDestinationCountryInAgreement(string destinationCountry, int certificateTypeId, CertificateOfOriginDetails destCountryDetail)
        {
            _destinationCountryId = GetCountryId(destinationCountry, "DestinationCountry");
            var isInTrade = _servicesAdapter.IsTradeAgreementForCountry(certificateTypeId, _destinationCountryId.Value, false);
            if (!isInTrade)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.DestinationCountryNotInAgreement, null, null));
            }
            else
            {
                var countryCode = _systemTablesUtil.GetCodeById<Country>((int)_destinationCountryId);
                destCountryDetail.DisplayedValue = countryCode.EnglishName;
            }
        }

        private void CheckManifestNumber(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            var manifestNum = request.Content.NonManipulationCertificate.ManifestNum;
            var exportDeclarationNum = request.Content.AgentRequest.exportDeclarationNum;

            if (string.IsNullOrWhiteSpace(manifestNum) && string.IsNullOrWhiteSpace(exportDeclarationNum))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ManifestIdMissing, null, null));
            }
            if (string.IsNullOrWhiteSpace(exportDeclarationNum))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.ExportDeclarationMissing, null, null));
            }
        }

        #endregion CheckFieldForSpecificCertificate

        #region CheckOriginCriterionAndCustomsItem

        private int? CheckOriginCriterionAndGetId(bool isCriterionMandatory, string originCriterionCode, int certificateTypeCodeId)
        {
            if (!isCriterionMandatory) return null;

            int? originCriterionId = null;

            if (string.IsNullOrWhiteSpace(originCriterionCode))
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginCriteriaMissing, null, null));
            }
            else
            {
                var originCriterion = GetOriginCriterionType(originCriterionCode, certificateTypeCodeId);
                if (originCriterion != null)
                {
                    originCriterionId = originCriterion.ID;
                }
                else
                {
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.OriginCriteriaIllegal, null, null));
                }
            }

            return originCriterionId;
        }

        private int? CheckIfCustomsItemAndGetItem(string fullClassification, int certificateTypeId, bool? isCustomsItemMandatory)
        {
            int? customsItem = null;
            if (!fullClassification.IsNullOrEmpty())
            {
                customsItem = _servicesAdapter.GetCustomsItemIdByFullClassification(fullClassification, ECustomsBookType.Export);
                if (!customsItem.HasValue)
                {
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.ItemNumberNotFound, null, null));
                }
                if (fullClassification.Length < 6)
                {
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.ACustomsItemMustContainAtLeast6Digits, null, null));
                }
            }
            else
            {
                if (isCustomsItemMandatory == true)
                {
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.CustomsItemRequired, null, null));
                }
            }

            return customsItem;
        }

        private void CheckIfCertificateIsLinkedToDeclarationInAmendment(ExportDeclarationDetailsDTO exportDeclarationDetails, string exportDeclarationNum)
        {
            if (exportDeclarationDetails != null && exportDeclarationDetails.IsDeclarationInAmendmentProcess)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = exportDeclarationNum } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclarationLinkedToTheCertificate, parameters, null));
            }
        }

        #endregion CheckOriginCriterionAndCustomsItem

        private string CheckValidityField(string description)
        {
            if (description != null && description.Length > 255)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = description } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheDescriptionLengthCannotExceed255Characters, parameters, null));
            }
            return description;
        }

        #endregion CheckFields

        #region CheckEnums

        private CertificateOfOriginTypeCodeEnum CheckCertificateOfOriginTypeCodeEnum(int typeId)
        {
            CertificateOfOriginTypeCodeEnum certificateTypeCode = null;

            if (typeId == 0)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = CertificateOfOriginsConstants.CertificateOfOriginTypeCode } };
                throw new InfException(EMessages.MandatoryValue, parameters, null);
            }

            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var util = new CertificateOfOriginsUtil(uow);
                certificateTypeCode = util.GetCertificateTypeCode(typeId);

                if (certificateTypeCode == null)
                {
                    throw new InfException(EMessages.CertificateTypeNotExist, null, null);
                }
            }

            return certificateTypeCode;
        }

        private RequestReasonCodeEnum CheckRequestReasonCodeEnum(int typeId)
        {
            RequestReasonCodeEnum typeEnum = null;

            if (typeId == 0)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = CertificateOfOriginsConstants.RequestReasonCode } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.MandatoryValue, parameters, null));
            }
            else
            {
                try
                {
                    typeEnum = _systemTablesUtil.GetCodeById<RequestReasonCodeEnum>(typeId);
                }
                catch (InfException ex)
                {
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.RequestReasonNotExist, null, null));
                }
            }

            return typeEnum;
        }

        private OriginCriterion GetOriginCriterionType(string originCriterionCode, int certificateTypeId)
        {
            var filter = new SystemTablesFilter
            {
                Predicate = string.Format(CertificateOfOriginsConstants.SystemTablesFilterPredicate, OriginCriterion.PropOriginCriterionCode, originCriterionCode,
                                          OriginCriterion.PropCertificateOfOriginTypeCodeID, certificateTypeId),
            };

            return _systemTablesUtil.GetTablesSync<OriginCriterion>(filter).FirstOrDefault();
        }

        #endregion CheckEnums

        private void CancelUpdatedCertificates(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, CertificateOfOrigin newCerificate)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);
                var requestReason = request.Content.AgentRequest.requestReasonCode;

                if (requestReason == (int)ERequestReason.CertificateUpdate)
                {
                    var certificateToUpdateId = request.Content.AgentRequest.certificateID;
                    bl.CancelUpdatedCertificates(certificateToUpdateId, newCerificate.ID);
                }
            }
        }

        private List<CertificateOfOriginInvoiceDetail> CheckAndConvertInvoiceDetails(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request)
        {
            var details = new List<CertificateOfOriginInvoiceDetail>();
            var certificateTypeId = request.Content.AgentRequest.CertificateTypeCodeID;
            var isCriterionMandatory = request.Content.AgentRequest.IsCertificateTypeCodeMandatory;
            var isCustomsItemMandatory = request.Content.AgentRequest.IsCustomsItemMandatory;
            var isRequestReasonCodeIsGetStatus = request.Content.AgentRequest.requestReasonCode == (int)ERequestReason.GetRequestStatus;

            if (certificateTypeId == (int)ECertificateOfOriginType.NonManipulation) return null;
            if (request.Content.CertificateOfOrigin?.CertificateOfOriginRequestInvoiceDetail == null) return null;

            var invoiceDetails = request.Content.CertificateOfOrigin.CertificateOfOriginRequestInvoiceDetail;

            foreach (var invoiceDetail in invoiceDetails)
            {
                var detail = ConvertMessageInvoiceDetailToInvoiceDetail(invoiceDetail, certificateTypeId, isCriterionMandatory, isCustomsItemMandatory, isRequestReasonCodeIsGetStatus);
                details.Add(detail);
            }

            return details;
        }

        #region Country Util

        private bool IsCountryIsrael(int countryId)
        {
            var isIsrael = true;
            var countryIsraelId = Configuration.GetConfig<int>(CertificateOfOriginsConstants.CountryIsrael);

            if (countryId != countryIsraelId)
            {
                isIsrael = false;
            }

            return isIsrael;
        }

        private int GetCountryId(string countryCode, string fieldName)
        {
            var countryId = 0;

            try
            {
                countryId = _systemTablesUtil.GetIdByCode<Country>(Country.PropCountryAlphaCode_2, countryCode);
            }
            catch (InfException ex)
            {
                if (ex.UserMessage == EMessages.TheValueInFieldNotExistsInSystem)
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
                }
            }

            return countryId;
        }

        private int GetCountryGroupId(string countryGroup, string fieldName)
        {
            var groupId = 0;

            try
            {
                int countryGroupId;
                var isParseSucceed = int.TryParse(countryGroup, out countryGroupId);

                if (isParseSucceed)
                {
                    groupId = _systemTablesUtil.GetIdByCode<CountryGroup>(CountryGroup.PropID, countryGroupId);
                }
                else
                {
                    var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                    _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
                }
            }
            catch (InfException ex)
            {
                var parameters = new List<MalamValidationParameter> { new MalamValidationParameter { FieldName = fieldName } };
                _requestExceptions.Exceptions.Add(new InfException(EMessages.TheValueInFieldNotExistsInSystem, parameters, null));
            }

            return groupId;
        }

        #endregion Country Util

        #endregion Check Message

        #region ConvertMessageToCertificateOfOrigin

        private CertificateOfOrigin ConvertMessageToCertificateOfOrigin(PC_NG_2280_MSG01_CertificateOfOriginRequestRequest request, ICollection<CertificateOfOriginDetails> details, ICollection<CertificateOfOriginInvoiceDetail> invoiceDetails)
        {
            var agentRequest = request.Content.AgentRequest;
            var certificateToUpdateId = agentRequest.certificateID;
            var requestReason = request.Content.AgentRequest.requestReasonCode;
            var agentId = request.CustomerID;
            var certificateTypeName = agentRequest.CertificateTypeCodeName;
            var certificateTypeCode = agentRequest.CertificateTypeCodeID;

            var certificateOfOrigin = new CertificateOfOrigin
            {
                CertificateOfOriginDetails = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginDetails>(),
                RequestReasonCode = agentRequest.requestReasonCode,
                ReplacementReason = agentRequest.replacementReason,
                InternalApplication = agentRequest.internalApplication,
                ExportDeclarationNumber = agentRequest.exportDeclarationNum,
                TypeID = agentRequest.certificateOfOriginTypeCode,
                CustomerID = (certificateTypeCode == (int)ECertificateOfOriginType.NonManipulation || request.Content.CertificateOfOrigin == null) ? agentId : _exporterID, //_exporterID, //exporter not exist in NonManipulation
                CreateCustomerID = agentId,
                UpdateCustomerID = agentId,

                CertificateOfOriginInvoiceDetail = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginInvoiceDetail>(),
                CertificateOfOriginStatusID = (int)ECertificateOfOriginStatus.Received,
                DestinationCountry = _destinationCountryId, //field of certificate for sql search performance
                OrganizationUnitID = _organizationUnit,
                IsAttachedList = request.Content.CertificateOfOrigin?.IsAttachedList == null ? false : (bool)request.Content.CertificateOfOrigin.IsAttachedList,
                InSufficentworkingInd = request.Content.CertificateOfOrigin?.InSufficentworkingInd == null ? false : (bool)request.Content.CertificateOfOrigin.InSufficentworkingInd,
                InsufficentWorkingText = request.Content.CertificateOfOrigin?.InsufficentWorkingText
            };
            certificateOfOrigin.CertificateOfOriginDetails.AddRange(details);
            certificateOfOrigin.CertificateOfOriginInvoiceDetail.AddRange(invoiceDetails);

            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new CertificateOfOriginsBL(uow);

                switch (requestReason)
                {
                    case (int)ERequestReason.NewCertificate:
                    case (int)ERequestReason.RetrospectiveCertificate:
                    case (int)ERequestReason.EmptyCertificate:
                        certificateOfOrigin.CertificateNumber = agentRequest.certificateID.IsNullOrEmpty() ? bl.GetCertificateNumber() : agentRequest.certificateID;
                        break;

                    case (int)ERequestReason.CertificateReplacement:
                        if (agentRequest.certificateID.IsNullOrEmpty())
                        {
                            certificateOfOrigin.CertificateNumber = bl.GetCertificateNumber();
                        }
                        else certificateOfOrigin.CertificateNumber = agentRequest.certificateID;
                        var certificateToCancel = bl.GetCertificateOfOriginByExternalId(agentRequest.certificateIdToCancel);
                        if (certificateToCancel != null)
                        {
                            certificateOfOrigin.CertificateIDToCancel = certificateToCancel.ID;
                        }
                        break;

                    case (int)ERequestReason.CertificateUpdate:
                        certificateOfOrigin.CertificateNumber = certificateToUpdateId;
                        certificateOfOrigin.CertificateIDToCancel = _certificateToUpdateId;
                        break;

                    case (int)ERequestReason.ImportCertificateReplacement:
                        if (agentRequest.certificateID.IsNullOrEmpty())
                        {
                            certificateOfOrigin.CertificateNumber = bl.GetCertificateNumber();
                        }
                        else certificateOfOrigin.CertificateNumber = agentRequest.certificateID;

                        certificateOfOrigin.CertificateToReplaceInImport = agentRequest.certificateIdToCancel;
                        break;

                    case (int)ERequestReason.Draft:
                        certificateOfOrigin.CertificateNumber = agentRequest.certificateID.IsNullOrEmpty() ? bl.GetCertificateNumber() : agentRequest.certificateID;
                        break;

                    case (int)ERequestReason.CertificateCancellation:
                        certificateOfOrigin.CertificateNumber = agentRequest.certificateID;
                        certificateOfOrigin.RejectCancelReason = SystemTablesUtil.GetUIMessage((int)EMessages.CertificateOfOriginCancelFromMessage);
                        break;
                }
            }

            certificateOfOrigin.Title = certificateOfOrigin.CertificateNumber;
            return certificateOfOrigin;
        }

        private CertificateOfOriginInvoiceDetail ConvertMessageInvoiceDetailToInvoiceDetail(PC_NG_2280_MSG01_CertificateOfOriginRequestCertificateOfOriginCertificateOfOriginRequestInvoiceDetail messageInvoiceDetail, int certificateTypeId, bool isCriterionMandatory, bool? isCustomsItemMandatory, bool isRequestReasonCodeIsGetStatus)
        {
            var messageItemDetailes = messageInvoiceDetail.CertificateOfOriginRequestItemDetail;
            var itemDetailes = new Customs.CertificateOfOrigins.Entities.TrackableCollection<CertificateOfOriginItemDetail>();

            foreach (var messageDetail in messageItemDetailes)
            {
                var detail = ConvertMessageItemDetailToItemDetail(messageDetail, certificateTypeId, isCriterionMandatory, isCustomsItemMandatory);
                if (!isRequestReasonCodeIsGetStatus)
                    CheckContainerISOCode(detail);
                itemDetailes.Add(detail);
            }

            if (messageInvoiceDetail.InvoiceDate < DateTime.Today.AddYears(-5))
            {
                messageInvoiceDetail.InvoiceDate = DateTime.Today.AddYears(-5);
            }

            var invoiceDetail = new CertificateOfOriginInvoiceDetail
            {
                InvoiceDate = messageInvoiceDetail.InvoiceDate,
                InvoiceNumber = messageInvoiceDetail.InvoiceNum,
                InvoiceAmount = messageInvoiceDetail.InvoiceSum.HasValue ? (decimal)messageInvoiceDetail.InvoiceSum : 0,
                IsToPrint = messageInvoiceDetail.IsInvoicesForPrint,
                CurrencyTypeID = GetIdByCode<CurrencyType>(CurrencyType.PropCurrencyCode, messageInvoiceDetail.CurrencyType),
                InvoiceGoodsDescription = CheckValidityField(messageInvoiceDetail.DescriptionOfInvoice),
                CertificateOfOriginItemDetail = itemDetailes
            };

            return invoiceDetail;
        }

        private void CheckContainerISOCode(CertificateOfOriginItemDetail item)
        {
            if (item.PackingTypeID == CertificateOfOriginsConsts.PackingTypeContainer && item.ContainerISOCode == null)
            {
                _requestExceptions.Exceptions.Add(new InfException(EMessages.RequiredContainerISOCodeFieldIfItIsAContainer, null, null));
            }
        }

        private CertificateOfOriginItemDetail ConvertMessageItemDetailToItemDetail(PC_NG_2280_MSG01_CertificateOfOriginRequestCertificateOfOriginCertificateOfOriginRequestInvoiceDetailCertificateOfOriginRequestItemDetail messageItemDetail, int certificateTypeId, bool isCriterionMandatory, bool? isCustomsItemMandatory)
        {
            var itemDetail = new CertificateOfOriginItemDetail
            {
                GrossWeight = messageItemDetail.Weight,
                ItemGoodsDescription = messageItemDetail.ItemDescription,
                FullClassification = messageItemDetail.ItemId,
                MarksAndNumbers = messageItemDetail.MarksAndNumbers,
                Quantity = messageItemDetail.PackageQuantity.HasValue ? (int)messageItemDetail.PackageQuantity : 0,
                PackingTypeID = GetIdByCode<PackingType>(PackingType.PropCommonCode, messageItemDetail.PackageType),
                MeasurementUnitID = GetIdByCode<MeasurementUnit>(MeasurementUnit.PropExternalIDNum, messageItemDetail.MeasureType) ?? 0,
                RowNum = messageItemDetail.ItemSerial.HasValue ? (int)messageItemDetail.ItemSerial : 0,
                CustomsItemID = CheckIfCustomsItemAndGetItem(messageItemDetail.ItemId, certificateTypeId, isCustomsItemMandatory),
                OriginCriterionID = CheckOriginCriterionAndGetId(isCriterionMandatory, messageItemDetail.OriginCriterion, certificateTypeId),
                ContainerISOCode = messageItemDetail.ContainerISOCode
            };

            return itemDetail;
        }

        private int? GetIdByCode<T>(string externalPropName, string code) where T : class, ILov, new()
        {
            int? typeId = null;

            try
            {
                typeId = _systemTablesUtil.GetIdByCode<T>(externalPropName, code);
            }
            catch (InfException ex)
            {
                _requestExceptions.Exceptions.Add(ex);
            }

            return typeId;
        }

        private string GetGuidNumber()
        {
            var uniqueValue = Guid.NewGuid().ToString();
            return uniqueValue.Substring(0, 34);
        }

        #endregion ConvertMessageToCertificateOfOrigin

        #region CreateCertificateOfOriginRequestFeedbackResponse

        private PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse CreateCertificateOfOriginRequestFeedbackResponse(CertificateOfOrigin certificate, List<Exception> exceptions, int requestReasonCode)
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var util = new CertificateOfOriginsUtil(uow);

                var response = new PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackResponse();
                response.Content.CertificateOfOriginRequestFeedback = util.CreateCertificateOfOriginRequestFeedback(certificate);
                response.Content.Attachment = (requestReasonCode == (int)ERequestReason.EmptyCertificate || requestReasonCode == (int)ERequestReason.CertificateCancellation || requestReasonCode == (int)ERequestReason.GetRequestStatus || (requestReasonCode != (int)ERequestReason.Draft && certificate.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Published)) ? null : util.CreateAttachments(certificate);
                response.Content.ResponseContentHeader.ApplicationID = certificate.ID;
                if (!exceptions.IsNullOrEmpty())
                {
                    response.Content.ResponseContentHeader.Exception = exceptions.ToArray();
                }

                return response;
            }
        }

        #endregion CreateCertificateOfOriginRequestFeedbackResponse
    }
}