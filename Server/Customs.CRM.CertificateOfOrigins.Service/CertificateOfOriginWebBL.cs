using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using Customs.CertificateOfOrigins.Entities;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.Entities;
using Customs.CRM.Entities.CertificateOfOriginsPartial;
using Customs.DataDictionary.Entities;
using Customs.Inf.WebMultiDotNetSupport.CertificateOfOrigins;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Exceptions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Data.Extensions;

namespace Customs.CRM.CertificateOfOrigins.Service
{
    public class CertificateOfOriginWebBL : BaseBL
    {
        public CertificateOfOriginWebBL(IUnitOfWork uow) : base(uow)
        {
        }

        public CertificateOfOriginsResponse GetCertificateDetailForWeb(CertificateOfOriginsRequest requestData)
        {
            
            if (requestData.CertificateOfOriginGuid != null)
            {
                if ((!Guid.TryParse(requestData.CertificateOfOriginGuid, out var guid)))
                {
                    return new CertificateOfOriginsResponse() { ExeptionDescription = CertificateOfOriginsConsts.InvalidGuid };
                }
            }

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter{ParameterName = CertificateOfOriginsConsts.GuidSPParam, SqlDbType = SqlDbType.UniqueIdentifier, Value = requestData.CertificateOfOriginGuid!=null? Guid.Parse(requestData.CertificateOfOriginGuid):(object)DBNull.Value},
                new SqlParameter{ParameterName = CertificateOfOriginsConsts.CertificateOfOriginNumberSPParam, SqlDbType = SqlDbType.VarChar, Value = requestData.CertificateOfOriginNumber ?? (object)DBNull.Value},
                new SqlParameter{ParameterName = CertificateOfOriginsConsts.IssuingDateSPParam, SqlDbType = SqlDbType.Date, Value = requestData.IssuingDate.HasValue? requestData.IssuingDate.Value.Date: (object)DBNull.Value}
            };

            var filter = new ResultSetFilter<CertificateOfOrigin>
            {
                StoredProcedureName = CertificateOfOriginsConsts.GetCertificateOfOriginDataForWebQuerySP,
                MaterializeFunction = MaterializeCertificateOfOriginDataForWebQuerySP
            };

            filter.Parameters.AddRange(sqlParameters);
            var result = _uow.Repository.ExecuteResultSetFunction(filter);

            if (result == null)
            {
                return new CertificateOfOriginsResponse() { ExeptionDescription = CertificateOfOriginsConsts.NoMatchingCertificate };
            }
            var response = ConstructWebResponse(result);

            return response;
        }

        private CertificateOfOriginsResponse ConstructWebResponse(CertificateOfOrigin certificateOfOrigin)
        {
            var response = new CertificateOfOriginsResponse
            {
                DocumentID = certificateOfOrigin.DocumentID,
                CertificateNumber = certificateOfOrigin.CertificateNumber,
                QueryURL = GetQueryURL(certificateOfOrigin.GUID),
                CertificateOfOriginDetails = GetCertificateOfOriginDetails(certificateOfOrigin),
                CertificateOfOriginInvoiceDetails = GetCertificateOfOriginInvoiceDetails(certificateOfOrigin)
            };
            return response;
        }

        private List<CertificateOfOriginInvoiceDetailDTO> GetCertificateOfOriginInvoiceDetails(
            CertificateOfOrigin certificateOfOrigin)
        {
            var certificateOfOriginInvoiceDetailDTOList = new List<CertificateOfOriginInvoiceDetailDTO>();
            var isExistsInvoiceDetailWithIsPrintFalse =
                certificateOfOrigin.CertificateOfOriginInvoiceDetail.All(cooid => cooid.IsToPrint);

            foreach (var certificateOfOriginInvoiceDetail in certificateOfOrigin.CertificateOfOriginInvoiceDetail)
            {
                if ((certificateOfOrigin.TypeID == (int)ECertificateOfOriginType.IsrCol ||
                    certificateOfOrigin.TypeID == (int)ECertificateOfOriginType.MERCOSUR &&
                    certificateOfOriginInvoiceDetail.IsToPrint) ||
                    (certificateOfOrigin.TypeID == (int)ECertificateOfOriginType.EURMED ||
                     certificateOfOrigin.TypeID == (int)ECertificateOfOriginType.EUR1))
                {
                    var certificateOfOriginInvoiceDetailDTO = new CertificateOfOriginInvoiceDetailDTO
                    {
                        InvoiceNumber = certificateOfOriginInvoiceDetail.IsToPrint
                            ? certificateOfOriginInvoiceDetail.InvoiceNumber
                            : string.Empty,
                        InvoiceAmount = certificateOfOriginInvoiceDetail.InvoiceAmount,
                        InvoiceDate = certificateOfOriginInvoiceDetail.InvoiceDate,
                        InvoiceGoodsDescription = certificateOfOriginInvoiceDetail.InvoiceGoodsDescription
                    };
                    if (certificateOfOriginInvoiceDetail.CurrencyTypeID.HasValue)
                    {
                        var currencyType = SystemTablesUtil.GetCodeById<CurrencyType>(certificateOfOriginInvoiceDetail.CurrencyTypeID.Value);
                        certificateOfOriginInvoiceDetailDTO.CurrencyCode = currencyType.CurrencyCode;
                    }
                    certificateOfOriginInvoiceDetailDTO.CertificateOfOriginItemDetailDTOs = new List<CertificateOfOriginItemDetailDTO>();

                    certificateOfOriginInvoiceDetailDTOList.Add(certificateOfOriginInvoiceDetailDTO);
                }
            }

            return certificateOfOriginInvoiceDetailDTOList;
        }

