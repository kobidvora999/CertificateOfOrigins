using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Customs.CRM.CertificateOfOrigins.Client.Api;
using Customs.DataDictionary.Entities;
using Customs.Infrastructure.Entities.Enums;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;

namespace Customs.CRM.CertificateOfOrigins.Client.View
{

    public class ProgressBarSelector : FrameworkElement
    {
        public new DependencyObject GetTemplateChild(string childName)
        {
            return base.GetTemplateChild(childName);
        }
    }
    public class TemplateDetails
    {
        public DataTemplate CellTemplate { get; set; }
        public DataTemplate CellEditTemplate { get; set; }

        public TemplateDetails(DataTemplate cellTemplate, DataTemplate cellEditTemplate)
        {
            CellTemplate = cellTemplate;
            CellEditTemplate = cellEditTemplate;
        }
    }

    public static class CellTemplateManager
    {
        private static readonly Dictionary<string, TemplateDetails> TemplateDic =
            new Dictionary<string, TemplateDetails>();


        public static void RegisterTemplate(string templateName, TemplateDetails details)
        {
            if (TemplateDic.ContainsKey(templateName)) return;



            TemplateDic.Add(templateName, details);
        }

        public static TemplateDetails GetTemplate(string templateName)
        {
            return !TemplateDic.ContainsKey(templateName) ? null : TemplateDic[templateName];
        }
    }

    /// <summary>
    /// CellTemplateSelector
    /// </summary>
    public class CellTemplateSelector : DataTemplateSelector
    {
        #region Prop
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellComboBox { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellLovSelection { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellValueInt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellValueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellValueFloat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellValueBit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellValuestring { get; set; }


        public DataTemplate CellMeasurementUnit { get; set; }

        public DataTemplate CellCustomItem { get; set; }
        #endregion

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fwElement = container as FrameworkElement;
            DataTemplate dataTemplate;
            var type = CertificateOfOriginsFieldParser.SetRenderTypeToIWithRenderType(item);

            var template = CellTemplateManager.GetTemplate(type.ToString());
            if (template != null)
            {
                return template.CellTemplate;
            }

            if (type != null && type is RenderTypeKeys)
            {
                switch ((RenderTypeKeys)type)
                {
                    case RenderTypeKeys.LOVSelection:
                        dataTemplate = fwElement.TryFindResource("CellLovSelection") as DataTemplate;
                        return dataTemplate;
                    //return CellLovSelection;

                    case RenderTypeKeys.ComboBox:
                        dataTemplate = fwElement.TryFindResource("CellComboBox") as DataTemplate;
                        return dataTemplate;
                    //return CellComboBox;

                    case RenderTypeKeys.MeasurementUnit:
                        dataTemplate = fwElement.TryFindResource("CellMeasurmentUnit") as DataTemplate;
                        return dataTemplate;
                    //return CellMeasurementUnit;
                    case RenderTypeKeys.LOVSearchExtended:
                        dataTemplate = fwElement.TryFindResource("CellCustomItem") as DataTemplate;
                        return dataTemplate;
                        //return CellCustomItem;

                }
            }
            if (type != null)
            {
                switch ((EDataType)type)
                {
                    case EDataType.DateTime:
                        dataTemplate = fwElement.TryFindResource("CellValueDate") as DataTemplate;
                        return dataTemplate;
                    //return CellValueDate;
                    case EDataType.Int:
                        dataTemplate = fwElement.TryFindResource("CellValueInt") as DataTemplate;
                        return dataTemplate;
                    //return CellValueInt;
                    case EDataType.String:
                        dataTemplate = fwElement.TryFindResource("CellValuestring") as DataTemplate;
                        return dataTemplate;
                    //return CellValuestring;
                    case EDataType.Bool:
                        dataTemplate = fwElement.TryFindResource("CellValueBit") as DataTemplate;
                        return dataTemplate;
                    //return CellValueBit;
                    case EDataType.Float:
                        dataTemplate = fwElement.TryFindResource("CellValueFloat") as DataTemplate;
                        return dataTemplate;
                        //return CellValueFloat;
                }
            }

            return CellValueInt;
        }
    }

    /// <summary>
    /// CellEditTemplateSelector
    /// </summary>
    public class CellEditTemplateSelector : DataTemplateSelector
    {
        #region Prop
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditComboBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditLovSelection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditLovSelectionInitialized { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditValueInt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditValueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditValueFloat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditValueBit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate CellEditValuestring { get; set; }
        /// <summary>

        /// </summary>
        public DataTemplate CellEditMeasurementUnit { get; set; }

        public DataTemplate CellEditCustomItem { get; set; }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fwElement = container as FrameworkElement;
            DataTemplate dataTemplate;
            if (fwElement == null) return null;
            var type = CertificateOfOriginsFieldParser.SetRenderTypeToIWithRenderType(item);

            var template = CellTemplateManager.GetTemplate(type.ToString());
            if (template != null)
            {
                return template.CellEditTemplate;
            }

            if (type != null && type is RenderTypeKeys)
            {
                switch ((RenderTypeKeys)type)
                {
                    case RenderTypeKeys.LOVSelection:
                        if (item is IWithRenderType && (item as IWithRenderType).InitialType != null)
                        {
                            dataTemplate = fwElement.TryFindResource("CellEditLovSelectionInitialized") as DataTemplate;
                            return dataTemplate;

                        }
                        dataTemplate = fwElement.TryFindResource("CellEditLovSelection") as DataTemplate;
                        return dataTemplate;
                    //return CellEditLovSelection;
                    case RenderTypeKeys.ComboBox:
                        dataTemplate = fwElement.TryFindResource("CellEditComboBox") as DataTemplate;
                        return dataTemplate;
                    //return CellEditComboBox;
                    case RenderTypeKeys.MeasurementUnit:
                        dataTemplate = fwElement.TryFindResource("CellEditMeasurmentUnit") as DataTemplate;
                        return dataTemplate;
                    //return CellEditMeasurementUnit;
                    case RenderTypeKeys.LOVSearchExtended:
                        dataTemplate = fwElement.TryFindResource("CellEditCustomItem") as DataTemplate;
                        return dataTemplate;
                        //return CellEditCustomItem;
                }
            }

            if (type != null)
            {
                switch ((EDataType)type)
                {
                    case EDataType.DateTime:
                        dataTemplate = fwElement.TryFindResource("CellEditValueDate") as DataTemplate;
                        return dataTemplate;
                    //return CellEditValueDate;
                    case EDataType.Int:
                        dataTemplate = fwElement.TryFindResource("CellEditValueInt") as DataTemplate;
                        return dataTemplate;
                    //return CellEditValueInt;
                    case EDataType.String:
                        dataTemplate = fwElement.TryFindResource("CellEditValuestring") as DataTemplate;
                        return dataTemplate;
                    //return CellEditValuestring;
                    case EDataType.Bool:
                        dataTemplate = fwElement.TryFindResource("CellEditValueBit") as DataTemplate;
                        return dataTemplate;
                    //return CellEditValueBit; 
                    case EDataType.Float:
                        dataTemplate = fwElement.TryFindResource("CellEditValueFloat") as DataTemplate;
                        return dataTemplate;
                        //return CellEditValueFloat;
                }
            }
            return fwElement.TryFindResource("CellEditValueInt") as DataTemplate;//CellEditValueInt;
        }
    }
}

