using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Customs.CertificateOfOrigins.Entities;
using Customs.DataDictionary.Entities;
using Customs.Infrastructure.Entities.Enums;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;

namespace Customs.CRM.CertificateOfOrigins.Client.Api
{
    public static class CertificateOfOriginsFieldParser
    {
        private static void SetLovTypeInItemAccordingToAttribute(List<PropertyMetadataAttribute> attrList, IWithRenderType item)
        {
            if (item == null) return;
            var mUnit = attrList.FirstOrDefault(a => a is MeasurementUnitMetadataAttribute);

            if (mUnit != null)
            {
                item.SelectedUnit = ((MeasurementUnitMetadataAttribute)mUnit).SelectedUnit;
                item.MeasurementCategory = ((MeasurementUnitMetadataAttribute)mUnit).EMeasurementCategory;
            }
            else
            {
                var attrRenderType = attrList.FirstOrDefault(a => a.RenderType != null);
                if (attrRenderType != null)
                {
                    item.LovType = attrRenderType.RenderType;
                    if (ObjectMetadataAttribute.ContainsResource(attrRenderType.RenderType))
                    {
                        item.InitialType = attrRenderType.RenderType;
                    }
                }

                var attrPredicate = attrList.FirstOrDefault(a => a is PredicateMetadataAttribute);
                if (attrPredicate != null)
                {
                    item.Predicate = ((PredicateMetadataAttribute)attrPredicate).Predicate;
                    if (item.Predicate.Contains("\\\""))
                    {
                        item.Predicate = item.Predicate.Replace("\\\"", "\"");
                    }
                }
                else
                {
                    item.Predicate = null;
                }
            }
        }

        private static RiskFactorFields GetSelectedField(object item)
        {
            if (item == null) return null;
            
            if (item is CertificateOfOriginDetails certificateOfOriginDetails)
            {
                #region create fake riskfactorfield dto from the data in population field

                var ret = new RiskFactorFields();

                //Set data type
                ret.EndFieldDataTypeID = certificateOfOriginDetails.CertificateDetailsTypeCode.DataTypeID;

                //Set LOV type
                if (certificateOfOriginDetails.CertificateDetailsTypeCode.DetailTypeFormat != null)
                {
                    ret.EndFieldXmlConfiguration = string.Format(
                        "<MalamAttributeContainer Count=\"1\"><MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP.PropertyMetadataAttribute><RenderType>{0}</RenderType></MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP.PropertyMetadataAttribute></MalamAttributeContainer>",
                        certificateOfOriginDetails.CertificateDetailsTypeCode.DetailTypeFormat);
                }
                return ret;
                #endregion
            }
            return null;
        }

        public static object SetRenderTypeToIWithRenderType(object item)
        {
            var field = GetSelectedField(item);
            List<PropertyMetadataAttribute> attr = null;
            if (field != null)
            {
                attr = GetAttribute(field.EndFieldXmlConfiguration);
            }
            if (!attr.IsNullOrEmpty())
            {
                SetLovTypeInItemAccordingToAttribute(attr, item as IWithRenderType);
            }
           
            var fieldDataType = (int)EDataType.String;
            if (field != null)
            {
                fieldDataType = (int)field.EndFieldDataTypeID;
            }
            return GetRenderTypeKeyOrEDataType(attr, fieldDataType);
        }

        private static EDataType SwitchFieldTypeId(int? dataTypeID)
        {
            switch (dataTypeID)
            {
                case (int)EDataType.Bool:
                    return EDataType.Bool;
                case (int)EDataType.DateTime:
                    return EDataType.DateTime;
                case (int)EDataType.Float:
                    return EDataType.Float;
                case (int)EDataType.Int:
                    return EDataType.Int;
                case (int)EDataType.String:
                    return EDataType.String;
            }
            return EDataType.Int;
        }

        private static object GetRenderTypeKeyOrEDataType(List<PropertyMetadataAttribute> attrList, int dataTypeID)
        {
            if (attrList.IsNullOrEmpty() || !attrList.Any(a => a.RenderType != null)) return SwitchFieldTypeId(dataTypeID);

            var attr = attrList.First(a => a.RenderType != null);

            if (typeof(ILookUp).IsAssignableFrom(attr.RenderType))
            {
                return RenderTypeKeys.ComboBox;
            }
            if (typeof(IEntity).IsAssignableFrom(attr.RenderType))
            {
                return RenderTypeKeys.LOVSearchExtended;
            }
            if (typeof(ILov).IsAssignableFrom(attr.RenderType))
            {
                return RenderTypeKeys.LOVSelection;
            }
            if (RenderTypeKeys.MeasurementUnit == attr.RenderTypeKey)
            {
                return attr.RenderTypeKey;
            }
            if (typeof(IEntity).IsAssignableFrom(attr.RenderType))
            {
                return attr.RenderTypeKey;
            }

            return SwitchFieldTypeId(dataTypeID);
        }

        public static List<PropertyMetadataAttribute> GetAttribute(string xmlConfiguration, string propertyGroupName = "RiskAssessment")//RiskFactorFields riskFactorField)
        {
            var listOfAttr = new List<PropertyMetadataAttribute>();
            if (!string.IsNullOrEmpty(xmlConfiguration))
            {
                var x = new XmlSerializer(typeof(MalamAttributeContainer));
                var sr = new StringReader(xmlConfiguration);
                var c = (MalamAttributeContainer)x.Deserialize(sr);

                if (c.IsNullOrEmpty()) return null;

                //priority will given to Attribute with PropertyGroupName=="RiskAssessment" on one with out
                listOfAttr =
                    c.OfType<PropertyMetadataAttribute>().Where(pg => pg.PropertyGroupName == propertyGroupName).
                        ToList();
                PropertyMetadataAttribute attr;
                if (!listOfAttr.Any(a => a.RenderType != null))
                {
                    attr = c.OfType<PropertyMetadataAttribute>().Where(a => a.RenderType != null).FirstOrDefault();
                    if (attr != null)
                    {
                        listOfAttr.Add(attr);
                    }
                }
                if (!listOfAttr.Any(a => a is PredicateMetadataAttribute))
                {
                    attr = c.OfType<PropertyMetadataAttribute>().Where(a => a is PredicateMetadataAttribute).FirstOrDefault();
                    if (attr != null)
                    {
                        listOfAttr.Add(attr);
                    }
                }
                return listOfAttr;
            }
            return null;
        }

    }
}