        private List<FieldDataDTO> GetCertificateOfOriginDetails(CertificateOfOrigin certificateOfOrigin)
        {
            var fieldDataDTOs = new List<FieldDataDTO>();
            var properties = typeof(CertificateOfOrigin).GetProperties();
            if (certificateOfOrigin.RequestReasonCode == (int)ERequestReason.RetrospectiveCertificate)
            {
                var fieldID = GetPropertyFieldID(properties, "RequestReasonCode");
                var dataDicField = SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldID);
                fieldDataDTOs.Add(new FieldDataDTO { Label = dataDicField.EnglishName, Value = "Issued Retrospectively" });
            }
            if (certificateOfOrigin.CertificateIDToCancel.HasValue)
            {
                var fieldID = GetPropertyFieldID(properties, "CertificateIDToCancel");
                var dataDicField = SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldID);
                fieldDataDTOs.Add(new FieldDataDTO
                {
                    Label = dataDicField.EnglishName,
                    Value =
                    $"Replacing certificate {certificateOfOrigin.CertificateIDToCancel.Value}"
                });
            }
            fieldDataDTOs.Add(new FieldDataDTO { Label = CertificateOfOriginsConsts.IssuingDate , Value = certificateOfOrigin.IssuingDate });
            if (!string.IsNullOrWhiteSpace(certificateOfOrigin.ExportDeclarationNumber) &&
                certificateOfOrigin.CertificateOfOriginDetails.Any(cood => cood.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.IsExportDecForPrint &&
                                                                           bool.TryParse(cood.Value, out var isExportDeclaration)))
            {
                var fieldID = GetPropertyFieldID(properties, "ExportDeclarationNumber");
                var dataDicField = SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldID);
                fieldDataDTOs.Add(new FieldDataDTO { Label = dataDicField.EnglishName, Value = certificateOfOrigin.ExportDeclarationNumber });
            }

            foreach (var certificateOfOriginDetails in certificateOfOrigin.CertificateOfOriginDetails)
            {
                switch (certificateOfOriginDetails.CertificateDetailsTypeCodeID)
                {
                    case (int)ECertificateDetailsType.PlaceOfManufacture:
                    case (int)ECertificateDetailsType.ZipCodeOfManufacture:
                    case (int)ECertificateDetailsType.Observations:
                    case (int)ECertificateDetailsType.IsDeclaredByManufacturer:
                    case (int)ECertificateDetailsType.IsDeclaredByExporter:
                    case (int)ECertificateDetailsType.ExporterAddress:
                    case (int)ECertificateDetailsType.ExporterName:
                        fieldDataDTOs.Add(new FieldDataDTO
                        {
                            Label = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeEnglishName,
                            Value = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeValue
                        });
                        break;
                    case (int)ECertificateDetailsType.DateOfDeclaration:
                        if (!string.IsNullOrWhiteSpace(certificateOfOriginDetails.Value) &&
                            DateTime.TryParse(certificateOfOriginDetails.Value, out var dateOfDeclaration))
                            fieldDataDTOs.Add(new FieldDataDTO
                            {
                                Label = certificateOfOriginDetails.CertificateDetailsTypeCode.EnglishName,
                                Value = dateOfDeclaration.ToString("dd MMMM yyyy", new CultureInfo("En-US"))
                            });
                        break;
                    case (int)ECertificateDetailsType.ConsigneeName:
                    case (int)ECertificateDetailsType.ConsigneeAddress:
                    case (int)ECertificateDetailsType.ConsigneeCountry:
                        if (certificateOfOrigin.CertificateOfOriginDetails.Any(cood =>
                            cood.CertificateDetailsTypeCodeID == (int)ECertificateDetailsType.IsConsigneeForPrint &&
                            bool.TryParse(cood.Value, out var isConsigneeForPrint)))
                        {
                            if (certificateOfOrigin.TypeID == (int)ECertificateOfOriginType.EUR1 ||
                                certificateOfOrigin.TypeID == (int)ECertificateOfOriginType.EURMED)
                            {
                                if (certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeIsToPrint)
                                {
                                    fieldDataDTOs.Add(new FieldDataDTO
                                    {
                                        Label = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeEnglishName,
                                        Value = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeValue
                                    });
                                }
                            }
                            else
                            {
                                fieldDataDTOs.Add(new FieldDataDTO
                                {
                                    Label = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeEnglishName,
                                    Value = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeValue
                                });
                            }
                        }
                        break;
                    case (int)ECertificateDetailsType.CountryOfDeclaration:
                    case (int)ECertificateDetailsType.DestinationCountry:
                    case (int)ECertificateDetailsType.ExporterCountry:
                    case (int)ECertificateDetailsType.OriginCountry:
                    case (int)ECertificateDetailsType.CustomsHouse:
                    case (int)ECertificateDetailsType.ExporterId:
                    case (int)ECertificateDetailsType.TradeAgreementCountry1:
                    case (int)ECertificateDetailsType.TradeAgreementCountry2:
                    case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
                    case (int)ECertificateDetailsType.ConsigneeRemarks:
                    case (int)ECertificateDetailsType.OriginGroupOfCountries:
                    case (int)ECertificateDetailsType.DestinationGroupOfCountries:
                    case (int)ECertificateDetailsType.Transport:
                    case (int)ECertificateDetailsType.IssuingCountry:
                    case (int)ECertificateDetailsType.CityOfDeclaration:
                    case (int)ECertificateDetailsType.PortOfShipment:
                        fieldDataDTOs.Add(new FieldDataDTO
                        {
                            Label = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeEnglishName,
                            Value = certificateOfOriginDetails.CertificateOfOriginWebPrintOutDTO.CertificateDetailsTypeValue
                        });
                        break; ;
                }
            }

            return fieldDataDTOs;
        }

        private int GetPropertyFieldID(PropertyInfo[] properties, string propName)
        {
            var prop = properties.FirstOrDefault(p => p.Name == propName);
            var customAttribute = prop.GetCustomAttributes(typeof(FieldIDAttribute), false).FirstOrDefault();
            var fieldID = customAttribute.GetPropertyValue<int>("FieldID");
            return fieldID;
        }

        private string GetQueryURL(Guid? guid)
        {
            if (guid.HasValue)
            {
                var url = Configuration.GetConfig<string>("CertificateOfOriginQueryURL");
                return string.Format(url, guid.Value.ToString());
            }
            return string.Empty;
        }

        private CertificateOfOrigin MaterializeCertificateOfOriginDataForWebQuerySP(DbDataReader reader)
        {
            var certificationOfOrigin = reader.Materialize<CertificateOfOrigin>()?.SingleOrDefault();
            if (certificationOfOrigin == null) return null;

            reader.NextResult();
            var certificateOfOriginInvoiceDetail = reader.Materialize<CertificateOfOriginInvoiceDetail>().ToList();

            reader.NextResult();
            var certificateOfOriginDetails = reader.Materialize<CertificateOfOriginDetails>().ToList();

            reader.NextResult();
            var certificateTypeCodeTable = reader.Materialize<CertificateDetailsTypeCodeEnum>().ToList();

            reader.NextResult();
            var certificateOfOriginWebPrintOutDTOList = reader.Materialize<CertificateOfOriginWebPrintOutDTO>().ToList();

            foreach (var certificateOfOriginDetail in certificateOfOriginDetails)
            {
                certificateOfOriginDetail.CertificateDetailsTypeCode = certificateTypeCodeTable.FirstOrDefault(ctct => ctct.ID == certificateOfOriginDetail.CertificateDetailsTypeCodeID);
                certificateOfOriginDetail.CertificateOfOriginWebPrintOutDTO = certificateOfOriginWebPrintOutDTOList.FirstOrDefault(coowp => coowp.CertificateDetailsTypeID == certificateOfOriginDetail.CertificateDetailsTypeCodeID);
            }

            certificationOfOrigin.CertificateOfOriginDetails.AddRange(certificateOfOriginDetails);
            certificationOfOrigin.CertificateOfOriginInvoiceDetail.AddRange(certificateOfOriginInvoiceDetail);

            return certificationOfOrigin;
        }
    }
}